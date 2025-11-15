using UnityEngine;
using System.Collections.Generic;

public class GrassGPUInstancing : MonoBehaviour
{
    [Header("Grass Settings")]
    [SerializeField] private Mesh grassMesh;
    [SerializeField] private Material grassMaterial;
    [SerializeField] private int grassPerChunk = 1500;
    [SerializeField] private float grassScale = 1f;

    [Header("Performance")]
    [SerializeField] private float maxRenderDistance = 80f;
    [SerializeField] private bool enableCulling = true;
    [SerializeField] private float chunkUpdateInterval = 2f; // Mise à jour toutes les 2s

    [Header("Biome Filter")]
    [SerializeField] private bool spawnInForest = true;
    [SerializeField] private bool spawnInPlains = true;

    private Dictionary<Vector2Int, List<Matrix4x4[]>> chunkGrassBatches = new Dictionary<Vector2Int, List<Matrix4x4[]>>();
    private HashSet<Vector2Int> loadedChunks = new HashSet<Vector2Int>();
    private int batchSize = 1023;
    private float lastUpdateTime;
    private ProceduralWorldGenerator worldGenerator;
    private Transform player;

    void Start()
    {
        worldGenerator = GetComponent<ProceduralWorldGenerator>();

        // Trouve le joueur
        StartCoroutine(FindPlayerDelayed());
    }

    System.Collections.IEnumerator FindPlayerDelayed()
    {
        // Attends que le joueur soit spawné
        yield return new WaitForSeconds(3f);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("Player found for grass system!");
        }
        else
        {
            Debug.LogWarning("Player not found! Using camera instead.");
            if (Camera.main != null)
            {
                player = Camera.main.transform;
            }
        }
    }

    void Update()
    {
        // Mise à jour périodique des chunks d'herbe
        if (Time.time - lastUpdateTime > chunkUpdateInterval && player != null)
        {
            UpdateGrassChunks();
            lastUpdateTime = Time.time;
        }

        // Render l'herbe
        RenderGrass();
    }

    void UpdateGrassChunks()
    {
        if (worldGenerator == null || player == null) return;

        Vector3 playerPos = player.position;
        int chunkSize = 64; // Doit correspondre à ton chunk size

        // Détermine quels chunks doivent être chargés
        HashSet<Vector2Int> chunksToLoad = new HashSet<Vector2Int>();

        int loadRadius = Mathf.CeilToInt(maxRenderDistance / chunkSize) + 1;

        for (int x = -loadRadius; x <= loadRadius; x++)
        {
            for (int z = -loadRadius; z <= loadRadius; z++)
            {
                int chunkX = Mathf.FloorToInt(playerPos.x / chunkSize) + x;
                int chunkZ = Mathf.FloorToInt(playerPos.z / chunkSize) + z;

                Vector2Int chunkCoord = new Vector2Int(chunkX, chunkZ);

                // Vérifie la distance
                Vector3 chunkWorldPos = new Vector3(chunkX * chunkSize, 0, chunkZ * chunkSize);
                if (Vector3.Distance(new Vector3(playerPos.x, 0, playerPos.z), new Vector3(chunkWorldPos.x, 0, chunkWorldPos.z)) <= maxRenderDistance)
                {
                    chunksToLoad.Add(chunkCoord);
                }
            }
        }

        // Charge les nouveaux chunks
        foreach (var coord in chunksToLoad)
        {
            if (!loadedChunks.Contains(coord))
            {
                LoadGrassForChunk(coord);
                loadedChunks.Add(coord);
            }
        }

        // Décharge les chunks trop loin
        List<Vector2Int> chunksToUnload = new List<Vector2Int>();
        foreach (var coord in loadedChunks)
        {
            if (!chunksToLoad.Contains(coord))
            {
                chunksToUnload.Add(coord);
            }
        }

        foreach (var coord in chunksToUnload)
        {
            UnloadGrassForChunk(coord);
            loadedChunks.Remove(coord);
        }
    }

    void LoadGrassForChunk(Vector2Int chunkCoord)
    {
        if (worldGenerator == null) return;

        TerrainChunk chunk = worldGenerator.GetChunkAt(chunkCoord);
        if (chunk == null) return;

        GenerateGrassForChunk(chunk);
    }

    void UnloadGrassForChunk(Vector2Int chunkCoord)
    {
        if (chunkGrassBatches.ContainsKey(chunkCoord))
        {
            chunkGrassBatches.Remove(chunkCoord);
        }
    }

    public void GenerateGrassForChunk(TerrainChunk chunk)
    {
        List<Matrix4x4> matrices = new List<Matrix4x4>();
        System.Random chunkRandom = new System.Random(chunk.coord.GetHashCode());

        int spawnRadius = 64; // Chunk size

        for (int i = 0; i < grassPerChunk; i++)
        {
            Vector3 localPos = new Vector3(
                (float)chunkRandom.NextDouble() * spawnRadius,
                0,
                (float)chunkRandom.NextDouble() * spawnRadius
            );

            Vector3 worldPos = chunk.gameObject.transform.position + localPos;

            int gridX = Mathf.Clamp((int)localPos.x, 0, chunk.heightMap.GetLength(0) - 1);
            int gridZ = Mathf.Clamp((int)localPos.z, 0, chunk.heightMap.GetLength(1) - 1);

            float height = chunk.heightMap[gridX, gridZ];
            BiomeType biome = chunk.biomeMap[gridX, gridZ];

            // Filtre par biome
            bool allowSpawn = false;
            if (spawnInForest && biome == BiomeType.Forest) allowSpawn = true;
            if (spawnInPlains && biome == BiomeType.Plains) allowSpawn = true;

            if (!allowSpawn) continue;

            worldPos.y = height - 0.05f; // Légèrement enfoncé

            Quaternion rotation = Quaternion.Euler(
                (float)chunkRandom.NextDouble() * 15 - 7.5f,
                (float)chunkRandom.NextDouble() * 360,
                (float)chunkRandom.NextDouble() * 15 - 7.5f
            );

            float scale = (float)(chunkRandom.NextDouble() * 0.6 + 0.7) * grassScale;
            Vector3 scaleVec = new Vector3(scale, scale * (float)(chunkRandom.NextDouble() * 0.4 + 0.8), scale);

            matrices.Add(Matrix4x4.TRS(worldPos, rotation, scaleVec));
        }

        // Divise en batches
        List<Matrix4x4[]> batches = new List<Matrix4x4[]>();

        for (int i = 0; i < matrices.Count; i += batchSize)
        {
            int count = Mathf.Min(batchSize, matrices.Count - i);
            Matrix4x4[] batch = new Matrix4x4[count];

            for (int j = 0; j < count; j++)
            {
                batch[j] = matrices[i + j];
            }

            batches.Add(batch);
        }

        chunkGrassBatches[chunk.coord] = batches;
    }

    void RenderGrass()
    {
        if (grassMesh == null || grassMaterial == null || player == null) return;

        Vector3 camPos = player.position;

        foreach (var kvp in chunkGrassBatches)
        {
            foreach (var batch in kvp.Value)
            {
                if (batch.Length == 0) continue;

                if (enableCulling)
                {
                    Vector3 batchPos = batch[0].GetColumn(3);
                    float dist = Vector3.Distance(new Vector3(camPos.x, 0, camPos.z), new Vector3(batchPos.x, 0, batchPos.z));

                    if (dist > maxRenderDistance)
                        continue;
                }

                Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, batch);
            }
        }
    }

    public void ClearAllGrass()
    {
        chunkGrassBatches.Clear();
        loadedChunks.Clear();
    }
}