using UnityEngine;
using System.Collections.Generic;

public class AbandonedExpeditionSpawner : MonoBehaviour
{
    [Header("Expedition Settings")]
    [SerializeField] private GameObject[] campfirePrefabs;
    [SerializeField] private GameObject[] tentPrefabs;
    [SerializeField] private GameObject[] cratePrefabs;
    [SerializeField] private GameObject[] wreckagePrefabs;

    [SerializeField] private int minExpeditions = 3;
    [SerializeField] private int maxExpeditions = 8;

    private System.Random prng;

    public void Initialize(int seed)
    {
        prng = new System.Random(seed + 3000);
    }

    public List<ExpeditionSite> GenerateExpeditionSites(Dictionary<Vector2Int, TerrainChunk> chunks)
    {
        List<ExpeditionSite> sites = new List<ExpeditionSite>();
        int siteCount = prng.Next(minExpeditions, maxExpeditions + 1);

        for (int i = 0; i < siteCount; i++)
        {
            ExpeditionSite site = CreateExpeditionSite(chunks, i);
            if (site != null)
            {
                sites.Add(site);
            }
        }

        return sites;
    }

    ExpeditionSite CreateExpeditionSite(Dictionary<Vector2Int, TerrainChunk> chunks, int siteIndex)
    {
        // Choisit une position plate (plains ou forest)
        Vector3 position = FindSuitableLocation(chunks);

        GameObject siteParent = new GameObject($"ExpeditionSite_{siteIndex}");
        siteParent.transform.position = position;

        List<GameObject> objects = new List<GameObject>();

        // Spawn campfire au centre
        if (campfirePrefabs.Length > 0)
        {
            GameObject campfire = Instantiate(
                campfirePrefabs[prng.Next(campfirePrefabs.Length)],
                position,
                Quaternion.identity,
                siteParent.transform
            );
            objects.Add(campfire);
        }

        // Spawn 2-4 tentes autour
        int tentCount = prng.Next(2, 5);
        for (int i = 0; i < tentCount && tentPrefabs.Length > 0; i++)
        {
            float angle = (360f / tentCount) * i;
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * 5f;

            GameObject tent = Instantiate(
                tentPrefabs[prng.Next(tentPrefabs.Length)],
                position + offset,
                Quaternion.Euler(0, angle + 180, 0),
                siteParent.transform
            );
            objects.Add(tent);
        }

        // Spawn 1-3 caisses avec loot
        int crateCount = prng.Next(1, 4);
        for (int i = 0; i < crateCount && cratePrefabs.Length > 0; i++)
        {
            Vector3 randomOffset = new Vector3(
                (float)(prng.NextDouble() * 8 - 4),
                0,
                (float)(prng.NextDouble() * 8 - 4)
            );

            GameObject crate = Instantiate(
                cratePrefabs[prng.Next(cratePrefabs.Length)],
                position + randomOffset,
                Quaternion.Euler(0, (float)prng.NextDouble() * 360, 0),
                siteParent.transform
            );

            // Ajoute component LootChest
            if (crate.GetComponent<LootChest>() == null)
            {
                crate.AddComponent<LootChest>();
            }

            objects.Add(crate);
        }

        // Quelques wreckages
        if (wreckagePrefabs.Length > 0 && prng.NextDouble() > 0.5)
        {
            Vector3 randomOffset = new Vector3(
                (float)(prng.NextDouble() * 10 - 5),
                0,
                (float)(prng.NextDouble() * 10 - 5)
            );

            GameObject wreckage = Instantiate(
                wreckagePrefabs[prng.Next(wreckagePrefabs.Length)],
                position + randomOffset,
                Quaternion.Euler(0, (float)prng.NextDouble() * 360, 0),
                siteParent.transform
            );
            objects.Add(wreckage);
        }

        return new ExpeditionSite
        {
            parentObject = siteParent,
            position = position,
            objects = objects
        };
    }

    Vector3 FindSuitableLocation(Dictionary<Vector2Int, TerrainChunk> chunks)
    {
        // Simplifié : position aléatoire
        // À améliorer : vérifier que c'est plat et dans un bon biome
        return new Vector3(
            (float)prng.NextDouble() * 500,
            25f,
            (float)prng.NextDouble() * 500
        );
    }
}

public class ExpeditionSite
{
    public GameObject parentObject;
    public Vector3 position;
    public List<GameObject> objects;
}