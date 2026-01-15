using UnityEngine;

/// <summary>
/// SIMPLE Level Two objectives - handles archive exploration progression
/// Manages the explore archive → tunnel → maze → return to archive flow
/// </summary>
public class SimpleLevelTwoObjectives : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private SimpleObjectiveUI simpleObjectiveUI;
    
    [Header("Settings")]
    [SerializeField] private bool autoStartObjectives = true;
    [SerializeField] private bool debugMode = true;
    
    // Current objective index for sequential progression
    private int currentObjectiveIndex = 0;
    private string[] objectives = {
        "Explore the Archive",
        "Explore Further Inside", 
        "Defeat the Skeletons",    // Combat Wing battle
        "Explore Further Inside",  // Return to exploration after battle
        "Explore the Maze",
        "Return to the Archive",
        "Defeat the Warden",       // NEW: Boss battle
        "Make Your Choice"         // NEW: Final choice (take fragment or leave)
    };
    
    // Completion flags
    private bool hasExploredArchive = false;
    private bool hasExploredTunnel = false;
    private bool hasEnteredCombatWing = false;
    private bool hasDefeatedSkeletons = false;
    private bool hasExploredMaze = false;
    private bool hasReturnedToArchive = false;
    private bool hasEnteredBossRoom = false;     // NEW
    private bool hasDefeatedWarden = false;      // NEW
    
    private void Awake()
    {
        Debug.Log("[SimpleLevelTwoObjectives] Awake() called - Script is loading!");
    }
    
    private void Start()
    {
        Debug.Log("[SimpleLevelTwoObjectives] Start() called");
        InitializeSystem();
        
        // Restore state from GameSession
        RestoreObjectiveState();
        
        // Auto-start objectives for Level Two
        if (autoStartObjectives && !GameSession.LevelTwoObjectivesStarted)
        {
            Debug.Log("[SimpleLevelTwoObjectives] Auto-starting Level Two objectives...");
            StartObjectives();
        }
        else if (GameSession.LevelTwoObjectivesStarted)
        {
            Debug.Log("[SimpleLevelTwoObjectives] Level Two objectives already started - showing current objective");
            ShowCurrentObjective();
        }
        else
        {
            Debug.Log("[SimpleLevelTwoObjectives] Waiting to start Level Two objectives...");
        }
    }
    
    private void InitializeSystem()
    {
        // Find the SimpleObjectiveUI (should be manually assigned or found in scene)
        if (simpleObjectiveUI == null)
        {
            simpleObjectiveUI = FindFirstObjectByType<SimpleObjectiveUI>();
            if (simpleObjectiveUI == null)
            {
                Debug.LogError("[SimpleLevelTwoObjectives] SimpleObjectiveUI not found! Please add SimpleObjectiveUI component to your existing panel GameObject and assign your text/slider references.");
                return;
            }
            else
            {
                Debug.Log("[SimpleLevelTwoObjectives] Found SimpleObjectiveUI in scene");
            }
        }
        
        if (debugMode) Debug.Log("[SimpleLevelTwoObjectives] System initialized");
    }
    
    /// <summary>
    /// Start showing objectives
    /// </summary>
    public void StartObjectives()
    {
        Debug.Log("[SimpleLevelTwoObjectives] StartObjectives() called - showing first objective");
        GameSession.LevelTwoObjectivesStarted = true;
        SaveObjectiveState(); // Save initial state
        ShowCurrentObjective();
    }
    
    /// <summary>
    /// Restore objective state from GameSession (after scene reload)
    /// </summary>
    private void RestoreObjectiveState()
    {
        if (!GameSession.LevelTwoObjectivesStarted)
        {
            Debug.Log("[SimpleLevelTwoObjectives] No saved Level Two objective state found");
            return;
        }
        
        // Restore progress
        currentObjectiveIndex = GameSession.CurrentLevelTwoObjectiveIndex;
        hasExploredArchive = GameSession.HasExploredArchive;
        hasExploredTunnel = GameSession.HasExploredTunnel;
        hasExploredMaze = GameSession.HasExploredMaze;
        hasReturnedToArchive = GameSession.HasReturnedToArchive;
        
        Debug.Log($"[SimpleLevelTwoObjectives] Restored state: objective {currentObjectiveIndex + 1}/{objectives.Length}");
    }
    
    /// <summary>
    /// Save objective state to GameSession (before scene changes)
    /// </summary>
    private void SaveObjectiveState()
    {
        GameSession.CurrentLevelTwoObjectiveIndex = currentObjectiveIndex;
        GameSession.HasExploredArchive = hasExploredArchive;
        GameSession.HasExploredTunnel = hasExploredTunnel;
        GameSession.HasExploredMaze = hasExploredMaze;
        GameSession.HasReturnedToArchive = hasReturnedToArchive;
        
        Debug.Log($"[SimpleLevelTwoObjectives] Saved state: objective {currentObjectiveIndex + 1}/{objectives.Length}");
    }
    
    private void ShowCurrentObjective()
    {
        if (currentObjectiveIndex >= objectives.Length)
        {
            if (debugMode) Debug.Log("[SimpleLevelTwoObjectives] All Level Two objectives completed!");
            
            if (simpleObjectiveUI != null)
            {
                simpleObjectiveUI.SetObjectiveText("All Objectives Complete!");
                simpleObjectiveUI.SetProgress(1.0f); // Show 100% completion
            }
            
            // Play completion audio
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play("AllObjectivesComplete");
            
            return;
        }
        
        // Safety check
        if (simpleObjectiveUI == null)
        {
            Debug.LogError("[SimpleLevelTwoObjectives] SimpleObjectiveUI is null! Cannot show objective.");
            return;
        }
        
        string currentObjective = objectives[currentObjectiveIndex];
        simpleObjectiveUI.SetObjectiveText(currentObjective);
        
        // Update progress slider (show current progress out of total objectives)
        float progress = (float)currentObjectiveIndex / (float)objectives.Length;
        simpleObjectiveUI.SetProgress(progress);
        
        // Force show the objective panel
        simpleObjectiveUI.ShowPanel();
        
        // Play new objective audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play("NewObjective");
        
        if (debugMode) Debug.Log($"[SimpleLevelTwoObjectives] Showing objective {currentObjectiveIndex + 1}/{objectives.Length}: {currentObjective} (Progress: {progress:P0})");
    }
    
    private void CompleteCurrentObjective()
    {
        if (currentObjectiveIndex >= objectives.Length) return;
        
        string completedObjective = objectives[currentObjectiveIndex];
        if (debugMode) Debug.Log($"[SimpleLevelTwoObjectives] Completed: {completedObjective}");
        
        // Show completion progress (100% for this objective)
        simpleObjectiveUI.SetProgress(1.0f);
        
        // Play objective completion sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play("ObjectiveComplete");
        
        currentObjectiveIndex++;
        
        // Save state after updating
        SaveObjectiveState();
        
        // Small delay before showing next objective
        Invoke(nameof(ShowCurrentObjective), 1.5f);
    }
    
    // Event-triggered methods (called by other game systems)
    
    /// <summary>
    /// Call this when player enters the tunnel
    /// </summary>
    public void OnTunnelEntered()
    {
        Debug.Log($"[SimpleLevelTwoObjectives] OnTunnelEntered called - hasExploredArchive: {hasExploredArchive}, currentObjectiveIndex: {currentObjectiveIndex}");
        
        if (!hasExploredArchive && currentObjectiveIndex == 0)
        {
            Debug.Log("[SimpleLevelTwoObjectives] Conditions met - completing 'Explore the Archive' objective");
            hasExploredArchive = true;
            SaveObjectiveState(); // Save progress
            CompleteCurrentObjective(); // Complete "Explore the Archive", show "Explore Further Inside"
        }
        else
        {
            Debug.LogWarning($"[SimpleLevelTwoObjectives] Tunnel entered but conditions not met - hasExploredArchive: {hasExploredArchive}, currentObjectiveIndex: {currentObjectiveIndex} (expected 0)");
        }
    }
    
    /// <summary>
    /// Call this when player enters the maze
    /// </summary>
    public void OnMazeEntered()
    {
        Debug.Log($"[SimpleLevelTwoObjectives] OnMazeEntered called - hasExploredTunnel: {hasExploredTunnel}, currentObjectiveIndex: {currentObjectiveIndex}");
        
        // Maze is now objective index 4 (after skeleton defeat)
        if (!hasExploredTunnel && currentObjectiveIndex == 3)
        {
            Debug.Log("[SimpleLevelTwoObjectives] Conditions met - completing 'Explore Further Inside' objective");
            hasExploredTunnel = true;
            SaveObjectiveState(); // Save progress
            CompleteCurrentObjective(); // Complete "Explore Further Inside", show "Explore the Maze"
        }
        else
        {
            Debug.LogWarning($"[SimpleLevelTwoObjectives] Maze entered but conditions not met - hasExploredTunnel: {hasExploredTunnel}, currentObjectiveIndex: {currentObjectiveIndex} (expected 3)");
        }
    }
    
    /// <summary>
    /// Call this when player enters the Combat Wing (skeleton trigger area)
    /// </summary>
    public void OnCombatWingEntered()
    {
        Debug.Log($"[SimpleLevelTwoObjectives] OnCombatWingEntered called - hasEnteredCombatWing: {hasEnteredCombatWing}, currentObjectiveIndex: {currentObjectiveIndex}");
        
        if (!hasEnteredCombatWing && currentObjectiveIndex == 1)
        {
            Debug.Log("[SimpleLevelTwoObjectives] Entering Combat Wing - showing 'Defeat the Skeletons' objective");
            hasEnteredCombatWing = true;
            SaveObjectiveState();
            CompleteCurrentObjective(); // Complete "Explore Further Inside", show "Defeat the Skeletons"
        }
    }
    
    /// <summary>
    /// Call this when skeletons are defeated (after battle victory)
    /// </summary>
    public void OnSkeletonsDefeated()
    {
        Debug.Log($"[SimpleLevelTwoObjectives] OnSkeletonsDefeated called - hasDefeatedSkeletons: {hasDefeatedSkeletons}, currentObjectiveIndex: {currentObjectiveIndex}");
        
        if (!hasDefeatedSkeletons && currentObjectiveIndex == 2)
        {
            Debug.Log("[SimpleLevelTwoObjectives] Skeletons defeated - returning to 'Explore Further Inside'");
            hasDefeatedSkeletons = true;
            SaveObjectiveState();
            CompleteCurrentObjective(); // Complete "Defeat the Skeletons", show "Explore Further Inside"
        }
    }
    
    /// <summary>
    /// Call this when maze exploration is complete
    /// </summary>
    public void OnMazeExplored()
    {
        Debug.Log($"[SimpleLevelTwoObjectives] OnMazeExplored called - hasExploredMaze: {hasExploredMaze}, currentObjectiveIndex: {currentObjectiveIndex}");
        
        // Maze explore is now objective index 4
        if (!hasExploredMaze && currentObjectiveIndex == 4)
        {
            Debug.Log("[SimpleLevelTwoObjectives] Conditions met - completing 'Explore the Maze' objective");
            hasExploredMaze = true;
            SaveObjectiveState(); // Save progress
            CompleteCurrentObjective(); // Complete "Explore the Maze", show "Return to the Archive"
        }
        else
        {
            Debug.LogWarning($"[SimpleLevelTwoObjectives] Maze explored but conditions not met - hasExploredMaze: {hasExploredMaze}, currentObjectiveIndex: {currentObjectiveIndex} (expected 4)");
        }
    }
    
    /// <summary>
    /// Call this when player returns to the archive
    /// </summary>
    public void OnReturnedToArchive()
    {
        Debug.Log($"[SimpleLevelTwoObjectives] OnReturnedToArchive called - hasReturnedToArchive: {hasReturnedToArchive}, currentObjectiveIndex: {currentObjectiveIndex}");
        
        // Return to archive is now objective index 5
        if (!hasReturnedToArchive && currentObjectiveIndex == 5)
        {
            Debug.Log("[SimpleLevelTwoObjectives] Conditions met - completing 'Return to the Archive' objective");
            hasReturnedToArchive = true;
            SaveObjectiveState(); // Save progress
            CompleteCurrentObjective(); // Complete "Return to the Archive", show "Defeat the Warden"
        }
        else
        {
            Debug.LogWarning($"[SimpleLevelTwoObjectives] Returned to archive but conditions not met - hasReturnedToArchive: {hasReturnedToArchive}, currentObjectiveIndex: {currentObjectiveIndex} (expected 5)");
        }
    }
    
    /// <summary>
    /// Call this when player enters the boss room (triggers boss fight)
    /// </summary>
    public void OnBossRoomEntered()
    {
        Debug.Log($"[SimpleLevelTwoObjectives] OnBossRoomEntered called - hasEnteredBossRoom: {hasEnteredBossRoom}, currentObjectiveIndex: {currentObjectiveIndex}");
        
        // Boss room is objective index 6
        if (!hasEnteredBossRoom && currentObjectiveIndex == 6)
        {
            Debug.Log("[SimpleLevelTwoObjectives] Entering boss room - 'Defeat the Warden' objective active");
            hasEnteredBossRoom = true;
            SaveObjectiveState();
            // Don't complete yet - objective stays as "Defeat the Warden" during battle
        }
    }
    
    /// <summary>
    /// Call this when the Warden is defeated
    /// </summary>
    public void OnWardenDefeated()
    {
        Debug.Log($"[SimpleLevelTwoObjectives] OnWardenDefeated called - hasDefeatedWarden: {hasDefeatedWarden}, currentObjectiveIndex: {currentObjectiveIndex}");
        
        if (!hasDefeatedWarden && currentObjectiveIndex == 6)
        {
            Debug.Log("[SimpleLevelTwoObjectives] Warden defeated - showing 'Make Your Choice' objective");
            hasDefeatedWarden = true;
            SaveObjectiveState();
            CompleteCurrentObjective(); // Complete "Defeat the Warden", show "Make Your Choice"
        }
    }
    
    /// <summary>
    /// Set a custom background image for the objective panel
    /// </summary>
    public void SetCustomPanelImage(Sprite customSprite)
    {
        if (simpleObjectiveUI != null)
        {
            simpleObjectiveUI.SetPanelSprite(customSprite);
        }
    }
    
    // Testing methods - Right-click on ObjectiveManager in inspector to test
    [ContextMenu("Test: Start Objectives")]
    public void TestStartObjectives()
    {
        StartObjectives();
    }
    
    [ContextMenu("Test: Tunnel Entered")]
    public void TestTunnelEntered()
    {
        OnTunnelEntered();
    }
    
    [ContextMenu("Test: Maze Entered")]
    public void TestMazeEntered()
    {
        OnMazeEntered();
    }
    
    [ContextMenu("Test: Maze Explored")]
    public void TestMazeExplored()
    {
        OnMazeExplored();
    }
    
    [ContextMenu("Test: Returned to Archive")]
    public void TestReturnedToArchive()
    {
        OnReturnedToArchive();
    }
    
    [ContextMenu("Debug: Show Current State")]
    public void DebugShowCurrentState()
    {
        Debug.Log("=== LEVEL TWO OBJECTIVES DEBUG STATE ===");
        Debug.Log($"ObjectivesStarted: {GameSession.LevelTwoObjectivesStarted}");
        Debug.Log($"CurrentObjectiveIndex: {currentObjectiveIndex} (showing: '{(currentObjectiveIndex < objectives.Length ? objectives[currentObjectiveIndex] : "COMPLETE")}')");
        Debug.Log($"Progress flags - Archive: {hasExploredArchive}, Tunnel: {hasExploredTunnel}, Maze: {hasExploredMaze}, Return: {hasReturnedToArchive}");
        Debug.Log($"SimpleObjectiveUI found: {(simpleObjectiveUI != null ? "YES" : "NO")}");
        Debug.Log("==========================================");
    }
}