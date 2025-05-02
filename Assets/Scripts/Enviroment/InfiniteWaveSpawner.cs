using UnityEngine;
using System.Collections;
using TMPro; // Add this for TextMeshPro
using Random = UnityEngine.Random;

public class InfiniteWaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private float minWaveInterval = 5f;
    [SerializeField] private float maxWaveInterval = 30f;
    [SerializeField] private int maxWave = 100;

    [Header("UnitLVL2 Configuration")]
    [SerializeField] private WaveUnit[] waveUnits = new WaveUnit[4];
    [SerializeField] private Transform[] spawnPositions;
    [SerializeField] private Transform _endPoint;

    [Header("UI")]
    [SerializeField] private TMP_Text nextWaveText;

    private int currentWave = 0;
    private bool isActive = true;
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

            float t = (float)(currentWave - 1) / (maxWave - 1);
            float waveInterval = Mathf.Lerp(minWaveInterval, maxWaveInterval, t);

            yield return StartCoroutine(CountdownToNextWave(waveInterval));
        }

        isActive = false;
        nextWaveText.text = "All waves complete!";
        Debug.Log("Max wave reached. Spawner stopped.");
    }

    private IEnumerator CountdownToNextWave(float duration)
    {
        float timeLeft = duration;
        while (timeLeft > 0f)
        {
            nextWaveText.text = $"Next Wave in {timeLeft:F1}s";
            yield return null;
            timeLeft -= Time.deltaTime;
        }
    }

    private void SpawnWave(int waveNumber)
    {
        int totalUnitsToSpawn = waveNumber;
        int unitsSpawnedThisWave = 0;
        int unitTypeCount = Mathf.Min(waveNumber / 2 + 1, waveUnits.Length); // Gradually introduce unitLvl2 types

        while (unitsSpawnedThisWave < totalUnitsToSpawn)
        {
            for (int i = 0; i < unitTypeCount && unitsSpawnedThisWave < totalUnitsToSpawn; i++)
            {
                if (waveUnits[i].unitPrefab == null) continue;

                Vector3 spawnPos = GetRandomSpawnPosition();
                GameObject enemy = Instantiate(waveUnits[i].unitPrefab, spawnPos, Quaternion.identity);

                if (enemy.TryGetComponent(out UnitMovement movement))
                    movement.agent.SetDestination(_endPoint.position);

                unitsSpawnedThisWave++;
            }
        }

        totalUnitsSpawned += unitsSpawnedThisWave;
        Debug.Log($"Wave {waveNumber} spawned {unitsSpawnedThisWave} units. Total so far: {totalUnitsSpawned}");
    }

    private Vector3 GetRandomSpawnPosition()
    {
        if (spawnPositions.Length == 0) return transform.position;
        return spawnPositions[Random.Range(0, spawnPositions.Length)].position;
    }
}
