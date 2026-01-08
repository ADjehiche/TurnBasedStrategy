using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles screen blinking effect (like eyes closing and opening)
/// Useful for wake-up sequences or transitions
/// </summary>
public class ScreenBlinker : MonoBehaviour
{
    public static ScreenBlinker Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private Image blinkOverlay; // Full-screen black image
    [SerializeField] private Canvas blinkCanvas; // Canvas for the overlay
    
    [Header("Settings")]
    [SerializeField] private Color blinkColor = Color.black;
    [SerializeField] private float defaultBlinkDuration = 0.3f;
    [SerializeField] private AnimationCurve blinkCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Setup blink overlay if not assigned
        if (blinkOverlay == null)
        {
            SetupBlinkOverlay();
        }
        
        // Start with overlay invisible
        if (blinkOverlay != null)
        {
            Color c = blinkColor;
            c.a = 0f;
            blinkOverlay.color = c;
        }
    }
    
    /// <summary>
    /// Automatically create the blink overlay if not assigned
    /// </summary>
    private void SetupBlinkOverlay()
    {
        // Find or create canvas
        if (blinkCanvas == null)
        {
            GameObject canvasObj = new GameObject("BlinkCanvas");
            blinkCanvas = canvasObj.AddComponent<Canvas>();
            blinkCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            blinkCanvas.sortingOrder = 9999; // Very high to be on top of everything
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // No GraphicRaycaster needed - this canvas doesn't receive input
            
            canvasObj.transform.SetParent(transform);
        }
        
        // Create overlay image
        GameObject overlayObj = new GameObject("BlinkOverlay");
        overlayObj.transform.SetParent(blinkCanvas.transform, false);
        
        blinkOverlay = overlayObj.AddComponent<Image>();
        blinkOverlay.color = blinkColor;
        blinkOverlay.raycastTarget = false; // Don't block clicks when invisible
        
        // Make it fullscreen
        RectTransform rt = blinkOverlay.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        
        if (showDebugLogs)
            Debug.Log("[ScreenBlinker] Blink overlay created automatically");
    }
    
    /// <summary>
    /// Perform a single blink (fade to black and back)
    /// </summary>
    public IEnumerator Blink(float duration = -1f)
    {
        if (duration < 0)
            duration = defaultBlinkDuration;
        
        if (blinkOverlay == null)
        {
            Debug.LogWarning("[ScreenBlinker] No blink overlay assigned!");
            yield break;
        }
        
        if (showDebugLogs)
            Debug.Log($"[ScreenBlinker] Single blink - Duration: {duration}s");
        
        float halfDuration = duration / 2f;
        
        // Fade to black (eyes closing)
        yield return StartCoroutine(FadeOverlay(0f, 1f, halfDuration));
        
        // Fade to transparent (eyes opening)
        yield return StartCoroutine(FadeOverlay(1f, 0f, halfDuration));
    }
    
    /// <summary>
    /// Perform multiple blinks in sequence
    /// </summary>
    public IEnumerator BlinkMultiple(int count = 2, float blinkDuration = -1f, float pauseBetween = 0.2f)
    {
        if (blinkDuration < 0)
            blinkDuration = defaultBlinkDuration;
        
        if (showDebugLogs)
            Debug.Log($"[ScreenBlinker] Multiple blinks - Count: {count}, Duration: {blinkDuration}s");
        
        for (int i = 0; i < count; i++)
        {
            yield return StartCoroutine(Blink(blinkDuration));
            
            // Pause between blinks (except after the last one)
            if (i < count - 1)
            {
                yield return new WaitForSeconds(pauseBetween);
            }
        }
        
        if (showDebugLogs)
            Debug.Log("[ScreenBlinker] Multiple blinks complete");
    }
    
    /// <summary>
    /// Fade the overlay between two alpha values
    /// </summary>
    private IEnumerator FadeOverlay(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float curveValue = blinkCurve.Evaluate(progress);
            float currentAlpha = Mathf.Lerp(fromAlpha, toAlpha, curveValue);
            
            Color c = blinkColor;
            c.a = currentAlpha;
            blinkOverlay.color = c;
            
            yield return null;
        }
        
        // Ensure we end exactly at target alpha
        Color finalColor = blinkColor;
        finalColor.a = toAlpha;
        blinkOverlay.color = finalColor;
    }
}
