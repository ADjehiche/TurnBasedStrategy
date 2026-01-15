using UnityEngine;

/// <summary>
/// Manages card rewards in exploration areas (chests, interactions, etc.)
/// Shows ONLY starter cards (simpler rewards for exploration vs battle)
/// Can be called from chest scripts, trigger zones, or interaction scripts
/// </summary>
public class ExplorationRewardManager : MonoBehaviour
{
    public static ExplorationRewardManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CardRewardUI cardRewardUI;

    [Header("Reward Settings")]
    [SerializeField] private int numberOfOptions = 2; // How many cards to show
    [SerializeField] private bool allowDuplicates = true; // Can show same card type twice

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Ensure CardCollection exists (important for exploration scenes)
        EnsureCardCollectionExists();
    }

    /// <summary>
    /// Safety method to create CardCollection if it doesn't exist.
    /// Important for exploration scenes that may be tested directly.
    /// </summary>
    private void EnsureCardCollectionExists()
    {
        if (CardCollection.Instance == null)
        {
            Debug.LogWarning("[ExplorationRewardManager] CardCollection not found! Creating it now...");
            GameObject collectionObj = new GameObject("CardCollection");
            collectionObj.AddComponent<CardCollection>();
            
            // Initialize with starting cards
            if (CardCollection.Instance != null && CardCollection.Instance.OwnedCards.Count == 0)
            {
                CardCollection.Instance.InitializeStartingCollection();
                Debug.Log("[ExplorationRewardManager] CardCollection created and initialized with starter cards");
            }
        }
    }

    /// <summary>
    /// Show card reward selection with ONLY starter cards
    /// Call this from chest scripts, trigger zones, or interaction handlers
    /// </summary>
    public void ShowExplorationReward()
    {
        if (CardCollection.Instance == null)
        {
            Debug.LogError("[ExplorationRewardManager] CardCollection.Instance is null! Cannot show rewards.");
            return;
        }

        // Find CardRewardUI if not assigned
        if (cardRewardUI == null)
        {
            cardRewardUI = FindFirstObjectByType<CardRewardUI>();
        }

        if (cardRewardUI == null)
        {
            Debug.LogError("[ExplorationRewardManager] CardRewardUI not found! Add it to your exploration scene.");
            return;
        }

        // Lock player movement during reward selection
        LockPlayerMovement(true);

        // Show the reward UI with starter cards only
        cardRewardUI.ShowExplorationReward(numberOfOptions);

        Debug.Log("[ExplorationRewardManager] Showing exploration card reward");
    }

    /// <summary>
    /// Call this after player selects a card to unlock movement
    /// </summary>
    public void OnRewardClaimed()
    {
        LockPlayerMovement(false);
        Debug.Log("[ExplorationRewardManager] Reward claimed, player movement unlocked");
    }

    /// <summary>
    /// Lock/unlock player movement during reward selection
    /// Also manages cursor visibility for UI interaction
    /// </summary>
    private void LockPlayerMovement(bool locked)
    {
        if (PlayerMovementLock.Instance != null)
        {
            if (locked)
            {
                PlayerMovementLock.Instance.LockMovement("ExplorationReward");
            }
            else
            {
                PlayerMovementLock.Instance.UnlockMovement("ExplorationReward");
            }
        }
        else
        {
            Debug.LogWarning("[ExplorationRewardManager] PlayerMovementLock not found! Movement not locked.");
        }

        // CRITICAL: Unlock cursor for card selection UI
        if (locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("[ExplorationRewardManager] Cursor unlocked for card selection");
        }
        else
        {
            // Return to locked cursor for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("[ExplorationRewardManager] Cursor locked for gameplay");
        }
    }

    /// <summary>
    /// Static helper method - call from any script to show reward
    /// Example: ExplorationRewardManager.ShowReward();
    /// </summary>
    public static void ShowReward()
    {
        if (Instance != null)
        {
            Instance.ShowExplorationReward();
        }
        else
        {
            Debug.LogError("[ExplorationRewardManager] Instance not found! Add ExplorationRewardManager to your scene.");
        }
    }
}
