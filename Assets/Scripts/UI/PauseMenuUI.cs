using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject BackGround;
    public Button MenuButton;
    public Button ResumeButton;
    public Button RestartButton;


    void Start()
    {
         pauseMenu.SetActive(false);
        BackGround.SetActive(false);
        if (MenuButton != null)
            MenuButton.onClick.AddListener(() => { ReturnToMainMenu(); });
        else
            Debug.LogWarning("MenuButton is not assigned in the inspector.");

        if (ResumeButton != null)
            ResumeButton.onClick.AddListener(() => { ResumeGame(); });
        else
            Debug.LogWarning("ResumeButton is not assigned in the inspector.");
        if (RestartButton != null)
        RestartButton.onClick.AddListener(() => { RestartLevel(); });
        else
            Debug.LogWarning("RestartButton is not assigned in the inspector.");

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

    private IEnumerator DelayedTogglePauseMenu(float delay)
    {
 
        AudioManager.Instance.PlayButtonClick();
        yield return new WaitForSecondsRealtime(delay);
        TogglePauseMenu();
    }

    public void ResumeGame()
    {
       StartCoroutine(DelayedTogglePauseMenu(0.3f));
    }

    public void RestartLevel()
    {
        StartCoroutine(DelayedAction(0.3f, () => GameManager.Instance.ResetMap()));
    }

    public void ReturnToMainMenu()
    {
        StartCoroutine(DelayedAction(0.3f, () => GameManager.Instance.OpenMainMenu()));
    }


    private IEnumerator DelayedAction(float delay, System.Action action)
    {
        
        AudioManager.Instance.PlayButtonClick();
        yield return new WaitForSecondsRealtime(delay);
        action?.Invoke();
    }
}
