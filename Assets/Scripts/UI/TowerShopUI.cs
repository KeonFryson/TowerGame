using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerShopUI : MonoBehaviour
{
    [System.Serializable]
    public class TowerButton
    {
        public GameObject towerPrefab;
        // Removed iconSprite and towerName fields
    }

    [Header("Tower Buttons")]
    [SerializeField] private GameObject TowerShopPanel;
    [SerializeField] private TowerButton[] towerButtons;
    [SerializeField] private TowerPlacement towerPlacement;
    [Header("Button Template")]
    [SerializeField] private GameObject buttonTemplate; // Assign your TowerButton prefab in the inspector
    [SerializeField] private Transform buttonParent;    // Assign the parent container for buttons

    private void Start()
    {
        if (towerPlacement == null)
        {
            towerPlacement = FindFirstObjectByType<TowerPlacement>();
        }

        SetupButtons();
        TowerShopPanel.SetActive(false);
    }

    private void SetupButtons()
    {
        // Remove old buttons if any
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        foreach (TowerButton towerButton in towerButtons)
        {
            if (towerButton.towerPrefab != null)
            {
                GameObject buttonObj = Instantiate(buttonTemplate, buttonParent);

                // Find components in the template
                Image icon = buttonObj.transform.Find("Icon").GetComponent<Image>();
                TextMeshProUGUI nameText = buttonObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
                Button buyButton = buttonObj.transform.Find("Button").GetComponent<Button>();
                TextMeshProUGUI costText = buttonObj.transform.Find("Button/BuyButton").GetComponent<TextMeshProUGUI>();

               

                Tower tower = towerButton.towerPrefab.GetComponent<Tower>();
                int cost = tower != null ? tower.GetCost() : 0;

                // Use towerPrefab's name for towerName
                string towerName = towerButton.towerPrefab.name;

                // Get sprite from towerPrefab
                Sprite sprite = null;
                SpriteRenderer sr = towerButton.towerPrefab.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sprite = sr.sprite;
                }
                else
                {
                    Image img = towerButton.towerPrefab.GetComponent<Image>();
                    if (img != null)
                    {
                        sprite = img.sprite;
                    }
                }

                if (icon != null && sprite != null)
                {
                    icon.sprite = sprite;
                }
                if (nameText != null)
                {
                    nameText.text = towerName;
                }
                if (costText != null)
                {
                    costText.text = "$" + cost;
                }

                GameObject prefab = towerButton.towerPrefab;
                buyButton.onClick.AddListener(() => OnTowerButtonClicked(prefab));

            }
        }
    }

    private void OnTowerButtonClicked(GameObject towerPrefab)
    {
        if (towerPlacement != null)
        {
            towerPlacement.StartPlacingTower(towerPrefab);
        }
    }

    public void ToggleShop()
    {
        bool isActive = !TowerShopPanel.activeSelf;
        OpenTowerShop(isActive);
    }

    private void OpenTowerShop(bool isOpen)
    {
        TowerShopPanel.SetActive(isOpen);
    }
}