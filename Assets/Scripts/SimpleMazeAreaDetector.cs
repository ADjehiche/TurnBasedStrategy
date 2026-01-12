using UnityEngine;

/// <summary>
/// Simple approach: Two separate triggers for maze entry and exit
/// Place one trigger just outside the maze entrance (Entry Trigger)
/// Place another trigger just inside the maze entrance (Exit Trigger)
/// 
/// This avoids directional detection complexity
/// </summary>
public class SimpleMazeAreaDetector : MonoBehaviour
{
    [Header("Trigger Setup")]
    [SerializeField] private bool isEntryTrigger = true; // true = entry trigger, false = exit trigger
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private MazeGuidanceController guidanceController;
    
    void Start()
    {
        guidanceController = FindFirstObjectByType<MazeGuidanceController>();
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError("[SimpleMazeAreaDetector] No collider found!");
        }
        
        if (debugMode)
        {
            Debug.Log($"[SimpleMazeAreaDetector] Setup as {(isEntryTrigger ? "ENTRY" : "EXIT")} trigger");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isEntryTrigger)
            {
                // Player entered the entry trigger = entering maze
                if (debugMode)
                    Debug.Log("[SimpleMazeAreaDetector] Player entering maze (entry trigger)");
                    
                if (guidanceController != null)
                {
                    guidanceController.OnPlayerEnteredMazeArea();
                }
            }
            else
            {
                // Player entered the exit trigger = exiting maze  
                if (debugMode)
                    Debug.Log("[SimpleMazeAreaDetector] Player exiting maze (exit trigger)");
                    
                if (guidanceController != null)
                {
                    guidanceController.OnPlayerExitedMazeArea();
                }
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (isEntryTrigger)
            {
                Gizmos.color = new Color(0, 1, 0, 0.3f); // Green for entry
                Gizmos.DrawCube(transform.position, col.bounds.size);
                
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, "MAZE ENTRY");
                #endif
            }
            else
            {
                Gizmos.color = new Color(1, 0, 0, 0.3f); // Red for exit
                Gizmos.DrawCube(transform.position, col.bounds.size);
                
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, "MAZE EXIT");
                #endif
            }
        }
    }
}