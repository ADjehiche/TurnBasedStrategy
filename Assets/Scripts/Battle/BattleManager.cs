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
            Debug.Log("Battle ended - returning to LevelOne after delay");
            Invoke("ReturnToLevelOne", delayBeforeReturn);
        }
    }
    
    private void ReturnToLevelOne()
    {
        GameSession.EnemyDefeated = true;
        Debug.Log($"[BattleManager] Setting EnemyDefeated to true, GameManager exists: {GameManager.Instance != null}");
        
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
        
        SceneManager.LoadScene("LevelOne", LoadSceneMode.Single);
    }
}