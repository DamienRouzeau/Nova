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

        if (avgHeight > 25f) // Pas de lac en hauteur
        {
            return null;
        }

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
        // Au lieu de retourner 20f fixe, sample le heightmap

        // Convertis world pos en local chunk pos
        Vector3 localPos = worldPos - chunk.gameObject.transform.position;
        int sampleCount = 10;
        float totalHeight = 0;

        for (int i = 0; i < sampleCount; i++)
        {
            float angle = (360f / sampleCount) * i * Mathf.Deg2Rad;
            float x = localPos.x + Mathf.Cos(angle) * radius * 0.5f;
            float z = localPos.z + Mathf.Sin(angle) * radius * 0.5f;

            int gridX = Mathf.Clamp((int)x, 0, chunk.heightMap.GetLength(0) - 1);
            int gridZ = Mathf.Clamp((int)z, 0, chunk.heightMap.GetLength(1) - 1);

            totalHeight += chunk.heightMap[gridX, gridZ];
        }

        return totalHeight / sampleCount;
    }
}

public class Lake
{
    public GameObject gameObject;
    public Vector3 center;
    public float radius;
    public float waterLevel;
}
