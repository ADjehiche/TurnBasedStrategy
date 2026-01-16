using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CaptionManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject captionPanel;
    [SerializeField] private TMP_Text captionText;
    [SerializeField] private Image backgroundImage;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Caption Styles")]
    [SerializeField] private Color instructionColor = Color.white;
    [SerializeField] private Color monologueColor = Color.yellow;
    [SerializeField] private Color systemColor = Color.cyan;
    [SerializeField] private Color flashbackColor = new Color(1f, 0.3f, 0.3f, 1f); // Red for flashbacks
    
    // Singleton pattern for easy access
    public static CaptionManager Instance { get; private set; }
    
    private Coroutine currentCaptionCoroutine;
    
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
        
        // Initially hide the caption panel
        if (captionPanel != null)
        {
            captionPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Show a caption with specified text, color, and duration
    /// </summary>
    public void ShowCaption(string text, CaptionType type = CaptionType.Instruction, float? customDuration = null)
    {
        // Stop any current caption
        if (currentCaptionCoroutine != null)
        {
            StopCoroutine(currentCaptionCoroutine);
        }
        
        // Start new caption
        currentCaptionCoroutine = StartCoroutine(DisplayCaptionCoroutine(text, type, customDuration ?? displayDuration));
    }
    
    /// <summary>
    /// Show an instruction caption (like "Escape the cell")
    /// </summary>
    public void ShowInstruction(string text, float? duration = null)
    {
        ShowCaption(text, CaptionType.Instruction, duration);
    }
    
    /// <summary>
    /// Show an internal monologue caption (like "I wonder if this key would work on the door")
    /// </summary>
    public void ShowMonologue(string text, float? duration = null)
    {
        ShowCaption(text, CaptionType.Monologue, duration);
    }
    
    /// <summary>
    /// Show a system message (like "Key picked up")
    /// </summary>
    public void ShowSystemMessage(string text, float? duration = null)
    {
        ShowCaption(text, CaptionType.System, duration);
    }
    
    /// <summary>
    /// Show a flashback caption (red color for memory sequences)
    /// </summary>
    public void ShowFlashback(string text, float? duration = null)
    {
        ShowCaption(text, CaptionType.Flashback, duration);
    }
    
    private IEnumerator DisplayCaptionCoroutine(string text, CaptionType type, float duration)
    {
        if (captionPanel == null || captionText == null)
        {
            Debug.LogWarning("CaptionManager: Missing UI components!");
            yield break;
        }
        
        // Set up the caption
        captionText.text = text;
        captionText.color = GetColorForType(type);
        captionPanel.SetActive(true);
        
        // Fade in
        yield return StartCoroutine(AnimateAlpha(0f, 1f, fadeInDuration));
        
        // Display
        yield return new WaitForSeconds(duration);
        
        // Fade out
        yield return StartCoroutine(AnimateAlpha(1f, 0f, fadeOutDuration));
        
        // Hide panel
        captionPanel.SetActive(false);
        currentCaptionCoroutine = null;
    }
    
    private IEnumerator AnimateAlpha(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float curveValue = fadeCurve.Evaluate(progress);
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, curveValue);
            
            // Apply alpha to text
            Color textColor = captionText.color;
            textColor.a = alpha;
            captionText.color = textColor;
            
            // Apply alpha to background if it exists
            if (backgroundImage != null)
            {
                Color bgColor = backgroundImage.color;
                bgColor.a = alpha * 0.7f; // Slightly transparent background
                backgroundImage.color = bgColor;
            }
            
            yield return null;
        }
        
        // Ensure final alpha is set
        Color finalTextColor = captionText.color;
        finalTextColor.a = toAlpha;
        captionText.color = finalTextColor;
        
        if (backgroundImage != null)
        {
            Color finalBgColor = backgroundImage.color;
            finalBgColor.a = toAlpha * 0.7f;
            backgroundImage.color = finalBgColor;
        }
    }
    
    private Color GetColorForType(CaptionType type)
    {
        switch (type)
        {
            case CaptionType.Instruction:
                return instructionColor;
            case CaptionType.Monologue:
                return monologueColor;
            case CaptionType.System:
                return systemColor;
            case CaptionType.Flashback:
                return flashbackColor;
            default:
                return instructionColor;
        }
    }
    
    /// <summary>
    /// Hide any currently displayed caption immediately
    /// </summary>
    public void HideCaption()
    {
        if (currentCaptionCoroutine != null)
        {
            StopCoroutine(currentCaptionCoroutine);
            currentCaptionCoroutine = null;
        }
        
        if (captionPanel != null)
        {
            captionPanel.SetActive(false);
        }
    }
}

public enum CaptionType
{
    Instruction,
    Monologue,
    System,
    Flashback
}