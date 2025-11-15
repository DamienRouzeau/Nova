using UnityEngine;
using System.Collections.Generic;


public class CaveGenerator : MonoBehaviour
{
    [Header("Cave Settings")]
    [SerializeField] private int caveCount = 5;
    [SerializeField] private float minCaveDepth = 10f;
    [SerializeField] private float maxCaveDepth = 30f;
    [SerializeField] private GameObject caveEntrancePrefab;

    private System.Random prng;

    public void Initialize(int seed)
    {
        prng = new System.Random(seed + 2000);
    }

    public List<CaveEntrance> GenerateCaves(Dictionary<Vector2Int, TerrainChunk> chunks)
    {
        List<CaveEntrance> caves = new List<CaveEntrance>();

        for (int i = 0; i < caveCount; i++)
        {
            CaveEntrance cave = CreateCaveEntrance(chunks);
            if (cave != null)
            {
                caves.Add(cave);
            }
        }

        return caves;
    }

    CaveEntrance CreateCaveEntrance(Dictionary<Vector2Int, TerrainChunk> chunks)
    {
        // Trouve une position sur une pente (montagne)
        // Simplifié pour l'exemple

        if (caveEntrancePrefab != null)
        {
            // Position aléatoire
            Vector3 pos = new Vector3(
                (float)prng.NextDouble() * 500,
                30f,
                (float)prng.NextDouble() * 500
            );

            GameObject caveObj = Instantiate(caveEntrancePrefab, pos, Quaternion.identity);

            return new CaveEntrance
            {
                gameObject = caveObj,
                position = pos,
                depth = (float)(prng.NextDouble() * (maxCaveDepth - minCaveDepth) + minCaveDepth)
            };
        }

        return null;
    }
}

public class CaveEntrance
{
    public GameObject gameObject;
    public Vector3 position;
    public float depth;
}