using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject shopHighlight;
    [SerializeField] private GameObject towerPlacementHighlight;
    [SerializeField] private GameObject upgradeHighlight;
    [SerializeField] private TMPro.TextMeshProUGUI instructionText;
  
    [SerializeField] private TowerPlacement towerPlacement;
    [SerializeField] private TowerDataPanelUI towerDataPanelUI;
    [SerializeField] private Image ArrowShopPointer;
    [SerializeField] private Image ArrowUpgradePointer;

    private int step = 0;

    private void Awake()
    {
       
      
        if (towerPlacement != null)
        {
           // towerPlacement = FindFirstObjectByType<TowerPlacement>();
            towerPlacement.OnTowerPlace += OnTowerPlaced;
        }
        if (towerDataPanelUI != null)
        {
           // towerDataPanelUI = FindFirstObjectByType<TowerDataPanelUI>();
            towerDataPanelUI.OnUpgrade += NextStep;
        }
    }


    private void Start()
    {
        if (towerDataPanelUI != null)
        {
            // towerDataPanelUI = FindFirstObjectByType<TowerDataPanelUI>();
            towerDataPanelUI.OnUpgrade += NextStep;
        }
         
        ShowStep(0);
    }

    private void ShowStep(int stepIndex)
    {
        //shopHighlight.SetActive(stepIndex == 0);
        
        towerPlacementHighlight.SetActive(stepIndex >= 1);
        ArrowShopPointer.gameObject.SetActive(stepIndex == 1);
        ArrowUpgradePointer.gameObject.SetActive(stepIndex == 2);
        // upgradeHighlight.SetActive(stepIndex == 2);

        switch (stepIndex)
        {
            case 0:
                instructionText.text = "Step 1: Open the shop by clicking the Tab button.";
                 
                break;
            case 1:
                instructionText.text = "Step 2: Place a tower by selecting it from the shop and clicking on the map.";
                
                break;
            case 2:
                instructionText.text = "Step 3: Upgrade your tower by selecting it and clicking the upgrade button.";
               
                
                break;
            default:
                instructionText.text = "Tutorial Complete! Start playing.";
                StartCoroutine(WaitAndLoadNextScene(3f)); // Wait 3 seconds before loading the next scene

                break;
        }
    }


   private void LoadNextScene()
    {
        SceneManager.LoadScene(1);

    }

    private IEnumerator WaitAndLoadNextScene(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadNextScene();
    }

    public void OnShopOpened()
    {
        if (step == 0) // Assuming step 0 is "Open Shop"
        {
            NextStep();
        }
    }

    void OnTowerPlaced()
    {
        if (step == 1) // Assuming step 1 is "Place Tower"
        {
            NextStep();
        }
    }

    private void NextStep()
    {
        step++;
        ShowStep(step);
    }
}