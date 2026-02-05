using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class PlayerButtonInput : MonoBehaviour
{


    private InputSystem_Actions inputActions;
    private TowerShopUI towerShopUI;
    private PauseMenuUI pauseMenuUI;



    void Awake()
    {
        inputActions = new InputSystem_Actions();
        towerShopUI = FindFirstObjectByType<TowerShopUI>();
        pauseMenuUI = FindFirstObjectByType<PauseMenuUI>(); // Correctly assign pauseMenuUI

        inputActions.UI.TowerShop.performed += ctx => towerShopUI.ToggleShop();
        inputActions.UI.PauseMenu.performed += ctx => pauseMenuUI.TogglePauseMenu();
    }
    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
         

    }



    void Update()
    {



    }
 


}
