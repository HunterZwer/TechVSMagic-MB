using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;
using Unity.Mathematics;
    

public class BattleManager : MonoBehaviour
{
    [Header("Time Scale Settings")]
    [Range(0.1f, 10f)]
    [SerializeField] private float timeScale = 1f;
    [SerializeField] private Slider timeScaleSlider;
    [SerializeField] private TextMeshProUGUI timeScaleText;
    
    [Header("Formation Settings")]
    [SerializeField] private float horizontalSpacing = 2f;    // Space between units in a row
    [SerializeField] private float verticalSpacing = 5f;      // Space between unit types
    // [SerializeField] private float unitLineOffset = 3f;       // Front-back offset between lines
    
    [Header("Team A Units")]
    [SerializeField] private GameObject teamA_Unit1Prefab;
    [SerializeField] private GameObject teamA_Unit2Prefab;
    [SerializeField] private GameObject teamA_Unit3Prefab;
    [SerializeField] private GameObject teamA_Unit4Prefab;
    
    [Header("Team A Unit Counts")]
    [SerializeField] private int teamA_Unit1Count = 5;
    [SerializeField] private int teamA_Unit2Count = 3;
    [SerializeField] private int teamA_Unit3Count = 2;
    [SerializeField] private int teamA_Unit4Count = 1;

    [Header("Team B Units")]
    [SerializeField] private GameObject teamB_Unit1Prefab;
    [SerializeField] private GameObject teamB_Unit2Prefab;
    [SerializeField] private GameObject teamB_Unit3Prefab;
    [SerializeField] private GameObject teamB_Unit4Prefab;
    
    [Header("Team B Unit Counts")]
    [SerializeField] private int teamB_Unit1Count = 5;
    [SerializeField] private int teamB_Unit2Count = 3;
    [SerializeField] private int teamB_Unit3Count = 2;
    [SerializeField] private int teamB_Unit4Count = 1;

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 teamASpawnCenter = new Vector3(-20, 0, 0);
    [SerializeField] private Vector3 teamBSpawnCenter = new Vector3(20, 0, 0);

    [Header("Distance Tracking")]
    [SerializeField] private TextMeshProUGUI allyDistanceText;
    [SerializeField] private TextMeshProUGUI teamDistanceText;

    [Header("Battle Results")]
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private GameObject resultPanel;

    // Lists to track all units
    private List<GameObject> teamAUnits = new List<GameObject>();
    private List<GameObject> teamBUnits = new List<GameObject>();

    // Battle state
    private bool battleEnded = false;
    private float checkInterval = 1f;
    private float nextCheckTime = 0f;
    
    private void Start()
    {
        // Initialize UI
        if (timeScaleSlider != null)
        {
            timeScaleSlider.value = timeScale;
            timeScaleSlider.onValueChanged.AddListener(SetTimeScale);
        }
        UpdateTimeScaleText();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        // Spawn teams
        SpawnTeams();
    }

    private void Update()
    {
        // Update time scale
        Time.timeScale = timeScale;

        // Check battle state periodically
        if (Time.time > nextCheckTime && !battleEnded)
        {
            nextCheckTime = Time.time + checkInterval;
            
            // Update distances
            UpdateDistanceTexts();
            
            // Check if battle is over
            CheckBattleState();
        }
    }

   private void SpawnTeams()
    {
        // Spawn Team A units in organized lines
        SpawnTeamUnits(
            new GameObject[] { teamA_Unit1Prefab, teamA_Unit2Prefab, teamA_Unit3Prefab, teamA_Unit4Prefab },
            new int[] { teamA_Unit1Count, teamA_Unit2Count, teamA_Unit3Count, teamA_Unit4Count },
            teamASpawnCenter,
            teamAUnits
        );

        // Spawn Team B units in organized lines
        SpawnTeamUnits(
            new GameObject[] { teamB_Unit1Prefab, teamB_Unit2Prefab, teamB_Unit3Prefab, teamB_Unit4Prefab },
            new int[] { teamB_Unit1Count, teamB_Unit2Count, teamB_Unit3Count, teamB_Unit4Count },
            teamBSpawnCenter,
            teamBUnits
        );
    }

    private void SpawnTeamUnits(GameObject[] prefabs, int[] counts, Vector3 basePosition, List<GameObject> teamList)
    {
        Vector3 currentPosition = basePosition;
        
        for (int unitType = 0; unitType < prefabs.Length; unitType++)
        {
            if (prefabs[unitType] == null) continue;

            // Spawn units in a line for this type
            SpawnUnitLine(
                prefabs[unitType],
                counts[unitType],
                currentPosition,
                teamList
            );

            // Move position for next unit type
            currentPosition += new Vector3(0, 0, verticalSpacing);
        }
    }

    private void SpawnUnitLine(GameObject prefab, int count, Vector3 lineCenter, List<GameObject> teamList)
    {
        // Calculate starting position for the line
        Vector3 startPosition = lineCenter - new Vector3((count - 1) * horizontalSpacing / 2, 0, 0);

        for (int i = 0; i < count; i++)
        {
            // Calculate position in the line
            Vector3 spawnPosition = startPosition + new Vector3(i * horizontalSpacing, 0, 0);

            // Instantiate and add to team
            GameObject unit = Instantiate(prefab, spawnPosition, Quaternion.identity);
            teamList.Add(unit);
        }
    }


    private void UpdateDistanceTexts()
    {
        // Calculate average ally distance (within teams)
        float teamAAllyDistance = CalculateAverageDistance(teamAUnits);
        float teamBAllyDistance = CalculateAverageDistance(teamBUnits);
        float averageAllyDistance = (teamAAllyDistance + teamBAllyDistance) / 2f;

        // Calculate average distance between teams
        float teamDistance = CalculateAverageTeamDistance(teamAUnits, teamBUnits);
    }

public struct UnitPosition
    {
        public float x;
        public float y;
        public float z;
    }

    // Modified methods that could benefit from Burst
    private float CalculateAverageDistance(List<GameObject> units)
    {
        if (units.Count <= 1) return 0;

        // Convert to Burst-compatible data structure
        var positions = new NativeArray<UnitPosition>(units.Count, Allocator.Temp);
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null)
            {
                var pos = units[i].transform.position;
                positions[i] = new UnitPosition { x = pos.x, y = pos.y, z = pos.z };
            }
        }

        // Run the calculation with Burst
        float result = BurstAverageDistance(positions);

        // Clean up
        positions.Dispose();

        return result;
    }


    private static float BurstAverageDistance(NativeSlice<UnitPosition> positions)
    {
        float totalDistance = 0;
        int validPairs = 0;

        for (int i = 0; i < positions.Length; i++)
        {
            var pos1 = positions[i];
            for (int j = i + 1; j < positions.Length; j++)
            {
                var pos2 = positions[j];
                float dx = pos2.x - pos1.x;
                float dy = pos2.y - pos1.y;
                float dz = pos2.z - pos1.z;
                totalDistance += math.sqrt(dx*dx + dy*dy + dz*dz);
                validPairs++;
            }
        }

        return validPairs > 0 ? totalDistance / validPairs : 0;
    }

    // Similarly modify the team distance calculation
    private float CalculateAverageTeamDistance(List<GameObject> teamA, List<GameObject> teamB)
    {
        if (teamA.Count == 0 || teamB.Count == 0) return 0;

        // Convert positions
        var positionsA = new NativeArray<UnitPosition>(teamA.Count, Allocator.Temp);
        var positionsB = new NativeArray<UnitPosition>(teamB.Count, Allocator.Temp);

        for (int i = 0; i < teamA.Count; i++)
        {
            if (teamA[i] != null)
            {
                var pos = teamA[i].transform.position;
                positionsA[i] = new UnitPosition { x = pos.x, y = pos.y, z = pos.z };
            }
        }

        for (int i = 0; i < teamB.Count; i++)
        {
            if (teamB[i] != null)
            {
                var pos = teamB[i].transform.position;
                positionsB[i] = new UnitPosition { x = pos.x, y = pos.y, z = pos.z };
            }
        }

        // Run calculation
        float result = BurstAverageTeamDistance(positionsA, positionsB);

        // Clean up
        positionsA.Dispose();
        positionsB.Dispose();

        return result;
    }
    
    private static float BurstAverageTeamDistance(NativeSlice<UnitPosition> positionsA, NativeSlice<UnitPosition> positionsB)
    {
        float totalDistance = 0;
        int validPairs = 0;

        for (int i = 0; i < positionsA.Length; i++)
        {
            var posA = positionsA[i];
            for (int j = 0; j < positionsB.Length; j++)
            {
                var posB = positionsB[j];
                float dx = posB.x - posA.x;
                float dy = posB.y - posA.y;
                float dz = posB.z - posA.z;
                totalDistance += math.sqrt(dx*dx + dy*dy + dz*dz);
                validPairs++;
            }
        }

        return validPairs > 0 ? totalDistance / validPairs : 0;
    }

    private void CheckBattleState()
    {
        // Clean up destroyed units
        teamAUnits.RemoveAll(unit => unit == null);
        teamBUnits.RemoveAll(unit => unit == null);

        // Check if either team is defeated
        if (teamAUnits.Count == 0 && teamBUnits.Count > 0)
        {
            DeclareWinner("Enemy Team Wins!");
        }
        else if (teamBUnits.Count == 0 && teamAUnits.Count > 0)
        {
            DeclareWinner("Player Team Wins!");
        }
        else if (teamAUnits.Count == 0 && teamBUnits.Count == 0)
        {
            DeclareWinner("It's a Draw!");
        }
    }

    private void DeclareWinner(string result)
    {
        battleEnded = true;
        
        if (resultText != null)
            resultText.text = result;
        
        if (resultPanel != null)
            resultPanel.SetActive(true);
            
        // Optional: Slow down time to emphasize the end
        // timeScale = 0.5f;
        UpdateTimeScaleText();
    }

    public void SetTimeScale(float value)
    {
        timeScale = value;
        UpdateTimeScaleText();
    }

    private void UpdateTimeScaleText()
    {
        if (timeScaleText != null)
            timeScaleText.text = $"Time Scale: {timeScale:F1}x";
    }
   
}