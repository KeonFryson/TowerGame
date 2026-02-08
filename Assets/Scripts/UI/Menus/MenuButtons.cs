
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuButtons : MonoBehaviour
{
    [SerializeField] private Button PlayButton;
    [SerializeField] private Button QuitButton;

    private void Awake()
    {
        if (PlayButton != null)
        {
            PlayButton.onClick.AddListener(PlayGame);
        }
        if (QuitButton != null)
        {
            QuitButton.onClick.AddListener(QuitGame);
        }
    }

    public void PlayGame()
    {
        StartCoroutine(PlayGameRoutine());
    }

    private IEnumerator PlayGameRoutine()
    {
        Debug.Log("Play pressed");
        Debug.Log("AudioManager: " + AudioManager.Instance);
        Debug.Log("GameManager: " + GameManager.Instance);

        AudioManager.Instance.PlayButtonClick();
        yield return new WaitForSeconds(0.3f);

        Debug.Log("Trying to load scene 1...");
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
        StartCoroutine(DelayedAction(0.3f, () => GameManager.Instance.OpenMainMenu()));
    }

    public void ResumeGame()
    {
        StartCoroutine(DelayedAction(0.3f, () => GameManager.Instance.ResumeGame()));
    }

    private IEnumerator DelayedAction(float delay, System.Action action)
    {
        AudioManager.Instance.PlayButtonClick();
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }



}
