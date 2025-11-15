using UnityEngine;
using System.Collections.Generic;

// ==================== PLACEMENT SETTINGS ====================
[CreateAssetMenu(fileName = "PlacementSettings", menuName = "Procedural/Placement Settings")]
public class PlacementSettings : ScriptableObject
{
    [Header("General")]
    public bool enabled = true;
    public string placementName = "Objects";

    [Header("Density")]
    [Range(0f, 0.1f)]
    public float density = 0.01f; // Objets par unité carrée

    [Header("Height Constraints")]
    public float minHeight = 0f;
    public float maxHeight = 100f;

    [Header("Biome Restrictions")]
    public BiomeType[] allowedBiomes;

    [Header("Prefabs per Biome")]
    public BiomePrefabList[] biomePrefabs;

    [Header("Randomization")]
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public float minYOffset = 0f;
    public float maxYOffset = 0f;

    [Header("Spacing (Optional)")]
    public bool useMinDistance = false;
    public float minDistanceBetweenObjects = 5f;

    public GameObject GetPrefabForBiome(BiomeType biome, System.Random prng)
    {
        foreach (var biomePrefabList in biomePrefabs)
        {
            if (biomePrefabList.biome == biome && biomePrefabList.prefabs.Length > 0)
            {
                int index = prng.Next(0, biomePrefabList.prefabs.Length);
                return biomePrefabList.prefabs[index];
            }
        }

        // Fallback : n'importe quel prefab
        if (biomePrefabs.Length > 0 && biomePrefabs[0].prefabs.Length > 0)
        {
            int index = prng.Next(0, biomePrefabs[0].prefabs.Length);
            return biomePrefabs[0].prefabs[index];
        }

        return null;
    }
}

[System.Serializable]
public class BiomePrefabList
{
    public BiomeType biome;
    public GameObject[] prefabs;
    [Range(0f, 1f)]
    public float spawnChance = 1f; // Probabilité de spawn dans ce biome
}

// ==================== BIOME DATA ====================
[CreateAssetMenu(fileName = "BiomeData", menuName = "Procedural/Biome Data")]
public class BiomeData : ScriptableObject
{
    [Header("Basic Info")]
    public BiomeType biomeType;
    public string biomeName;
    public Color biomeColor = Color.white;

    [Header("Terrain Properties")]
    public float heightMultiplier = 1f;
    public float roughness = 1f; // Affecte le Perlin noise

    [Header("Textures")]
    public Texture2D groundTexture;
    public Texture2D normalMap;
    public Color tintColor = Color.white;

    [Header("Vegetation")]
    public float vegetationDensity = 0.5f;
    public GameObject[] treePrefabs;
    public GameObject[] grassPrefabs;
    public GameObject[] rockPrefabs;

    [Header("Resources")]
    public ResourceSpawnData[] resourceSpawns;

    [Header("Creatures")]
    public CreatureSpawnData[] creatureSpawns;
}

[System.Serializable]
public class ResourceSpawnData
{
    public GameObject resourcePrefab;
    [Range(0f, 1f)]
    public float spawnChance = 0.1f;
    public int minGroupSize = 1;
    public int maxGroupSize = 3;
}

[System.Serializable]
public class CreatureSpawnData
{
    public GameObject creaturePrefab;
    [Range(0f, 1f)]
    public float spawnChance = 0.05f;
    public int maxCreaturesPerChunk = 3;
}