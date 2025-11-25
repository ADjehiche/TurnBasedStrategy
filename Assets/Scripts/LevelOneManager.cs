using UnityEngine;

public class LevelOneReturnManager : MonoBehaviour
{
    [SerializeField] private Transform player;     //  Player here
    [SerializeField] private GameObject enemyRoot;
    [SerializeField] private LevelOneCaptionController captionController; // Reference to caption controller

    void Start()
    {
        // Put the player back to where they entered the battle from
        if (player != null)
        {
            Vector3 spawnPosition;
            
            // Use battle trigger center if available (preferred), otherwise use return position
            if (GameSession.BattleTriggerCenter != Vector3.zero)
            {
                spawnPosition = GameSession.BattleTriggerCenter;
                
                // Move player backward (negative Z) to avoid re-triggering the battle trigger
                spawnPosition += new Vector3(0, 0, -3f); // 3 units back
                
                Debug.Log($"[LevelOneReturnManager] Using battle trigger center with offset: {spawnPosition}");
            }
            else if (GameSession.HasReturnPosition)
            {
                spawnPosition = GameSession.ReturnPosition;
                spawnPosition.y = player.position.y;
                Debug.Log($"[LevelOneReturnManager] Using return position: {spawnPosition}");
            }
            else
            {
                spawnPosition = player.position;
                Debug.Log("[LevelOneReturnManager] No saved position, keeping current position");
            }
            
            player.position = spawnPosition;
            // Rotation now uses whatever is set in Unity Editor (no override)

            GameSession.HasReturnPosition = false;
        }

        // Clean up items that were already collected
        CleanupCollectedItems();
        
        // Restore companion if it was active
        RestoreCompanion();

        if (enemyRoot != null && GameSession.EnemyDefeated)
        {
            // Instead of deactivating the entire enemyRoot, handle individual enemies
            HandleEnemiesAfterDefeat();
        }

        // Ensure caption controller is available if not assigned in inspector
        if (captionController == null)
        {
            captionController = FindFirstObjectByType<LevelOneCaptionController>();
        }
    }
    
    private void HandleEnemiesAfterDefeat()
    {
        // Find all enemies in the enemyRoot (including inactive ones)
        LevelOneEnemyAutoHide[] autoHideEnemies = enemyRoot.GetComponentsInChildren<LevelOneEnemyAutoHide>(true);
        
        Debug.Log($"[LevelOneReturnManager] Found {autoHideEnemies.Length} enemies with LevelOneEnemyAutoHide component");
        
        if (autoHideEnemies.Length > 0)
        {
            // If there are enemies with auto-hide components, let them handle their own death animations
            // Don't deactivate the enemyRoot - let individual enemies manage themselves
            Debug.Log("[LevelOneReturnManager] Letting enemies with LevelOneEnemyAutoHide handle their own death animations");
            
            // Make sure the enemyRoot is active so animations can play
            if (!enemyRoot.activeSelf)
            {
                Debug.Log("[LevelOneReturnManager] Activating enemyRoot to allow death animations");
                enemyRoot.SetActive(true);
            }
        }
        else
        {
            // No auto-hide enemies found, deactivate the entire enemyRoot as before
            Debug.Log("[LevelOneReturnManager] No auto-hide enemies found, deactivating enemyRoot");
            enemyRoot.SetActive(true);
        }
    }
    
    private void CleanupCollectedItems()
    {
        // Remove original key if it was collected
        if (GameSession.OriginalKeyCollected)
        {
            GameObject originalKey = GameObject.Find("Key");
            if (originalKey != null)
            {
                Debug.Log("[LevelOneReturnManager] Destroying original key (already collected)");
                Destroy(originalKey);
            }
        }
        
        // Handle door state if it was opened
        if (GameSession.DoorOpened)
        {
            GameObject doorTriggerObj = GameObject.Find("Door");
            if (doorTriggerObj != null)
            {
                DoorTrigger doorTrigger = doorTriggerObj.GetComponent<DoorTrigger>();
                
                if (doorTrigger != null)
                {
                    // Instantly open the door instead of destroying it
                    Debug.Log("[LevelOneReturnManager] Door was already opened - setting to open position");
                    
                    // Get the door GameObject reference from the trigger
                    GameObject door = doorTrigger.GetType()
                        .GetField("door", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.GetValue(doorTrigger) as GameObject;
                    
                    if (door == null)
                    {
                        // Fallback: assume the door is the same GameObject
                        door = doorTriggerObj;
                    }
                    
                    // Get open settings from the trigger (default values if not accessible)
                    float openAngle = -90f;  // Default from DoorTrigger
                    bool useYAxis = true;    // Default from DoorTrigger
                    
                    // Rotate door to open position instantly
                    Vector3 axis = useYAxis ? Vector3.up : Vector3.right;
                    door.transform.Rotate(axis, openAngle, Space.World);
                    
                    // Remove the trigger component so door can't be interacted with again
                    Destroy(doorTrigger);
                    Debug.Log("[LevelOneReturnManager] Door opened instantly and trigger removed");
                }
            }
        }
    }
    
    private void RestoreCompanion()
    {
        // Check if companion was active before battle
        if (!GameSession.CompanionActive)
        {
            return; // Companion not active, nothing to restore
        }
        
        // Find companion in scene
        GameObject companionObj = GameObject.FindGameObjectWithTag("Companion");
        
        if (companionObj == null)
        {
            Debug.LogWarning("[LevelOneReturnManager] Companion was active but not found in scene! Make sure CompanionBlob has 'Companion' tag.");
            return;
        }
        
        CompanionFollower companion = companionObj.GetComponent<CompanionFollower>();
        
        if (companion != null && player != null)
        {
            // Position companion near player
            Vector3 companionPosition = player.position + new Vector3(-1.5f, 1f, -1.5f);
            companionObj.transform.position = companionPosition;
            
            // Activate following
            companion.SetFollowing(true);
            
            Debug.Log("[LevelOneReturnManager] Companion restored and following player");
        }
        else
        {
            Debug.LogWarning("[LevelOneReturnManager] Companion found but missing CompanionFollower component!");
        }
    }
}