using UnityEngine;
using System.Collections;

/// <summary>
/// ACTUALLY WORKING maze detection system!
/// Uses a large trigger that covers the ENTIRE maze area
/// But with smart logic to prevent false exits
/// </summary>
public class ActualWorkingMazeDetector : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private float exitDelay = 1f; // Delay before confirming exit to prevent false exits
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private MazeGuidanceController guidanceController;
    private bool playerInMaze = false;
    private Coroutine exitCheckCoroutine;
    
    void Start()
    {
        guidanceController = FindFirstObjectByType<MazeGuidanceController>();
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        if (debugMode)
        {
            Debug.Log("[ActualWorkingMazeDetector] Ready to detect maze entry/exit");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playerInMaze)
        {
            // Player entered maze
            playerInMaze = true;
            
            // Cancel any pending exit check
            if (exitCheckCoroutine != null)
            {
                StopCoroutine(exitCheckCoroutine);
                exitCheckCoroutine = null;
            }
            
            if (debugMode)
                Debug.Log("[ActualWorkingMazeDetector] ✅ Player ENTERED maze - timer started");
                
            if (guidanceController != null)
            {
                guidanceController.OnPlayerEnteredMazeArea();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerInMaze)
        {
            // Don't immediately exit - use a delay to prevent false exits
            // (like when player briefly steps outside trigger bounds)
            if (exitCheckCoroutine != null)
            {
                StopCoroutine(exitCheckCoroutine);
            }
            
            exitCheckCoroutine = StartCoroutine(CheckRealExit(other.transform));
        }
    }
    
    /// <summary>
    /// Check if player is really exiting or just briefly outside trigger
    /// </summary>
    private IEnumerator CheckRealExit(Transform playerTransform)
    {
        if (debugMode)
            Debug.Log("[ActualWorkingMazeDetector] ⏳ Checking if player really exited...");
            
        yield return new WaitForSeconds(exitDelay);
        
        // After delay, check if player is still outside the trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.bounds.Contains(playerTransform.position))
        {
            // Player is definitely outside
            playerInMaze = false;
            
            if (debugMode)
                Debug.Log("[ActualWorkingMazeDetector] ❌ Player EXITED maze - timer stopped");
                
            if (guidanceController != null)
            {
                guidanceController.OnPlayerExitedMazeArea();
            }
        }
        else
        {
            // False alarm - player came back
            if (debugMode)
                Debug.Log("[ActualWorkingMazeDetector] 🔄 False exit - player came back");
        }
        
        exitCheckCoroutine = null;
    }
    
    /// <summary>
    /// Force check current player state (for debugging)
    /// </summary>
    [ContextMenu("Check Player State")]
    public void CheckPlayerState()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider col = GetComponent<Collider>();
            bool isInside = col != null && col.bounds.Contains(player.transform.position);
            Debug.Log($"Player position: {player.transform.position}");
            Debug.Log($"Trigger bounds: {col.bounds}");
            Debug.Log($"Player inside trigger: {isInside}");
            Debug.Log($"playerInMaze state: {playerInMaze}");
        }
    }
    
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            // Draw trigger bounds
            Gizmos.color = playerInMaze ? new Color(0, 1, 0, 0.2f) : new Color(1, 1, 0, 0.2f);
            Gizmos.DrawCube(transform.position, col.bounds.size);
            
            // Draw wireframe
            Gizmos.color = playerInMaze ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * col.bounds.size.y * 0.6f, 
                playerInMaze ? "PLAYER IN MAZE" : "MAZE DETECTOR");
            #endif
        }
    }
}