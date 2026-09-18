using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData data;
    public Vector2Int gridPosition;
    public bool IsConnectedToRoad { get; private set; } = true;

    private bool effectsApplied = false;

    private static readonly string[] citizenNames = {
        "Алексей", "Дмитрий", "Елена", "Мария", "Иван", "Ольга", "Николай", "Анна", "Сергей", "Татьяна"
    };

    private static readonly string[] neutralPhrases = {
        "Наш город развивается очень быстро!",
        "Мне нравится планировка жилых кварталов.",
        "Интересно, какое здание мэр построит следующим?",
        "Сегодня отличный день для прогулки в парке!"
    };

    private static readonly string[] lowHappinessPhrases = {
        "Мэр, в городе грустно жить. Нам нужно больше парков!",
        "Уровень жизни падает, планируете ли вы что-то предпринимать?",
        "Счастье жителей на нуле. Нам нужно поднять настроение!"
    };

    private static readonly string[] noRoadPhrases = {
        "К нашему дому невозможно подъехать! Проложите дорогу!",
        "Мы отрезаны от остального города. Где дорожные службы?",
        "Пожалуйста, соедините наше здание с дорожной сетью."
    };

    private static readonly string[] lowEnergyPhrases = {
        "Опять отключили свет! Нам не хватает электроэнергии!",
        "Бытовые приборы не работают. Постройте еще одну электростанцию!",
        "Город погружается во тьму, решите проблему с энергией!"
    };

    public void Initialize(BuildingData d, Vector2Int pos)
    {
        data = d;
        gridPosition = pos;

        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        if (d != null && d.icon != null) sr.sprite = d.icon;
        sr.sortingOrder = 5;

        if (ResourceManager.Instance != null && d != null)
        {
            ResourceManager.Instance.AddBuildingEffects(d);
            ResourceManager.Instance.RegisterBuilding(this);
            effectsApplied = true;
        }

        UpdateRoadConnection();
    }

    public void UpdateRoadConnection()
    {
        if (GridManager.Instance == null) { IsConnectedToRoad = true; return; }

        int x = gridPosition.x, y = gridPosition.y;
        IsConnectedToRoad =
            IsRoad(x + 1, y) || IsRoad(x - 1, y) ||
            IsRoad(x, y + 1) || IsRoad(x, y - 1);

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = IsConnectedToRoad ? Color.white : new Color(0.7f, 0.7f, 0.7f);
    }

    bool IsRoad(int x, int y)
    {
        var c = GridManager.Instance.GetCell(x, y);
        return c != null && c.type == GridManager.TileType.Road;
    }

    public void Interact()
    {
        if (UIManager.Instance == null || ResourceManager.Instance == null) return;

        string residentName = citizenNames[Random.Range(0, citizenNames.Length)];
        string phrase = "";

        if (!IsConnectedToRoad)
        {
            phrase = noRoadPhrases[Random.Range(0, noRoadPhrases.Length)];
        }
        else if (ResourceManager.Instance.energyConsumption > ResourceManager.Instance.energy)
        {
            phrase = lowEnergyPhrases[Random.Range(0, lowEnergyPhrases.Length)];
        }
        else if (ResourceManager.Instance.happiness < 40)
        {
            phrase = lowHappinessPhrases[Random.Range(0, lowHappinessPhrases.Length)];
        }
        else
        {
            phrase = $"{neutralPhrases[Random.Range(0, neutralPhrases.Length)]} (Текущий бюджет: {ResourceManager.Instance.money}$)";
        }

        UIManager.Instance.ShowDialogue($"{residentName} (Житель)", phrase);
    }

    void OnDestroy()
    {
        if (effectsApplied && ResourceManager.Instance != null && data != null)
        {
            ResourceManager.Instance.RemoveBuildingEffects(data);
            ResourceManager.Instance.UnregisterBuilding(this);
            effectsApplied = false;
        }
    }

    public void Demolish()
    {
        if (GridManager.Instance != null)
            GridManager.Instance.SetOccupied(gridPosition.x, gridPosition.y, false);
        Destroy(gameObject);
    }
}