using UnityEngine;

/// <summary>
/// Simple input handler to open the Card Merge UI with a keyboard shortcut
/// Add this to a persistent GameObject in your scene
/// </summary>
public class CardMergeInputHandler : MonoBehaviour
{
    [Header("Keyboard Shortcut")]
    [SerializeField] private KeyCode openMergeKey = KeyCode.M;
    [SerializeField] private bool requireShiftKey = false;
    [SerializeField] private bool requireControlKey = false;
    
    [Header("Settings")]
    [SerializeField] private bool enableInBattle = false;
    [SerializeField] private bool showDebugMessages = false;
    
    private void Update()
    {
        // Check if in battle (optional restriction)
        if (!enableInBattle && IsInBattle())
        {
            return;
        }
        
        // Check modifier keys
        bool shiftPressed = !requireShiftKey || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool controlPressed = !requireControlKey || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        
        // Check if merge key pressed with required modifiers
        if (Input.GetKeyDown(openMergeKey) && shiftPressed && controlPressed)
        {
            ToggleMergePanel();
        }
        
        // Alternative: ESC to close if panel is open
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CardMergeUI.Instance != null && IsMergePanelOpen())
            {
                CardMergeUI.Instance.ClosePanel();
            }
        }
    }
    
    /// <summary>
    /// Toggle the merge panel open/closed
    /// </summary>
    private void ToggleMergePanel()
    {
        if (CardMergeUI.Instance == null)
        {
            if (showDebugMessages)
            {
                Debug.LogWarning("[CardMergeInputHandler] CardMergeUI instance not found!");
            }
            return;
        }
        
        if (IsMergePanelOpen())
        {
            CardMergeUI.Instance.ClosePanel();
            if (showDebugMessages)
            {
                Debug.Log("[CardMergeInputHandler] Closed merge panel");
            }
        }
        else
        {
            CardMergeUI.OpenMerge();
            if (showDebugMessages)
            {
                Debug.Log($"[CardMergeInputHandler] Opened merge panel with key: {openMergeKey}");
            }
        }
    }
    
    /// <summary>
    /// Check if merge panel is currently open
    /// </summary>
    private bool IsMergePanelOpen()
    {
        // This assumes your merge panel has a specific GameObject you can check
        // Adjust based on your actual implementation
        if (CardMergeUI.Instance == null) return false;
        
        // Try to access the merge panel through reflection or public property
        // For now, we'll assume there's a way to check this
        return false; // TODO: Implement proper check
    }
    
    /// <summary>
    /// Check if player is currently in battle
    /// </summary>
    private bool IsInBattle()
    {
        // Check your game's battle state
        // Adjust based on your actual battle system
        
        // Example implementations:
        // return BattleManager.Instance != null && BattleManager.Instance.IsInBattle;
        // return GameManager.Instance.CurrentState == GameState.Battle;
        
        return false; // Default: allow opening merge UI anywhere
    }
}
