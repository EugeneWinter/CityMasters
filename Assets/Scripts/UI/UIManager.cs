using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Ресурсы")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI populationText;
    public TextMeshProUGUI happinessText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI targetText;
    public TextMeshProUGUI messageText;

    [Header("Панели")]
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject pausePanel;
    public GameObject buildPanel;

    [Header("Диалоги")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueNameText;
    public TextMeshProUGUI dialogueContentText;

    private Coroutine messageRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourcesChanged += UpdateUI;
            ResourceManager.Instance.OnMessage += ShowMessage;
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnWin += HandleWin;
            GameManager.Instance.OnLose += HandleLose;
        }

        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);

        EnsureDialogueUI();

        UpdateUI();
        UpdateTarget();
    }

    void EnsureDialogueUI()
    {
        if (dialoguePanel != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject panelGo = new GameObject("DialoguePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
        panelGo.transform.SetParent(canvas.transform, false);
        dialoguePanel = panelGo;

        RectTransform panelRt = panelGo.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0f);
        panelRt.anchorMax = new Vector2(0.5f, 0f);
        panelRt.pivot = new Vector2(0.5f, 0f);
        panelRt.sizeDelta = new Vector2(500, 140);
        panelRt.anchoredPosition = new Vector2(0, 40);
        panelGo.GetComponent<UnityEngine.UI.Image>().color = new Color(0.12f, 0.12f, 0.12f, 0.95f);

        GameObject nameGo = new GameObject("NameText", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameGo.transform.SetParent(panelGo.transform, false);
        dialogueNameText = nameGo.GetComponent<TextMeshProUGUI>();
        dialogueNameText.fontSize = 18;
        dialogueNameText.fontStyle = FontStyles.Bold;
        dialogueNameText.color = new Color(1f, 0.78f, 0f);
        dialogueNameText.alignment = TextAlignmentOptions.Left;

        RectTransform nameRt = nameGo.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0, 1);
        nameRt.anchorMax = new Vector2(1, 1);
        nameRt.pivot = new Vector2(0.5f, 1);
        nameRt.anchoredPosition = new Vector2(20, -15);
        nameRt.sizeDelta = new Vector2(-40, 25);

        GameObject contentGo = new GameObject("ContentText", typeof(RectTransform), typeof(TextMeshProUGUI));
        contentGo.transform.SetParent(panelGo.transform, false);
        dialogueContentText = contentGo.GetComponent<TextMeshProUGUI>();
        dialogueContentText.fontSize = 15;
        dialogueContentText.color = Color.white;
        dialogueContentText.alignment = TextAlignmentOptions.TopLeft;

        RectTransform contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 0);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 0.5f);
        contentRt.anchoredPosition = new Vector2(0, -20);
        contentRt.sizeDelta = new Vector2(-40, -60);

        GameObject buttonGo = new GameObject("CloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
        buttonGo.transform.SetParent(panelGo.transform, false);

        RectTransform buttonRt = buttonGo.GetComponent<RectTransform>();
        buttonRt.anchorMin = new Vector2(1, 1);
        buttonRt.anchorMax = new Vector2(1, 1);
        buttonRt.pivot = new Vector2(1, 1);
        buttonRt.anchoredPosition = new Vector2(-10, -10);
        buttonRt.sizeDelta = new Vector2(24, 24);
        buttonGo.GetComponent<UnityEngine.UI.Image>().color = new Color(0.8f, 0.2f, 0.2f);

        GameObject btnTextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTextGo.transform.SetParent(buttonGo.transform, false);
        TextMeshProUGUI btnTxt = btnTextGo.GetComponent<TextMeshProUGUI>();
        btnTxt.text = "X";
        btnTxt.fontSize = 12;
        btnTxt.fontStyle = FontStyles.Bold;
        btnTxt.color = Color.white;
        btnTxt.alignment = TextAlignmentOptions.Center;

        RectTransform btnTextRt = btnTextGo.GetComponent<RectTransform>();
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.sizeDelta = Vector2.zero;

        UnityEngine.UI.Button btn = buttonGo.GetComponent<UnityEngine.UI.Button>();
        btn.onClick.AddListener(CloseDialogue);

        dialoguePanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourcesChanged -= UpdateUI;
            ResourceManager.Instance.OnMessage -= ShowMessage;
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnWin -= HandleWin;
            GameManager.Instance.OnLose -= HandleLose;
        }
    }

    void HandleWin() { if (winPanel) winPanel.SetActive(true); }
    void HandleLose() { if (losePanel) losePanel.SetActive(true); }

    void Update()
    {
        if (GameManager.Instance == null) return;

        if (timeText != null)
        {
            float t = GameManager.Instance.GetTimeLeft();
            timeText.text = $"Время: {Mathf.FloorToInt(t / 60):00}:{Mathf.FloorToInt(t % 60):00}";
            timeText.color = t < 60f ? Color.red : Color.white;
        }
        if (pausePanel != null) pausePanel.SetActive(GameManager.Instance.isPaused);
        if (speedText != null)
        {
            int idx = GameManager.Instance.GetSpeedIndex();
            speedText.text = $"x{GameManager.Instance.speedSteps[idx]:0.#}";
        }
    }

    void UpdateTarget()
    {
        if (targetText == null || GameManager.Instance == null) return;
        targetText.text =
            $"Цель: {GameManager.Instance.targetPopulation} жителей, " +
            $"{GameManager.Instance.targetHappiness}% счастья";
    }

    void UpdateUI()
    {
        var rm = ResourceManager.Instance;
        if (rm == null) return;

        if (moneyText)
        {
            moneyText.text = $"Бюджет: {rm.money}$";
            moneyText.color = rm.money < 0 ? Color.red : Color.white;
        }
        if (populationText) populationText.text = $"Жители: {rm.population}/{rm.maxPopulation}";
        if (happinessText)
        {
            happinessText.text = $"Счастье: {rm.happiness}%";
            happinessText.color = rm.happiness < 30 ? Color.red :
                                  rm.happiness > 70 ? Color.green : Color.white;
        }
        if (energyText)
        {
            energyText.text = $"Энергия: {rm.energy}/{rm.energyConsumption}";
            energyText.color = rm.energyConsumption > rm.energy ? Color.red : Color.white;
        }
    }

    public void ShowMessage(string msg, Color color)
    {
        if (messageText == null) return;
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(msg, color));
    }

    IEnumerator MessageRoutine(string msg, Color color)
    {
        messageText.text = msg;
        messageText.color = color;
        float t = 0f;
        while (t < 1.5f) { t += Time.unscaledDeltaTime; yield return null; }
        while (messageText.color.a > 0f)
        {
            Color c = messageText.color;
            c.a -= Time.unscaledDeltaTime * 2f;
            messageText.color = c;
            yield return null;
        }
        messageText.text = "";
    }

    public void ShowDialogue(string citizenName, string content)
    {
        EnsureDialogueUI();
        if (dialoguePanel == null) return;
        dialoguePanel.SetActive(true);
        if (dialogueNameText) dialogueNameText.text = citizenName;
        if (dialogueContentText) dialogueContentText.text = content;
    }

    public void CloseDialogue()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
    }

    public void OnRestartClick() => GameManager.Instance?.RestartLevel();
    public void OnNextLevelClick() => GameManager.Instance?.LoadNextLevel();
    public void OnPauseClick() => GameManager.Instance?.TogglePause();
    public void OnRoadClick() => BuildingPlacer.Instance?.SelectRoad();
    public void OnDemolishClick() => BuildingPlacer.Instance?.SelectDemolish();
    public void OnSpeed1() => GameManager.Instance?.SetSpeed(0);
    public void OnSpeed2() => GameManager.Instance?.SetSpeed(1);
    public void OnSpeed3() => GameManager.Instance?.SetSpeed(2);
    public void OnToggleBuildPanel() { if (buildPanel) buildPanel.SetActive(!buildPanel.activeSelf); }
}