using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int startingMoney = 650;
    [SerializeField] private int startingLives = 100;

    private int currentMoney;
    private int currentLives;
    private int currentWave = 0;

    [HideInInspector]
    public UnityEvent<int> OnMoneyChanged;
    [HideInInspector]
    public UnityEvent<int> OnLivesChanged;
    [HideInInspector]
    public UnityEvent<int> OnWaveChanged;
    [HideInInspector]
    public UnityEvent OnGameOver;

     
    private float[] gameSpeeds = { 1f, 2f, 4f };
    private int currentSpeedIndex = 0;
    [HideInInspector]
    public UnityEvent<float> OnGameSpeedChanged;

    private bool isPaused = false;
    private float previousTimeScale = 1f;
    [HideInInspector]
    public UnityEvent<bool> OnPauseStateChanged;
    bool GameOverFlag = false;
    public bool IsGameOver => GameOverFlag;

    private WInLoseUI winLoseUI;

    private void Awake()
    {
        if(winLoseUI == null)
        {
            winLoseUI = FindFirstObjectByType<WInLoseUI>();
        }

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentMoney = startingMoney;
        currentLives = startingLives;
        SetGameSpeed(0);

        
    }

    private void Start()
    {
        OnMoneyChanged?.Invoke(currentMoney);
        OnLivesChanged?.Invoke(currentLives);
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            OnMoneyChanged?.Invoke(currentMoney);
            return true;
        }
        return false;
    }

    public void LoseLife(int amount)
    {
        currentLives -= amount;
        OnLivesChanged?.Invoke(currentLives);

        if (currentLives <= 0)
        {
            LoseGame();
        }
    }

    public void SetCurrentWave(int wave)
    {
        currentWave = wave;
        OnWaveChanged?.Invoke(currentWave);
    }

   public void LoseGame()
    {
        if (GameOverFlag) return;
        GameOverFlag = true;
        Debug.Log("Game Over!");
        OnGameOver?.Invoke();
        CameraMovement.Instance.enabled = false;
        PauseGame();
        winLoseUI.ShowLose();
        

    }

    public void WinGame()
    {
        if (GameOverFlag) return;
        GameOverFlag = true;
        OnGameOver?.Invoke();
        Debug.Log("You Win!");
        CameraMovement.Instance.enabled = false;
        PauseGame();
        winLoseUI.ShowWin();
        
    }

    public int GetWave() => currentWave;
    public int GetMoney() => currentMoney;
    public int GetLives() => currentLives;

    public void SetGameSpeed(int speedIndex)
    {
        if (isPaused)
        {
            currentSpeedIndex = speedIndex;
            return;
        }

        if (speedIndex < 0 || speedIndex >= gameSpeeds.Length)
            return;

        currentSpeedIndex = speedIndex;
        Time.timeScale = gameSpeeds[speedIndex];
        previousTimeScale = gameSpeeds[speedIndex];
        OnGameSpeedChanged?.Invoke(gameSpeeds[speedIndex]);
    }

    public float GetCurrentGameSpeed() => gameSpeeds[currentSpeedIndex];

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (!isPaused)
        {
            isPaused = true;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            OnPauseStateChanged?.Invoke(true);
        }
    }

    public void ResetMap()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void ResumeGame()
    {
        if (isPaused)
        {
            isPaused = false;
            Time.timeScale = gameSpeeds[currentSpeedIndex];
            OnPauseStateChanged?.Invoke(false);
        }
    }

    public void OpenMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public bool IsPaused() => isPaused;
}