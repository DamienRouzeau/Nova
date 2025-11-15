Shader "Custom/TerrainBiomeTextures"
{
    Properties
    {
        _WaterTex("Water", 2D) = "white" {}
        _BeachTex("Beach", 2D) = "white" {}
        _PlainsTex("Plains", 2D) = "white" {}
        _ForestTex("Forest", 2D) = "white" {}
        _DesertTex("Desert", 2D) = "white" {}
        _MountainTex("Mountain", 2D) = "white" {}

        _TextureScale("Texture Scale", Float) = 5.0
        _BlendSharpness("Blend Sharpness", Range(1, 10)) = 5.0
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }

            CGPROGRAM
            #pragma surface surf Standard fullforwardshadows
            #pragma target 3.0

            sampler2D _WaterTex, _BeachTex, _PlainsTex, _ForestTex, _DesertTex, _MountainTex;
            float _TextureScale;
            float _BlendSharpness;

            struct Input
            {
                float3 worldPos;
                float4 color : COLOR;
            };

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                // Tiling basé sur world position
                float2 uv = IN.worldPos.xz / _TextureScale;

                // Ajoute variation avec noise
                float noise = frac(sin(dot(uv * 20.0, float2(12.9898, 78.233))) * 43758.5453);
                float2 uvNoise = uv + noise * 0.02;

                // Sample textures avec variation
                float4 waterCol = tex2D(_WaterTex, uvNoise);
                float4 beachCol = tex2D(_BeachTex, uvNoise);
                float4 plainsCol = tex2D(_PlainsTex, uvNoise);
                float4 forestCol = tex2D(_ForestTex, uvNoise);
                float4 desertCol = tex2D(_DesertTex, uvNoise);
                float4 mountainCol = tex2D(_MountainTex, uvNoise);

                // Vertex color = biome
                float4 biomeColor = IN.color;

                // Calcul des poids (PLUS SHARP)
                float waterW = saturate(pow(1.0 - distance(biomeColor.rgb, float3(0.0, 0.3, 0.8)), _BlendSharpness));
                float beachW = saturate(pow(1.0 - distance(biomeColor.rgb, float3(0.9, 0.9, 0.6)), _BlendSharpness));
                float plainsW = saturate(pow(1.0 - distance(biomeColor.rgb, float3(0.3, 0.8, 0.2)), _BlendSharpness));
                float forestW = saturate(pow(1.0 - distance(biomeColor.rgb, float3(0.4, 0.6, 0.15)), _BlendSharpness));
                float desertW = saturate(pow(1.0 - distance(biomeColor.rgb, float3(0.9, 0.7, 0.3)), _BlendSharpness));
                float mountainW = saturate(pow(1.0 - distance(biomeColor.rgb, float3(0.6, 0.6, 0.6)), _BlendSharpness));

                float total = waterW + beachW + plainsW + forestW + desertW + mountainW + 0.001;

                waterW /= total;
                beachW /= total;
                plainsW /= total;
                forestW /= total;
                desertW /= total;
                mountainW /= total;

                // Blend final
                float4 final =
                    waterCol * waterW +
                    beachCol * beachW +
                    plainsCol * plainsW +
                    forestCol * forestW +
                    desertCol * desertW +
                    mountainCol * mountainW;

                // Ajoute variation de luminosité
                final.rgb *= (0.85 + noise * 0.3);

                o.Albedo = final.rgb;
                o.Metallic = 0;
                o.Smoothness = 0.2;
            }
            ENDCG
        }
}