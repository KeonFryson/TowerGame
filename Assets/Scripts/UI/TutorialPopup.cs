using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TutorialPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image enemyImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button closeButton;

    public event Action OnClosed;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    public void SetEnemyInfo(GameObject enemyPrefab)
    {
        if (enemyPrefab == null)
            return;

        Enemy enemy = enemyPrefab.GetComponent<Enemy>();
        if (enemy != null)
        {
            nameText.text = enemyPrefab.name;
            statsText.text = $"HP: {enemy.Health}\nDamage: {enemy.Damage}\n Speed: {enemy.MoveSpeed}\nArmor: {enemy.Armor}\nMoneyReward: {enemy.MoneyReward}";
            descriptionText.text = enemy.GetDescription(); // Implement this in your Enemy class
            var sr = enemyPrefab.GetComponent<SpriteRenderer>();
            if (enemyImage != null && sr != null)
                enemyImage.sprite = sr.sprite;
        }
        else
        {
            nameText.text = enemyPrefab.name;
            statsText.text = "";
            descriptionText.text = "A new enemy has appeared!";
            enemyImage.sprite = null;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        
        if (OnClosed != null)
            OnClosed.Invoke();

        Destroy(gameObject);
    }
}