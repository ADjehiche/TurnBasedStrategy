using UnityEngine;
using UnityEngine.UI;
using System;

public class TurnManager : MonoBehaviour
{
    // Singleton pattern to ensure only one instance
    public static TurnManager Instance { get; private set; }
    
    public enum TurnState
    {
        PlayerTurn,
        EnemyTurn,
        None
    }

    public TurnState CurrentTurn { get; private set; } = TurnState.None;
    
    public event Action<TurnState> OnTurnChanged;
    
    [Header("UI References")]
    [SerializeField] private Button endTurnButton;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;


    [Header("Turn Resources")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private HandManager handManager;
    [SerializeField] private PlayerStamina playerStamina;

    [Header("Turn Rules")]
    [SerializeField] private int cardsPerTurn = 4;
    [SerializeField] private bool refillStaminaEachTurn = true;



    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"[TurnManager] DUPLICATE INSTANCE DETECTED! Destroying {gameObject.name}. Active instance is on {Instance.gameObject.name}");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        if (enableDebugLogs)
        {
            Debug.Log($"[TurnManager] Awake called on {gameObject.name} (InstanceID: {GetInstanceID()})");
        }
    }
    
    private void Start()
    {
        // Setup button listener programmatically to ensure correct reference
        if (endTurnButton == null)
        {
            endTurnButton = GameObject.Find("EndTurnButton")?.GetComponent<Button>();
        }

        if (endTurnButton != null)
        {
            // Remove all existing listeners to prevent duplicates
            endTurnButton.onClick.RemoveAllListeners();

            // Add listener programmatically
            endTurnButton.onClick.AddListener(OnEndTurnButtonPressed);

            if (enableDebugLogs)
            {
                Debug.Log($"[TurnManager] EndTurnButton listener added successfully to {endTurnButton.gameObject.name}");
            }
        }
        else
        {
            Debug.LogError("[TurnManager] EndTurnButton not found! Please assign it in the inspector or ensure it exists in the scene.");
        }

        if (deckManager == null)   deckManager   = FindObjectOfType<DeckManager>();
        if (handManager == null)   handManager   = FindObjectOfType<HandManager>();
        if (playerStamina == null) playerStamina = FindObjectOfType<PlayerStamina>();

        StartPlayerTurn();
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[TurnManager] OnDestroy called on {gameObject.name}");
        }
    }

    public void StartPlayerTurn()
    {
      
        CurrentTurn = TurnState.PlayerTurn;

        if (enableDebugLogs)
            Debug.Log($"[TurnManager] StartPlayerTurn -> PlayerTurn");

        // Tick player buff status effects (reflect, dodge, invisibility, etc.) at start of turn
        if (PlayerStatusEffects.Instance != null)
        {
            PlayerStatusEffects.Instance.TickStatuses();
        }

        // Tick player debuff status effects (bleed, weakness) at start of turn
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.TickStatuses();
        }

        // Tick status effects (bleed, weaken, etc.) for ALL enemies at the start of player turn
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.TickAllEnemyStatuses();
        }
        else
        {
            // Fallback for single enemy (backwards compatibility)
            var enemy = UnityEngine.Object.FindFirstObjectByType<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TickStatuses();
            }
        }

        // Refill stamina at the start of the turn
        if (refillStaminaEachTurn && playerStamina != null)
            playerStamina.Refill();

        // Draw fresh hand for the new turn
        DrawCardsForPlayerTurn();
       

        // Enable the End Turn button
        if (endTurnButton != null)
            endTurnButton.interactable = true;

        OnTurnChanged?.Invoke(CurrentTurn);
    }
    
    
    public void OnEndTurnButtonPressed()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[TurnManager] OnEndTurnButtonPressed called. CurrentTurn: {CurrentTurn}, Instance: {GetInstanceID()}, GameObject: {gameObject.name}");
        }
        
        if (Instance != this)
        {
            Debug.LogError($"[TurnManager] Button called on WRONG INSTANCE! This is {gameObject.name} (ID: {GetInstanceID()}), but Instance is {Instance?.gameObject.name} (ID: {Instance?.GetInstanceID()})");
            return;
        }
        
        if (CurrentTurn != TurnState.PlayerTurn)
        {
            Debug.LogWarning($"[TurnManager] Button pressed but it is NOT PlayerTurn ({CurrentTurn}).");
            return;
        }
        
        EndPlayerTurn();
    }

    private void EndPlayerTurn()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[TurnManager] EndPlayerTurn called. Transitioning from {CurrentTurn}");
        }

        CurrentTurn = TurnState.EnemyTurn;

        if (endTurnButton != null)
        {
            endTurnButton.interactable = false;
        }
        
        // Clear temporary stamina at end of player turn
        if (PlayerStamina.Instance != null)
        {
            PlayerStamina.Instance.ClearTemporaryStamina();
        }
        
        // Discard all remaining (unplayed) cards at end of player turn
        if (handManager != null)
            handManager.DiscardAllInHand();

        StartCoroutine(ProcessEnemyTurn());
    }
    
    private void DrawCardsForPlayerTurn()
    {
        if (deckManager == null || handManager == null) return;

        // NEW: Use CardCollection's rule-based drawing if available
        if (CardCollection.Instance != null)
        {
            // CRITICAL: Reshuffle if draw pile is low (not just empty)
            // This prevents ending up with only 1-2 cards in the last hand
            if (deckManager.DrawPile.Count < cardsPerTurn && deckManager.DiscardPile.Count > 0)
            {
                Debug.Log($"[TurnManager] ⚠️ Draw pile low ({deckManager.DrawPile.Count} cards, need {cardsPerTurn}). Reshuffling discard pile.");
                deckManager.DrawPile.AddRange(deckManager.DiscardPile);
                deckManager.DiscardPile.Clear();
                DeckManager.Shuffle(deckManager.DrawPile);
                Debug.Log($"[TurnManager] ✅ Reshuffled. Draw pile now has {deckManager.DrawPile.Count} cards.");
            }

            var hand = CardCollection.Instance.DrawHandWithRules(deckManager.DrawPile, cardsPerTurn);
            
            if (hand.Count == 0)
            {
                Debug.LogWarning("[TurnManager] No cards could be drawn with rules!");
                return;
            }

            // Remove drawn cards from draw pile
            foreach (var card in hand)
            {
                deckManager.DrawPile.Remove(card);
            }

            // Add to hand display
            foreach (var card in hand)
            {
                handManager.AddCardToHand(card);
            }

            Debug.Log($"[TurnManager] Drew {hand.Count} cards with composition rules");
            return;
        }

        // Fallback: Original draw logic
        var cards = deckManager.Draw(cardsPerTurn);
        if (cards == null || cards.Count == 0) return;

        Debug.Log($"[TurnManager] Drew {cards.Count} cards (fallback method)");

        foreach (var c in cards)
            handManager.AddCardToHand(c);
    }
    
    private System.Collections.IEnumerator ProcessEnemyTurn()
    {
        if (enableDebugLogs)
        {
            Debug.Log("[TurnManager] Processing enemy turn...");
        }

        // Wait before enemies act
        yield return new WaitForSeconds(2f);

        // Use EnemyManager to execute all enemy turns
        if (EnemyManager.Instance != null)
        {
            yield return EnemyManager.Instance.ExecuteAllEnemyTurns();
        }
        else
        {
            // Fallback for single enemy (backwards compatibility)
            Debug.LogWarning("[TurnManager] No EnemyManager found, using legacy single-enemy attack");
            
            // Play skeleton scream before attack
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("SkeletonScream");
            }
            
            // Small delay after scream, then play slash sound (during attack animation)
            yield return new WaitForSeconds(0.5f);
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("SkeletonSlash");
            }
            
            // Brief moment for slash to register, then apply damage
            yield return new WaitForSeconds(0.2f);
            
            // Enemy attacks player
            int damage = UnityEngine.Random.Range(1, 6);

            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.TakeDamage(damage);

                if (enableDebugLogs)
                    Debug.Log($"[TurnManager] Enemy attacked player for {damage} damage!");
            }
            else
            {
                Debug.LogWarning("[TurnManager] Enemy tried to attack, but no PlayerHealth instance found!");
            }
        }
        
        // Return to player turn
        StartPlayerTurn();
    }
    
    // Optional: Method to manually check for duplicate instances
    [ContextMenu("Check For Duplicate TurnManagers")]
    private void CheckForDuplicates()
    {
        TurnManager[] allManagers = FindObjectsByType<TurnManager>(FindObjectsSortMode.None);
        if (allManagers.Length > 1)
        {
            Debug.LogError($"[TurnManager] FOUND {allManagers.Length} TurnManager instances!");
            foreach (var manager in allManagers)
            {
                Debug.LogError($"  - {manager.gameObject.name} (InstanceID: {manager.GetInstanceID()}), Scene: {manager.gameObject.scene.name}");
            }
        }
        else
        {
            Debug.Log($"[TurnManager] Only one instance found: {gameObject.name}");
        }
    }
}