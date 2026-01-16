using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Exit trigger behind boss that shows game ending:
/// - Evil path (has purple fragment): Red screen + evil text + laughter
/// - Good path (no fragment): White screen + thanks message
/// </summary>
public class GameEndingTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Canvas endingCanvas;
    [SerializeField] private Image screenOverlay;
    [SerializeField] private TextMeshProUGUI endingText;
    [SerializeField] private TextMeshProUGUI thanksText;
    
    [Header("Colors")]
    [SerializeField] private Color evilColor = Color.red;
    [SerializeField] private Color goodColor = Color.white;
    
    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float textDelay = 1f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip evilLaughterClip;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Text")]
    [SerializeField] private string evilMessage = "You have chosen evil!";
    [SerializeField] private string goodMessage = "";
    [SerializeField] private string thanksMessage = "Thanks for playing";
    
    [Header("Return Button")]
    [SerializeField] private Button returnToTitleButton;
    [SerializeField] private string titleSceneName = "TitleScene";
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private bool hasTriggered = false;
    
    void Start()
    {
        // Hide ending UI initially
        if (endingCanvas != null)
        {
            endingCanvas.gameObject.SetActive(false);
        }
        
        // Get or create audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;
        
        // Only trigger after boss is defeated
        if (!GameSession.BossDefeated)
        {
            if (debugMode) Debug.Log("[GameEndingTrigger] Boss not defeated yet, ignoring");
            return;
        }
        
        hasTriggered = true;
        
        // Determine which ending
        bool isEvilEnding = GameSession.HasCollectedPurpleFragment;
        
        if (debugMode) Debug.Log($"[GameEndingTrigger] 🎮 Game ending triggered! Evil: {isEvilEnding}");
        
        StartCoroutine(PlayEnding(isEvilEnding));
    }
    
    private IEnumerator PlayEnding(bool isEvil)
    {
        // Lock player
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.LockMovement("Game ending");
        }
        
        // Show canvas
        if (endingCanvas != null)
        {
            endingCanvas.gameObject.SetActive(true);
        }
        
        // Set colors
        Color targetColor = isEvil ? evilColor : goodColor;
        Color textColor = isEvil ? Color.white : Color.black;
        
        // Set initial state (fully transparent)
        if (screenOverlay != null)
        {
            screenOverlay.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
        }
        if (endingText != null)
        {
            endingText.text = isEvil ? evilMessage : goodMessage;
            endingText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
        }
        if (thanksText != null)
        {
            thanksText.text = thanksMessage;
            thanksText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
        }
        
        // Fade in screen overlay
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / fadeInDuration;
            
            if (screenOverlay != null)
            {
                screenOverlay.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            }
            
            yield return null;
        }
        
        // Ensure fully opaque
        if (screenOverlay != null)
        {
            screenOverlay.color = targetColor;
        }
        
        yield return new WaitForSeconds(textDelay);
        
        // Fade in ending text (evil only)
        if (isEvil && endingText != null && !string.IsNullOrEmpty(evilMessage))
        {
            elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                endingText.color = new Color(textColor.r, textColor.g, textColor.b, elapsed);
                yield return null;
            }
            
            // Play evil laughter
            if (evilLaughterClip != null && audioSource != null)
            {
                audioSource.clip = evilLaughterClip;
                audioSource.Play();
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("EvilLaughter");
            }
            
            yield return new WaitForSeconds(2f);
        }
        
        // Fade in thanks text
        if (thanksText != null)
        {
            elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                thanksText.color = new Color(textColor.r, textColor.g, textColor.b, elapsed);
                yield return null;
            }
        }
        
        if (debugMode) Debug.Log("[GameEndingTrigger] ✅ Game ending complete!");
        
        // Show return button
        ShowReturnButton();
    }
    
    private void ShowReturnButton()
    {
        if (returnToTitleButton != null)
        {
            returnToTitleButton.gameObject.SetActive(true);
            returnToTitleButton.onClick.AddListener(ReturnToTitle);
            
            // Unlock cursor for button interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    /// <summary>
    /// Called when Return to Title button is clicked
    /// </summary>
    public void ReturnToTitle()
    {
        // Reset game state so player can start fresh
        GameSession.Reset();
        
        if (debugMode) Debug.Log($"[GameEndingTrigger] Returning to title: {titleSceneName}");
        
        SceneManager.LoadScene(titleSceneName);
    }
    
    [ContextMenu("Test: Evil Ending")]
    public void TestEvilEnding()
    {
        if (Application.isPlaying)
        {
            GameSession.HasCollectedPurpleFragment = true;
            GameSession.BossDefeated = true;
            StartCoroutine(PlayEnding(true));
        }
    }
    
    [ContextMenu("Test: Good Ending")]
    public void TestGoodEnding()
    {
        if (Application.isPlaying)
        {
            GameSession.HasCollectedPurpleFragment = false;
            GameSession.BossDefeated = true;
            StartCoroutine(PlayEnding(false));
        }
    }
}
