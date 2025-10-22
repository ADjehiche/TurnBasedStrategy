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
        {
            Debug.Log($"[TurnManager] StartPlayerTurn - CurrentTurn set to {CurrentTurn} (Instance: {GetInstanceID()})");
        }
        
        if (endTurnButton != null)
        {
            endTurnButton.interactable = true;
        }
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
        
        StartCoroutine(ProcessEnemyTurn());
    }
    
    private System.Collections.IEnumerator ProcessEnemyTurn()
    {
        if (enableDebugLogs)
        {
            Debug.Log("[TurnManager] Processing enemy turn...");
        }

        // Wait for enemy actions
        yield return new WaitForSeconds(2f);
        
        // Enemy attacks player
        int damage = UnityEngine.Random.Range(1, 6); // Random value between 1–5 (upper bound exclusive)

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
        
        // Return to player turn
        StartPlayerTurn();
    }
    
    // Optional: Method to manually check for duplicate instances
    [ContextMenu("Check For Duplicate TurnManagers")]
    private void CheckForDuplicates()
    {
        TurnManager[] allManagers = FindObjectsOfType<TurnManager>();
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