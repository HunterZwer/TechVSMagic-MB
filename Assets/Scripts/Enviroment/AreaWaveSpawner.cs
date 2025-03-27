using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using CameraRelated;

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
    [SerializeField] private float respawnDelay = 30f;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(10f, 2f, 10f); // Size of spawn area
    [SerializeField] private Color gizmoColor = new Color(1f, 0f, 0f, 0.3f); // Red with transparency

    [Header("Unit Configuration")]
    [SerializeField] private WaveUnit[] waveUnits = new WaveUnit[4];
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform[] SpawnPositions;
    
    private bool hasSpawned = false;
    private SmoothCameraChange _smoothCamera;
    private ZoomCamera _zoomCamera;
    private BoxCollider triggerCollider;
    private KeyboardCameraMovement _keyboardCameraMovement;
    private EdgeScroller _edgeScroller;
    

    private void Awake()
    {
        GameObject cameraHolder = GameObject.FindGameObjectWithTag("CameraHolder");
        _smoothCamera = cameraHolder.GetComponent<SmoothCameraChange>();
        _zoomCamera = cameraHolder.GetComponent<ZoomCamera>();
        _keyboardCameraMovement = cameraHolder.GetComponent<KeyboardCameraMovement>();
        _edgeScroller = cameraHolder.GetComponent<EdgeScroller>();
        
        // Get the trigger collider
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            Debug.LogError("AreaWaveSpawner requires a BoxCollider component!");
        }
        else
        {
            triggerCollider.isTrigger = true;
        }
        
        // Define the spawn bounds (area where enemies will spawn)
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !hasSpawned)
        {
            Vector3 spawnerPosition = this.gameObject.transform.position;
            Vector3 cameraOffset = new Vector3(-8f, 0f, -2.5f); // Your camera offset
            Vector3 targetPosition = spawnerPosition + cameraOffset;
        
            // Keep original Y position
            targetPosition.y = _smoothCamera.transform.position.y;
            _smoothCamera.MoveCameraTo(targetPosition);
            _zoomCamera.SetZoom(5f);
            StartSpawning();
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
            int amount = GetWaveAmount(unit, waveNumber);
            if (amount <= 0 || unit.unitPrefab == null) continue;
            
            for (int i = 0; i < amount; i++)
            {
                Vector3 spawnPosition = GetRandomSpawnPosition(SpawnPositions);
                GameObject instantiatedUnit = Instantiate(unit.unitPrefab, spawnPosition, Quaternion.identity);
                instantiatedUnit.TryGetComponent(out UnitMovement unitMovement);
                unitMovement.agent.SetDestination(transform.position);
            }
        }
    }

    private Vector3 GetRandomSpawnPosition(Transform[] spawnPositions)
    {
        return spawnPositions[Random.Range(0, spawnPositions.Length)].position;
    }

    private int GetWaveAmount(WaveUnit unit, int waveNumber)
    {
        return waveNumber == 1 ? unit.wave1Amount : unit.wave2Amount;
    }
    
    // Draw the spawn area in the editor for visualization
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position, spawnAreaSize);
    }
}