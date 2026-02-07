using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSelectorUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Image previousMapImage;
    [SerializeField] private TMPro.TextMeshProUGUI previousMapNameText;
    [SerializeField] private Image currentMapImage;
    [SerializeField] private TMPro.TextMeshProUGUI currentMapNameText;
    [SerializeField] private Image nextMapImage;
    [SerializeField] private TMPro.TextMeshProUGUI nextMapNameText;
    [SerializeField] private Button playButton;

    [System.Serializable]
    public class MapData
    {
        public string mapName;
        public Sprite previewSprite;
        public int sceneIndex;
    }

    [SerializeField] private MapData[] maps;

    private int currentIndex = 0;

    // Drag detection
    private Vector2 dragStartPos;
    private bool isDragging = false;
     private float dragThreshold = 200f; // Minimum pixels to trigger a flip

    // For visual effect
    private Vector3 prevStartPos, currStartPos, nextStartPos;
    [SerializeField] private float imageSpacing = 200f; // Will be auto-calculated

    // For carousel effect
    [SerializeField] private float dragOffset = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        prevStartPos = previousMapImage.rectTransform.localPosition;
        currStartPos = currentMapImage.rectTransform.localPosition;
        nextStartPos = nextMapImage.rectTransform.localPosition;

        // Register Play button click event
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        dragThreshold = imageSpacing;

        UpdateMapDisplay();
    }

    private void OnPlayButtonClicked()
    {
        int sceneIndex = GetSelectedMapSceneIndex();
        if (sceneIndex >= 0)
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }

    private void ShowPreviousMap(bool animate = false)
    {
        currentIndex = (currentIndex - 1 + maps.Length) % maps.Length;
        if (animate)
        {
            previousMapImage.rectTransform.localPosition = prevStartPos - Vector3.right * imageSpacing;
            currentMapImage.rectTransform.localPosition = currStartPos - Vector3.right * imageSpacing;
            nextMapImage.rectTransform.localPosition = nextStartPos - Vector3.right * imageSpacing;

            UpdateMapDisplay();

            LeanTween.moveLocal(previousMapImage.gameObject, prevStartPos, 0.2f).setEaseOutCubic();
            LeanTween.moveLocal(currentMapImage.gameObject, currStartPos, 0.2f).setEaseOutCubic();
            LeanTween.moveLocal(nextMapImage.gameObject, nextStartPos, 0.2f).setEaseOutCubic();
        }
        else
        {
            UpdateMapDisplay();
        }
    }

    private void ShowNextMap(bool animate = false)
    {
        currentIndex = (currentIndex + 1) % maps.Length;
        if (animate)
        {
            previousMapImage.rectTransform.localPosition = prevStartPos + Vector3.right * imageSpacing;
            currentMapImage.rectTransform.localPosition = currStartPos + Vector3.right * imageSpacing;
            nextMapImage.rectTransform.localPosition = nextStartPos + Vector3.right * imageSpacing;

            UpdateMapDisplay();

            LeanTween.moveLocal(previousMapImage.gameObject, prevStartPos, 0.2f).setEaseOutCubic();
            LeanTween.moveLocal(currentMapImage.gameObject, currStartPos, 0.2f).setEaseOutCubic();
            LeanTween.moveLocal(nextMapImage.gameObject, nextStartPos, 0.2f).setEaseOutCubic();
        }
        else
        {
            UpdateMapDisplay();
        }
    }

    private void UpdateMapDisplay()
    {
        if (maps.Length == 0) return;

        int prevIndex = (currentIndex - 1 + maps.Length) % maps.Length;
        int nextIndex = (currentIndex + 1) % maps.Length;

        previousMapImage.sprite = maps[prevIndex].previewSprite;
        currentMapImage.sprite = maps[currentIndex].previewSprite;
        nextMapImage.sprite = maps[nextIndex].previewSprite;

        previousMapNameText.text = maps[prevIndex].mapName;
        currentMapNameText.text = maps[currentIndex].mapName;
        nextMapNameText.text = maps[nextIndex].mapName;

        // Reset positions
        previousMapImage.rectTransform.localPosition = prevStartPos;
        currentMapImage.rectTransform.localPosition = currStartPos;
        nextMapImage.rectTransform.localPosition = nextStartPos;
    }

    public int GetSelectedMapSceneIndex()
    {
        return maps[currentIndex].sceneIndex;
    }

    public string GetSelectedMapName()
    {
        return maps[currentIndex].mapName;
    }

    // Drag handling
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        dragStartPos = eventData.position;
        dragOffset = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        float deltaX = eventData.position.x - dragStartPos.x;
        dragOffset = deltaX;

        // Carousel effect: wrap when dragOffset exceeds spacing
        if (dragOffset > imageSpacing)
        {
            dragOffset = 0f;
            currentIndex = (currentIndex - 1 + maps.Length) % maps.Length;
            UpdateMapDisplay();

            // Animate images to their positions
            LeanTween.moveLocal(previousMapImage.gameObject, prevStartPos, 0.2f).setEaseOutCubic();
            LeanTween.moveLocal(currentMapImage.gameObject, currStartPos, 0.2f).setEaseOutCubic();
            LeanTween.moveLocal(nextMapImage.gameObject, nextStartPos, 0.2f).setEaseOutCubic();

            isDragging = false; // Stop drag after snap
            return;
        }
        if (dragOffset < -imageSpacing)
        {
            dragOffset = 0f;
            currentIndex = (currentIndex + 1) % maps.Length;
            UpdateMapDisplay();

            // Animate images to their positions
            LeanTween.moveLocal(previousMapImage.gameObject, prevStartPos, 0.2f).setEaseOutCubic();
            LeanTween.moveLocal(currentMapImage.gameObject, currStartPos, 0.2f).setEaseOutCubic();
            LeanTween.moveLocal(nextMapImage.gameObject, nextStartPos, 0.2f).setEaseOutCubic();

            isDragging = false; // Stop drag after snap
            return;
        }

        // Move images horizontally with dragOffset
        previousMapImage.rectTransform.localPosition = prevStartPos + Vector3.right * dragOffset;
        currentMapImage.rectTransform.localPosition = currStartPos + Vector3.right * dragOffset;
        nextMapImage.rectTransform.localPosition = nextStartPos + Vector3.right * dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        if (Mathf.Abs(dragOffset) > dragThreshold)
        {
            if (dragOffset > 0)
            {
                // Animate to previous map position, then update display
                LeanTween.moveLocal(previousMapImage.gameObject, prevStartPos, 0.2f).setEaseOutCubic();
                LeanTween.moveLocal(currentMapImage.gameObject, currStartPos, 0.2f).setEaseOutCubic();
                LeanTween.moveLocal(nextMapImage.gameObject, nextStartPos, 0.2f).setEaseOutCubic()
                    .setOnComplete(() => ShowPreviousMap(false));
            }
            else
            {
                // Animate to next map position, then update display
                LeanTween.moveLocal(previousMapImage.gameObject, prevStartPos, 0.2f).setEaseOutCubic();
                LeanTween.moveLocal(currentMapImage.gameObject, currStartPos, 0.2f).setEaseOutCubic();
                LeanTween.moveLocal(nextMapImage.gameObject, nextStartPos, 0.2f).setEaseOutCubic()
                    .setOnComplete(() => ShowNextMap(false));
            }
        }
        else
        {
            // Animate images back to their positions
            LeanTween.moveLocal(previousMapImage.gameObject, prevStartPos, 0.2f).setEaseOutCubic();
            LeanTween.moveLocal(currentMapImage.gameObject, currStartPos, 0.2f).setEaseOutCubic();
            LeanTween.moveLocal(nextMapImage.gameObject, nextStartPos, 0.2f).setEaseOutCubic();
        }
        dragOffset = 0f;
    }
}