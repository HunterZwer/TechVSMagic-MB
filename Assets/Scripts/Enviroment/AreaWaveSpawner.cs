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
    
    private bool hasSpawned = false;
    private SmoothCameraChange _smoothCamera;
    private ZoomCamera _zoomCamera;
    private BoxCollider triggerCollider;
    private Bounds spawnBounds;
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
        spawnBounds = new Bounds(transform.position, spawnAreaSize);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !hasSpawned)
        {
            Vector3 spawnerPosition = this.gameObject.transform.position;
            Vector3 cameraOffset = new Vector3(-187f, 0f, -183f); // Your camera offset
            Vector3 targetPosition = spawnerPosition + cameraOffset;
        
            // Keep original Y position
            targetPosition.y = _smoothCamera.transform.position.y;
            DisableAnyMovement();
            _smoothCamera.MoveCameraTo(targetPosition);
            _zoomCamera.SetZoom(5f);
            StartSpawning();
            DisableAnyMovement();
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
                Vector3 spawnPosition = GetRandomBorderPosition(spawnBounds);
                GameObject instantiatedUnit = Instantiate(unit.unitPrefab, spawnPosition, Quaternion.identity);
                instantiatedUnit.TryGetComponent(out UnitMovement unitMovement);
                unitMovement.agent.SetDestination(transform.position);
            }
        }
    }

    private Vector3 GetRandomBorderPosition(Bounds bounds)
    {
        float x, z;

        // Randomly select which edge to spawn on: 0 = min Z, 1 = max Z, 2 = min X, 3 = max X
        int edge = Random.Range(0, 4);

        if (edge == 0) // Min Z border
        {
            x = Random.Range(bounds.min.x, bounds.max.x);
            z = bounds.min.z;
        }
        else if (edge == 1) // Max Z border
        {
            x = Random.Range(bounds.min.x, bounds.max.x);
            z = bounds.max.z;
        }
        else if (edge == 2) // Min X border
        {
            x = bounds.min.x;
            z = Random.Range(bounds.min.z, bounds.max.z);
        }
        else // Max X border
        {
            x = bounds.max.x;
            z = Random.Range(bounds.min.z, bounds.max.z);
        }

        return new Vector3(x, transform.position.y, z);
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

    private void DisableAnyMovement()
    {

        _keyboardCameraMovement.enabled = false;
        _edgeScroller.enabled = false;
        Invoke(nameof(ReEnable), 1f);  // Calls ReEnable after 1 second
        void ReEnable()
        {
            _keyboardCameraMovement.enabled = true;
            _edgeScroller.enabled = true;
        }
        
    }
}