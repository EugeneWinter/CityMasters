using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Настройки сетки")]
    public int width = 60;
    public int height = 40;
    public float cellSize = 1f;

    [Header("Спрайты")]
    public GameObject tilePrefab;
    public Sprite deepWaterSprite;
    public Sprite solidGrassSprite;
    public Sprite[] grassSprites = new Sprite[16];

    public enum TileType { Water, Grass, Road, Building }

    [System.Serializable]
    public class TileCell
    {
        public TileType type;
        public GameObject tileObject;
        public SpriteRenderer spriteRenderer;
        public Building building;
        public bool isOccupied;
    }

    private TileCell[,] grid;

    public System.Action OnRoadsChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        grid = new TileCell[width, height];
    }

    void Start()
    {
        if (Application.isPlaying)
            GenerateMap();
    }

    public void GenerateMap()
    {
        grid = new TileCell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new TileCell { type = TileType.Water };
                SpawnTile(x, y, grid[x, y]);
            }
        }

        int islandSize = 18;
        int minX = (width - islandSize) / 2;
        int maxX = minX + islandSize;
        int minY = (height - islandSize) / 2;
        int maxY = minY + islandSize;

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                grid[x, y].type = TileType.Grass;
            }
        }

        UpdateAllTerrainGraphics();

        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(width * cellSize / 2f - 0.5f, height * cellSize / 2f - 0.5f, -10f);
            Camera.main.backgroundColor = new Color(0.18f, 0.52f, 0.85f);
        }
    }

    void SpawnTile(int x, int y, TileCell cell)
    {
        Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0f);
        GameObject go;

        if (tilePrefab != null)
        {
            go = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
        }
        else
        {
            go = new GameObject($"Tile_{x}_{y}");
            go.transform.SetParent(transform);
            go.transform.position = pos;
            go.AddComponent<SpriteRenderer>();
        }

        go.name = $"Tile_{x}_{y}";
        go.transform.localScale = new Vector3(1.005f, 1.005f, 1f);

        cell.tileObject = go;
        cell.spriteRenderer = go.GetComponent<SpriteRenderer>();
        cell.spriteRenderer.sortingOrder = -10;
    }

    public void UpdateAllTerrainGraphics()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                UpdateTerrainTile(x, y);
    }

    void UpdateTerrainTile(int x, int y)
    {
        var cell = grid[x, y];
        if (cell == null || cell.spriteRenderer == null) return;

        if (cell.type == TileType.Road || cell.type == TileType.Building)
            return;

        if (cell.type == TileType.Water)
        {
            cell.spriteRenderer.sprite = deepWaterSprite;
            cell.spriteRenderer.color = Color.white;
            return;
        }

        int mask = 0;
        if (IsLand(x, y + 1)) mask |= 1;
        if (IsLand(x + 1, y)) mask |= 2;
        if (IsLand(x, y - 1)) mask |= 4;
        if (IsLand(x - 1, y)) mask |= 8;

        Sprite targetSprite = solidGrassSprite;

        if (grassSprites != null && mask < grassSprites.Length && grassSprites[mask] != null)
        {
            targetSprite = grassSprites[mask];
        }

        cell.spriteRenderer.sprite = targetSprite;
        cell.spriteRenderer.color = Color.white;
    }

    bool IsLand(int x, int y)
    {
        if (!IsInBounds(x, y)) return false;
        var t = grid[x, y].type;
        return t == TileType.Grass || t == TileType.Road || t == TileType.Building;
    }

    public bool IsInBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;
    public TileCell GetCell(int x, int y) => IsInBounds(x, y) ? grid[x, y] : null;
    public Vector3 GridToWorld(int x, int y) => new Vector3(x * cellSize, y * cellSize, 0f);
    public Vector2Int WorldToGrid(Vector3 world) =>
        new Vector2Int(Mathf.RoundToInt(world.x / cellSize), Mathf.RoundToInt(world.y / cellSize));

    public bool CanBuildAt(int x, int y)
    {
        var cell = GetCell(x, y);
        return cell != null && !cell.isOccupied && cell.type == TileType.Grass;
    }

    public bool CanBuildRoadAt(int x, int y)
    {
        var cell = GetCell(x, y);
        return cell != null && !cell.isOccupied && cell.type == TileType.Grass;
    }

    public void SetOccupied(int x, int y, bool occupied, Building b = null)
    {
        var cell = GetCell(x, y);
        if (cell == null) return;
        cell.isOccupied = occupied;
        cell.building = b;
        cell.type = occupied ? TileType.Building : TileType.Grass;
        UpdateTerrainTile(x, y);
    }

    public void SetRoad(int x, int y)
    {
        var cell = GetCell(x, y);
        if (cell == null) return;
        cell.isOccupied = true;
        cell.type = TileType.Road;
        OnRoadsChanged?.Invoke();
    }

    public void ClearRoad(int x, int y)
    {
        var cell = GetCell(x, y);
        if (cell == null) return;
        cell.isOccupied = false;
        cell.building = null;
        cell.type = TileType.Grass;
        UpdateTerrainTile(x, y);
        OnRoadsChanged?.Invoke();
    }
}