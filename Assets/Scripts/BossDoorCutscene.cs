using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the boss door reveal cutscene when both fragments are collected.
/// Pans camera to show the boss door/tower, plays effects, then returns.
/// </summary>
public class BossDoorCutscene : MonoBehaviour
{
    public static BossDoorCutscene Instance { get; private set; }
    
    [Header("Target")]
    [SerializeField] private Transform bossDoorTarget;
    
    [Header("Camera Settings")]
    [SerializeField] private float panDuration = 2f;
    [SerializeField] private float holdDuration = 3f;
    [SerializeField] private float returnDuration = 2f;
    [SerializeField] private float zoomFOV = 35f;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 2f, -8f);
    
    [Header("Dialogue")]
    [SerializeField] private string[] cutsceneDialogue = {
        "[You] The door... it's opening.",
        "[Fragment] The tower awaits. Our power grows."
    };
    [SerializeField] private float dialogueDuration = 2.5f;
    
    [Header("Effects")]
    [SerializeField] private AudioClip doorRevealSound;
    [SerializeField] private Light doorLight;
    [SerializeField] private float doorLightIntensity = 3f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private Camera mainCamera;
    private Transform cameraParent;
    private Vector3 originalCamPosition;
    private Quaternion originalCamRotation;
    private float originalFOV;
    private bool isPlaying = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraParent = mainCamera.transform.parent;
            originalFOV = mainCamera.fieldOfView;
        }
        
        // Start door light off
        if (doorLight != null)
        {
            doorLight.intensity = 0f;
        }
    }
    
    /// <summary>
    /// Play the boss door reveal cutscene
    /// </summary>
    public void PlayCutscene()
    {
        if (isPlaying || bossDoorTarget == null)
        {
            if (debugMode) Debug.LogWarning("[BossDoorCutscene] Cannot play - already playing or no target");
            return;
        }
        
        if (debugMode) Debug.Log("[BossDoorCutscene] 🏰 Starting boss door reveal cutscene");
        StartCoroutine(CutsceneSequence());
    }
    
    private IEnumerator CutsceneSequence()
    {
        isPlaying = true;
        
        // Lock player movement
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.LockMovement("Boss door cutscene");
        }
        
        // Store original camera state
        if (mainCamera != null)
        {
            originalCamPosition = mainCamera.transform.position;
            originalCamRotation = mainCamera.transform.rotation;
            originalFOV = mainCamera.fieldOfView;
        }
        
        // Calculate target camera position
        Vector3 targetCamPos = bossDoorTarget.position + bossDoorTarget.TransformDirection(cameraOffset);
        Quaternion targetCamRot = Quaternion.LookRotation(bossDoorTarget.position - targetCamPos);
        
        // Show first dialogue
        if (cutsceneDialogue.Length > 0 && CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(cutsceneDialogue[0], dialogueDuration);
        }
        
        // Play reveal sound
        if (doorRevealSound != null && AudioManager.Instance != null)
        {
            AudioSource.PlayClipAtPoint(doorRevealSound, bossDoorTarget.position);
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("DoorUnlock");
        }
        
        // PAN TO BOSS DOOR
        float elapsed = 0f;
        while (elapsed < panDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / panDuration);
            
            if (mainCamera != null)
            {
                mainCamera.transform.position = Vector3.Lerp(originalCamPosition, targetCamPos, t);
                mainCamera.transform.rotation = Quaternion.Slerp(originalCamRotation, targetCamRot, t);
                mainCamera.fieldOfView = Mathf.Lerp(originalFOV, zoomFOV, t);
            }
            
            yield return null;
        }
        
        // HOLD ON BOSS DOOR
        // Fade in door light
        if (doorLight != null)
        {
            StartCoroutine(FadeDoorLight(0f, doorLightIntensity, holdDuration / 2f));
        }
        
        // Show second dialogue during hold
        yield return new WaitForSeconds(holdDuration / 2f);
        
        if (cutsceneDialogue.Length > 1 && CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(cutsceneDialogue[1], dialogueDuration);
        }
        
        yield return new WaitForSeconds(holdDuration / 2f);
        
        // RETURN TO PLAYER
        Vector3 currentCamPos = mainCamera.transform.position;
        Quaternion currentCamRot = mainCamera.transform.rotation;
        float currentFOV = mainCamera.fieldOfView;
        
        elapsed = 0f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / returnDuration);
            
            if (mainCamera != null)
            {
                mainCamera.transform.position = Vector3.Lerp(currentCamPos, originalCamPosition, t);
                mainCamera.transform.rotation = Quaternion.Slerp(currentCamRot, originalCamRotation, t);
                mainCamera.fieldOfView = Mathf.Lerp(currentFOV, originalFOV, t);
            }
            
            yield return null;
        }
        
        // Ensure exact restoration
        if (mainCamera != null)
        {
            mainCamera.transform.position = originalCamPosition;
            mainCamera.transform.rotation = originalCamRotation;
            mainCamera.fieldOfView = originalFOV;
        }
        
        // Unlock player
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.UnlockMovement("Boss door cutscene complete");
        }
        
        isPlaying = false;
        
        if (debugMode) Debug.Log("[BossDoorCutscene] 🏰 Boss door reveal complete!");
    }
    
    private IEnumerator FadeDoorLight(float from, float to, float duration)
    {
        if (doorLight == null) yield break;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            doorLight.intensity = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        doorLight.intensity = to;
    }
    
    /// <summary>
    /// Check if cutscene can play (has target assigned)
    /// </summary>
    public bool CanPlay => bossDoorTarget != null && !isPlaying;
    
    [ContextMenu("Test: Play Cutscene")]
    public void TestPlayCutscene()
    {
        if (Application.isPlaying)
        {
            PlayCutscene();
        }
    }
}
