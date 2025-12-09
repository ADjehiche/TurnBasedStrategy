using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base class for tutorial zones
/// Handles prompt display, completion detection, and feedback
/// </summary>
public abstract class TutorialZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] protected string promptText = "INITIALIZING...";
    [SerializeField] protected GameObject promptTextObject; // 3D Text Mesh or TextMeshPro
    [SerializeField] protected AudioClip completionSound;
    [SerializeField] protected GameObject nextZoneBarrier; // Object to disable when complete
    [SerializeField] protected Light nextZoneSpotlight; // Spotlight to turn on for next zone
    
    [Header("Feedback")]
    [SerializeField] protected Light feedbackLight; // Optional light that turns green
    [SerializeField] protected Color completionColor = Color.green;
    
    [Header("Events")]
    public UnityEvent onZoneComplete;
    
    protected bool isComplete = false;
    protected AudioSource audioSource;
    
    protected virtual void Start()
    {
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Show prompt
        if (promptTextObject != null)
        {
            promptTextObject.SetActive(true);
        }
    }
    
    protected virtual void CompleteZone()
    {
        if (isComplete) return;
        
        isComplete = true;
        
        // Play completion sound
        if (completionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completionSound);
        }
        
        // Change light color
        if (feedbackLight != null)
        {
            feedbackLight.color = completionColor;
            Debug.Log($"[TutorialZone] Changed light color to {completionColor}");
        }
        else
        {
            Debug.LogWarning("[TutorialZone] Feedback light is null! Assign it in Inspector.");
        }
        
        // Disable barrier to next zone
        if (nextZoneBarrier != null)
        {
            nextZoneBarrier.SetActive(false);
        }
        
        // Turn on next zone spotlight
        if (nextZoneSpotlight != null)
        {
            nextZoneSpotlight.gameObject.SetActive(true); // Enable GameObject
            nextZoneSpotlight.enabled = true; // Enable Light component
            Debug.Log($"[TutorialZone] Activated next zone spotlight: {nextZoneSpotlight.name}");
        }
        
        // Hide prompt
        if (promptTextObject != null)
        {
            promptTextObject.SetActive(false);
        }
        
        // Trigger events
        onZoneComplete?.Invoke();
        
        Debug.Log($"[TutorialZone] {gameObject.name} completed!");
    }
    
    public bool IsComplete() => isComplete;
}
