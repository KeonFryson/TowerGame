using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // Add this for EventTrigger

public class TowerDataPanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image spriteImage;
    [SerializeField] private TMP_Dropdown targetingDropdown;
    [SerializeField] private Transform upgradeButtonsParent; // Should have 3 children: Upgrade1, Upgrade2, Upgrade3
    [SerializeField] private Sprite[] upgradeBarSprites; // Assign Green_1, Green2, Green_3, Green_end, Green_Full in order
    [SerializeField] private Button SellButton;
    [SerializeField] private TextMeshProUGUI SellPrice;

    private Tower currentTower;
    private int sellAmount;

    private void Start()
    {
        // Setup Sell button
        if (SellButton != null)
        {
            SellButton.onClick.AddListener(OnSellButtonPressed);
        }
    }

    private void Update()
    {
        // Refresh UI if panel is open and a tower is selected
        if (panel.activeSelf && currentTower != null)
        {
            ShowTowerData(currentTower);
        }

        if (currentTower == null)
        {
            Hide();
        }
        else
        {
            sellAmount = currentTower.GetSellPrice();
            if (SellPrice != null)
            {
                SellPrice.text = $"${sellAmount}";
            }
        }
    }

    public void ShowTowerData(Tower tower)
    {
        if (tower == null) return;

        currentTower = tower;
        panel.SetActive(true);

        string displayName = tower.name.Replace("(Clone)", "").Trim();
        nameText.text = displayName;
        spriteImage.sprite = tower.GetComponentInChildren<SpriteRenderer>().sprite;

        // Setup upgrade buttons for 3 paths
        for (int path = 0; path < 3; path++)
        {
            Transform upgradeButtonGroup = upgradeButtonsParent.GetChild(path);
            Button upgradeButton = upgradeButtonGroup.Find("Button").GetComponent<Button>();
            TextMeshProUGUI buttonText = upgradeButtonGroup.Find("Button/Text (TMP)").GetComponent<TextMeshProUGUI>();
            Image upgradeIcon = upgradeButtonGroup.Find("UpgradeNumberImage").GetComponent<Image>();

            TowerUpgrade nextUpgrade = tower.GetNextUpgrade(path);

            // Setup targeting dropdown
            if (targetingDropdown != null)
            {
                targetingDropdown.ClearOptions();
                var options = System.Enum.GetNames(typeof(Tower.TargetMode));
                targetingDropdown.AddOptions(new System.Collections.Generic.List<string>(options));
                targetingDropdown.value = (int)tower.targetMode;
                targetingDropdown.onValueChanged.RemoveAllListeners();
                targetingDropdown.onValueChanged.AddListener(idx =>
                {
                    tower.targetMode = (Tower.TargetMode)idx;
                });
            }

            // Set the upgrade bar image based on the current tier
            int tier = tower.upgradeTiers[path];
            if (upgradeBarSprites != null && upgradeBarSprites.Length >= 5)
            {
                // 0: Green_1, 1: Green2, 2: Green_3, 3: Green_end, 4: Green_Full
                if (tier == 0)
                    upgradeIcon.sprite = upgradeBarSprites[0];
                else if (tier == 1)
                    upgradeIcon.sprite = upgradeBarSprites[1];
                else if (tier == 2)
                    upgradeIcon.sprite = upgradeBarSprites[2];
                else if (tier == 3)
                    upgradeIcon.sprite = upgradeBarSprites[3];
                else // Maxed
                    upgradeIcon.sprite = upgradeBarSprites[4];
            }

            // Remove previous listeners
            upgradeButton.onClick.RemoveAllListeners();

            // Remove previous EventTriggers
            EventTrigger trigger = upgradeButton.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = upgradeButton.gameObject.AddComponent<EventTrigger>();
            }
            trigger.triggers.Clear();

            if (nextUpgrade != null && tower.CanUpgrade(path))
            {
                upgradeButton.interactable = GameManager.Instance.GetMoney() >= nextUpgrade.cost;
                buttonText.text = $"{nextUpgrade.upgradeName}\n${nextUpgrade.cost}";

                // Add upgrade logic
                int capturedPath = path;
                upgradeButton.onClick.AddListener(() =>
                {
                    if (GameManager.Instance.GetMoney() >= nextUpgrade.cost)
                    {
                        GameManager.Instance.SpendMoney(nextUpgrade.cost);
                        tower.ApplyUpgrade(capturedPath);
                        ToolTip.HideToolTipStatic(); // Hide tooltip after upgrade
                        ShowTowerData(tower); // Refresh UI
                    }
                });

                // Tooltip on hover
                string tooltipText = GetUpgradeTooltip(nextUpgrade);
                // Pointer Enter
                var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                entryEnter.callback.AddListener((_) => ToolTip.ShowToolTipStatic(tooltipText));
                trigger.triggers.Add(entryEnter);

                // Pointer Exit
                var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                entryExit.callback.AddListener((_) => ToolTip.HideToolTipStatic());
                trigger.triggers.Add(entryExit);
            }
            else
            {
                upgradeButton.interactable = false;
                buttonText.text = "Maxed";

                // Remove tooltip triggers for maxed buttons
                trigger.triggers.Clear();
            }
        }
    }

    private string GetUpgradeTooltip(TowerUpgrade upgrade)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(upgrade.upgradeName);
        sb.AppendLine(upgrade.description);
        sb.AppendLine($"Cost: ${upgrade.cost}");

        // Only show stats that change
        if (upgrade.rangeBonus != 0)
            sb.AppendLine($"Range: {(upgrade.rangeBonus > 0 ? "+" : "")}{upgrade.rangeBonus}");
        if (upgrade.fireRateBonus != 0)
            sb.AppendLine($"Fire Rate: {(upgrade.fireRateBonus > 0 ? "+" : "")}{upgrade.fireRateBonus}");
        if (upgrade.damageBonus != 0)
            sb.AppendLine($"Damage: {(upgrade.damageBonus > 0 ? "+" : "")}{upgrade.damageBonus}");
        if (upgrade.projectilesPerShotBonus != 0)
            sb.AppendLine($"Projectiles Per Shot: {(upgrade.projectilesPerShotBonus > 0 ? "+" : "")}{upgrade.projectilesPerShotBonus}");
        if (upgrade.spreadAngleBonus != 0)
            sb.AppendLine($"Spread Angle: {(upgrade.spreadAngleBonus > 0 ? "+" : "")}{upgrade.spreadAngleBonus}");
        if (upgrade.projectileSizeBonus != 0)
            sb.AppendLine($"Projectile Size: {(upgrade.projectileSizeBonus > 0 ? "+" : "")}{upgrade.projectileSizeBonus}");
        if (upgrade.pierceBonus != 0)
            sb.AppendLine($"Pierce: {(upgrade.pierceBonus > 0 ? "+" : "")}{upgrade.pierceBonus}");

        if (upgrade.projectileEffects != null && upgrade.projectileEffects.Count > 0)
        {
            sb.Append("Effects: ");
            for (int i = 0; i < upgrade.projectileEffects.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(upgrade.projectileEffects[i]);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public void OnSellButtonPressed()
    {
        if (currentTower != null)
        {
            GameManager.Instance.AddMoney(sellAmount);
            Destroy(currentTower.gameObject);
            Hide();
        }
    }

    public void Hide()
    {
        panel.SetActive(false);
        currentTower = null;
    }
}