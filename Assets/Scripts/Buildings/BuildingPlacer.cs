using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    public static BuildingPlacer Instance { get; private set; }

    public BuildingData selectedBuilding;
    public bool roadMode = false;
    public bool demolishMode = false;

    [Header("Refund")]
    public float buildingRefundRate = 0.5f;

    private GameObject ghostPreview;
    private SpriteRenderer ghostRenderer;
    private Camera cam;

    private Vector2Int lastDragCell = new Vector2Int(int.MinValue, int.MinValue);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() { cam = Camera.main; EnsureGhost(); }

    void EnsureGhost()
    {
        if (ghostPreview == null)
        {
            ghostPreview = new GameObject("GhostPreview");
            ghostRenderer = ghostPreview.AddComponent<SpriteRenderer>();
            ghostRenderer.sortingOrder = 100;
            ghostPreview.SetActive(false);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && (GameManager.Instance.isGameOver || GameManager.Instance.isPaused))
        { if (ghostPreview) ghostPreview.SetActive(false); return; }

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0;
        Vector2Int gp = GridManager.Instance.WorldToGrid(mouse);

        UpdateGhost(gp);

        if (!IsOverUI())
        {
            bool click = Input.GetMouseButtonDown(0);
            bool drag = Input.GetMouseButton(0) && (roadMode || demolishMode) && gp != lastDragCell;

            if (click || drag)
            {
                if (roadMode) AutoTileRoad.Instance.PlaceRoad(gp.x, gp.y);
                else if (demolishMode) DemolishAt(gp.x, gp.y);
                else if (click && selectedBuilding != null) TryPlace(gp.x, gp.y);
                else if (click && selectedBuilding == null && !roadMode && !demolishMode) TryInteractAt(gp.x, gp.y);
                lastDragCell = gp;
            }
        }

        if (Input.GetMouseButtonDown(1)) CancelSelection();
    }

    void UpdateGhost(Vector2Int gp)
    {
        EnsureGhost();

        if (!GridManager.Instance.IsInBounds(gp.x, gp.y))
        { ghostPreview.SetActive(false); return; }

        Sprite s = null;
        bool ok = false;

        if (selectedBuilding != null)
        {
            s = selectedBuilding.icon;
            ok = GridManager.Instance.CanBuildAt(gp.x, gp.y) &&
                 ResourceManager.Instance.CanAfford(selectedBuilding.cost);
        }
        else if (roadMode)
        {
            if (AutoTileRoad.Instance != null && AutoTileRoad.Instance.roadSprites.Length > 0)
                s = AutoTileRoad.Instance.roadSprites[0];
            ok = GridManager.Instance.CanBuildRoadAt(gp.x, gp.y) &&
                 ResourceManager.Instance.CanAfford(AutoTileRoad.Instance.roadCost);
        }
        else if (demolishMode)
        {
            var cell = GridManager.Instance.GetCell(gp.x, gp.y);
            ok = cell != null && (cell.building != null || cell.type == GridManager.TileType.Road);
            if (cell != null && cell.building != null && cell.building.data != null)
                s = cell.building.data.icon;
        }
        else
        { ghostPreview.SetActive(false); return; }

        ghostPreview.SetActive(true);
        ghostPreview.transform.position = GridManager.Instance.GridToWorld(gp.x, gp.y);
        ghostRenderer.sprite = s;

        if (demolishMode)
            ghostRenderer.color = ok ? new Color(1f, 0.4f, 0.1f, 0.7f) : new Color(0.5f, 0.5f, 0.5f, 0.3f);
        else
            ghostRenderer.color = ok ? new Color(0.2f, 1f, 0.2f, 0.6f) : new Color(1f, 0.2f, 0.2f, 0.6f);
    }

    void TryPlace(int x, int y)
    {
        if (selectedBuilding == null) return;
        if (!GridManager.Instance.CanBuildAt(x, y))
        { ResourceManager.Instance?.Notify("Здесь нельзя строить!", Color.red); return; }
        if (!ResourceManager.Instance.CanAfford(selectedBuilding.cost))
        { ResourceManager.Instance?.Notify("Недостаточно денег!", Color.red); return; }
        if (selectedBuilding.prefab == null)
        { Debug.LogError($"У '{selectedBuilding.buildingName}' не назначен prefab!"); return; }

        ResourceManager.Instance.Spend(selectedBuilding.cost);
        Vector3 pos = GridManager.Instance.GridToWorld(x, y);
        GameObject go = Instantiate(selectedBuilding.prefab, pos, Quaternion.identity);

        var b = go.GetComponent<Building>();
        if (b == null) b = go.AddComponent<Building>();

        b.Initialize(selectedBuilding, new Vector2Int(x, y));
        GridManager.Instance.SetOccupied(x, y, true, b);
    }

    void DemolishAt(int x, int y)
    {
        var cell = GridManager.Instance.GetCell(x, y);
        if (cell == null) return;

        if (cell.building != null)
        {
            int refund = Mathf.RoundToInt(cell.building.data.cost * buildingRefundRate);
            ResourceManager.Instance.money += refund;
            ResourceManager.Instance.Notify($"+{refund}$", Color.yellow);
            cell.building.Demolish();
            return;
        }
        if (cell.type == GridManager.TileType.Road)
            AutoTileRoad.Instance.RemoveRoad(x, y);
    }

    void TryInteractAt(int x, int y)
    {
        var cell = GridManager.Instance.GetCell(x, y);
        if (cell != null && cell.building != null)
        {
            cell.building.Interact();
        }
        else
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseDialogue();
            }
        }
    }

    public void SelectBuilding(BuildingData data)
    {
        selectedBuilding = data;
        roadMode = false;
        demolishMode = false;
    }

    public void SelectRoad() { selectedBuilding = null; roadMode = true; demolishMode = false; }
    public void SelectDemolish() { selectedBuilding = null; roadMode = false; demolishMode = true; }

    public void CancelSelection()
    {
        selectedBuilding = null;
        roadMode = false;
        demolishMode = false;
        if (ghostPreview) ghostPreview.SetActive(false);
        if (UIManager.Instance != null) UIManager.Instance.CloseDialogue();
    }

    bool IsOverUI() => UnityEngine.EventSystems.EventSystem.current != null &&
                       UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
}