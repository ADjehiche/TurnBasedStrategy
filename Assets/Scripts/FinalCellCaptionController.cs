using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// Caption controller for Final_Cell scene - handles defeat ending when player loses to the final boss
/// Shows defeat captions, then "Thanks for playing" with return to title button
/// </summary>
public class FinalCellCaptionController : MonoBehaviour
{
    [Header("Caption Messages")]
    [SerializeField] private string defeatMessage1 = "[You] The Warden was too powerful...";
    [SerializeField] private string defeatMessage2 = "You have been locked away once more.";
    [SerializeField] private string thanksMessage = "Thanks for playing";
    
    [Header("Timing")]
    [SerializeField] private float startDelay = 2f;
    [SerializeField] private float messageDuration = 4f;
    [SerializeField] private float pauseBetweenMessages = 1.5f;
    [SerializeField] private float thanksDelay = 2f;
    
    [Header("Return Button UI")]
    [SerializeField] private Canvas endingCanvas;
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
            PlayerMovementLock.Instance.LockMovement("Final Cell ending");
        }
        
        // Hide ending UI initially
        if (endingCanvas != null)
        {
            endingCanvas.gameObject.SetActive(false);
        }
        
        // Show the defeat ending sequence
        StartCoroutine(DefeatEndingSequence());
    }
    
    private IEnumerator DefeatEndingSequence()
    {
        if (debugMode) Debug.Log("[FinalCellCaptionController] Starting defeat ending sequence");
        
        // Initial pause to let player orient themselves
        yield return new WaitForSeconds(startDelay);
        
        // First defeat message
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(defeatMessage1, messageDuration);
        }
        else
        {
            Debug.LogWarning("[FinalCellCaptionController] CaptionManager not found!");
        }
        
        yield return new WaitForSeconds(messageDuration + pauseBetweenMessages);
        
        // Second defeat message
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(defeatMessage2, messageDuration);
        }
        
        yield return new WaitForSeconds(messageDuration + thanksDelay);
        
        if (debugMode) Debug.Log("[FinalCellCaptionController] Ending sequence complete - showing return button");
        
        // Show return to title button
        ShowReturnButton();
    }
    
    private void ShowReturnButton()
    {
        // Pause the game (like pause menu does)
        
        // CRITICAL: Unlock cursor FIRST so player can click the button (like pause menu)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EnsureEventSystem();
        EnsureEndingCanvasInteractive();
        
        // Show the ending canvas with button
        if (endingCanvas != null)
        {
            endingCanvas.gameObject.SetActive(true);
        }
        
        // Setup button
        if (returnToTitleButton != null)
        {
            returnToTitleButton.onClick.RemoveAllListeners();
            returnToTitleButton.onClick.AddListener(ReturnToTitle);
            returnToTitleButton.interactable = true;
            returnToTitleButton.Select();
            
            if (debugMode) Debug.Log("[FinalCellCaptionController] Return button shown and configured");
        }
        else
        {
            if (debugMode) Debug.LogWarning("[FinalCellCaptionController] Return to title button not assigned!");
        }
        
        if (debugMode) Debug.Log("[FinalCellCaptionController] Cursor unlocked for button interaction");
    }
    
    /// <summary>
    /// Called when Return to Title button is clicked
    /// </summary>
    public void ReturnToTitle()
    {
        Debug.Log("[FinalCellCaptionController] ReturnToTitle clicked");

        if (debugMode) Debug.Log($"[FinalCellCaptionController] Returning to title: {titleSceneName}");
        
        // Unpause the game before leaving
        Time.timeScale = 1f;
        
        // Reset game state so player can start fresh
        GameSession.Reset();
        GameSession.LostToFinalBoss = false; // Clear the loss flag
        
        if (!Application.CanStreamedLevelBeLoaded(titleSceneName))
        {
            Debug.LogError($"[FinalCellCaptionController] Cannot load scene '{titleSceneName}'. Is it added to Build Settings?");
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
    
    [ContextMenu("Test: Play Ending")]
    public void TestEnding()
    {
        if (Application.isPlaying)
        {
            StartCoroutine(DefeatEndingSequence());
        }
    }
}
