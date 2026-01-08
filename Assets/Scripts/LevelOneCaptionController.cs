using UnityEngine;
using System.Collections;

public class LevelOneCaptionController : MonoBehaviour
{
    [Header("Caption Messages")]
    [SerializeField] private string wakeUpInstruction = "[You] Where am I?";
    [SerializeField] private string wakeUpMonologue = "[You] My head... I can't remember anything.";
    [SerializeField] private string hallwayObservation = "[You] These ancient pillars...";
    [SerializeField] private string keyPickupMessage = "[System] Cell key";
    [SerializeField] private string keyPickupMonologue = "[You] A way out.";
    [SerializeField] private string doorOpenCelebration = "[You] Free. For now.";
    [SerializeField] private string skeletonDefeated = "[System] Guardian defeated";
    [SerializeField] private string skeletonKeyPickup = "[You] This key... I wonder if it opens another cell";
    
    [Header("Timing")]
    [SerializeField] private float startDelay = 2f;
    [SerializeField] private float instructionDuration = 3f;
    [SerializeField] private float monologueDuration = 2.5f;
    
    void Start()
    {
        // Show the initial wake-up sequence
        if (!GameSession.HasShownStartInstruction)
        {
            StartCoroutine(WakeUpSequence());
        }
        
        // Show hallway observation when entering hallway (called externally)
    }
    
    private IEnumerator WakeUpSequence()
    {
        // Lock movement during wake-up
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.LockMovement("Wake-up sequence");
        
        yield return new WaitForSeconds(startDelay);
        
        if (!GameSession.HasShownStartInstruction && CaptionManager.Instance != null)
        {
            // First thought: Confusion
            CaptionManager.Instance.ShowInstruction(wakeUpInstruction, 2f);
            
            yield return new WaitForSeconds(2.5f);
            
            // Pan camera while thinking AND add blinking effect
            if (CameraPanner.Instance != null)
            {
                StartCoroutine(CameraPanner.Instance.PanLookAround(3f, 45f));
            }
            
            // Add blinking effect (2 blinks) during the camera pan
            if (ScreenBlinker.Instance != null)
            {
                StartCoroutine(ScreenBlinker.Instance.BlinkMultiple(2, 0.3f, 0.3f));
            }
            
            // Second thought: Amnesia
            CaptionManager.Instance.ShowMonologue(wakeUpMonologue, monologueDuration);
            
            yield return new WaitForSeconds(monologueDuration + 1f);
            
            GameSession.HasShownStartInstruction = true;
            Debug.Log("[LevelOneCaptionController] Wake-up sequence shown");
        }
        
        // Unlock movement after wake-up
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.UnlockMovement("Wake-up complete");
    }
    
    /// <summary>
    /// Call when entering hallway
    /// </summary>
    public void OnEnterHallway()
    {
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(hallwayObservation, 2f);
        }
    }
    
    /// <summary>
    /// Call this when the player picks up the key
    /// </summary>
    public void OnKeyPickedUp()
    {
        if (!GameSession.HasShownKeyPickup && CaptionManager.Instance != null)
        {
            StartCoroutine(ShowKeyPickupSequence());
            GameSession.HasShownKeyPickup = true;
        }
    }
    
    private IEnumerator ShowKeyPickupSequence()
    {
        // System message
        CaptionManager.Instance.ShowSystemMessage(keyPickupMessage, 1.5f);
        
        yield return new WaitForSeconds(2f);
        
        // Player thought
        CaptionManager.Instance.ShowMonologue(keyPickupMonologue, monologueDuration);
    }
    
    /// <summary>
    /// Call this when the door opens
    /// </summary>
    public void OnDoorOpened()
    {
        if (!GameSession.HasShownDoorOpen && CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(doorOpenCelebration, monologueDuration);
            GameSession.HasShownDoorOpen = true;
        }
    }
    
    /// <summary>
    /// Call this when skeleton is defeated
    /// </summary>
    public void OnSkeletonDefeated()
    {
        if (!GameSession.HasShownEnemySpotted && CaptionManager.Instance != null)
        {
            StartCoroutine(ShowSkeletonDefeatedSequence());
            GameSession.HasShownEnemySpotted = true;
        }
    }
    
    private IEnumerator ShowSkeletonDefeatedSequence()
    {
        // System: Defeated
        CaptionManager.Instance.ShowSystemMessage(skeletonDefeated, 1.5f);
        
        yield return new WaitForSeconds(2f);
        
        // Player: Glowing key observation
        CaptionManager.Instance.ShowMonologue(skeletonKeyPickup, 2.5f);
    }
    
    /// <summary>
    /// Call when enemy is spotted (for EnemyLookDetector compatibility)
    /// Shows warning before skeleton encounter
    /// </summary>
    public void OnEnemySpotted()
    {
        // This is now handled by SkeletonWarningTrigger
        // But keep method for backward compatibility with EnemyLookDetector
        Debug.Log("[LevelOneCaptionController] Enemy spotted (handled by warning trigger)");
    }
    
    /// <summary>
    /// Reset the caption states (useful for testing)
    /// </summary>
    [ContextMenu("Reset Caption States")]
    public void ResetStates()
    {
        GameSession.HasShownStartInstruction = false;
        GameSession.HasShownKeyPickup = false;
        GameSession.HasShownDoorOpen = false;
        GameSession.HasShownEnemySpotted = false;
        Debug.Log("LevelOneCaptionController: States reset");
    }
}