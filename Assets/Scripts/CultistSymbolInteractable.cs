using UnityEngine;
using System.Collections;

/// <summary>
/// Interactable cultist symbol with two-stage dialogue
/// Shows different dialogue before and after Fragment joins
/// Rewards exploration and reveals lore
/// </summary>
public class CultistSymbolInteractable : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private int symbolNumber = 1; // Which symbol is this (1 or 2)
    
    [Header("Dialogue - Before Fragment")]
    [Tooltip("Player's confused observations before Fragment explains")]
    [SerializeField] private string[] dialogueBeforeFragment = new string[]
    {
        "[You] Strange markings... I don't recognize them.",
        "[You] They look ancient. Deliberate."
    };
    
    [Header("Dialogue - After Fragment")]
    [Tooltip("Fragment's explanation of what the symbols mean")]
    [SerializeField] private string[] dialogueAfterFragment = new string[]
    {
        "[Fragment] Binding runes. They sealed your power.",
        "[You] Can you break them?",
        "[Fragment] Already broken. You're free now."
    };
    
    [Header("Timing")]
    [SerializeField] private float dialogueDuration = 2.5f;
    [SerializeField] private float pauseBetweenLines = 0.5f;
    [SerializeField] private float cutsceneDelay = 2f; // Delay before cutscene plays after both symbols activated
    
    [Header("UI")]
    [SerializeField] private string promptMessage = "[System] Press E to inspect";
    [SerializeField] private bool showPrompt = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    private bool playerInRange = false;
    private bool isShowingDialogue = false;
    private bool hasBeenActivated = false; // Track if this symbol has been activated
    
    void Update()
    {
        if (playerInRange && !isShowingDialogue)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                Interact();
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            if (showPrompt && CaptionManager.Instance != null)
            {
                CaptionManager.Instance.ShowSystemMessage(promptMessage, 3f);
            }
            
            if (showDebugLogs)
                Debug.Log($"[CultistSymbol] Player entered range of {gameObject.name}");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            if (showDebugLogs)
                Debug.Log($"[CultistSymbol] Player left range of {gameObject.name}");
        }
    }
    
    private void Interact()
    {
        if (showDebugLogs)
            Debug.Log($"[CultistSymbol] Player inspecting {gameObject.name}");
        
        // Mark this symbol as activated if Fragment is with player
        if (GameSession.CompanionActive && !hasBeenActivated)
        {
            hasBeenActivated = true;
            ActivateSymbol();
            
            // Show Fragment's explanation
            StartCoroutine(ShowDialogueSequence(dialogueAfterFragment));
        }
        else if (GameSession.CompanionActive && hasBeenActivated)
        {
            // Already activated - show Fragment's explanation again
            StartCoroutine(ShowDialogueSequence(dialogueAfterFragment));
        }
        else
        {
            // Show player's confusion (no Fragment yet)
            StartCoroutine(ShowDialogueSequence(dialogueBeforeFragment));
        }
    }
    
    private void ActivateSymbol()
    {
        // Mark the appropriate symbol as activated in GameSession
        if (symbolNumber == 1)
        {
            GameSession.Symbol1Activated = true;
            Debug.Log("[CultistSymbol] Symbol 1 activated!");
        }
        else if (symbolNumber == 2)
        {
            GameSession.Symbol2Activated = true;
            Debug.Log("[CultistSymbol] Symbol 2 activated!");
        }
        
        // Check if both symbols are now activated
        if (GameSession.BothSymbolsActivated)
        {
            Debug.Log("[CultistSymbol] Both symbols activated! Starting cutscene after delay...");
            StartCoroutine(DelayedUnlockExitDoor());
        }
    }
    
    private IEnumerator DelayedUnlockExitDoor()
    {
        // Wait for the configured delay
        yield return new WaitForSeconds(cutsceneDelay);
        
        // Now trigger the cutscene
        UnlockExitDoor();
    }
    
    private void UnlockExitDoor()
    {
        // Show caption that the seals are breaking/door is opening
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowSystemMessage("[System] The ancient seals break... the exit opens", 4f);
        }
        
        // Trigger the cutscene - the cutscene itself will handle opening the door
        TriggerSymbolsCutscene();
        
        Debug.Log("[CultistSymbol] Both symbols activated! Cutscene triggered.");
    }
    
    private void TriggerSymbolsCutscene()
    {
        // Trigger cutscene played objective (final objective)
        var objectiveManager = FindFirstObjectByType<SimpleLevelOneObjectives>();
        if (objectiveManager != null)
        {
            objectiveManager.OnCutscenePlayed();
            Debug.Log("[CultistSymbol] Cutscene played - final objective triggered");
        }
        
        GameObject cutsceneObj = GameObject.Find("DoorUnlock_Sequence");
        
        if (cutsceneObj != null)
        {
            // Activate the cutscene Timeline
            cutsceneObj.SetActive(true);
            Debug.Log("[CultistSymbol] DoorUnlock_Sequence cutscene triggered!");
            
            // If it's a Timeline, try to play it
            var playableDirector = cutsceneObj.GetComponent<UnityEngine.Playables.PlayableDirector>();
            if (playableDirector != null)
            {
                playableDirector.Play();
                Debug.Log("[CultistSymbol] PlayableDirector started!");
            }
        }
        else
        {
            Debug.LogWarning("[CultistSymbol] DoorUnlock_Sequence GameObject not found!");
        }
    }
    
    private IEnumerator ShowDialogueSequence(string[] dialogue)
    {
        isShowingDialogue = true;
        
        foreach (string line in dialogue)
        {
            if (CaptionManager.Instance != null)
            {
                CaptionManager.Instance.ShowMonologue(line, dialogueDuration);
            }
            else
            {
                Debug.Log(line);
            }
            
            yield return new WaitForSeconds(dialogueDuration + pauseBetweenLines);
        }
        
        isShowingDialogue = false;
        
        // Show prompt again after dialogue
        if (showPrompt && playerInRange && CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowSystemMessage(promptMessage, 2f);
        }
    }
    
    /// <summary>
    /// Manually trigger the interaction (for testing or external calls)
    /// </summary>
    public void TriggerInteraction()
    {
        Interact();
    }
}
