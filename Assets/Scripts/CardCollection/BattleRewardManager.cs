using UnityEngine;
using System.Collections;

/// <summary>
/// Manages post-battle rewards, including card selection.
/// Triggers when battle ends in victory.
/// </summary>
public class BattleRewardManager : MonoBehaviour
{
    public static BattleRewardManager Instance { get; private set; }

    [Header("Reward Settings")]
    [SerializeField] private bool showCardRewardAfterBattle = true;
    [SerializeField] private float delayBeforeReward = 2f;

    [Header("References")]
    [SerializeField] private CardRewardUI cardRewardUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // SAFETY: Create CardCollection if it doesn't exist (for testing from Battle_Template directly)
        EnsureCardCollectionExists();
    }

    /// <summary>
    /// Safety method to create CardCollection if it doesn't exist.
    /// This allows testing Battle_Template scene directly without going through TitleScene.
    /// </summary>
    private void EnsureCardCollectionExists()
    {
        if (CardCollection.Instance == null)
        {
            Debug.LogWarning("[BattleRewardManager] CardCollection not found! Creating it now...");
            GameObject collectionObj = new GameObject("CardCollection");
            collectionObj.AddComponent<CardCollection>();
            
            // Initialize with starting cards
            if (CardCollection.Instance != null && CardCollection.Instance.OwnedCards.Count == 0)
            {
                CardCollection.Instance.InitializeStartingCollection();
                Debug.Log("[BattleRewardManager] CardCollection created and initialized with 15 starting cards");
            }
        }
    }

    private void OnEnable()
    {
        BattleState.OnBattleOverChanged += HandleBattleEnd;
    }

    private void OnDisable()
    {
        BattleState.OnBattleOverChanged -= HandleBattleEnd;
    }

    private void HandleBattleEnd(bool isOver)
    {
        if (!isOver) return;

        // Check if player won (all enemies defeated)
        if (EnemyManager.Instance != null && EnemyManager.Instance.AllEnemiesDefeated())
        {
            Debug.Log("[BattleRewardManager] Battle won! Showing rewards...");
            
            // Disable TurnManager to stop the game
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.enabled = false;
                Debug.Log("[BattleRewardManager] Disabled TurnManager");
            }
            
            StartCoroutine(ShowRewardsAfterDelay());
        }
        else
        {
            Debug.Log("[BattleRewardManager] Battle lost - no rewards");
        }
    }

    private IEnumerator ShowRewardsAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeReward);

        if (showCardRewardAfterBattle)
        {
            ShowCardReward();
        }
    }

    private void ShowCardReward()
    {
        // Find CardRewardUI if not assigned
        if (cardRewardUI == null)
        {
            cardRewardUI = FindFirstObjectByType<CardRewardUI>();
        }

        if (cardRewardUI != null)
        {
            cardRewardUI.ShowRewardSelection();
        }
        else
        {
            Debug.LogWarning("[BattleRewardManager] CardRewardUI not found! Cannot show card reward.");
        }
    }
}
