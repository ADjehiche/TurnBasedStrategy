using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Initializes the card collection system when the game starts.
/// Place this in your Title Scene or first scene.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Card Collection")]
    [SerializeField] private bool initializeCollectionOnStart = true;
    [SerializeField] private GameObject cardCollectionPrefab;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Start()
    {
        // Create CardCollection if it doesn't exist
        if (CardCollection.Instance == null)
        {
            if (cardCollectionPrefab != null)
            {
                Instantiate(cardCollectionPrefab);
            }
            else
            {
                GameObject collectionObj = new GameObject("CardCollection");
                collectionObj.AddComponent<CardCollection>();
            }

            if (showDebugLogs)
            {
                Debug.Log("[GameInitializer] CardCollection created");
            }
        }

        // Initialize starting collection if needed
        if (initializeCollectionOnStart && CardCollection.Instance != null)
        {
            // Check if collection is already initialized
            if (CardCollection.Instance.OwnedCards.Count == 0)
            {
                CardCollection.Instance.InitializeStartingCollection();
                
                if (showDebugLogs)
                {
                    Debug.Log("[GameInitializer] Initialized starting card collection");
                }
            }
            else
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[GameInitializer] Collection already has {CardCollection.Instance.OwnedCards.Count} cards");
                }
            }
        }
    }

    /// <summary>
    /// Call this to reset the player's collection (new game)
    /// </summary>
    public void ResetCollection()
    {
        if (CardCollection.Instance != null)
        {
            CardCollection.Instance.ClearCollection();
            CardCollection.Instance.InitializeStartingCollection();
            Debug.Log("[GameInitializer] Collection reset and reinitialized");
        }
    }
}
