using UnityEngine;
using System.Collections.Generic;

public class CreatureSpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] creaturePrefabs;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private int minCreatures = 1;
    [SerializeField] private int maxCreatures = 3;
    [SerializeField] private float respawnTime = 300f; // 5 minutes

    [Header("Runtime")]
    private List<GameObject> spawnedCreatures = new List<GameObject>();
    private float nextSpawnTime;

    void Start()
    {
        SpawnCreatures();
    }

    void Update()
    {
        // Vérifie si besoin de respawn
        if (Time.time >= nextSpawnTime)
        {
            CleanupDeadCreatures();

            if (spawnedCreatures.Count == 0)
            {
                SpawnCreatures();
            }
        }
    }

    void SpawnCreatures()
    {
        if (creaturePrefabs.Length == 0) return;

        int count = Random.Range(minCreatures, maxCreatures + 1);

        for (int i = 0; i < count; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            // Ajuste Y selon terrain
            RaycastHit hit;
            if (Physics.Raycast(spawnPos + Vector3.up * 50, Vector3.down, out hit, 100f))
            {
                spawnPos.y = hit.point.y;
            }

            GameObject creature = Instantiate(
                creaturePrefabs[Random.Range(0, creaturePrefabs.Length)],
                spawnPos,
                Quaternion.identity
            );

            spawnedCreatures.Add(creature);
        }

        nextSpawnTime = Time.time + respawnTime;
    }

    void CleanupDeadCreatures()
    {
        spawnedCreatures.RemoveAll(c => c == null);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}