using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public int currentLevel = 1;
    public bool isGameOver = false;
    public bool isPaused = false;

    [Header("Win/Lose Conditions")]
    public int targetPopulation = 50;
    public int targetHappiness = 70;
    public float timeLimit = 600f;
    private float currentTime;

    [Header("Speed")]
    public float[] speedSteps = { 1f, 2f, 4f };
    private int speedIndex = 0;

    [Header("Win Hold")]
    public float winHoldTime = 3f;
    private float winHoldTimer = 0f;

    public System.Action OnWin;
    public System.Action OnLose;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        currentLevel = Mathf.Max(1, PlayerPrefs.GetInt("CurrentLevel", 1));
    }

    void Start()
    {
        LoadLevelSettings();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { TogglePause(); return; }
        if (Input.GetKeyDown(KeyCode.Space)) TogglePause();

        if (Input.GetKeyDown(KeyCode.Alpha1)) SetSpeed(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetSpeed(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetSpeed(2);

        if (isGameOver || isPaused) return;

        currentTime -= Time.deltaTime;
        CheckWinLose();
    }

    void LoadLevelSettings()
    {
        if (currentLevel == 1)
        {
            targetPopulation = 30;
            targetHappiness = 50;
            timeLimit = 600f;
            if (ResourceManager.Instance != null) ResourceManager.Instance.money = 1000;
        }
        else if (currentLevel >= 2)
        {
            targetPopulation = 80;
            targetHappiness = 70;
            timeLimit = 900f;
            if (ResourceManager.Instance != null) ResourceManager.Instance.money = 1500;
        }
        currentTime = timeLimit;
    }

    void CheckWinLose()
    {
        var rm = ResourceManager.Instance;
        if (rm == null) return;

        bool winCondition = targetPopulation > 0 &&
                            rm.population >= targetPopulation &&
                            rm.happiness >= targetHappiness;

        if (winCondition)
        {
            winHoldTimer += Time.deltaTime;
            if (winHoldTimer >= winHoldTime) { Win(); return; }
        }
        else winHoldTimer = 0f;

        if (currentTime <= 0f || rm.money < -500) Lose();
    }

    public void Win()
    {
        if (isGameOver) return;
        isGameOver = true;
        Time.timeScale = 0f;
        OnWin?.Invoke();
    }

    public void Lose()
    {
        if (isGameOver) return;
        isGameOver = true;
        Time.timeScale = 0f;
        OnLose?.Invoke();
    }

    public void TogglePause()
    {
        if (isGameOver) return;
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : speedSteps[speedIndex];
    }

    public void SetSpeed(int index)
    {
        if (isGameOver || isPaused) return;
        speedIndex = Mathf.Clamp(index, 0, speedSteps.Length - 1);
        Time.timeScale = speedSteps[speedIndex];
    }

    public int GetSpeedIndex() => speedIndex;
    public float GetWinProgress() => winHoldTime > 0 ? winHoldTimer / winHoldTime : 0f;

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        currentLevel++;
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next >= SceneManager.sceneCountInBuildSettings)
            next = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(next);
    }

    public float GetTimeLeft() => Mathf.Max(0, currentTime);
}