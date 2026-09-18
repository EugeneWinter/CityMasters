using UnityEngine;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Ресурсы")]
    public int money = 1000;
    public int population = 0;
    public int maxPopulation = 0;
    public int happiness = 50;
    public int energy = 0;
    public int energyConsumption = 0;

    [Header("Доход/Расход")]
    public float incomeInterval = 5f;
    public float populationGrowthInterval = 3f;

    private float incomeTimer;
    private float growthTimer;

    private readonly List<Building> registeredBuildings = new List<Building>();

    public System.Action OnResourcesChanged;
    public System.Action<string, Color> OnMessage;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        incomeTimer += Time.deltaTime;
        if (incomeTimer >= incomeInterval) { incomeTimer = 0; CollectIncome(); }

        growthTimer += Time.deltaTime;
        if (growthTimer >= populationGrowthInterval) { growthTimer = 0; GrowPopulation(); }
    }

    void CollectIncome()
    {
        int income = population * 2;
        foreach (var b in registeredBuildings)
            if (b != null && b.data != null && b.IsConnectedToRoad)
                income += b.data.incomePerTick;

        int expense = maxPopulation / 5;
        int delta = income - expense;
        money += delta;

        if (energyConsumption > energy)
            happiness = Mathf.Max(0, happiness - 3);
        else if (population < maxPopulation * 0.9f)
            happiness = Mathf.Min(100, happiness + 1);

        OnResourcesChanged?.Invoke();
    }

    void GrowPopulation()
    {
        if (population >= maxPopulation) return;
        if (happiness < 30) return;
        if (energyConsumption > energy) return;

        population = Mathf.Min(population + 1, maxPopulation);
        OnResourcesChanged?.Invoke();
    }

    public bool CanAfford(int cost) => money >= cost;

    public void Spend(int cost)
    {
        money -= cost;
        OnResourcesChanged?.Invoke();
    }

    public void RegisterBuilding(Building b)
    {
        if (b != null && !registeredBuildings.Contains(b)) registeredBuildings.Add(b);
    }

    public void UnregisterBuilding(Building b)
    {
        registeredBuildings.Remove(b);
    }

    public void AddBuildingEffects(BuildingData data)
    {
        if (data == null) return;
        maxPopulation += data.populationProvided;
        happiness = Mathf.Clamp(happiness + data.happinessBonus, 0, 100);
        energy += data.energyProvided;
        energyConsumption += data.energyConsumed;
        OnResourcesChanged?.Invoke();
    }

    public void RemoveBuildingEffects(BuildingData data)
    {
        if (data == null) return;
        maxPopulation -= data.populationProvided;
        happiness = Mathf.Clamp(happiness - data.happinessBonus, 0, 100);
        energy -= data.energyProvided;
        energyConsumption -= data.energyConsumed;
        if (population > maxPopulation) population = Mathf.Max(0, maxPopulation);
        OnResourcesChanged?.Invoke();
    }

    public void Notify(string msg, Color color) => OnMessage?.Invoke(msg, color);
}