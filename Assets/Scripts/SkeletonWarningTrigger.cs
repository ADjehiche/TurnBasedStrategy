using UnityEngine;
using System.Collections;

/// <summary>
/// Triggers a warning sequence before the skeleton battle
/// Only triggers once - checks if skeleton already defeated
/// Automatically starts battle after warning sequence
/// Destroys itself after skeleton is defeated
/// </summary>
public class SkeletonWarningTrigger : MonoBehaviour
{
    [Header("Warning Settings")]
    [SerializeField] private float warningDuration = 3f;
    
    [Header("Captions")]
    [SerializeField] private string warningMessage1 = "[You] Wait... movement ahead.";
    [SerializeField] private string warningMessage2 = "[System] Warning: Guardian detected";
    [SerializeField] private string warningMessage3 = "[System] Prepare for battle!";
    
    [Header("Battle")]
    [SerializeField] private GameManager gameManager;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool hasTriggered = false;
    
    void Start()
    {
        // Auto-find GameManager
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("[SkeletonWarningTrigger] No GameManager found!");
            }
        }
        
        // If skeleton already defeated, destroy this trigger immediately
        if (GameSession.EnemyDefeated)
        {
            if (showDebugLogs)
                Debug.Log("[SkeletonWarningTrigger] Skeleton already defeated - destroying trigger");
            
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Don't trigger if skeleton already defeated
        if (GameSession.EnemyDefeated)
        {
            if (showDebugLogs)
                Debug.Log("[SkeletonWarningTrigger] Skeleton defeated - ignoring trigger");
            return;
        }
        
        if (hasTriggered || !other.CompareTag("Player"))
            return;
        
        hasTriggered = true;
        
        if (showDebugLogs)
            Debug.Log("[SkeletonWarningTrigger] Player entered warning zone");
        
        StartCoroutine(WarningSequence());
    }
    
    private IEnumerator WarningSequence()
    {
        // Lock movement during warning
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.LockMovement("Skeleton warning");
        
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(warningMessage1, 1f);
            yield return new WaitForSeconds(1.2f);
            
            CaptionManager.Instance.ShowSystemMessage(warningMessage2, 1f);
            yield return new WaitForSeconds(1.2f);
            
            CaptionManager.Instance.ShowInstruction(warningMessage3, 0.8f);
            yield return new WaitForSeconds(1f);
        }
        else
        {
            Debug.Log(warningMessage1);
            Debug.Log(warningMessage2);
            Debug.Log(warningMessage3);
            yield return new WaitForSeconds(warningDuration);
        }
        
        // Unlock movement before battle (battle system will handle its own locks)
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.UnlockMovement("Warning complete - starting battle");
        
        // Small pause before battle starts
        yield return new WaitForSeconds(0.5f);
        
        // Automatically start battle
        if (gameManager != null)
        {
            if (showDebugLogs)
                Debug.Log("[SkeletonWarningTrigger] ⚔️ Starting battle!");
            
            gameManager.StartBattle();
        }
        else
        {
            Debug.LogError("[SkeletonWarningTrigger] Cannot start battle - GameManager is null!");
        }
        
        // Disable collider so it won't trigger again
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }
}
