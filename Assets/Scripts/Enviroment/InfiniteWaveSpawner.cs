using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class InfiniteWaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private float waveInterval = 30f;
    [SerializeField] private int maxWave = 10;

    [Header("Unit Configuration")]
    [SerializeField] private WaveUnit[] waveUnits = new WaveUnit[4];
    [SerializeField] private Transform[] spawnPositions;

    [SerializeField] private Transform _endPoint;

    private int currentWave = 0;
    private bool isActive = true;
    private BoxCollider triggerCollider;
    private int totalUnitsSpawned = 0;

    private void Start()
    {
        StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        while (isActive && currentWave < maxWave)
        {
            currentWave++;
            SpawnWave(currentWave);
            yield return new WaitForSeconds(waveInterval);
        }

        isActive = false; // ✅ Stop further waves
        Debug.Log("Max wave reached. Spawner stopped.");
    }

    private void SpawnWave(int waveNumber)
    {
        int waveUnitCount = 0;
        foreach (WaveUnit unit in waveUnits)
        {
            if (unit.unitPrefab == null) continue;

            int baseAmount = unit.startAmount + (waveNumber - 1) * unit.incrementPerWave;
            int randomAmount = Random.Range(0, unit.maxRandomVariance + 1);
            int totalToSpawn = baseAmount + randomAmount;
            
            waveUnitCount += totalToSpawn;

            for (int i = 0; i < totalToSpawn; i++)
            {
                Vector3 spawnPos = GetRandomSpawnPosition();
                GameObject enemy = Instantiate(unit.unitPrefab, spawnPos, Quaternion.identity);

                if (enemy.TryGetComponent(out UnitMovement movement))
                {
                    movement.agent.SetDestination(_endPoint.position);
                }
            }
        }
        
        
        totalUnitsSpawned += waveUnitCount;
        Debug.Log($"Wave {waveNumber} spawned {waveUnitCount} units. Total so far: {totalUnitsSpawned}");
    }

    private Vector3 GetRandomSpawnPosition()
    {
        if (spawnPositions.Length == 0) return transform.position;
        return spawnPositions[Random.Range(0, spawnPositions.Length)].position;
    }
}
