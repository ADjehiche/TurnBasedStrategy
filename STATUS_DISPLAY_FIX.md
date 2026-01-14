# Status Display Fix - January 14, 2026

## Issue
Player status effects (bleed, weaken icons) were not being hidden properly when their values reached 0, unlike enemy status displays which worked correctly.

## Root Cause
Both `PlayerStatusDisplay` and `EnemyStatusDisplay` had two issues:

1. **Missing initialization**: Status icons weren't explicitly hidden on `Start()`, so if they were active in the editor, they'd stay visible even with 0 values
2. **Text not cleared**: When status values became 0, the text inside the icons wasn't being cleared - only the `SetActive(false)` was called on the root GameObject. While this should hide everything, clearing the text ensures clean state management

## Fix Applied

### Changes to `PlayerStatusDisplay.cs` and `EnemyStatusDisplay.cs`

1. **Added `Start()` initialization**:
```csharp
private void Start()
{
    // Initialize all status icons as hidden
    ClearAll(); // or SetBleedTurns(0); SetWeakenPercent(0);
}
```

2. **Modified `SetBleedTurns()` to clear text**:
```csharp
public void SetBleedTurns(int turnsLeft)
{
    if (bleedRoot != null)
        bleedRoot.SetActive(turnsLeft > 0);

    if (bleedText != null)
    {
        if (turnsLeft > 0)
            bleedText.text = turnsLeft.ToString();
        else
            bleedText.text = ""; // Clear text when hidden
    }
}
```

3. **Modified `SetWeakenPercent()` to clear text**:
```csharp
public void SetWeakenPercent(int percent)
{
    if (weakenRoot != null)
        weakenRoot.SetActive(percent > 0);

    if (weakenText != null)
    {
        if (percent > 0)
            weakenText.text = $"-{percent}%";
        else
            weakenText.text = ""; // Clear text when hidden
    }
}
```

## Testing Checklist

1. ✅ **Bleed countdown**: Apply bleed to player → status icon shows → bleed counts down → icon hides when reaches 0
2. ✅ **Weaken countdown**: Apply weakness to player → status icon shows → weakness expires → icon hides
3. ✅ **Enemy statuses**: Verify enemy status icons still work correctly (bleed/weaken show/hide properly)
4. ✅ **Battle start**: Status icons should be hidden at battle start (no ghost icons)
5. ✅ **Multiple battles**: Icons should reset properly between battles

## How Status Display Works

### Player Status Flow
```
Card Applied → PlayerHealth.AddBleed()/AddWeaken() 
            → PlayerStatusDisplay.SetBleedTurns()/SetWeakenPercent()
            → Shows icon with value

Turn Start → TurnManager.StartPlayerTurn() 
          → PlayerHealth.TickStatuses()
          → Decrements values
          → Updates display
          → When value reaches 0, hides icon
```

### Enemy Status Flow
```
Card Applied → EnemyHealth.AddBleed()/AddWeaken()
            → EnemyStatusDisplay.SetBleedTurns()/SetWeakenPercent()
            → Shows icon with value

Turn Start → EnemyManager.TickAllEnemyStatuses()
          → EnemyHealth.TickStatuses()
          → Decrements values
          → Updates display
          → When value reaches 0, hides icon
```

## Expected Behavior

### ✅ Correct (After Fix)
- Status icons hidden on battle start
- Icons appear when status applied (with correct number/text)
- Icons update each turn as values decrease
- Icons hide immediately when value reaches 0
- No ghost icons or old values visible

### ❌ Incorrect (Before Fix)
- Status icons might be visible with old values from editor
- Icons might stay visible even when value is 0
- Text might show old values even when icon should be hidden

## Related Files
- `Assets/Scripts/Battle/PlayerStatusDisplay.cs` - Player status UI manager
- `Assets/Scripts/Battle/EnemyStatusDisplay.cs` - Enemy status UI manager
- `Assets/Scripts/Battle/PlayerHealth.cs` - Calls PlayerStatusDisplay methods
- `Assets/Scripts/Battle/EnemyHealth.cs` - Calls EnemyStatusDisplay methods
- `Assets/Scripts/Battle/TurnManager.cs` - Triggers TickStatuses() each turn

## Console Logs to Watch
```
[PlayerHealth] Added X bleed stack(s). Total: Y
[PlayerHealth] Player takes X bleed damage (Bleed Y)
[PlayerHealth] Player bleed expired
[PlayerHealth] Applied X% weakness for Y turn(s)
[PlayerHealth] Weakness expired
[EnemyHealth] EnemyContainer X weakness expired
```

These logs confirm status effects are being applied, ticked, and expired correctly. The display should match these state changes.
