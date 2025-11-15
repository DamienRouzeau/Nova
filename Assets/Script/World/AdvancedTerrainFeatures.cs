using UnityEngine;
using System.Collections.Generic;

public class LakeGenerator : MonoBehaviour
{
    [Header("Lake Settings")]
    [SerializeField] private GameObject waterPrefab;
    [SerializeField] private int minLakes = 2;
    [SerializeField] private int maxLakes = 5;
    [SerializeField] private float minLakeRadius = 20f;
    [SerializeField] private float maxLakeRadius = 50f;
    [SerializeField] private float lakeDepth = 5f;

    private ProceduralWorldGenerator worldGenerator;
    private System.Random prng;

    public void Initialize(ProceduralWorldGenerator generator, int seed)
    {
        worldGenerator = generator;
        prng = new System.Random(seed + 1000); // Offset pour seed différent
    }

    public List<Lake> GenerateLakes(Dictionary<Vector2Int, TerrainChunk> chunks)
    {
        List<Lake> lakes = new List<Lake>();
        int lakeCount = prng.Next(minLakes, maxLakes + 1);

        for (int i = 0; i < lakeCount; i++)
        {
            Lake lake = CreateRandomLake(chunks, i);
            if (lake != null)
            {
                lakes.Add(lake);
            }
        }

        return lakes;
    }

    Lake CreateRandomLake(Dictionary<Vector2Int, TerrainChunk> chunks, int lakeIndex)
    {
        // Choisit un chunk aléatoire
        int chunkIndex = prng.Next(0, chunks.Count);
        TerrainChunk chunk = null;

        int counter = 0;
        foreach (var pair in chunks)
        {
            if (counter == chunkIndex)
            {
                chunk = pair.Value;
                break;
            }
            counter++;
        }

        if (chunk == null) return null;

        // Position aléatoire dans le chunk
        float radius = (float)(prng.NextDouble() * (maxLakeRadius - minLakeRadius) + minLakeRadius);
        Vector3 center = chunk.gameObject.transform.position +
                        new Vector3((float)prng.NextDouble() * 64, 0, (float)prng.NextDouble() * 64);

        // Trouve la hauteur moyenne
        float avgHeight = GetAverageHeight(chunk, center, radius);

        // Crée l'objet lac
        if (waterPrefab != null)
        {
            GameObject lakeObj = Instantiate(waterPrefab, new Vector3(center.x, avgHeight - lakeDepth / 2f, center.z), Quaternion.identity);
            lakeObj.transform.localScale = new Vector3(radius / 5f, 1, radius / 5f);
            lakeObj.name = $"Lake_{lakeIndex}";

            return new Lake
            {
                gameObject = lakeObj,
                center = center,
                radius = radius,
                waterLevel = avgHeight
            };
        }

        return null;
    }

    float GetAverageHeight(TerrainChunk chunk, Vector3 worldPos, float radius)
    {
        // Simplifié : retourne une hauteur
        return 20f; // À améliorer avec sampling du heightmap
    }
}

public class Lake
{
    public GameObject gameObject;
    public Vector3 center;
    public float radius;
    public float waterLevel;
}
