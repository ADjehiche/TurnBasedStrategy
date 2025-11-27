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
    
    [Header("UI")]
    [SerializeField] private string promptMessage = "[System] Press E to inspect";
    [SerializeField] private bool showPrompt = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    private bool playerInRange = false;
    private bool isShowingDialogue = false;
    
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
        
        // Check if Fragment is with player
        if (GameSession.CompanionActive)
        {
            // Show Fragment's explanation
            StartCoroutine(ShowDialogueSequence(dialogueAfterFragment));
        }
        else
        {
            // Show player's confusion
            StartCoroutine(ShowDialogueSequence(dialogueBeforeFragment));
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
