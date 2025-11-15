using UnityEngine;
using System.Collections.Generic;

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