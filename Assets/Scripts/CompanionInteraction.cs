using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

/// <summary>
/// Handles player interaction with companion to activate following
/// Uses the caption system for companion dialogue
/// Attach to companion GameObject
/// </summary>
public class CompanionInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Key code to interact with companion")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    
    [Header("Dialogue - Initial Greeting")]
    [SerializeField] private string companionGreeting = "[Fragment] Hello... I've been waiting.";
    [SerializeField] private string interactionPrompt = "[System] Press E to speak";
    
    [Header("Dialogue - Conversation")]
    [SerializeField] private string playerQuestion1 = "[You] What are you?";
    [SerializeField] private string companionResponse1 = "[Fragment] Fragment. A piece of you.";
    [SerializeField] private string playerQuestion2 = "[You] My memory?";
    [SerializeField] private string companionExplanation = "[Fragment] Yes. Your power. Stolen by them.";
    [SerializeField] private string playerRequest = "[You] Can you help me escape?";
    [SerializeField] private string companionAgreement = "[Fragment] Together, we escape.";
    
    [Header("Timing")]
    [SerializeField] private float promptDuration = 2f;
    [SerializeField] private float dialogueDuration = 2.5f;
    
    [Header("References")]
    [Tooltip("Reference to the CompanionFollower component")]
    [SerializeField] private CompanionFollower companionFollower;
    [SerializeField] private string blobColour = "Yellow";
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool playerInRange = false;
    private bool hasInteracted = false;
    
    void Start()
    {
        // ALWAYS log initialization to verify script is running
        Debug.Log($"[CompanionInteraction] ⭐ SCRIPT STARTED on {gameObject.name}");
        
        // Auto-find CompanionFollower if not assigned
        if (companionFollower == null)
        {
            companionFollower = GetComponent<CompanionFollower>();
            if (companionFollower == null)
            {
                Debug.LogError("[CompanionInteraction] No CompanionFollower found! Add component or assign reference.");
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[CompanionInteraction] Initialized on {gameObject.name}. Waiting for player...");
        }
    }
    
    void Update()
    {
        // Only check for interaction if player is in range and hasn't interacted yet
        if (playerInRange && !hasInteracted)
        {
            // Check if interaction key is pressed
            if (Input.GetKeyDown(interactionKey))
            {
                // ALWAYS log F key press
                Debug.Log($"[CompanionInteraction] ⚡ {interactionKey} KEY PRESSED! playerInRange={playerInRange}, hasInteracted={hasInteracted}");
                    
                Interact();
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (showDebugLogs)
            Debug.Log($"[CompanionInteraction] Trigger entered by: {other.gameObject.name}, Tag: {other.tag}");
        
        // Check if player entered trigger
        if (other.CompareTag("Player") && !hasInteracted)
        {
            playerInRange = true;
            ShowGreeting();
            
            if (showDebugLogs)
                Debug.Log("[CompanionInteraction] ✅ Player in range - showing greeting");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // Check if player left trigger
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            if (showDebugLogs)
                Debug.Log("[CompanionInteraction] Player left range");
        }
    }
    
    private void Interact()
    {
        if (hasInteracted)
        {
            if (showDebugLogs)
                Debug.Log("[CompanionInteraction] Already interacted, ignoring");
            return;
        }
        
        if (showDebugLogs)
            Debug.Log("[CompanionInteraction] 🎭 Starting befriend dialogue sequence...");
        
        // Start dialogue sequence
        StartCoroutine(BefriendDialogueSequence());
        
        hasInteracted = true;
        
        // Disable this component after interaction (one-time only)
        enabled = false;
    }
    
    private void ShowGreeting()
    {
        // Show companion's greeting and interaction prompt
        if (CaptionManager.Instance != null)
        {
            StartCoroutine(GreetingSequence());
        }
        else
        {
            // Fallback to console
            Debug.Log($"💬 {companionGreeting}");
            Debug.Log($"[Hint] {interactionPrompt}");
        }
    }
    
    private IEnumerator GreetingSequence()
    {
        // Companion speaks
        CaptionManager.Instance.ShowMonologue(companionGreeting, promptDuration);
        
        yield return new WaitForSeconds(1f); // Short pause
        
        // Show interaction instruction immediately as a system message so it's visible
        if (playerInRange && !hasInteracted)
        {
            CaptionManager.Instance.ShowSystemMessage(interactionPrompt, 5f); // Show for longer
        }
    }
    
    private IEnumerator BefriendDialogueSequence()
    {
        // Lock movement during dialogue
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.LockMovement("Fragment dialogue");
        
        if (CaptionManager.Instance != null)
        {
            // 1. Player asks what it is
            CaptionManager.Instance.ShowMonologue(playerQuestion1, dialogueDuration);
            yield return new WaitForSeconds(dialogueDuration + 0.5f);
            
            // 2. Fragment reveals it's a piece of player
            CaptionManager.Instance.ShowMonologue(companionResponse1, dialogueDuration);
            yield return new WaitForSeconds(dialogueDuration + 0.5f);
            
            // 3. Player realizes it's their memory
            CaptionManager.Instance.ShowMonologue(playerQuestion2, dialogueDuration);
            yield return new WaitForSeconds(dialogueDuration + 0.5f);
            
            // 4. Fragment explains the situation
            CaptionManager.Instance.ShowMonologue(companionExplanation, dialogueDuration + 0.5f);
            yield return new WaitForSeconds(dialogueDuration + 1f);
            
            // 5. Player asks for help
            CaptionManager.Instance.ShowMonologue(playerRequest, dialogueDuration);
            yield return new WaitForSeconds(dialogueDuration + 0.5f);
            
            // 6. Fragment agrees
            CaptionManager.Instance.ShowMonologue(companionAgreement, dialogueDuration);
            yield return new WaitForSeconds(dialogueDuration + 0.5f);
            
            // 7. System message - Fragment joined
            CaptionManager.Instance.ShowSystemMessage("[System] Fragment joined", 2f);
            yield return new WaitForSeconds(2.5f);
            
            // 8. Hint about exploring symbols
            if(blobColour == "Yellow")
            {
                CaptionManager.Instance.ShowSystemMessage("[System] Fragment may know more about the markings on the walls", 3f);
            }
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            Debug.Log(playerQuestion1);
            Debug.Log(companionResponse1);
            Debug.Log(playerQuestion2);
            Debug.Log(companionExplanation);
            Debug.Log(playerRequest);
            Debug.Log(companionAgreement);
        }
        
        // Unlock movement after dialogue
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.UnlockMovement("Dialogue complete");
        
        // Activate companion following
        if (companionFollower != null)
        {
            if (showDebugLogs)
                Debug.Log("[CompanionInteraction] Activating CompanionFollower...");
                
            companionFollower.StartFollowing();
            
            if (showDebugLogs)
                Debug.Log("[CompanionInteraction] ✅ Fragment activated and following!");
        }
        else
        {
            Debug.LogError("[CompanionInteraction] ❌ CompanionFollower is null! Cannot start following.");
        }
    }
    
    /// <summary>
    /// Check if player has already interacted
    /// </summary>
    public bool HasInteracted()
    {
        return hasInteracted;
    }
}
