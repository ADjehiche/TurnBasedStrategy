using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStateHandler : MonoBehaviour
{
    private const string BATTLE_TRIGGER_TAG = "battleTrigger";
    private const string ENEMY_TAG = "Enemy";

    void Start()
    {
        Debug.Log($"[PlayerStateHandler] Start - GameManager exists: {GameManager.Instance != null}");
        Debug.Log($"[PlayerStateHandler] Start - hasSavedState: {(GameManager.Instance != null ? GameManager.Instance.hasSavedState : "null")}");
        Debug.Log($"[PlayerStateHandler] Start - GameSession.LevelOneEnemyDefeated: {GameSession.LevelOneEnemyDefeated}");
        
        // Check if we need to restore state after battle
        bool shouldRestoreState = false;
        
        if (GameManager.Instance != null && GameManager.Instance.hasSavedState)
        {
            // Restore player position
            transform.position = GameManager.Instance.playerPosition;
            Debug.Log($"[PlayerStateHandler] Restored player position to: {transform.position}");
            shouldRestoreState = true;
        }
        
        // Always check if enemy was defeated and handle battle trigger accordingly
        if (GameSession.LevelOneEnemyDefeated || shouldRestoreState)
        {
            Debug.Log("[PlayerStateHandler] Enemy defeated or state restored - cleaning up battle triggers and enemies");
            DestroyBattleTriggerAndEnemy();
        }
    }

    void OnDisable()
    {
        if (SceneManager.GetActiveScene().name == "LevelOne")
        {
            GameManager.Instance.playerPosition = transform.position;
            GameManager.Instance.hasSavedState = true;
        }
    }

    private void DestroyBattleTriggerAndEnemy()
    {
        // Always destroy battle triggers after battle
        GameObject[] battleTriggers = GameObject.FindGameObjectsWithTag(BATTLE_TRIGGER_TAG);
        Debug.Log($"[PlayerStateHandler] Found {battleTriggers.Length} battle triggers with tag '{BATTLE_TRIGGER_TAG}'");
        
        foreach (GameObject trigger in battleTriggers)
        {
            Debug.Log($"[PlayerStateHandler] Destroying battle trigger: {trigger.name}");
            Destroy(trigger);
        }

        // Enemy handling is now managed by LevelOneReturnManager
        Debug.Log("[PlayerStateHandler] Enemy handling delegated to LevelOneReturnManager");
    }
}