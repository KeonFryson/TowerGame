using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Settings")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float timeBetweenEnemies = 0.5f;

    [Header("Wave Points Formula")]
    [SerializeField] private float startingWavePoints = 10f;
    [SerializeField] private float waveLinearScale = 2.5f;
    [SerializeField] private float waveQuadraticScale = 0.15f;

    [Header("Tutorial")]
    [SerializeField] private bool enableTutorial = true;
    [SerializeField] private TutorialPopup tutorialPopupPrefab;

    private SaveData saveData;

    private int currentWave = 0;
    private bool waveInProgress = false;
    private int enemiesAlive = 0;

    // Track which enemy types have been seen
    private HashSet<string> seenEnemyTypes = new HashSet<string>();

    private void Awake()
    {
        waveInProgress = false;

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Find SaveData in the scene
        saveData = FindFirstObjectByType<SaveData>();
        if (saveData != null && saveData.LoadTutorialCompleted())
        {
            enableTutorial = false;
        }

        enemyPrefabs = Resources.LoadAll<GameObject>("Prefabs/Enemy").ToList();
        Debug.Log($"Loaded {enemyPrefabs.Count} enemy prefabs from Resources/Prefabs/Enemy");

        foreach (var prefab in enemyPrefabs)
        {
            Debug.Log($"Loaded prefab: {prefab.name}");
        }
    }

    private void Start()
    {
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        yield return new WaitForSeconds(timeBetweenWaves);

        while (!GameManager.Instance.IsGameOver)
        {
            currentWave++;
            waveInProgress = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCurrentWave(currentWave);
            }

            // Generate wave using points budget and tier mixing
            List<GameObject> waveEnemies = GenerateWave(currentWave);

            yield return StartCoroutine(SpawnWave(waveEnemies));

            // Wait until all enemies are defeated before starting the next wave
            while (enemiesAlive > 0)
            {
                yield return null;
            }

            waveInProgress = false;

            yield return new WaitForSeconds(timeBetweenWaves);
        }

    }

    // Wave points formula
    private float GetWavePoints(int wave)
    {
        return startingWavePoints + (wave * waveLinearScale) + (wave * wave * waveQuadraticScale);
    }

    // Tier mixing ratios
    private Dictionary<EnemyTier, float> GetTierRatios(int wave)
    {
        if (wave < 10)
            return new() { { EnemyTier.Baby, 1f } };
        if (wave < 25)
            return new() { { EnemyTier.Baby, 0.9f }, { EnemyTier.Weak, 0.1f } };
        if (wave < 30)
            return new() { { EnemyTier.Baby, 0.3f }, { EnemyTier.Weak, 0.4f }, { EnemyTier.Mid, 0.3f } };
        if (wave < 50)
            return new() { { EnemyTier.Baby, 0.1f }, { EnemyTier.Weak, 0.2f }, { EnemyTier.Mid, 0.4f }, { EnemyTier.Strong, 0.3f } };
        if (wave < 80)
            return new() { { EnemyTier.Baby, 0.05f }, { EnemyTier.Weak, 0.1f }, { EnemyTier.Mid, 0.25f }, { EnemyTier.Strong, 0.35f }, { EnemyTier.Elite, 0.25f } };
        return new() { { EnemyTier.Baby, 0.05f }, { EnemyTier.Weak, 0.05f }, { EnemyTier.Mid, 0.15f }, { EnemyTier.Strong, 0.25f }, { EnemyTier.Elite, 0.25f }, { EnemyTier.Boss, 0.2f } };
    }

    // Generate a wave using points and tier mixing
    public List<GameObject> GenerateWave(int wave)
    {
        float points = GetWavePoints(wave);
        Debug.Log($"Generating wave {wave} with {points} points");
        var ratios = GetTierRatios(wave);

        // Group prefabs by tier, but only for tiers in ratios
        Dictionary<EnemyTier, List<GameObject>> tierPrefabs = new();
        foreach (var tier in ratios.Keys)
        {
            tierPrefabs[tier] = enemyPrefabs
                .Where(p =>
                {
                    Enemy enemy = p.GetComponent<Enemy>();
                    return enemy != null && enemy.Tier == tier;
                })
                .ToList();
        }

        List<GameObject> waveEnemies = new();

        foreach (var kvp in ratios)
        {
            float tierPoints = points * kvp.Value;
            var available = tierPrefabs[kvp.Key].Where(e => e != null).ToList();

            bool added = false;
            while (tierPoints > 0 && available.Count > 0)
            {
                var prefab = available[Random.Range(0, available.Count)];
                var enemy = prefab.GetComponent<Enemy>();
                int cost = enemy != null ? enemy.Cost : 1;
                if (tierPoints >= cost)
                {
                    waveEnemies.Add(prefab);
                    tierPoints -= cost;
                    added = true;
                }
                else
                {
                    break;
                }
            }
            // Always add at least one from this tier if possible
            if (!added && available.Count > 0)
            {
                var cheapest = available.OrderBy(e => e.GetComponent<Enemy>().Cost).First();
                waveEnemies.Add(cheapest);
            }
        }

        // Shuffle for randomness
        for (int i = 0; i < waveEnemies.Count; i++)
        {
            int j = Random.Range(i, waveEnemies.Count);
            var temp = waveEnemies[i];
            waveEnemies[i] = waveEnemies[j];
            waveEnemies[j] = temp;
        }

        return waveEnemies;
    }

    // Spawn the generated wave
    private IEnumerator SpawnWave(List<GameObject> waveEnemies)
    {
        foreach (var prefab in waveEnemies)
        {
            Vector3 spawnPos = PathManager.Instance != null ? PathManager.Instance.GetWaypoint(0) : Vector3.zero;

            // Tutorial: Check for new enemy type
            if (enableTutorial)
            {
                string enemyType = prefab.name;
                if (!seenEnemyTypes.Contains(enemyType))
                {
                    seenEnemyTypes.Add(enemyType);
                    yield return StartCoroutine(ShowTutorialForEnemy(prefab));
                }
            }

            // Inside SpawnWave, after showing tutorial for a new enemy type
            if (enableTutorial)
            {
                string enemyType = prefab.name;
                if (!seenEnemyTypes.Contains(enemyType))
                {
                    seenEnemyTypes.Add(enemyType);
                    yield return StartCoroutine(ShowTutorialForEnemy(prefab));

                    // Check if all enemy types have been seen
                    if (seenEnemyTypes.Count == enemyPrefabs.Count)
                    {
                        if (saveData != null)
                        {
                            saveData.SaveTutorialCompleted(true);
                        }
                    }
                }
            }

            Instantiate(prefab, spawnPos, Quaternion.identity);
            enemiesAlive++;

            yield return new WaitForSeconds(timeBetweenEnemies);
        }
    }

    // Show tutorial popup and pause game
    private IEnumerator ShowTutorialForEnemy(GameObject enemyPrefab)
    {
       GameManager.Instance.PauseGame();

        TutorialPopup popup = null;
        if (tutorialPopupPrefab != null)
        {
            popup = Instantiate(tutorialPopupPrefab);
            popup.SetEnemyInfo(enemyPrefab);
            popup.Show();
        }
        else
        {
            Debug.LogWarning("TutorialPopupPrefab not assigned in WaveManager.");
        }

        // Wait for player to close popup
        bool closed = false;
        if (popup != null)
        {
            popup.OnClosed += () => closed = true;
            while (!closed)
            {
                yield return null;
            }
        }
        else
        {
            // Fallback: wait for any key
            while (!Input.anyKeyDown)
            {
                yield return null;
            }
        }

      GameManager.Instance.ResumeGame();
    }

    // Call this method from the Enemy script when an enemy dies
    public void OnEnemyDefeated()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }
}