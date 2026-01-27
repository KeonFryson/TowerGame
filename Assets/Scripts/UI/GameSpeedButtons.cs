using UnityEngine;
using UnityEngine.UI;

public class GameSpeedButtons : MonoBehaviour
{
    [SerializeField] private Button speed1xButton;
    [SerializeField] private Button speed2xButton;
    [SerializeField] private Button speed3xButton;

    private void Start()
    {
        speed1xButton.onClick.AddListener(() => SetSpeed(0));
        speed2xButton.onClick.AddListener(() => SetSpeed(1));
        speed3xButton.onClick.AddListener(() => SetSpeed(2));
    }

    private void SetSpeed(int index)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameSpeed(index);
        }
    }
}