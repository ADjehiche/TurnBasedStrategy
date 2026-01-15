using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Handles player positioning and state restoration when returning to Level Two
/// Mirrors LevelOneReturnManager functionality
/// </summary>
public class LevelTwoReturnManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject[] enemyRoots; // Combat Wing enemies (assign both skeletons)
    
    [Header("Battle Triggers to Disable")]
    [Tooltip("Battle triggers that should be disabled after enemies are defeated")]
    [SerializeField] private GameObject[] combatWingTriggers;
    
    [Header("Boss Door Cutscene")]
    [Tooltip("Timeline cutscene to play when returning after both fragments collected")]
    [SerializeField] private PlayableDirector bossDoorCutscene;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    void Start()
    {
        // Position player correctly after battle
        HandlePlayerPosition();
        
        // Handle enemy state (death animations, cleanup)
        HandleEnemyState();
        
        // Disable battle triggers if enemies defeated
        HandleBattleTriggers();
        
        // Restore red companion if it was active
        RestoreRedCompanion();
        
        // Notify objectives system if skeletons were defeated
        if (GameSession.EnemyDefeated || GameSession.CombatWingVictory)
        {
            NotifySkeletonsDefeated();
        }
        
        // Play boss door cutscene if returning from flashback with both fragments
        if (GameSession.HasPlayedRageFlashback && GameSession.CanUnlockBossDoor)
        {
            TriggerBossDoorCutscene();
        }
    }
    
    /// <summary>
    /// Play boss door cutscene when both fragments collected
    /// </summary>
    private void TriggerBossDoorCutscene()
    {
        if (bossDoorCutscene != null)
        {
            // Small delay to let scene initialize
            StartCoroutine(PlayCutsceneDelayed());
        }
    }
    
    private System.Collections.IEnumerator PlayCutsceneDelayed()
    {
        yield return new WaitForSeconds(1f);
        
        if (debugMode) Debug.Log("[LevelTwoReturnManager] 🏰 Playing boss door cutscene!");
        bossDoorCutscene.Play();
    }
    
    /// <summary>
    /// Notify objectives system that skeletons were defeated
    /// </summary>
    private void NotifySkeletonsDefeated()
    {
        SimpleLevelTwoObjectives objectives = FindFirstObjectByType<SimpleLevelTwoObjectives>();
        if (objectives != null)
        {
            objectives.OnSkeletonsDefeated();
            if (debugMode) Debug.Log("[LevelTwoReturnManager] Notified objectives: Skeletons defeated");
        }
    }
    
    /// <summary>
    /// Position player at battle trigger location (not spawn point)
    /// </summary>
    private void HandlePlayerPosition()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogError("[LevelTwoReturnManager] Player not found!");
                return;
            }
        }
        
        Vector3 spawnPosition = player.position;
        Quaternion spawnRotation = player.rotation;
        
        // Check if respawning from death
        if (GameSession.IsRespawning && GameSession.HasCheckpoint)
        {
            spawnPosition = GameSession.CheckpointPosition;
            spawnRotation = GameSession.CheckpointRotation;
            spawnPosition += new Vector3(0, 0, -3f); // Move back to avoid re-trigger
            
            if (debugMode) Debug.Log($"[LevelTwoReturnManager] ⚰️ Respawning at checkpoint: {spawnPosition}");
            
            // Reset player health
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ResetHealth();
            }
            
            GameSession.IsRespawning = false;
        }
        // Skip positioning if boss was defeated - BossRoomManager handles this
        else if (GameSession.BossDefeated)
        {
            if (debugMode) Debug.Log("[LevelTwoReturnManager] 👹 Boss battle return - skipping (BossRoomManager handles positioning)");
            return;
        }
        // Use battle trigger center if returning from battle
        else if (GameSession.BattleTriggerCenter != Vector3.zero && GameSession.EnemyDefeated)
        {
            spawnPosition = GameSession.BattleTriggerCenter;
            spawnPosition += new Vector3(0, 0, -3f); // Move back 3 units to avoid re-trigger
            
            if (debugMode) Debug.Log($"[LevelTwoReturnManager] 🗡️ Returning from battle to: {spawnPosition}");
        }
        // Use return position if available
        else if (GameSession.HasReturnPosition)
        {
            spawnPosition = GameSession.ReturnPosition;
            spawnPosition.y = player.position.y;
            
            if (debugMode) Debug.Log($"[LevelTwoReturnManager] Using return position: {spawnPosition}");
        }
        else
        {
            if (debugMode) Debug.Log("[LevelTwoReturnManager] No saved position, keeping current position");
            return; // Don't change position
        }
        
        player.position = spawnPosition;
        player.position = new Vector3(player.position.x, 1.5f, player.position.z); // Always spawn at y=1.5
        player.rotation = spawnRotation;
        
        // Clear the return position flag
        GameSession.HasReturnPosition = false;
    }
    
    /// <summary>
    /// Handle enemy death animations or cleanup
    /// </summary>
    private void HandleEnemyState()
    {
        if (enemyRoots == null || enemyRoots.Length == 0 || !GameSession.EnemyDefeated) return;
        
        foreach (GameObject enemyRoot in enemyRoots)
        {
            if (enemyRoot == null) continue;
            
            // Find enemies with death components
            Animator[] animators = enemyRoot.GetComponentsInChildren<Animator>(true);
            
            foreach (Animator anim in animators)
            {
                // Trigger death animation if it has one
                if (anim.HasState(0, Animator.StringToHash("Death")) || 
                    anim.HasState(0, Animator.StringToHash("Die")))
                {
                    anim.SetTrigger("Die");
                    if (debugMode) Debug.Log($"[LevelTwoReturnManager] Triggering death animation on {anim.gameObject.name}");
                }
            }
            
            // Also check for LevelOneEnemyAutoHide components (reused from Level One)
            var autoHideEnemies = enemyRoot.GetComponentsInChildren<LevelOneEnemyAutoHide>(true);
            if (autoHideEnemies.Length > 0)
            {
                if (debugMode) Debug.Log($"[LevelTwoReturnManager] Found {autoHideEnemies.Length} auto-hide enemies in {enemyRoot.name}");
                // They handle their own death animations
            }
        }
    }
    
    /// <summary>
    /// Disable battle triggers after enemies defeated
    /// </summary>
    private void HandleBattleTriggers()
    {
        if (!GameSession.EnemyDefeated && !GameSession.CombatWingVictory) return;
        
        // Disable assigned triggers
        if (combatWingTriggers != null)
        {
            foreach (GameObject trigger in combatWingTriggers)
            {
                if (trigger != null)
                {
                    trigger.SetActive(false);
                    if (debugMode) Debug.Log($"[LevelTwoReturnManager] Disabled trigger: {trigger.name}");
                }
            }
        }
        
        // Also find any BattleTrigger components and disable them
        BattleTrigger[] triggers = FindObjectsByType<BattleTrigger>(FindObjectsSortMode.None);
        foreach (BattleTrigger trigger in triggers)
        {
            // Check if this trigger uses Battle_2 (Combat Wing battle)
            var sceneName = trigger.GetType()
                .GetField("battleSceneName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(trigger) as string;
                
            if (sceneName == "Battle_2")
            {
                trigger.gameObject.SetActive(false);
                if (debugMode) Debug.Log($"[LevelTwoReturnManager] Auto-disabled Battle_2 trigger: {trigger.gameObject.name}");
            }
        }
    }
    
    [Header("Red Companion Restore")]
    [SerializeField] private GameObject redCompanionPrefab;
    
    /// <summary>
    /// Restore the red companion if it was active before scene change
    /// </summary>
    private void RestoreRedCompanion()
    {
        if (!GameSession.RedCompanionActive) return;
        
        // Check if red companion already exists in scene
        CompanionFollower[] existingCompanions = FindObjectsByType<CompanionFollower>(FindObjectsSortMode.None);
        foreach (var comp in existingCompanions)
        {
            if (comp.gameObject.name.Contains("Red") || comp.gameObject.name.Contains("Rage"))
            {
                if (debugMode) Debug.Log("[LevelTwoReturnManager] Red companion already exists, skipping spawn");
                return;
            }
        }
        
        if (redCompanionPrefab == null)
        {
            Debug.LogWarning("[LevelTwoReturnManager] Red Companion prefab not assigned! Cannot restore.");
            return;
        }
        
        // Spawn near player
        Vector3 spawnPos = player != null ? player.position : Vector3.zero;
        spawnPos += Vector3.left * 2f + Vector3.up * 0.5f;
        
        GameObject companion = Instantiate(redCompanionPrefab, spawnPos, Quaternion.identity);
        
        CompanionFollower follower = companion.GetComponent<CompanionFollower>();
        if (follower != null)
        {
            follower.StartFollowing();
        }
        
        if (debugMode) Debug.Log("[LevelTwoReturnManager] 🔴 Red companion restored!");
    }
}
