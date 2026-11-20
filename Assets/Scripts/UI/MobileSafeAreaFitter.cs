using UnityEngine;

/// <summary>Fits a UI panel inside Android display cutouts and gesture/navigation safe areas.</summary>
[RequireComponent(typeof(RectTransform))]
public sealed class MobileSafeAreaFitter : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea;
    private int lastWidth;
    private int lastHeight;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void Update()
    {
        if (lastSafeArea != Screen.safeArea || lastWidth != Screen.width || lastHeight != Screen.height)
            ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        Rect safe = Screen.safeArea;
        lastSafeArea = safe;
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        if (Screen.width <= 0 || Screen.height <= 0) return;
        rectTransform.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
        rectTransform.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
