using UnityEngine;

/// <summary>
/// Handles UI and game audio feedback for button clicks, disabled button attempts,
/// tower selection, and enemy death. Attach this to a persistent UI GameObject (e.g., Canvas).
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("UI Audio Clips")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonDisabledSound;

    [Header("Game Audio Clips")]
    [SerializeField] private AudioClip towerSelectedSound;
    [SerializeField] private AudioClip enemyDeathSound;
    [SerializeField] private AudioClip towerUpgradeSound;
    [SerializeField] private AudioClip towerPlaceSound;

    private AudioSource audioSource;

    private static AudioManager instance;

    /// <summary>
    /// Singleton instance for global access.
    /// </summary>
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("UIAudio");
                    instance = go.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
       

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
         
    }

    /// <summary>
    /// Play the button click sound.
    /// </summary>
    public void PlayButtonClick()
    {
        if (buttonClickSound != null)
            audioSource.PlayOneShot(buttonClickSound);
    }

    /// <summary>
    /// Play the Tower Upgrade sound.
    /// </summary>
    public void PlayTowerUpgrade()
    {
        if (towerUpgradeSound != null)
            audioSource.PlayOneShot(towerUpgradeSound);
    }
    /// <summary>
    /// Play the Tower lace sound.
    /// </summary>
    public void PlayTowerPlace()
    {
        if (towerPlaceSound != null)
            audioSource.PlayOneShot(towerPlaceSound);
    }

    /// <summary>
    /// Play the disabled button sound.
    /// </summary>
    public void PlayButtonDisabled()
    {
        if (buttonDisabledSound != null)
            audioSource.PlayOneShot(buttonDisabledSound);
    }

    /// <summary>
    /// Play the tower selected sound.
    /// </summary>
    public void PlayTowerSelected()
    {
        if (towerSelectedSound != null)
            audioSource.PlayOneShot(towerSelectedSound);
    }

    /// <summary>
    /// Play the enemy death sound.
    /// </summary>
    public void PlayEnemyDeath()
    {
        if (enemyDeathSound != null)
            audioSource.PlayOneShot(enemyDeathSound);
    }

    /// <summary>
    /// Set the button click sound at runtime.
    /// </summary>
    public void SetButtonClickSound(AudioClip clip)
    {
        buttonClickSound = clip;
    }

    /// <summary>
    /// Set the disabled button sound at runtime.
    /// </summary>
    public void SetButtonDisabledSound(AudioClip clip)
    {
        buttonDisabledSound = clip;
    }

    /// <summary>
    /// Set the tower selected sound at runtime.
    /// </summary>
    public void SetTowerSelectedSound(AudioClip clip)
    {
        towerSelectedSound = clip;
    }

    /// <summary>
    /// Set the enemy death sound at runtime.
    /// </summary>
    public void SetEnemyDeathSound(AudioClip clip)
    {
        enemyDeathSound = clip;
    }
}