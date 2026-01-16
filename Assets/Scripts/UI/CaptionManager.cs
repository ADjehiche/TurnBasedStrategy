using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;

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
<<<<<<< Updated upstream
    [SerializeField] private Color flashbackColor = new Color(1f, 0.3f, 0.3f, 1f); // Red for flashbacks
    
=======

    [Header("Logging (Option A)")]
    [SerializeField] private bool enableCaptionLogging = true;

>>>>>>> Stashed changes
    // Singleton pattern for easy access
    public static CaptionManager Instance { get; private set; }

    // ✅ IMPORTANT: This event notifies CaptionAudioPlayer every time a caption is shown
    public static event System.Action<string, CaptionType> OnCaptionShown;

    private Coroutine currentCaptionCoroutine;

    // CSV path (runtime output)
    private static string CaptionsPath =>
        Path.Combine(Application.persistentDataPath, "captions_log.csv");

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

        // Helpful: show where Unity is saving the file
        if (enableCaptionLogging)
        {
            Debug.Log("[CaptionManager] Caption log path: " + CaptionsPath);
        }
    }

    /// <summary>
    /// Show a caption with specified text, color, and duration
    /// </summary>
    public void ShowCaption(string text, CaptionType type = CaptionType.Instruction, float? customDuration = null)
    {
        // ✅ Notify audio system (CaptionAudioPlayer listens to this)
        OnCaptionShown?.Invoke(text, type);

        // LOG EVERY LINE THAT APPEARS
        if (enableCaptionLogging)
        {
            LogCaptionLine(text, type);
        }

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
<<<<<<< Updated upstream
    
    /// <summary>
    /// Show a flashback caption (red color for memory sequences)
    /// </summary>
    public void ShowFlashback(string text, float? duration = null)
    {
        ShowCaption(text, CaptionType.Flashback, duration);
    }
    
=======

>>>>>>> Stashed changes
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

    // =========================
    // CAPTION LOGGING (CSV)
    // =========================
    private void LogCaptionLine(string rawText, CaptionType type)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return;

        string scene = SceneManager.GetActiveScene().name;

        // Default speaker based on type
        string speaker = type == CaptionType.System ? "System" : "Unknown";
        string text = rawText;

        // If formatted like: "[You] blah blah" -> parse it
        if (rawText.StartsWith("[") && rawText.Contains("]"))
        {
            int end = rawText.IndexOf("]");
            if (end > 1)
            {
                speaker = rawText.Substring(1, end - 1).Trim();
                text = rawText.Substring(end + 1).Trim();
            }
        }
        else
        {
            // fallback speaker from type
            if (type == CaptionType.Monologue) speaker = "You/Fragment";
            if (type == CaptionType.Instruction) speaker = "Instruction";
            if (type == CaptionType.System) speaker = "System";
        }

        // CSV-safe quoting
        string safeScene = scene.Replace("\"", "\"\"");
        string safeSpeaker = speaker.Replace("\"", "\"\"");
        string safeText = text.Replace("\"", "\"\"");

        string csvLine =
            $"\"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}\",\"{safeScene}\",\"{type}\",\"{safeSpeaker}\",\"{safeText}\"";

        try
        {
            // Write header if file doesn't exist
            if (!File.Exists(CaptionsPath))
            {
                File.AppendAllText(CaptionsPath, "\"time\",\"scene\",\"caption_type\",\"speaker\",\"text\"\n");
            }

            File.AppendAllText(CaptionsPath, csvLine + "\n");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[CaptionManager] Failed to write caption log: " + e.Message);
        }
    }
}

public enum CaptionType
{
    Instruction,
    Monologue,
<<<<<<< Updated upstream
    System,
    Flashback
}
=======
    System
}
>>>>>>> Stashed changes
