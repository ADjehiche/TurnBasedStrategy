using UnityEngine;

public class LevelOneReturnManager : MonoBehaviour
{
    [SerializeField] private Transform player;     //  Player here
    [SerializeField] private GameObject enemyRoot;
    [SerializeField] private LevelOneCaptionController captionController; // Reference to caption controller

    void Start()
    {
        // Put the player back to where they entered the battle from
        if (GameSession.HasReturnPosition && player != null)
        {
            var p = GameSession.ReturnPosition;
            p.y = player.position.y;   
            player.position = p;
            
            player.rotation = Quaternion.Euler(0, 30, 0);

            GameSession.HasReturnPosition = false; 
        }

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
}