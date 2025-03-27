using System.Collections;
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
            healthJobHandle.Complete(); // Ensure no running job before disposal
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

        healthJobHandle = healthJob.Schedule(healthJobHandle); // Chain jobs correctly

        StartCoroutine(WaitForJobCompletion());
    }

    // Coroutine to wait for the job asynchronously
    private IEnumerator WaitForJobCompletion()
    {
        yield return new WaitUntil(() => healthJobHandle.IsCompleted);
        healthJobHandle.Complete();

        targetHealthPercentage = healthPercentageArray[0];

        StartCoroutine(SmoothHealthChange(targetHealthPercentage, 0.5f));

        UpdateColor(targetHealthPercentage);
    }

    // Coroutine for smooth health change using MoveTowards
    private IEnumerator SmoothHealthChange(float targetValue, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            HealthBarSlider.value = Mathf.MoveTowards(HealthBarSlider.value, targetValue, Time.deltaTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
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
