using UnityEngine;

/// <summary>
/// SIMPLE Level One objectives - just updates one text field
/// No complex management, just simple text changes for each objective
/// </summary>
public class SimpleLevelOneObjectives : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private SimpleObjectiveUI simpleObjectiveUI;
    
    [Header("Settings")]
    [SerializeField] private bool autoStartObjectives = false; // Changed to false - objectives start when key is picked up
    [SerializeField] private bool debugMode = true;
    
    // Current objective index for sequential progression
    private int currentObjectiveIndex = 0;
    private string[] objectives = {
        "Explore the Cell",
        "Escape the Cell", 
        "Defeat the Skeleton",
        "Explore the Dungeon",
        "Escape the Dungeon"
    };
    
    // Completion flags
    private bool hasFoundKey = false;
    private bool hasEscapedCell = false;
    private bool hasDefeatedSkeleton = false;
    private bool hasExploredDungeon = false;
    private bool hasEscapedDungeon = false;
    
    private void Awake()
    {
        Debug.Log("[SimpleLevelOneObjectives] Awake() called - Script is loading!");
    }
    
    private void Start()
    {
        Debug.Log("[SimpleLevelOneObjectives] Start() called");
        InitializeSystem();
        
        // Restore state from GameSession
        RestoreObjectiveState();
        
        // Check if skeleton was just defeated and trigger objective update
        if (GameSession.EnemyDefeated && !hasDefeatedSkeleton && GameSession.ObjectivesStarted)
        {
            Debug.Log("[SimpleLevelOneObjectives] Skeleton was defeated - triggering objective update");
            Invoke(nameof(CheckSkeletonDefeated), 0.5f); // Small delay to ensure UI is ready
        }
        
        // Don't auto-start objectives - they start when key is picked up
        if (autoStartObjectives && !GameSession.ObjectivesStarted)
        {
            Debug.Log("[SimpleLevelOneObjectives] Auto-starting objectives...");
            StartObjectives();
        }
        else if (GameSession.ObjectivesStarted)
        {
            Debug.Log("[SimpleLevelOneObjectives] Objectives already started - showing current objective");
            ShowCurrentObjective();
        }
        else
        {
            Debug.Log("[SimpleLevelOneObjectives] Waiting for key pickup to start objectives...");
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
                Debug.LogError("[SimpleLevelOneObjectives] SimpleObjectiveUI not found! Please add SimpleObjectiveUI component to your existing panel GameObject and assign your text/slider references.");
                return;
            }
            else
            {
                Debug.Log("[SimpleLevelOneObjectives] Found SimpleObjectiveUI in scene");
            }
        }
        
        if (debugMode) Debug.Log("[SimpleLevelOneObjectives] System initialized");
    }
    
    /// <summary>
    /// Start showing objectives (call this when key is picked up)
    /// </summary>
    public void StartObjectives()
    {
        Debug.Log("[SimpleLevelOneObjectives] StartObjectives() called - showing first objective");
        GameSession.ObjectivesStarted = true;
        SaveObjectiveState(); // Save initial state
        ShowCurrentObjective();
        
        // No auto-progression - objectives are triggered by events
        HandleAutoProgression();
    }
    
    /// <summary>
    /// Restore objective state from GameSession (after scene reload)
    /// </summary>
    private void RestoreObjectiveState()
    {
        if (!GameSession.ObjectivesStarted)
        {
            Debug.Log("[SimpleLevelOneObjectives] No saved objective state found");
            return;
        }
        
        // Restore progress
        currentObjectiveIndex = GameSession.CurrentObjectiveIndex;
        hasFoundKey = GameSession.HasFoundKey;
        hasEscapedCell = GameSession.HasEscapedCell;
        hasDefeatedSkeleton = GameSession.HasDefeatedSkeleton;
        hasExploredDungeon = GameSession.HasExploredDungeon;
        hasEscapedDungeon = GameSession.HasEscapedDungeon;
        
        Debug.Log($"[SimpleLevelOneObjectives] Restored state: objective {currentObjectiveIndex + 1}/{objectives.Length}");
    }
    
    /// <summary>
    /// Save objective state to GameSession (before scene changes)
    /// </summary>
    private void SaveObjectiveState()
    {
        GameSession.CurrentObjectiveIndex = currentObjectiveIndex;
        GameSession.HasFoundKey = hasFoundKey;
        GameSession.HasEscapedCell = hasEscapedCell;
        GameSession.HasDefeatedSkeleton = hasDefeatedSkeleton;
        GameSession.HasExploredDungeon = hasExploredDungeon;
        GameSession.HasEscapedDungeon = hasEscapedDungeon;
        
        Debug.Log($"[SimpleLevelOneObjectives] Saved state: objective {currentObjectiveIndex + 1}/{objectives.Length}");
    }
    
    /// <summary>
    /// Check and trigger skeleton defeated objective (called after returning from battle)
    /// </summary>
    private void CheckSkeletonDefeated()
    {
        if (GameSession.EnemyDefeated && currentObjectiveIndex == 2)
        {
            Debug.Log("[SimpleLevelOneObjectives] Processing skeleton defeat after battle return");
            OnSkeletonDefeated();
        }
    }
    
    private void ShowCurrentObjective()
    {
        if (currentObjectiveIndex >= objectives.Length)
        {
            if (debugMode) Debug.Log("[SimpleLevelOneObjectives] All objectives completed!");
            
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
            Debug.LogError("[SimpleLevelOneObjectives] SimpleObjectiveUI is null! Cannot show objective.");
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
        
        if (debugMode) Debug.Log($"[SimpleLevelOneObjectives] Showing objective {currentObjectiveIndex + 1}/{objectives.Length}: {currentObjective} (Progress: {progress:P0})");
    }
    
    private void CompleteCurrentObjective()
    {
        if (currentObjectiveIndex >= objectives.Length) return;
        
        string completedObjective = objectives[currentObjectiveIndex];
        if (debugMode) Debug.Log($"[SimpleLevelOneObjectives] Completed: {completedObjective}");
        
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
    
    private void HandleAutoProgression()
    {
        // No auto-progression - objectives are triggered by game events
        // Key pickup starts objectives, cell escape triggers next, etc.
    }
    
    // Event-triggered methods (called by other game systems)
    
    /// <summary>
    /// Call this when player picks up the key
    /// </summary>
    public void OnKeyPickedUp()
    {
        if (!hasFoundKey && currentObjectiveIndex == 0)
        {
            hasFoundKey = true;
            SaveObjectiveState(); // Save progress
            CompleteCurrentObjective(); // Complete "Explore the Cell", show "Escape the Cell"
        }
    }
    
    /// <summary>
    /// Call this when player escapes the cell
    /// </summary>
    public void OnCellEscaped()
    {
        if (!hasEscapedCell && currentObjectiveIndex == 1)
        {
            hasEscapedCell = true;
            SaveObjectiveState(); // Save progress
            CompleteCurrentObjective(); // Complete "Escape the Cell", show "Defeat the Skeleton"
            
            // Show card reward for escaping the cell
            if (ExplorationRewardManager.Instance != null)
            {
                Debug.Log("[SimpleLevelOneObjectives] Cell escaped! Showing card reward...");
                ExplorationRewardManager.ShowReward();
            }
            else
            {
                Debug.LogWarning("[SimpleLevelOneObjectives] ExplorationRewardManager.Instance is null! Cannot show card reward.");
            }
        }
    }
    
    /// <summary>
    /// Call this when skeleton is defeated
    /// </summary>
    public void OnSkeletonDefeated()
    {
        if (!hasDefeatedSkeleton && currentObjectiveIndex == 2)
        {
            hasDefeatedSkeleton = true;
            SaveObjectiveState(); // Save progress
            CompleteCurrentObjective(); // Complete "Defeat the Skeleton", immediately show "Explore the Dungeon"
        }
    }
    
    /// <summary>
    /// Call this when the cutscene plays (triggers final objective)
    /// </summary>
    public void OnCutscenePlayed()
    {
        if (!hasExploredDungeon && currentObjectiveIndex == 3)
        {
            hasExploredDungeon = true;
            SaveObjectiveState(); // Save progress
            CompleteCurrentObjective();
            
            // Show card reward after exploring the dungeon
            if (ExplorationRewardManager.Instance != null)
            {
                Debug.Log("[SimpleLevelOneObjectives] Dungeon explored! Showing card reward...");
                ExplorationRewardManager.ShowReward();
            }
            else
            {
                Debug.LogWarning("[SimpleLevelOneObjectives] ExplorationRewardManager.Instance is null! Cannot show card reward.");
            }
        }
    }
    
    /// <summary>
    /// Call this when player escapes the dungeon
    /// </summary>
    public void OnDungeonEscaped()
    {
        if (!hasEscapedDungeon && currentObjectiveIndex == 4)
        {
            hasEscapedDungeon = true;
            SaveObjectiveState(); // Save progress
            CompleteCurrentObjective(); // Complete "Escape the Dungeon"
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
    
    [ContextMenu("Test: Key Picked Up")]
    public void TestKeyPickedUp()
    {
        OnKeyPickedUp();
    }
    
    [ContextMenu("Test: Cell Escaped")]
    public void TestCellEscaped()
    {
        OnCellEscaped();
    }
    
    [ContextMenu("Test: Skeleton Defeated")]
    public void TestSkeletonDefeated()
    {
        OnSkeletonDefeated();
    }
    
    [ContextMenu("Test: Cutscene Played")]
    public void TestCutscenePlayed()
    {
        OnCutscenePlayed();
    }
    
    [ContextMenu("Test: Dungeon Escaped")]
    public void TestDungeonEscaped()
    {
        OnDungeonEscaped();
    }
}