using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HealthTracker : MonoBehaviour
{
    [SerializeField] private Slider HealthBarSlider;
    [SerializeField] private Image sliderFill;
    [SerializeField] private Material greenEmission;
    [SerializeField] private Material yellowEmission;
    [SerializeField] private Material redEmission;

    private NativeArray<float> healthPercentageArray;
    private JobHandle healthJobHandle;
    private float targetHealthPercentage = 1f;

    private void Awake()
    {
        healthPercentageArray = new NativeArray<float>(1, Allocator.Persistent);
    }

    private void OnDestroy()
    {
        if (healthPercentageArray.IsCreated)
        {
            healthJobHandle.Complete();
            healthPercentageArray.Dispose();
        }
    }

    // Public method to update health
    public void UpdateSliderValue(float currentHealth, float maxHealth)
    {
        // Ensure the previous job is finished before scheduling a new one
        healthJobHandle.Complete();

        // Schedule a new job safely
        HealthCalculationJob healthJob = new HealthCalculationJob
        {
            CurrentHealth = currentHealth,
            MaxHealth = maxHealth,
            Result = healthPercentageArray
        };

        healthJobHandle = healthJob.Schedule();

        // Wait for the job to complete without using Update
        WaitForJobCompletion();
    }

    // Uses async/await instead of Update()
    private async void WaitForJobCompletion()
    {
        while (!healthJobHandle.IsCompleted) // Non-blocking wait
        {
            await Task.Yield();
        }

        healthJobHandle.Complete();
        targetHealthPercentage = healthPercentageArray[0];

        // Smooth transition of health bar
        await SmoothHealthChange(targetHealthPercentage, 0.5f);
        UpdateColor(targetHealthPercentage);
    }

    // Smooth health bar change without coroutines
    private async Task SmoothHealthChange(float targetValue, float duration)
    {
        float elapsedTime = 0f;
        float initialValue = HealthBarSlider.value;

        while (elapsedTime < duration)
        {
            HealthBarSlider.value = Mathf.Lerp(initialValue, targetValue, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            await Task.Yield(); // Non-blocking delay
        }

        HealthBarSlider.value = targetValue;
    }

    // Set the color based on the health percentage
    private void UpdateColor(float healthPercentage)
    {
        if (healthPercentage >= 0.6f)
        {
            sliderFill.material = greenEmission;
        }
        else if (healthPercentage >= 0.3f)
        {
            sliderFill.material = yellowEmission;
        }
        else
        {
            sliderFill.material = redEmission;
        }
    }

    // Burst Job for calculating health percentage
    [BurstCompile]
    private struct HealthCalculationJob : IJob
    {
        public float CurrentHealth;
        public float MaxHealth;
        public NativeArray<float> Result;

        public void Execute()
        {
            Result[0] = math.clamp(CurrentHealth / MaxHealth, 0f, 1f);
        }
    }
}
