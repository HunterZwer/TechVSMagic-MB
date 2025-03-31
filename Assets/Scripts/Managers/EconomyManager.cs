using System.Collections;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public int Gold { get; private set; }
    public int Silver { get; private set; }

    [SerializeField] private int goldPerSecond = 10;
    [SerializeField] private int silverPerSecond = 10;
    [SerializeField] private int startGold = 100;
    [SerializeField] private int startSilver = 100;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        Gold = startGold;
        Silver = startSilver;
        StartCoroutine(PassiveIncomeRoutine());
    }

    private IEnumerator PassiveIncomeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            AddResources(goldPerSecond, silverPerSecond);
        }
    }

    public bool CanAfford(int gold, int silver)
    {
        return Gold >= gold && Silver >= silver;
    }

    public void SpendResources(int gold, int silver)
    {
        Gold -= gold;
        Silver -= silver;
        UpdateUI();
    }

    public void AddResources(int gold, int silver)
    {
        Gold += gold;
        Silver += silver;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Implement your UI update logic here
        Debug.Log($"Resources: {Gold} Gold, {Silver} Silver");
    }
}