using UnityEngine;
using System.Collections;

/// <summary>
/// Triggers automatic Fragment dialogue when approaching blood trail
/// Only triggers after Fragment has joined the party
/// One-time trigger for atmospheric storytelling
/// </summary>
public class BloodTrailTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string fragmentComment = "[Fragment] Blood. Old blood.";
    [SerializeField] private string playerQuestion = "[You] Someone tried to escape before?";
    [SerializeField] private string fragmentResponse = "[Fragment] Many did. None succeeded.";
    [SerializeField] private string playerRealization = "[You] Until now.";
    
    [Header("Timing")]
    [SerializeField] private float dialogueDuration = 2f;
    [SerializeField] private float pauseBetweenLines = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    private bool hasTriggered = false;
    
    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player"))
            return;
        
        // Only trigger if Fragment is with the player
        if (!GameSession.CompanionActive)
        {
            if (showDebugLogs)
                Debug.Log("[BloodTrail] Fragment not active, skipping dialogue");
            return;
        }
        
        hasTriggered = true;
        
        if (showDebugLogs)
            Debug.Log("[BloodTrail] Triggering blood trail dialogue");
        
        StartCoroutine(BloodTrailDialogue());
    }
    
    private IEnumerator BloodTrailDialogue()
    {
        if (CaptionManager.Instance != null)
        {
            // Fragment notices
            CaptionManager.Instance.ShowMonologue(fragmentComment, dialogueDuration);
            yield return new WaitForSeconds(dialogueDuration + pauseBetweenLines);
            
            // Player asks
            CaptionManager.Instance.ShowMonologue(playerQuestion, dialogueDuration);
            yield return new WaitForSeconds(dialogueDuration + pauseBetweenLines);
            
            // Fragment responds
            CaptionManager.Instance.ShowMonologue(fragmentResponse, dialogueDuration);
            yield return new WaitForSeconds(dialogueDuration + pauseBetweenLines);
            
            // Player realizes they're different
            CaptionManager.Instance.ShowMonologue(playerRealization, dialogueDuration);
        }
        else
        {
            Debug.Log(fragmentComment);
            Debug.Log(playerQuestion);
            Debug.Log(fragmentResponse);
            Debug.Log(playerRealization);
        }
        
        // Disable trigger after use
        GetComponent<Collider>().enabled = false;
    }
}
