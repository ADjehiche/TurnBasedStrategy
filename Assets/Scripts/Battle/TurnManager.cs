using UnityEngine;
using System;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public enum TurnState
    {
        PlayerTurn,
        EnemyTurn,
        None
    }

    public TurnState CurrentTurn { get; private set; } = TurnState.None;

    public event Action<TurnState> OnTurnChanged;

    [Header("Turn Timings")]
    [SerializeField] private float enemyTurnDelay = 1.5f;
    [SerializeField] private UnityEngine.UI.Button endTurnButton; 

    void Awake()
    {
        Instance = this;
        Debug.Log("[TurnManager] Awake on " + gameObject.name);
    }

    void Start()
    {
        Debug.Log("[TurnManager] (Start)");
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        CurrentTurn = TurnState.PlayerTurn;
        Debug.Log("[TurnManager] StartPlayerTurn -> CurrentTurn = PlayerTurn");
        OnTurnChanged?.Invoke(CurrentTurn);
        Debug.Log("Player Turn started!");

        if (endTurnButton) endTurnButton.interactable = true;

    }

    public void EndPlayerTurn()
    {
        Debug.Log("[TurnManager] EndPlayerTurn() called; CurrentTurn = " + CurrentTurn);
        if (CurrentTurn != TurnState.PlayerTurn) 
            {
            Debug.LogWarning("[TurnManager] EndPlayerTurn ignored (not PlayerTurn).");
            return; 
            }
        Debug.Log("Player Turn ended!");
        if (endTurnButton) endTurnButton.interactable = false;
        Debug.Log("[TurnManager] passing to enemy…");
        StartCoroutine(SwitchToEnemyTurn());
    }

    private System.Collections.IEnumerator SwitchToEnemyTurn()
    {
        if (endTurnButton) endTurnButton.interactable = false;
        Debug.Log("[TurnManager] SwitchToEnemyTurn() start");
        yield return new WaitForSeconds(enemyTurnDelay);

        CurrentTurn = TurnState.EnemyTurn;
        Debug.Log("[TurnManager] EnemyTurnStarted ");
        OnTurnChanged?.Invoke(CurrentTurn);
    
        // temporary AI wait
        yield return new WaitForSeconds(1f);

        Debug.Log(" Turn Manager Enemy finished turn.");
        StartPlayerTurn();
    }

   public void OnEndTurnButtonPressed()
    {
        Debug.Log("[TurnManager] OnEndTurnButtonPressed() received");
        if (CurrentTurn == TurnState.PlayerTurn)
            EndPlayerTurn();
        else
            Debug.LogWarning("[TurnManager] Button pressed but it is NOT PlayerTurn (" + CurrentTurn + ")");
    }
 }