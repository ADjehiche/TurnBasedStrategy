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
    [SerializeField] private bool autoStartObjectives = true;
    [SerializeField] private bool debugMode = true;
    
    // Current objective index for sequential progression
    private int currentObjectiveIndex = 0;
    private string[] objectives = {
        "Wake Up",
        "Explore Your Cell", 
        "Find a Way Out",
        "Find the Cell Key",
        "Escape the Cell",
        "Escape the Dungeon",
        "Befriend the Glowing Entity"
    };
    
    // Completion flags
    private bool hasWokenUp = false;
    private bool hasExploredCell = false;
    private bool hasFoundWayOut = false;
    private bool hasFoundKey = false;
    private bool hasEscapedCell = false;
    private bool hasExploredHallway = false;
    private bool hasMeetCompanion = false;
    
    private void Start()
    {
        InitializeSystem();
        
        if (autoStartObjectives)
        {
            StartObjectives();
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
    
    private void StartObjectives()
    {
        ShowCurrentObjective();
        
        // Handle auto-progression for early objectives
        HandleAutoProgression();
    }
    
    private void ShowCurrentObjective()
    {
        if (currentObjectiveIndex >= objectives.Length)
        {
            if (debugMode) Debug.Log("[SimpleLevelOneObjectives] All objectives completed!");
            simpleObjectiveUI.ClearObjective();
            return;
        }
        
        string currentObjective = objectives[currentObjectiveIndex];
        simpleObjectiveUI.SetObjectiveText(currentObjective);
        
        if (debugMode) Debug.Log($"[SimpleLevelOneObjectives] Showing objective {currentObjectiveIndex + 1}: {currentObjective}");
    }
    
    private void CompleteCurrentObjective()
    {
        if (currentObjectiveIndex >= objectives.Length) return;
        
        string completedObjective = objectives[currentObjectiveIndex];
        if (debugMode) Debug.Log($"[SimpleLevelOneObjectives] Completed: {completedObjective}");
        
        currentObjectiveIndex++;
        
        // Small delay before showing next objective
        Invoke(nameof(ShowCurrentObjective), 1.5f);
    }
    
    private void HandleAutoProgression()
    {
        if (currentObjectiveIndex == 0) // Wake Up
        {
            Invoke(nameof(OnWakeUpComplete), 2f);
        }
        else if (currentObjectiveIndex == 1) // Explore Cell
        {
            Invoke(nameof(OnExploreCellComplete), 5f);
        }
        else if (currentObjectiveIndex == 2) // Find Way Out
        {
            Invoke(nameof(OnFindWayOutComplete), 3f);
        }
    }
    
    // Auto-progression methods
    private void OnWakeUpComplete()
    {
        if (!hasWokenUp && currentObjectiveIndex == 0)
        {
            hasWokenUp = true;
            CompleteCurrentObjective();
        }
    }
    
    private void OnExploreCellComplete()
    {
        if (!hasExploredCell && currentObjectiveIndex == 1)
        {
            hasExploredCell = true;
            CompleteCurrentObjective();
        }
    }
    
    private void OnFindWayOutComplete()
    {
        if (!hasFoundWayOut && currentObjectiveIndex == 2)
        {
            hasFoundWayOut = true;
            CompleteCurrentObjective();
        }
    }
    
    // Public methods for integration with existing systems
    
    /// <summary>
    /// Call when player picks up the key
    /// </summary>
    public void OnKeyPickedUp()
    {
        if (!hasFoundKey && currentObjectiveIndex == 3)
        {
            hasFoundKey = true;
            CompleteCurrentObjective();
            
            if (debugMode) Debug.Log("[SimpleLevelOneObjectives] Key picked up");
        }
    }
    
    /// <summary>
    /// Call when cell door is opened
    /// </summary>
    public void OnCellDoorOpened()
    {
        if (!hasEscapedCell && currentObjectiveIndex == 4)
        {
            hasEscapedCell = true;
            CompleteCurrentObjective();
            
            if (debugMode) Debug.Log("[SimpleLevelOneObjectives] Cell door opened");
        }
    }
    
    /// <summary>
    /// Call when player enters the hallway area
    /// </summary>
    public void OnEnterHallway()
    {
        if (!hasExploredHallway && currentObjectiveIndex == 5)
        {
            hasExploredHallway = true;
            CompleteCurrentObjective();
            
            if (debugMode) Debug.Log("[SimpleLevelOneObjectives] Entered hallway");
        }
    }
    
    /// <summary>
    /// Call when player meets the companion
    /// </summary>
    public void OnMeetCompanion()
    {
        if (!hasMeetCompanion && currentObjectiveIndex == 6)
        {
            hasMeetCompanion = true;
            CompleteCurrentObjective();
            
            if (debugMode) Debug.Log("[SimpleLevelOneObjectives] Met companion");
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
    
    // Testing methods
    [ContextMenu("Force Complete Current Objective")]
    public void ForceCompleteCurrentObjective()
    {
        CompleteCurrentObjective();
    }
    
    [ContextMenu("Skip To Key Objective")]
    public void SkipToKeyObjective()
    {
        currentObjectiveIndex = 3;
        ShowCurrentObjective();
    }
    
    [ContextMenu("Skip To Escape Cell")]
    public void SkipToEscapeCell()
    {
        currentObjectiveIndex = 4;
        ShowCurrentObjective();
    }
    
    [ContextMenu("Skip To Companion")]
    public void SkipToCompanion()
    {
        currentObjectiveIndex = 6;
        ShowCurrentObjective();
    }
}