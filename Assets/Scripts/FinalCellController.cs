using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Handles the Final_Cell scene ending when player loses to the final boss.
/// Uses CaptionManager to show defeat captions, then "Thanks for playing" with return to title button.
/// Similar pattern to GameEndingTrigger and FlashbackSceneController.
/// </summary>
public class FinalCellController : MonoBehaviour
{
    [Header("Caption Dialogue")]
    [SerializeField] private string[] defeatLines = new string[]
    {
        "The Warden was too powerful...",
        "You have been locked away once more."
    };
    
    [Header("Timing")]
    [SerializeField] private float initialDelay = 2f;
    [SerializeField] private float lineDuration = 4f;
    [SerializeField] private float pauseBetweenLines = 1.5f;
    [SerializeField] private float thanksDelay = 2f;
    
    [Header("UI Elements")]
    [SerializeField] private Canvas endingCanvas;
    [SerializeField] private Image screenOverlay;
    [SerializeField] private TextMeshProUGUI thanksText;
    
    [Header("Colors")]
    [SerializeField] private Color overlayColor = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark gray
    [SerializeField] private float fadeInDuration = 2f;
    
    [Header("Text")]
    [SerializeField] private string thanksMessage = "Thanks for playing";
    
    [Header("Return Button")]
    [SerializeField] private Button returnToTitleButton;
    [SerializeField] private string titleSceneName = "TitleScene";
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    void Start()
    {
        EnsureEventSystem();
        EnsureEndingCanvasInteractive();

        // Lock player movement during ending sequence
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.LockMovement("Final Cell ending sequence");
        }
        
        // Hide ending UI initially (button will show at the end)
        if (endingCanvas != null)
        {
            endingCanvas.gameObject.SetActive(false);
        }
        
        // Only play ending if player actually lost to the boss
        if (GameSession.LostToFinalBoss)
        {
            if (debugMode) Debug.Log("[FinalCellController] Player lost to final boss - playing ending sequence");
            StartCoroutine(PlayDefeatEnding());
        }
        else
        {
            // Shouldn't happen, but as a fallback show the ending anyway
            if (debugMode) Debug.LogWarning("[FinalCellController] Scene loaded but LostToFinalBoss flag not set - playing anyway");
            StartCoroutine(PlayDefeatEnding());
        }
    }
    
    private IEnumerator PlayDefeatEnding()
    {
        if (debugMode) Debug.Log("[FinalCellController] Starting defeat ending sequence");
        
        // Initial pause to let scene settle and player orient themselves
        yield return new WaitForSeconds(initialDelay);
        
        // Play each defeat caption line using CaptionManager
        for (int i = 0; i < defeatLines.Length; i++)
        {
            string line = defeatLines[i];
            
            if (debugMode) Debug.Log($"[FinalCellController] Showing line {i + 1}/{defeatLines.Length}: {line}");
            
            // Show line via CaptionManager (using monologue style for narrative)
            if (CaptionManager.Instance != null)
            {
                // First line uses player perspective, second is narration
                if (i == 0)
                {
                    CaptionManager.Instance.ShowMonologue($"[You] {line}", lineDuration);
                }
                else
                {
                    CaptionManager.Instance.ShowMonologue(line, lineDuration);
                }
            }
            else
            {
                Debug.LogWarning("[FinalCellController] CaptionManager not found!");
            }
            
            // Wait for line duration + pause
            yield return new WaitForSeconds(lineDuration + pauseBetweenLines);
        }
        
        // Additional pause before showing overlay
        yield return new WaitForSeconds(thanksDelay);
        
        // Show overlay with thanks text and button
        if (endingCanvas != null)
        {
            endingCanvas.gameObject.SetActive(true);
            
            // Fade in overlay if available
            if (screenOverlay != null)
            {
                yield return StartCoroutine(FadeInOverlay());
            }
            
            // Show thanks text on overlay if available
            if (thanksText != null)
            {
                yield return StartCoroutine(FadeInThanksText());
            }
        }
        
        if (debugMode) Debug.Log("[FinalCellController] ✅ Defeat ending complete!");
        
        // Show return button and unlock cursor
        ShowReturnButton();
    }
    
    private IEnumerator FadeInOverlay()
    {
        screenOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
        
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / fadeInDuration;
            screenOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, alpha);
            yield return null;
        }
        
        screenOverlay.color = overlayColor;
    }
    
    private IEnumerator FadeInThanksText()
    {
        thanksText.text = thanksMessage;
        Color textColor = Color.white;
        thanksText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
        
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / 1f;
            thanksText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
            yield return null;
        }
        
        thanksText.color = textColor;
    }
    
    private void ShowReturnButton()
    {
        // Pause the game (like pause menu does)
        Time.timeScale = 0f;
        
        // CRITICAL: Unlock cursor so player can click the button (like pause menu)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EnsureEventSystem();
        EnsureEndingCanvasInteractive();

        // Prevent full-screen graphics from eating clicks meant for the button
        if (screenOverlay != null) screenOverlay.raycastTarget = false;
        if (thanksText != null) thanksText.raycastTarget = false;
        
        if (returnToTitleButton != null)
        {
            returnToTitleButton.gameObject.SetActive(true);
            returnToTitleButton.interactable = true;
            returnToTitleButton.onClick.RemoveAllListeners();
            returnToTitleButton.onClick.AddListener(ReturnToTitle);
            returnToTitleButton.Select();
            
            if (debugMode) Debug.Log("[FinalCellController] Return button shown");
        }
        else
        {
            if (debugMode) Debug.LogWarning("[FinalCellController] Return to title button not assigned!");
        }
        
        if (debugMode) Debug.Log("[FinalCellController] Cursor unlocked and visible");
    }
    
    /// <summary>
    /// Called when Return to Title button is clicked
    /// </summary>
    public void ReturnToTitle()
    {
        Debug.Log("[FinalCellController] ReturnToTitle clicked");

        // Unpause the game before leaving
        Time.timeScale = 1f;
        
        // Reset game state so player can start fresh
        GameSession.Reset();
        GameSession.LostToFinalBoss = false; // Clear the loss flag
        
        if (debugMode) Debug.Log($"[FinalCellController] Returning to title: {titleSceneName}");

        if (!Application.CanStreamedLevelBeLoaded(titleSceneName))
        {
            Debug.LogError($"[FinalCellController] Cannot load scene '{titleSceneName}'. Is it added to Build Settings?");
            // Fallback: try build index 0 (commonly the title scene)
            SceneManager.LoadScene(0);
            return;
        }

        SceneManager.LoadScene(titleSceneName);
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        var eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<EventSystem>();

        // Prefer the Input System UI module if present, otherwise fall back to StandaloneInputModule.
        var inputSystemUiType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputSystemUiType != null)
        {
            eventSystemGo.AddComponent(inputSystemUiType);
        }
        else
        {
            eventSystemGo.AddComponent<StandaloneInputModule>();
        }
    }

    private void EnsureEndingCanvasInteractive()
    {
        if (endingCanvas == null) return;

        // This scene's ending canvas was set to World Space and had no GraphicRaycaster,
        // which makes buttons appear but not receive pointer clicks.
        if (endingCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            endingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            endingCanvas.worldCamera = null;
        }

        if (endingCanvas.GetComponent<GraphicRaycaster>() == null)
        {
            endingCanvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }
    
    [ContextMenu("Test: Play Defeat Ending")]
    public void TestDefeatEnding()
    {
        if (Application.isPlaying)
        {
            GameSession.LostToFinalBoss = true;
            StartCoroutine(PlayDefeatEnding());
        }
    }
}
