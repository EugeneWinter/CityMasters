using UnityEngine;
using System.Collections.Generic;

public class AutoTileRoad : MonoBehaviour
{
    public static AutoTileRoad Instance { get; private set; }

    public Sprite[] roadSprites = new Sprite[16];
    public GameObject roadPrefab;
    public int roadCost = 10;
    public float refundRate = 0.5f;

    private Dictionary<Vector2Int, GameObject> roads = new Dictionary<Vector2Int, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool PlaceRoad(int x, int y)
    {
        var pos = new Vector2Int(x, y);
        if (roads.ContainsKey(pos)) return false;
        if (GridManager.Instance == null || !GridManager.Instance.CanBuildRoadAt(x, y))
        {
            ResourceManager.Instance?.Notify("Нельзя строить здесь!", Color.red);
            return false;
        }
        if (ResourceManager.Instance == null || !ResourceManager.Instance.CanAfford(roadCost))
        {
            ResourceManager.Instance?.Notify("Недостаточно денег!", Color.red);
            return false;
        }

        ResourceManager.Instance.Spend(roadCost);

        Vector3 world = GridManager.Instance.GridToWorld(x, y);
        GameObject go = Instantiate(roadPrefab, world, Quaternion.identity, transform);
        go.name = $"Road_{x}_{y}";
        roads[pos] = go;

        GridManager.Instance.SetRoad(x, y);

        UpdateRoad(x, y);
        UpdateNeighbors(x, y);
        UpdateAdjacentBuildings(x, y);
        return true;
    }

    public void RemoveRoad(int x, int y)
    {
        var pos = new Vector2Int(x, y);
        if (!roads.ContainsKey(pos)) return;

        Destroy(roads[pos]);
        roads.Remove(pos);
        GridManager.Instance.ClearRoad(x, y);

        if (ResourceManager.Instance != null)
        {
            int refund = Mathf.RoundToInt(roadCost * refundRate);
            ResourceManager.Instance.money += refund;
            ResourceManager.Instance.Notify($"+{refund}$", Color.yellow);
        }

        UpdateNeighbors(x, y);
        UpdateAdjacentBuildings(x, y);
    }

    void UpdateAdjacentBuildings(int x, int y)
    {
        Vector2Int[] dirs = { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };
        foreach (var d in dirs)
        {
            var c = GridManager.Instance.GetCell(x + d.x, y + d.y);
            if (c != null && c.building != null) c.building.UpdateRoadConnection();
        }
    }

    void UpdateNeighbors(int x, int y)
    {
        UpdateRoad(x, y + 1); UpdateRoad(x + 1, y);
        UpdateRoad(x, y - 1); UpdateRoad(x - 1, y);
    }

    void UpdateRoad(int x, int y)
    {
        var pos = new Vector2Int(x, y);
        if (!roads.ContainsKey(pos)) return;

        int mask = 0;
        if (HasRoad(x, y + 1)) mask |= 1;
        if (HasRoad(x + 1, y)) mask |= 2;
        if (HasRoad(x, y - 1)) mask |= 4;
        if (HasRoad(x - 1, y)) mask |= 8;

        var sr = roads[pos].GetComponent<SpriteRenderer>();
        if (sr != null && mask < roadSprites.Length && roadSprites[mask] != null)
            sr.sprite = roadSprites[mask];
    }

    bool HasRoad(int x, int y) => roads.ContainsKey(new Vector2Int(x, y));
}