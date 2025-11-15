Shader "Custom/GrassInstanced"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
    }

        SubShader
        {
            Tags { "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" }

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_instancing
                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                sampler2D _MainTex;
                float _Cutoff;

                v2f vert(appdata v)
                {
                    v2f o;

                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    UNITY_SETUP_INSTANCE_ID(i);

                    fixed4 col = tex2D(_MainTex, i.uv);
                    clip(col.a - _Cutoff);
                    return col;
                }
                ENDCG
            }
        }
}