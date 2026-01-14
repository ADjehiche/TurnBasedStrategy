using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Debug UI for testing maze guidance system
/// Shows timer info and provides buttons to trigger events
/// </summary>
public class MazeGuidanceDebugUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button triggerHelpButton;
    [SerializeField] private Button collectBlueButton;
    [SerializeField] private Button resetButton;
    
    [Header("Settings")]
    [SerializeField] private bool enableDebugUI = true;
    [SerializeField] private float updateInterval = 0.5f; // How often to update UI
    
    private MazeGuidanceController guidanceController;
    private float lastUpdate;
    
    void Start()
    {
        // Find guidance controller
        guidanceController = FindFirstObjectByType<MazeGuidanceController>();
        
        if (!enableDebugUI)
        {
            gameObject.SetActive(false);
            return;
        }
        
        // Setup buttons
        if (triggerHelpButton != null)
        {
            triggerHelpButton.onClick.AddListener(() => {
                if (guidanceController != null)
                {
                    // Use reflection to call private method for testing
                    var method = guidanceController.GetType().GetMethod("ForceTriggerHelp", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    method?.Invoke(guidanceController, null);
                }
            });
        }
        
        if (collectBlueButton != null)
        {
            collectBlueButton.onClick.AddListener(() => {
                if (guidanceController != null)
                {
                    guidanceController.OnBlueFragmentCollected();
                }
            });
        }
        
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(() => {
                // Force player out and back in to reset
                if (guidanceController != null)
                {
                    guidanceController.OnPlayerExitedMazeArea();
                    guidanceController.OnPlayerEnteredMazeArea();
                }
            });
        }
    }
    
    void Update()
    {
        if (!enableDebugUI || guidanceController == null || statusText == null) return;
        
        // Update UI at intervals
        if (Time.time - lastUpdate >= updateInterval)
        {
            UpdateStatusText();
            lastUpdate = Time.time;
        }
    }
    
    private void UpdateStatusText()
    {
        if (guidanceController == null || statusText == null) return;
        
        string status = "<b>Maze Guidance Debug</b>\n\n";
        
        status += $"Player in Maze: {(guidanceController.IsPlayerInMazeArea() ? "<color=green>YES</color>" : "<color=red>NO</color>")}\n";
        status += $"Time in Maze: {guidanceController.GetTimeInMaze():F1}s\n";
        status += $"Help Triggered: {(guidanceController.HasTriggeredHelp() ? "<color=yellow>YES</color>" : "NO")}\n";
        status += $"Currently Guiding: {(guidanceController.IsGuidingPlayer() ? "<color=cyan>YES</color>" : "NO")}\n";
        
        // Show help countdown
        float helpTime = 600f; // 10 minutes
        float timeRemaining = helpTime - guidanceController.GetTimeInMaze();
        if (guidanceController.IsPlayerInMazeArea() && !guidanceController.HasTriggeredHelp() && timeRemaining > 0)
        {
            status += $"\n<color=orange>Help in: {timeRemaining:F1}s</color>";
        }
        
        statusText.text = status;
    }
    
    /// <summary>
    /// Toggle debug UI on/off
    /// </summary>
    public void ToggleDebugUI()
    {
        enableDebugUI = !enableDebugUI;
        gameObject.SetActive(enableDebugUI);
    }
}