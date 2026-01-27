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

    [Header("Wave Progression")]
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private float enemyIncreasePerWave = 1.5f;

    private int currentWave = 0;
    private bool waveInProgress = false;
    private int enemiesAlive = 0;

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

        while (true)
        {
            currentWave++;
            waveInProgress = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCurrentWave(currentWave);
            }

            int enemyCount = Mathf.RoundToInt(baseEnemiesPerWave * Mathf.Pow(enemyIncreasePerWave, currentWave - 1));

            yield return StartCoroutine(SpawnWave(enemyCount));

            // Wait until all enemies are defeated before starting the next wave
            while (enemiesAlive > 0)
            {
                yield return null;
            }

            waveInProgress = false;

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private IEnumerator SpawnWave(int enemyCount)
    {
        // Determine main and previous tier for this wave
        EnemyTier mainTier = EnemyTierHelper.GetTierByWave(currentWave);
        EnemyTier prevTier = EnemyTier.Baby;
        if ((int)mainTier > 0)
            prevTier = (EnemyTier)((int)mainTier - 1);

        // Calculate percentage of main tier (e.g., 10% at start of new tier, 100% at end)
        float tierProgress = EnemyTierHelper.GetTierProgress(currentWave);
        int mainTierCount = Mathf.RoundToInt(enemyCount * tierProgress);
        int prevTierCount = enemyCount - mainTierCount;

        // Build spawn list
        List<EnemyTier> spawnTiers = new List<EnemyTier>();
        spawnTiers.AddRange(Enumerable.Repeat(mainTier, mainTierCount));
        spawnTiers.AddRange(Enumerable.Repeat(prevTier, prevTierCount));

        // Shuffle spawnTiers for randomness
        for (int i = 0; i < spawnTiers.Count; i++)
        {
            int j = Random.Range(i, spawnTiers.Count);
            var temp = spawnTiers[i];
            spawnTiers[i] = spawnTiers[j];
            spawnTiers[j] = temp;
        }

        // Spawn enemies by tier
        foreach (var tier in spawnTiers)
        {
            SpawnEnemyOfTier(tier);
            yield return new WaitForSeconds(timeBetweenEnemies);
        }
    }

    private void SpawnEnemyOfTier(EnemyTier tier)
    {
        if (enemyPrefabs != null && enemyPrefabs.Count > 0 && PathManager.Instance != null)
        {
            var tierPrefabs = enemyPrefabs
                .Where(p =>
                {
                    Enemy enemy = p.GetComponent<Enemy>();
                    return enemy != null && enemy.Tier == tier;
                })
                .ToList();

            // Fallback to all prefabs if none match the tier
            var spawnList = tierPrefabs.Count > 0 ? tierPrefabs : enemyPrefabs;

            int index = Random.Range(0, spawnList.Count);
            GameObject prefab = spawnList[index];

            Vector3 spawnPos = PathManager.Instance.GetWaypoint(0);
            Instantiate(prefab, spawnPos, Quaternion.identity);
            enemiesAlive++;
        }
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