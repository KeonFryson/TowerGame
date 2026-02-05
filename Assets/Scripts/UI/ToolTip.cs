using UnityEngine;
using UnityEngine.InputSystem; // Add this for the new Input System

public class ToolTip : MonoBehaviour
{

    private static ToolTip instance;

    private TMPro.TextMeshProUGUI tooltipText;
    private RectTransform backgroundRectTransform;
    private RectTransform canvasRectTransform;

    private void Awake()
    {
        instance = this;

        tooltipText = transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
        backgroundRectTransform = transform.Find("Background").GetComponent<RectTransform>();
        canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
       HideToolTip();
    }

    private void Update()
    {
        Vector2 mouseScreenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

        Vector2 anchoredPos;
        // Convert mouse position to local point in canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            mouseScreenPos,
            null,
            out anchoredPos);
 
        transform.localPosition = anchoredPos;

        Vector2 anchoredPosition = transform.GetComponent<RectTransform>().anchoredPosition;
        if(anchoredPosition.x + backgroundRectTransform.rect.width > canvasRectTransform.rect.width)
        {
            anchoredPosition.x = canvasRectTransform.rect.width - backgroundRectTransform.rect.width;
        }
        transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
    }

    private void ShowToolTip(string tooltipString)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        tooltipText.SetText(tooltipString);
        float textPadding = 4f;
        Vector2 backgroundSize = new Vector2(tooltipText.preferredWidth + textPadding * 2f, tooltipText.preferredHeight + textPadding * 2f);
        backgroundRectTransform.sizeDelta = backgroundSize;
    }

    private void HideToolTip()
    {
        gameObject.SetActive(false);
    }


    public static void ShowToolTipStatic(string tooltipString)
    {
        instance.ShowToolTip(tooltipString);
    }

    public static void HideToolTipStatic()
    {
       instance.HideToolTip();
    }
}