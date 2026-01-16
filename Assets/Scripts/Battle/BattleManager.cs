using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private float delayBeforeReturn = 1.5f;
    
    private void Awake()
    {
        BattleState.Reset();
    }
    
    private void OnEnable()
    {
        BattleState.OnBattleOverChanged += HandleBattleStateChanged;
    }

    private void OnDisable()
    {
        BattleState.OnBattleOverChanged -= HandleBattleStateChanged;
    }

    private void HandleBattleStateChanged(bool isOver)
    {
        if (isOver)
        {
            // Check if player won (all enemies defeated)
            bool playerWon = EnemyManager.Instance != null && EnemyManager.Instance.AllEnemiesDefeated();
            
            if (playerWon && BattleRewardManager.Instance != null)
            {
                Debug.Log("[BattleManager] Battle won - waiting for reward selection before returning to level");
                // Don't return to level yet - let BattleRewardManager handle it
                // The reward UI will call ReturnToLevelOne() when player selects/skips reward
            }
            else
            {
                // Player lost or no reward system - return immediately
                Debug.Log("[BattleManager] Battle lost - returning to LevelOne after delay");
                Invoke("ReturnToLevelOne", delayBeforeReturn);
            }
        }
    }
    
    /// <summary>
    /// Call this after rewards are selected to return to exploration
    /// </summary>
    public void ReturnToLevelOne()
    {
        GameSession.LevelOneEnemyDefeated = true;
        Debug.Log($"[BattleManager] Setting EnemyDefeated to true, GameManager exists: {GameManager.Instance != null}");
        
        // Check if this was a Combat Wing battle (Battle_2 from LevelTwo)
        if (GameSession.BattleSceneName == "Battle_2" && GameSession.ReturnSceneName == "LevelTwo")
        {
            GameSession.CombatWingVictory = true;
            GameSession.RedFragmentSpawnPosition = GameSession.ReturnPosition;
            Debug.Log("[BattleManager] Combat Wing victory - Red Fragment will spawn on return");
        }
        
        // Check if this was the Boss battle (Battle_Boss from LevelTwo)
        if (GameSession.BattleSceneName == "Battle_Boss" && GameSession.ReturnSceneName == "LevelTwo")
        {
            GameSession.BossDefeated = true;
            Debug.Log("[BattleManager] Boss defeated! Warden dialogue and purple fragment will spawn on return");
        }
        
        // The skeleton defeat objective will be triggered when level loads
        Debug.Log($"[BattleManager] Returning to: {GameSession.ReturnSceneName}");
        
        // Try to ensure GameManager state is set
        if (GameManager.Instance != null)
        {
            Debug.Log("[BattleManager] GameManager found, ensuring hasSavedState is true");
            GameManager.Instance.hasSavedState = true;
        }
        else
        {
            Debug.LogWarning("[BattleManager] GameManager instance not found, using direct scene loading");
        }
        
        // Return to the scene we came from (dynamic - works for both LevelOne and LevelTwo)
        SceneManager.LoadScene(GameSession.ReturnSceneName, LoadSceneMode.Single);
    }
}