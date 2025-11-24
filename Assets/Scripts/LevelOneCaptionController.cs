using UnityEngine;
using System.Collections;

public class LevelOneCaptionController : MonoBehaviour
{
    [Header("Caption Messages")]
    [SerializeField] private string escapeInstruction = "Escape the cell";
    [SerializeField] private string keyPickupMonologue = "I wonder if this key would work on the door...";
    [SerializeField] private string keyFoundMessage = "Ooh, a key!";
    [SerializeField] private string doorOpenCelebration = "Yes! I'm free at last!";
    [SerializeField] private string enemySpottedWarning = "What... is that thing?!";
    
    [Header("Timing")]
    [SerializeField] private float startDelay = 2f; // Delay before showing initial instruction
    [SerializeField] private float instructionDuration = 4f;
    [SerializeField] private float monologueDuration = 3f;
    [SerializeField] private float warningDuration = 2.5f;
    
    void Start()
    {
        // Show the initial instruction after a short delay (only if not shown before)
        if (!GameSession.HasShownStartInstruction)
        {
            StartCoroutine(ShowStartInstructionDelayed());
        }
    }
    
    private IEnumerator ShowStartInstructionDelayed()
    {
        yield return new WaitForSeconds(startDelay);
        
        if (!GameSession.HasShownStartInstruction && CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowInstruction(escapeInstruction, instructionDuration);
            GameSession.HasShownStartInstruction = true;
            Debug.Log("LevelOneCaptionController: Showing start instruction");
        }
    }
    
    /// <summary>
    /// Call this when the player picks up the key
    /// </summary>
    public void OnKeyPickedUp()
    {
        if (!GameSession.HasShownKeyPickup && CaptionManager.Instance != null)
        {
            // First show a quick system message, then the monologue
            StartCoroutine(ShowKeyPickupSequence());
            GameSession.HasShownKeyPickup = true;
            Debug.Log("LevelOneCaptionController: Key pickup sequence triggered");
        }
    }
    
    private IEnumerator ShowKeyPickupSequence()
    {
        // Show immediate reaction when key is picked up
        CaptionManager.Instance.ShowSystemMessage(keyFoundMessage, 1.5f);
        
        // Wait briefly, then show the monologue
        yield return new WaitForSeconds(2f);
        
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
            Debug.Log("LevelOneCaptionController: Door open celebration triggered");
        }
    }
    
    /// <summary>
    /// Call this when the player spots an enemy
    /// </summary>
    public void OnEnemySpotted()
    {
        if (!GameSession.HasShownEnemySpotted && CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(enemySpottedWarning, warningDuration);
            GameSession.HasShownEnemySpotted = true;
            Debug.Log("LevelOneCaptionController: Enemy spotted warning triggered");
        }
    }
    
    /// <summary>
    /// Manually trigger the start instruction (useful for testing)
    /// </summary>
    [ContextMenu("Show Start Instruction")]
    public void ShowStartInstruction()
    {
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowInstruction(escapeInstruction, instructionDuration);
        }
    }
    
    /// <summary>
    /// Manually trigger the key pickup sequence (useful for testing)
    /// </summary>
    [ContextMenu("Trigger Key Pickup")]
    public void TriggerKeyPickup()
    {
        OnKeyPickedUp();
    }
    
    /// <summary>
    /// Manually trigger the door open celebration (useful for testing)
    /// </summary>
    [ContextMenu("Trigger Door Open")]
    public void TriggerDoorOpen()
    {
        OnDoorOpened();
    }
    
    /// <summary>
    /// Manually trigger the enemy spotted warning (useful for testing)
    /// </summary>
    [ContextMenu("Trigger Enemy Spotted")]
    public void TriggerEnemySpotted()
    {
        OnEnemySpotted();
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