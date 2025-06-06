﻿using UnityEngine;
using UnityEngine.UI;

public class UnitSpawner : MonoBehaviour
{
    [Header("UnitLVL2 Prefabs")]
    public GameObject[] unitPrefabs; // Assign 8 unitLvl2 prefabs in the Inspector

    [Header("UI Elements")]
    public Text selectedUnitText; // Assign UI Text to display selected unitLvl2 name

    private int selectedUnitIndex = 0;
    private bool isPaused = false;

    void Update()
    {
        HandleUnitSelection();
        HandlePauseToggle();
        HandleUnitSpawning();
    }

    void HandleUnitSelection()
    {
        // Check number keys 1-8 and update the selected unitLvl2
        for (int i = 0; i < unitPrefabs.Length; i++)
        {
            if (Input.GetKeyDown((KeyCode)(KeyCode.Alpha1 + i)))
            {
                selectedUnitIndex = i;
                UpdateUI();
            }
        }
    }

    void HandlePauseToggle()
    {
        // Toggle pause when Space is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0 : 1; // Pause/unpause the game
            Debug.Log("Pause: " + isPaused);
        }
    }

    void HandleUnitSpawning()
    {
        if (!isPaused) return; // Allow spawning only when paused

        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (unitPrefabs[selectedUnitIndex] != null)
                {
                    Vector3 spawnPosition = new Vector3(
                        hit.point.x,
                        hit.point.y + 0.25f,
                        hit.point.z
                    );
                    Instantiate(unitPrefabs[selectedUnitIndex], spawnPosition, Quaternion.identity);
                }
            }
        }
    }

    void UpdateUI()
    {
        if (selectedUnitText != null)
        {
            selectedUnitText.text = "Selected UnitLVL2: " + unitPrefabs[selectedUnitIndex].name;
        }
    }
}