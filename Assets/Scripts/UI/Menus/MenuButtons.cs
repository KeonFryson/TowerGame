
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuButtons : MonoBehaviour
{
     

    private void Awake()
    {
       
        
       
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OpenMainMenu()
    {
       GameManager.Instance.OpenMainMenu();
    }

    public void ResumeGame()
    {
        GameManager.Instance.ResumeGame();
    }


}
