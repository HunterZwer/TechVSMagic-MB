using UnityEngine;
using System.Collections;

[System.Serializable]
public class WaveUnit
{
    public GameObject unitPrefab;
    public int wave1Amount;
    public int wave2Amount;
}

public class AreaWaveSpawner : MonoBehaviour
{
    [Header("Spawn Area Settings")]
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(10, 0, 10);
    [SerializeField] private float respawnDelay = 30f;

    [Header("Unit Configuration")]
    [SerializeField] private WaveUnit[] waveUnits = new WaveUnit[4];
    
    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float checkInterval = 1f;

    private bool isActive = false;
    private bool hasSpawned = false;

    private void Start()
    {
        StartCoroutine(CheckForPlayers());
    }

    private IEnumerator CheckForPlayers()
    {
        while (!hasSpawned)
        {
            Collider[] players = Physics.OverlapBox(transform.position, spawnAreaSize/2, Quaternion.identity);
            foreach (Collider col in players)
            {
                if (col.CompareTag(playerTag))
                {
                    StartSpawning();
                    yield break;
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    private void StartSpawning()
    {
        hasSpawned = true;
        StartCoroutine(SpawnWaves());
    }

    private IEnumerator SpawnWaves()
    {
        // Spawn first wave
        SpawnWave(1);
        
        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);
        
        // Spawn second wave
        SpawnWave(2);
    }

    private void SpawnWave(int waveNumber)
    {
        foreach (WaveUnit unit in waveUnits)
        {
            for (int i = 0; i < GetWaveAmount(unit, waveNumber); i++)
            {
                Vector3 spawnPosition = transform.position + new Vector3(
                    Random.Range(-spawnAreaSize.x/2, spawnAreaSize.x/2),
                    0,
                    Random.Range(-spawnAreaSize.z/2, spawnAreaSize.z/2)
                );

                Instantiate(unit.unitPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }

    private int GetWaveAmount(WaveUnit unit, int waveNumber)
    {
        return waveNumber == 1 ? unit.wave1Amount : unit.wave2Amount;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}