using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject BackGround;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    
    void Start()
    {
         pauseMenu.SetActive(false);
        BackGround.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TogglePauseMenu()
    {
        bool isActive = !pauseMenu.activeSelf;
        OpenTowerShop(isActive);
        if (isActive)
        {
            GameManager.Instance.PauseGame(); // Pause the game
        }
        else
        {
            GameManager.Instance.ResumeGame(); // Resume the game
        }
    }

    
    private void OpenTowerShop(bool isOpen)
    {
        pauseMenu.SetActive(isOpen);
        BackGround.SetActive(isOpen);
    }
}
