using UnityEngine;
using System.Collections.Generic;

public class ProceduralWorldGenerator : MonoBehaviour
{
    [Header("World Settings")]
    [SerializeField] private int worldSize = 512; // Taille en unités
    [SerializeField] private int chunkSize = 64;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 12345;

    [Header("Terrain Height")]
    [SerializeField] private float heightMultiplier = 50f;
    [SerializeField] private float noiseScale = 0.01f;
    [SerializeField] private int octaves = 4;
    [SerializeField] private float persistence = 0.5f;
    [SerializeField] private float lacunarity = 2f;

    [Header("Biomes")]
    [SerializeField] private BiomeData[] biomes;
    [SerializeField] private float biomeBlendDistance = 20f;

    [Header("Terrain Material")]
    [SerializeField] private Material terrainMaterial;
    [SerializeField] private Texture2D[] splatmaps; // Textures pour chaque biome

    [Header("Placement")]
    [SerializeField] private PlacementSettings vegetationSettings;
    [SerializeField] private PlacementSettings resourceSettings;
    [SerializeField] private PlacementSettings lootSettings;
    [SerializeField] private PlacementSettings creatureSpawnSettings;

    [Header("Water")]
    [SerializeField] private GameObject waterPrefab;
    [SerializeField] private float waterLevel = 20f;

    [Header("Performance")]
    [SerializeField] private int maxObjectsPerFrame = 50;

    private System.Random prng;
    private Dictionary<Vector2Int, TerrainChunk> chunks = new Dictionary<Vector2Int, TerrainChunk>();
    private Queue<Vector2Int> chunksToGenerate = new Queue<Vector2Int>();
    private List<GameObject> spawnedObjects = new List<GameObject>();

    // Noise offset basé sur le seed
    private Vector2 noiseOffset;

    void Start()
    {
        Initialize();
        GenerateWorld();
    }

    void Initialize()
    {
        if (useRandomSeed)
        {
            seed = Random.Range(0, 999999);
        }

        prng = new System.Random(seed);
        noiseOffset = new Vector2(prng.Next(-10000, 10000), prng.Next(-10000, 10000));

        Debug.Log($"Generating world with seed: {seed}");
    }

    void GenerateWorld()
    {
        // Génère les chunks du monde
        int chunksPerSide = Mathf.CeilToInt((float)worldSize / chunkSize);

        for (int x = 0; x < chunksPerSide; x++)
        {
            for (int z = 0; z < chunksPerSide; z++)
            {
                Vector2Int chunkCoord = new Vector2Int(x, z);
                chunksToGenerate.Enqueue(chunkCoord);
            }
        }

        // Génère l'eau si nécessaire
        if (waterPrefab != null)
        {
            GameObject water = Instantiate(waterPrefab, new Vector3(worldSize / 2f, waterLevel, worldSize / 2f), Quaternion.identity, transform);
            water.transform.localScale = new Vector3(worldSize / 10f, 1, worldSize / 10f);
        }

        StartCoroutine(GenerateChunksCoroutine());
    }

    System.Collections.IEnumerator GenerateChunksCoroutine()
    {
        while (chunksToGenerate.Count > 0)
        {
            Vector2Int chunkCoord = chunksToGenerate.Dequeue();
            GenerateChunk(chunkCoord);

            yield return null; // Une frame entre chaque chunk
        }

        Debug.Log("World generation complete!");

        // Génère les objets (végétation, loot, etc.)
        yield return StartCoroutine(PopulateWorldCoroutine());
    }

    void GenerateChunk(Vector2Int coord)
    {
        GameObject chunkObj = new GameObject($"Chunk_{coord.x}_{coord.y}");
        chunkObj.transform.parent = transform;
        chunkObj.transform.position = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

        MeshFilter meshFilter = chunkObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = chunkObj.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = chunkObj.AddComponent<MeshCollider>();

        if (terrainMaterial != null)
        {
            meshRenderer.material = terrainMaterial;
        }

        // Génère le mesh du terrain
        TerrainMesh terrainMesh = GenerateTerrainMesh(coord);
        meshFilter.mesh = terrainMesh.mesh;
        meshCollider.sharedMesh = terrainMesh.mesh;

        // Stocke le chunk
        TerrainChunk chunk = new TerrainChunk
        {
            gameObject = chunkObj,
            coord = coord,
            heightMap = terrainMesh.heightMap,
            biomeMap = terrainMesh.biomeMap
        };

        chunks[coord] = chunk;
    }

    TerrainMesh GenerateTerrainMesh(Vector2Int chunkCoord)
    {
        int resolution = chunkSize + 1;
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[chunkSize * chunkSize * 6];
        Vector2[] uvs = new Vector2[vertices.Length];
        Color[] colors = new Color[vertices.Length]; // Pour biome blending

        float[,] heightMap = new float[resolution, resolution];
        BiomeType[,] biomeMap = new BiomeType[resolution, resolution];

        // Génère les vertices
        int vertIndex = 0;
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float worldX = chunkCoord.x * chunkSize + x;
                float worldZ = chunkCoord.y * chunkSize + z;

                // Génère la hauteur avec Perlin Noise
                float height = GenerateHeight(worldX, worldZ);
                heightMap[x, z] = height;

                // Détermine le biome
                BiomeType biome = DetermineBiome(worldX, worldZ, height);
                biomeMap[x, z] = biome;

                vertices[vertIndex] = new Vector3(x, height, z);
                uvs[vertIndex] = new Vector2((float)x / chunkSize, (float)z / chunkSize);
                colors[vertIndex] = GetBiomeColor(biome);

                vertIndex++;
            }
        }

        // Génère les triangles
        int triIndex = 0;
        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int i = z * resolution + x;

                triangles[triIndex] = i;
                triangles[triIndex + 1] = i + resolution;
                triangles[triIndex + 2] = i + 1;
                triangles[triIndex + 3] = i + 1;
                triangles[triIndex + 4] = i + resolution;
                triangles[triIndex + 5] = i + resolution + 1;

                triIndex += 6;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.RecalculateNormals();

        return new TerrainMesh { mesh = mesh, heightMap = heightMap, biomeMap = biomeMap };
    }

    float GenerateHeight(float x, float z)
    {
        float height = 0;
        float amplitude = 1;
        float frequency = 1;

        // Octaves de Perlin Noise pour variation
        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x + noiseOffset.x) * noiseScale * frequency;
            float sampleZ = (z + noiseOffset.y) * noiseScale * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
            height += perlinValue * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return height * heightMultiplier;
    }

    BiomeType DetermineBiome(float x, float z, float height)
    {
        // Noise pour distribution des biomes
        float biomeNoise = Mathf.PerlinNoise((x + noiseOffset.x) * 0.005f, (z + noiseOffset.y) * 0.005f);

        // Règles basées sur hauteur et noise
        if (height < waterLevel)
        {
            return BiomeType.Water;
        }
        else if (height < waterLevel + 5f)
        {
            return BiomeType.Beach;
        }
        else if (height > heightMultiplier * 0.7f)
        {
            return BiomeType.Mountain;
        }
        else if (biomeNoise > 0.6f)
        {
            return BiomeType.Desert;
        }
        else if (biomeNoise > 0.4f)
        {
            return BiomeType.Forest;
        }
        else
        {
            return BiomeType.Plains;
        }
    }

    Color GetBiomeColor(BiomeType biome)
    {
        switch (biome)
        {
            case BiomeType.Water: return new Color(0.2f, 0.4f, 0.8f);
            case BiomeType.Beach: return new Color(0.9f, 0.9f, 0.7f);
            case BiomeType.Plains: return new Color(0.4f, 0.7f, 0.3f);
            case BiomeType.Forest: return new Color(0.2f, 0.5f, 0.2f);
            case BiomeType.Desert: return new Color(0.9f, 0.8f, 0.5f);
            case BiomeType.Mountain: return new Color(0.5f, 0.5f, 0.5f);
            default: return Color.white;
        }
    }

    System.Collections.IEnumerator PopulateWorldCoroutine()
    {
        int objectsThisFrame = 0;

        foreach (var chunkPair in chunks)
        {
            TerrainChunk chunk = chunkPair.Value;

            // Place végétation
            if (vegetationSettings != null && vegetationSettings.enabled)
            {
                foreach (var obj in PlaceObjectsInChunk(chunk, vegetationSettings))
                {
                    spawnedObjects.Add(obj);
                    objectsThisFrame++;

                    if (objectsThisFrame >= maxObjectsPerFrame)
                    {
                        objectsThisFrame = 0;
                        yield return null;
                    }
                }
            }

            // Place ressources (minerais)
            if (resourceSettings != null && resourceSettings.enabled)
            {
                foreach (var obj in PlaceObjectsInChunk(chunk, resourceSettings))
                {
                    spawnedObjects.Add(obj);
                    objectsThisFrame++;

                    if (objectsThisFrame >= maxObjectsPerFrame)
                    {
                        objectsThisFrame = 0;
                        yield return null;
                    }
                }
            }

            // Place loot (coffres)
            if (lootSettings != null && lootSettings.enabled)
            {
                foreach (var obj in PlaceObjectsInChunk(chunk, lootSettings))
                {
                    spawnedObjects.Add(obj);
                    objectsThisFrame++;

                    if (objectsThisFrame >= maxObjectsPerFrame)
                    {
                        objectsThisFrame = 0;
                        yield return null;
                    }
                }
            }

            // Place spawns de créatures
            if (creatureSpawnSettings != null && creatureSpawnSettings.enabled)
            {
                foreach (var obj in PlaceObjectsInChunk(chunk, creatureSpawnSettings))
                {
                    spawnedObjects.Add(obj);
                    objectsThisFrame++;

                    if (objectsThisFrame >= maxObjectsPerFrame)
                    {
                        objectsThisFrame = 0;
                        yield return null;
                    }
                }
            }
        }

        Debug.Log($"Spawned {spawnedObjects.Count} objects in the world!");
    }

    List<GameObject> PlaceObjectsInChunk(TerrainChunk chunk, PlacementSettings settings)
    {
        List<GameObject> placedObjects = new List<GameObject>();

        int attempts = Mathf.RoundToInt(chunkSize * chunkSize * settings.density);

        for (int i = 0; i < attempts; i++)
        {
            // Position aléatoire dans le chunk
            float localX = (float)prng.NextDouble() * chunkSize;
            float localZ = (float)prng.NextDouble() * chunkSize;

            int gridX = Mathf.FloorToInt(localX);
            int gridZ = Mathf.FloorToInt(localZ);

            if (gridX >= chunkSize || gridZ >= chunkSize) continue;

            float height = chunk.heightMap[gridX, gridZ];
            BiomeType biome = chunk.biomeMap[gridX, gridZ];

            // Vérifie si on peut placer ici
            if (!CanPlaceObject(settings, height, biome)) continue;

            // Choisit un prefab selon le biome
            GameObject prefab = settings.GetPrefabForBiome(biome, prng);
            if (prefab == null) continue;

            // Position mondiale
            Vector3 worldPos = chunk.gameObject.transform.position + new Vector3(localX, height, localZ);

            // Offset aléatoire en Y si nécessaire
            worldPos.y += Random.Range(settings.minYOffset, settings.maxYOffset);

            // Rotation aléatoire
            Quaternion rotation = Quaternion.Euler(0, (float)prng.NextDouble() * 360f, 0);

            // Scale aléatoire
            float scale = Random.Range(settings.minScale, settings.maxScale);

            GameObject obj = Instantiate(prefab, worldPos, rotation, chunk.gameObject.transform);
            obj.transform.localScale = Vector3.one * scale;

            placedObjects.Add(obj);
        }

        return placedObjects;
    }

    bool CanPlaceObject(PlacementSettings settings, float height, BiomeType biome)
    {
        // Vérifie hauteur
        if (height < settings.minHeight || height > settings.maxHeight)
            return false;

        // Vérifie biome
        if (settings.allowedBiomes.Length > 0)
        {
            bool biomeAllowed = false;
            foreach (var allowedBiome in settings.allowedBiomes)
            {
                if (allowedBiome == biome)
                {
                    biomeAllowed = true;
                    break;
                }
            }
            if (!biomeAllowed) return false;
        }

        return true;
    }

    public int GetSeed() => seed;
    public TerrainChunk GetChunkAt(Vector2Int coord) => chunks.ContainsKey(coord) ? chunks[coord] : null;
}

// ==================== STRUCTURES ====================

public class TerrainChunk
{
    public GameObject gameObject;
    public Vector2Int coord;
    public float[,] heightMap;
    public BiomeType[,] biomeMap;
}

public class TerrainMesh
{
    public Mesh mesh;
    public float[,] heightMap;
    public BiomeType[,] biomeMap;
}

public enum BiomeType
{
    Water,
    Beach,
    Plains,
    Forest,
    Desert,
    Mountain
}