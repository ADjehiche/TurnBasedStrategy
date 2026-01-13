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
            cardRewardUI = FindObjectOfType<CardRewardUI>();
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
