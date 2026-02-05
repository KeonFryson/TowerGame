using UnityEngine;
 

public class WInLoseUI : MonoBehaviour
{
    public GameObject WinLoseMenu;
    public GameObject BackGround;
    public TMPro.TMP_Text WinLoseText;

    void Start()
    {
        WinLoseMenu.SetActive(false);
        BackGround.SetActive(false);
    }

    // Call this to show the win message
    public void ShowWin()
    {
        WinLoseText.text = "You Win!";
        WinLoseMenu.SetActive(true);
        BackGround.SetActive(true);
    }

    // Call this to show the lose message
    public void ShowLose()
    {
        WinLoseText.text = "Game Over";
        WinLoseMenu.SetActive(true);
        BackGround.SetActive(true);
    }


}
