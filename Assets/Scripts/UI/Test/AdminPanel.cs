using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AdminPanel : MonoBehaviour
{
    [Header("Wave Controls")]
    [SerializeField] private TMPro.TMP_InputField waveInputField;
    [SerializeField] private Button setWaveButton;
    [SerializeField] private Button generateWaveButton;
    [SerializeField] private TMPro.TextMeshProUGUI currentWaveText;

    [Header("Money Controls")]
    [SerializeField] private Button setMoneyButton;

    [Header("Scripted Wave Controls")]
    [SerializeField] private Button spawnSingleEnemyButton;
    [SerializeField] private TMPro.TMP_Dropdown enemyDropdown;

    private List<GameObject> enemyPrefabs;

    private void Start()
    {
        if (setWaveButton != null)
            setWaveButton.onClick.AddListener(SetWave);

        if (generateWaveButton != null)
            generateWaveButton.onClick.AddListener(GenerateWave);

        if (setMoneyButton != null)
            setMoneyButton.onClick.AddListener(AddMoney);

        if (spawnSingleEnemyButton != null)
            spawnSingleEnemyButton.onClick.AddListener(SpawnSingleEnemyScriptedWave);

        // Populate enemy dropdown
        enemyPrefabs = WaveManager.Instance != null ? WaveManager.Instance.EnemyPrefabs : new List<GameObject>();
        if (enemyDropdown != null && enemyPrefabs != null)
        {
            enemyDropdown.ClearOptions();
            var options = new List<string>();
            foreach (var prefab in enemyPrefabs)
            {
                options.Add(prefab != null ? prefab.name : "NULL");
            }
            enemyDropdown.AddOptions(options);
        }

        UpdateCurrentWaveText();
    }

    private void SetWave()
    {
        if (int.TryParse(waveInputField.text, out int wave))
        {
            typeof(WaveManager)
                .GetField("currentWave", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(WaveManager.Instance, wave);

            UpdateCurrentWaveText();
        }
    }

    private void GenerateWave()
    {
        if (int.TryParse(waveInputField.text, out int wave))
        {
            var waveEnemies = WaveManager.Instance.GenerateWave(wave);
            Debug.Log($"Generated {waveEnemies.Count} enemies for wave {wave} (admin panel test)");
        }
    }

    private void AddMoney()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(100000);
            Debug.Log($"Set player money to 100000 (admin panel)");
        }
    }

    private void SpawnSingleEnemyScriptedWave()
    {
        if (enemyDropdown != null && enemyPrefabs != null && enemyDropdown.value < enemyPrefabs.Count)
        {
            var prefab = enemyPrefabs[enemyDropdown.value];
            WaveManager.Instance.StartSingleEnemyScriptedWave(prefab);
            Debug.Log($"Spawned scripted wave with single enemy: {prefab.name}");
        }
    }

    private void UpdateCurrentWaveText()
    {
        if (currentWaveText != null && WaveManager.Instance != null)
        {
            currentWaveText.text = $"Current Wave: {WaveManager.Instance.GetCurrentWave()}";
        }
    }
}