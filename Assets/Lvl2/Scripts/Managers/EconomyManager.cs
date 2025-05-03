using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public int Gold { get; private set; }
    public int Silver { get; private set; }

    [SerializeField] public int goldPerSecond = 10;
    [SerializeField] public int silverPerSecond = 10;
    [SerializeField] private int startGold = 100;
    [SerializeField] private int startSilver = 100;
    [SerializeField] private Text goldAmountUI;
    [SerializeField] private Text silverAmountUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        Gold = startGold;
        Silver = startSilver;
        StartCoroutine(PassiveIncomeRoutine());
        UpdateUI();
    }

    private IEnumerator PassiveIncomeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            AddResources(goldPerSecond, silverPerSecond);
            FloatingTextManager.ShowText1("+"+goldPerSecond.ToString());
            FloatingTextManager.ShowText2("+"+silverPerSecond.ToString());
            UpdateUI();
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
        goldAmountUI.text = $"{Gold}";
        silverAmountUI.text = $"{Silver}";
    }
}