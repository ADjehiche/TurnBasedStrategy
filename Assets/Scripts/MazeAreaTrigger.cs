using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Trigger zone to detect when player enters/exits the maze area
/// Uses position-based logic to determine if player is entering or exiting the maze
/// 
/// Setup: Place trigger at maze entrance, with the maze area in the FORWARD direction (+Z)
/// </summary>
public class MazeAreaTrigger : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnPlayerEntered;
    public UnityEvent OnPlayerExited;
    
    [Header("Direction Setup")]
    [SerializeField] private Transform mazeDirection; // Point this towards the maze interior
    [SerializeField] private bool autoDetectDirection = true; // Auto-detect based on trigger position
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private bool playerInside = false;
    private bool mazeObjectiveTriggered = false; // Separate flag for objective (triggers once, any direction)
    private MazeGuidanceController guidanceController;
    private Vector3 mazeDirectionVector;
    
    void Start()
    {
        // Find the guidance controller
        guidanceController = FindFirstObjectByType<MazeGuidanceController>();
        
        // Ensure this is a trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError("[MazeAreaTrigger] No collider found! Add a collider and set it as trigger.");
        }
        
        // Setup direction vector
        if (mazeDirection != null)
        {
            mazeDirectionVector = (mazeDirection.position - transform.position).normalized;
        }
        else if (autoDetectDirection)
        {
            // Default: assume maze is in the forward direction of this trigger
            mazeDirectionVector = transform.forward;
        }
        else
        {
            mazeDirectionVector = Vector3.forward; // Default fallback
        }
        
        if (debugMode)
        {
            Debug.Log($"[MazeAreaTrigger] Maze direction set to: {mazeDirectionVector}");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[MazeAreaTrigger] OnTriggerEnter called by: {other.gameObject.name}, tag: {other.tag}, playerInside: {playerInside}");
        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            // OBJECTIVE TRIGGER (fires once, regardless of direction)
            if (!mazeObjectiveTriggered)
            {
                mazeObjectiveTriggered = true;
                Debug.Log("[MazeAreaTrigger] ===== OBJECTIVE TRIGGER (DIRECTION-INDEPENDENT) =====");
                
                SimpleLevelTwoObjectives objectiveManager = FindFirstObjectByType<SimpleLevelTwoObjectives>();
                if (objectiveManager != null)
                {
                    Debug.Log("[MazeAreaTrigger] Calling objectiveManager.OnMazeEntered()");
                    objectiveManager.OnMazeEntered();
                }
                else
                {
                    Debug.LogError("[MazeAreaTrigger] SimpleLevelTwoObjectives NOT found!");
                }
            }
            
            // Direction-based logic (for guidance controller enter/exit tracking)
            Vector3 playerPosition = other.transform.position;
            Vector3 triggerToPlayer = (playerPosition - transform.position).normalized;
            float dot = Vector3.Dot(triggerToPlayer, mazeDirectionVector);
            Debug.Log($"[MazeAreaTrigger] Dot product: {dot}, mazeDirectionVector: {mazeDirectionVector}");

            if (dot > 0 && !playerInside)
            {
                playerInside = true;
                Debug.Log("[MazeAreaTrigger] Player entered maze area (direction-based)");
                OnPlayerEntered?.Invoke();
                
                if (guidanceController != null)
                {
                    guidanceController.OnPlayerEnteredMazeArea();
                }
            }
            else if (dot < 0 && playerInside)
            {
                playerInside = false;
                Debug.Log("[MazeAreaTrigger] Player exited maze area (direction-based)");
                OnPlayerExited?.Invoke();
                
                if (guidanceController != null)
                {
                    guidanceController.OnPlayerExitedMazeArea();
                }
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // We'll use this as a backup to ensure proper state
        if (other.CompareTag("Player"))
        {
            // Check direction player is exiting
            Vector3 playerPosition = other.transform.position;
            Vector3 triggerToPlayer = (playerPosition - transform.position).normalized;
            float dot = Vector3.Dot(triggerToPlayer, mazeDirectionVector);
            
            // If player is exiting towards the outside (opposite of maze direction) and was inside
            if (dot < 0 && playerInside)
            {
                playerInside = false;
                
                if (debugMode)
                    Debug.Log("[MazeAreaTrigger] Player exited maze area (OnTriggerExit backup)");
                
                OnPlayerExited?.Invoke();
                
                if (guidanceController != null)
                {
                    guidanceController.OnPlayerExitedMazeArea();
                }
            }
        }
    }
    
    /// <summary>
    /// Check if player is currently inside the maze area
    /// </summary>
    public bool IsPlayerInside()
    {
        return playerInside;
    }
    
    private void OnDrawGizmos()
    {
        // Draw the trigger area in editor
        Gizmos.color = playerInside ? Color.green : Color.yellow;
        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawCube(transform.position, col.bounds.size);
        }
        
        // Draw direction arrow showing maze direction
        Vector3 direction = mazeDirection != null ? 
            (mazeDirection.position - transform.position).normalized : 
            transform.forward;
            
        Gizmos.color = Color.cyan;
        Vector3 arrowStart = transform.position;
        Vector3 arrowEnd = arrowStart + direction * 2f;
        
        // Draw arrow line
        Gizmos.DrawLine(arrowStart, arrowEnd);
        
        // Draw arrow head
        Vector3 arrowHead1 = arrowEnd + Quaternion.Euler(0, 150, 0) * direction * 0.5f;
        Vector3 arrowHead2 = arrowEnd + Quaternion.Euler(0, -150, 0) * direction * 0.5f;
        Gizmos.DrawLine(arrowEnd, arrowHead1);
        Gizmos.DrawLine(arrowEnd, arrowHead2);
        
        // Label
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(arrowEnd + Vector3.up * 0.5f, "MAZE DIRECTION");
        #endif
    }
}