using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Singleton manager for handling flashback sequences with screen effects
/// Handles scene transitions, overlays, and timing for memory flashbacks
/// </summary>
public class FlashbackManager : MonoBehaviour
{
    public static FlashbackManager Instance { get; private set; }
    
    [Header("UI Overlay")]
    [Tooltip("Fullscreen Image for color overlay effects (should cover entire screen)")]
    [SerializeField] private Image overlayImage;
    [SerializeField] private CanvasGroup overlayCanvasGroup;
    
    [Header("Flashback Settings")]
    [Tooltip("Color to fade to during flashback transition")]
    [SerializeField] private Color flashbackFadeColor = new Color(0.5f, 0f, 0f, 1f); // Dark red
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 2f;
    
    [Header("Camera Shake")]
    [SerializeField] private float shakeIntensity = 0.3f;
    [SerializeField] private float shakeDuration = 0.5f;
    
    [Header("Audio (Assign when available)")]
    [Tooltip("Sound when flashback starts")]
    [SerializeField] private AudioClip flashbackStartSFX;
    [Tooltip("Ambient sound during flashback")]
    [SerializeField] private AudioClip flashbackAmbience;
    [Tooltip("Thunder/lightning sound effect")]
    [SerializeField] private AudioClip thunderSFX;
    [Tooltip("Sound when flashback ends")]
    [SerializeField] private AudioClip flashbackEndSFX;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // State tracking
    private bool isFlashbackActive = false;
    private string sceneToReturnTo;
    private AudioSource audioSource;
    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    
    void Awake()
    {
        // Singleton with persistence across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Initialize overlay as invisible
        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.alpha = 0f;
        }
    }
    
    void Start()
    {
        mainCamera = Camera.main;
    }
    
    /// <summary>
    /// Check if a flashback is currently playing
    /// </summary>
    public bool IsFlashbackActive => isFlashbackActive;
    
    /// <summary>
    /// Start the Red Fragment (Rage) flashback sequence
    /// Loads a separate flashback scene, plays dialogue, then returns
    /// </summary>
    /// <param name="flashbackSceneName">Name of the flashback scene to load</param>
    /// <param name="onComplete">Callback when flashback completes</param>
    public void StartRageFlashback(string flashbackSceneName, System.Action onComplete = null)
    {
        if (isFlashbackActive)
        {
            if (showDebugLogs) Debug.LogWarning("[FlashbackManager] Flashback already in progress!");
            return;
        }
        
        StartCoroutine(RageFlashbackSequence(flashbackSceneName, onComplete));
    }
    
    /// <summary>
    /// Start a flashback with overlay effects only (no scene change)
    /// Use this if you don't want to load a separate scene
    /// </summary>
    public void StartOverlayFlashback(string[] dialogueLines, float lineDuration, System.Action onComplete = null)
    {
        if (isFlashbackActive)
        {
            if (showDebugLogs) Debug.LogWarning("[FlashbackManager] Flashback already in progress!");
            return;
        }
        
        StartCoroutine(OverlayFlashbackSequence(dialogueLines, lineDuration, onComplete));
    }
    
    /// <summary>
    /// Full flashback sequence with scene transition
    /// </summary>
    private IEnumerator RageFlashbackSequence(string flashbackSceneName, System.Action onComplete)
    {
        isFlashbackActive = true;
        sceneToReturnTo = SceneManager.GetActiveScene().name;
        
        if (showDebugLogs) Debug.Log($"[FlashbackManager] 🔴 Starting Rage Flashback -> {flashbackSceneName}");
        
        // Lock player movement
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.LockMovement("Rage Flashback");
        }
        
        // Play start SFX
        PlayAudioClip(flashbackStartSFX);
        
        // Fade to flashback color
        yield return StartCoroutine(FadeOverlay(0f, 1f, fadeInDuration));
        
        // Load flashback scene additively or switch
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(flashbackSceneName, LoadSceneMode.Single);
        while (!loadOp.isDone)
        {
            yield return null;
        }
        
        // Wait for FlashbackSceneController to signal completion
        // The flashback scene will call FlashbackManager.Instance.OnFlashbackSceneComplete()
        if (showDebugLogs) Debug.Log("[FlashbackManager] Flashback scene loaded, waiting for completion signal...");
        
        // Wait until the flashback scene signals it's done
        while (isFlashbackActive)
        {
            yield return null;
        }
        
        // Return to original scene
        if (showDebugLogs) Debug.Log($"[FlashbackManager] Returning to {sceneToReturnTo}");
        
        AsyncOperation returnOp = SceneManager.LoadSceneAsync(sceneToReturnTo, LoadSceneMode.Single);
        while (!returnOp.isDone)
        {
            yield return null;
        }
        
        // Fade out overlay
        yield return StartCoroutine(FadeOverlay(1f, 0f, fadeOutDuration));
        
        // Play end SFX
        PlayAudioClip(flashbackEndSFX);
        
        // Unlock player movement
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.UnlockMovement("Flashback Complete");
        }
        
        if (showDebugLogs) Debug.Log("[FlashbackManager] 🔴 Rage Flashback complete!");
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Overlay-only flashback (no scene change)
    /// </summary>
    private IEnumerator OverlayFlashbackSequence(string[] dialogueLines, float lineDuration, System.Action onComplete)
    {
        isFlashbackActive = true;
        
        if (showDebugLogs) Debug.Log("[FlashbackManager] 🔴 Starting overlay flashback");
        
        // Lock player
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.LockMovement("Rage Flashback");
        }
        
        // Play start SFX
        PlayAudioClip(flashbackStartSFX);
        
        // Start ambience
        if (flashbackAmbience != null && audioSource != null)
        {
            audioSource.clip = flashbackAmbience;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Fade in overlay
        yield return StartCoroutine(FadeOverlay(0f, 0.7f, fadeInDuration));
        
        // Show dialogue lines
        foreach (string line in dialogueLines)
        {
            if (CaptionManager.Instance != null)
            {
                CaptionManager.Instance.ShowMonologue(line, lineDuration);
            }
            
            // Camera shake on dramatic lines
            if (line.Contains("lightning") || line.Contains("Thunder"))
            {
                PlayAudioClip(thunderSFX);
                StartCoroutine(ShakeCamera());
            }
            
            yield return new WaitForSeconds(lineDuration + 0.5f);
        }
        
        // Stop ambience
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Fade out overlay
        yield return StartCoroutine(FadeOverlay(0.7f, 0f, fadeOutDuration));
        
        // Play end SFX
        PlayAudioClip(flashbackEndSFX);
        
        // Unlock player
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.UnlockMovement("Flashback Complete");
        }
        
        isFlashbackActive = false;
        
        if (showDebugLogs) Debug.Log("[FlashbackManager] 🔴 Overlay flashback complete!");
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Called by FlashbackSceneController when the flashback scene dialogue is complete
    /// </summary>
    public void OnFlashbackSceneComplete()
    {
        if (showDebugLogs) Debug.Log("[FlashbackManager] Flashback scene signaled completion");
        isFlashbackActive = false;
    }
    
    /// <summary>
    /// Fade the overlay image between alpha values
    /// </summary>
    private IEnumerator FadeOverlay(float fromAlpha, float toAlpha, float duration)
    {
        if (overlayCanvasGroup == null)
        {
            if (showDebugLogs) Debug.LogWarning("[FlashbackManager] No overlay CanvasGroup assigned!");
            yield break;
        }
        
        if (overlayImage != null)
        {
            overlayImage.color = flashbackFadeColor;
        }
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            overlayCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            yield return null;
        }
        
        overlayCanvasGroup.alpha = toAlpha;
    }
    
    /// <summary>
    /// Camera shake effect for dramatic moments
    /// </summary>
    private IEnumerator ShakeCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) yield break;
        }
        
        originalCameraPosition = mainCamera.transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;
            
            mainCamera.transform.localPosition = originalCameraPosition + new Vector3(x, y, 0);
            yield return null;
        }
        
        mainCamera.transform.localPosition = originalCameraPosition;
    }
    
    /// <summary>
    /// Play an audio clip if available
    /// </summary>
    private void PlayAudioClip(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // ===== TESTING METHODS =====
    
    [ContextMenu("Test: Overlay Flashback")]
    public void TestOverlayFlashback()
    {
        string[] testLines = {
            "...Blood. So much blood...",
            "They called me the Architect.",
            "I held lightning in my hands.",
            "What have I done?"
        };
        StartOverlayFlashback(testLines, 3f, () => Debug.Log("Test flashback complete!"));
    }
}
