using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProceduralWorldGenerator worldGenerator;
    [SerializeField] private LakeGenerator lakeGenerator;
    [SerializeField] private CaveGenerator caveGenerator;
    [SerializeField] private AbandonedExpeditionSpawner expeditionSpawner;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector3 playerSpawnHeight = new Vector3(0, 50, 0);

    [Header("Generation Progress")]
    [SerializeField] private bool showProgressUI = true;

    private GameObject player;
    private List<Lake> lakes = new List<Lake>();
    private List<CaveEntrance> caves = new List<CaveEntrance>();
    private List<ExpeditionSite> expeditionSites = new List<ExpeditionSite>();

    // État de génération
    public enum GenerationState
    {
        NotStarted,
        GeneratingTerrain,
        GeneratingLakes,
        GeneratingCaves,
        GeneratingExpeditions,
        SpawningPlayer,
        Complete
    }

    private GenerationState currentState = GenerationState.NotStarted;

    void Start()
    {
        StartCoroutine(GenerateWorldSequence());
    }

    IEnumerator GenerateWorldSequence()
    {
        Debug.Log("=== World Generation Started ===");

        // 1. Génère le terrain
        currentState = GenerationState.GeneratingTerrain;
        Debug.Log("Phase 1: Generating terrain...");

        if (worldGenerator != null)
        {
            // Attends que le terrain soit généré
            while (worldGenerator.GetChunkAt(Vector2Int.zero) == null)
            {
                yield return null;
            }

            // Attends que tous les chunks soient générés
            yield return new WaitForSeconds(2f);
        }

        Debug.Log("Terrain generation complete!");

        // 2. Génère les lacs
        currentState = GenerationState.GeneratingLakes;
        Debug.Log("Phase 2: Generating lakes...");

        if (lakeGenerator != null && worldGenerator != null)
        {
            lakeGenerator.Initialize(worldGenerator, worldGenerator.GetSeed());
            lakes = lakeGenerator.GenerateLakes(worldGenerator.GetAllChunks());
        }

        yield return new WaitForSeconds(0.5f);
        Debug.Log($"Generated {lakes.Count} lakes!");

        // 3. Génère les grottes
        currentState = GenerationState.GeneratingCaves;
        Debug.Log("Phase 3: Generating caves...");

        if (caveGenerator != null)
        {
            caveGenerator.Initialize(worldGenerator.GetSeed());
            caves = caveGenerator.GenerateCaves(worldGenerator.GetAllChunks());
        }

        yield return new WaitForSeconds(0.5f);
        Debug.Log($"Generated {caves.Count} cave entrances!");

        // 4. Génère les sites d'expéditions abandonnées
        currentState = GenerationState.GeneratingExpeditions;
        Debug.Log("Phase 4: Generating abandoned expeditions...");

        if (expeditionSpawner != null)
        {
            expeditionSpawner.Initialize(worldGenerator.GetSeed());
            expeditionSites = expeditionSpawner.GenerateExpeditionSites(worldGenerator.GetAllChunks());
        }

        yield return new WaitForSeconds(0.5f);
        Debug.Log($"Generated {expeditionSites.Count} expedition sites!");

        // 5. Spawn le joueur
        currentState = GenerationState.SpawningPlayer;
        Debug.Log("Phase 5: Spawning player...");

        SpawnPlayer();

        yield return new WaitForSeconds(0.5f);

        // 6. Terminé !
        currentState = GenerationState.Complete;
        Debug.Log("=== World Generation Complete ===");
        Debug.Log($"Seed: {worldGenerator.GetSeed()}");
        Debug.Log($"Lakes: {lakes.Count}, Caves: {caves.Count}, Expeditions: {expeditionSites.Count}");
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("No player prefab assigned!");
            return;
        }

        // Trouve un bon spawn point (plat, pas dans l'eau)
        Vector3 spawnPos = FindSafeSpawnPoint();

        player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        player.name = "Player";

        Debug.Log($"Player spawned at {spawnPos}");
    }

    Vector3 FindSafeSpawnPoint()
    {
        // Commence au centre du monde
        Vector3 center = new Vector3(256, 100, 256);

        // Raycast vers le bas pour trouver le sol
        RaycastHit hit;
        if (Physics.Raycast(center, Vector3.down, out hit, 200f))
        {
            // Vérifie que ce n'est pas de l'eau (à améliorer)
            if (hit.point.y > 20f) // Au-dessus du niveau d'eau
            {
                return hit.point + Vector3.up * 2f; // 2m au-dessus du sol
            }
        }

        // Fallback
        return new Vector3(256, 30, 256);
    }

    // ==================== UTILITY METHODS ====================

    public Lake GetNearestLake(Vector3 position)
    {
        Lake nearest = null;
        float minDist = float.MaxValue;

        foreach (var lake in lakes)
        {
            float dist = Vector3.Distance(position, lake.center);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = lake;
            }
        }

        return nearest;
    }

    public ExpeditionSite GetNearestExpedition(Vector3 position)
    {
        ExpeditionSite nearest = null;
        float minDist = float.MaxValue;

        foreach (var site in expeditionSites)
        {
            float dist = Vector3.Distance(position, site.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = site;
            }
        }

        return nearest;
    }

    public CaveEntrance GetNearestCave(Vector3 position)
    {
        CaveEntrance nearest = null;
        float minDist = float.MaxValue;

        foreach (var cave in caves)
        {
            float dist = Vector3.Distance(position, cave.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = cave;
            }
        }

        return nearest;
    }

    // Getters
    public GenerationState GetCurrentState() => currentState;
    public List<Lake> GetAllLakes() => lakes;
    public List<CaveEntrance> GetAllCaves() => caves;
    public List<ExpeditionSite> GetAllExpeditions() => expeditionSites;
    public GameObject GetPlayer() => player;

    // ==================== DEBUG ====================

    void OnGUI()
    {
        if (!showProgressUI) return;

        GUI.Box(new Rect(10, 10, 300, 100), "World Generation Progress");

        GUI.Label(new Rect(20, 35, 280, 20), $"State: {currentState}");
        GUI.Label(new Rect(20, 55, 280, 20), $"Seed: {worldGenerator?.GetSeed()}");
        GUI.Label(new Rect(20, 75, 280, 20), $"Lakes: {lakes.Count} | Caves: {caves.Count}");
    }
}