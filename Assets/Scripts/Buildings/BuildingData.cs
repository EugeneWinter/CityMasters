using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "City/Building Data")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public Sprite icon;
    public GameObject prefab;
    public int cost = 100;
    public int populationProvided = 0;
    public int happinessBonus = 0;
    public int energyProvided = 0;
    public int energyConsumed = 0;
    public int incomePerTick = 0;
    public BuildingType type;
    public Vector2Int size = new Vector2Int(1, 1);
}

public enum BuildingType { House, Factory, PowerPlant, Park, Port, Shop }