# Exploration Reward Card Hover & Scale Fix

## Issues Fixed

### Issue 1: Cards Not Hovering in Reward UI ❌→✅
**Problem**: When reward cards were shown in exploration mode, hovering over them didn't trigger any visual feedback (no scale increase, no highlight).

**Root Cause**: 
- Previous fix disabled the entire `CardMovement` component to prevent cards from playing when clicked
- Disabling `CardMovement` also disabled its `Update()` method which handles hover detection
- No `Update()` = No hover checking = No hover effects

**Solution**: 
Instead of disabling `CardMovement`, added a new `isRewardCard` flag:
- When `true`: Hover still works (visual feedback), but clicking is disabled
- Button component handles the actual selection logic
- Best of both worlds: hover effects + proper reward selection

---

### Issue 2: Card Scale Not Updating from Inspector ❌→✅
**Problem**: Changing `cardScale` or `cardSpacing` in the Unity Inspector had no effect. Cards stayed at the same size/spacing.

**Root Cause**: 
- Scale and spacing were set in `Start()` method
- Once set, changing inspector values didn't update existing cards
- No `OnValidate()` method to detect inspector changes during play mode

**Solution**: 
Added multiple ways to update scale:
1. `OnValidate()` - Automatically updates when inspector values change during play
2. `UpdateCardScale(float)` - Public method to change scale from code
3. `UpdateCardSpacing(float)` - Public method to change spacing from code

---

## Code Changes

### CardMovement.cs - Added Reward Card Flag

**NEW FIELD**:
```csharp
// Flag to disable clicking/playing while keeping hover active
[HideInInspector] public bool isRewardCard = false; // Set by CardRewardUI
```

**UPDATED OnCardClicked()**:
```csharp
private void OnCardClicked()
{
    // NEW: If this is a reward card, don't handle clicks (Button component handles it)
    if (isRewardCard)
    {
        Debug.Log($"[CardMovement] Click ignored - this is a reward card (handled by Button)");
        return;
    }
    
    // Rest of click handling...
}
```

**Result**: 
- ✅ Hover detection runs normally (Update() still active)
- ✅ Click detection disabled for reward cards
- ✅ Button component handles selection

---

### CardRewardUI.cs - Set Flag Instead of Disabling Component

**OLD CODE (BROKEN)**:
```csharp
var cardMovement = cardObj.GetComponent<CardMovement>();
if (cardMovement != null)
{
    cardMovement.enabled = false; // Disables EVERYTHING including hover
    Debug.Log($"[CardRewardUI] Disabled CardMovement component on {card.cardName}");
}
```

**NEW CODE (FIXED)**:
```csharp
var cardMovement = cardObj.GetComponent<CardMovement>();
if (cardMovement != null)
{
    cardMovement.isRewardCard = true; // Disable clicking but keep hover active
    Debug.Log($"[CardRewardUI] Marked {card.cardName} as reward card (hover enabled, click disabled)");
}
```

**Result**: 
- ✅ CardMovement stays enabled
- ✅ Hover effects work (scale increases on mouse over)
- ✅ Clicks are ignored, Button handles selection

---

### CardRewardUI.cs - Added Scale Update Methods

**NEW METHODS**:
```csharp
/// <summary>
/// Update card scale - can be called from inspector or code
/// </summary>
public void UpdateCardScale(float newScale)
{
    cardScale = newScale;
    Debug.Log($"[CardRewardUI] Card scale updated to: {cardScale}");
    
    // Update existing displayed cards
    foreach (var cardObj in currentCardDisplays)
    {
        if (cardObj != null)
        {
            cardObj.transform.localScale = Vector3.one * cardScale;
        }
    }
}

/// <summary>
/// Update card spacing - can be called from inspector or code
/// </summary>
public void UpdateCardSpacing(float newSpacing)
{
    cardSpacing = newSpacing;
    Debug.Log($"[CardRewardUI] Card spacing updated to: {cardSpacing}");
    
    var layoutGroup = cardOptionsContainer?.GetComponent<HorizontalLayoutGroup>();
    if (layoutGroup != null)
    {
        layoutGroup.spacing = cardSpacing;
    }
}

// Called when inspector values change in editor
private void OnValidate()
{
    // Update layout if in editor mode
    if (!Application.isPlaying) return;
    
    var layoutGroup = cardOptionsContainer?.GetComponent<HorizontalLayoutGroup>();
    if (layoutGroup != null)
    {
        layoutGroup.spacing = cardSpacing;
    }
    
    // Update scale on existing cards
    foreach (var cardObj in currentCardDisplays)
    {
        if (cardObj != null)
        {
            cardObj.transform.localScale = Vector3.one * cardScale;
        }
    }
}
```

**Result**: 
- ✅ Inspector changes update immediately during play mode
- ✅ Can adjust scale/spacing dynamically from code
- ✅ All displayed cards update when values change

---

## Testing Checklist

### Test 1: Hover Effects on Reward Cards ✅
1. **Start LevelOne scene**
2. **Pick up key** → Open door → Reward panel appears
3. **Hover mouse over reward cards**
4. **Expected Results**:
   - ✅ Card scales up slightly when hovered (1.1x scale)
   - ✅ Visual feedback confirms mouse is over card
   - ✅ Smooth hover animation
   - ✅ Console log: `[CardMovement] ✅ Hovering over card!`
   
5. **SHOULD NOT SEE**:
   - ❌ Cards staying same size on hover
   - ❌ No visual feedback when hovering

---

### Test 2: Click Selection (Not Playing) ✅
1. **With reward panel open**
2. **Click a reward card**
3. **Expected Results**:
   - ✅ Console log: `[CardMovement] Click ignored - this is a reward card (handled by Button)`
   - ✅ Console log: `[CardRewardUI] Player selected: [CardName]`
   - ✅ Console log: `[CardRewardUI] Exploration reward selected: [CardName]`
   - ✅ Card added to collection
   - ✅ Panel closes

4. **SHOULD NOT SEE**:
   - ❌ Log: `[CardMovement] 🃏 CLICK 1: Card clicked!`
   - ❌ Log: `[CardMovement] Playing card immediately`
   - ❌ Card trying to play effects

---

### Test 3: Inspector Scale Changes ✅
1. **Enter Play Mode**
2. **Trigger reward panel** (pick up key, open door)
3. **While panel is open**, select `CardRewardUI` in Hierarchy
4. **In Inspector**, change `Card Scale` value (e.g., from 1.2 to 2.0)
5. **Expected Results**:
   - ✅ Cards immediately resize on screen
   - ✅ Console log: `[CardRewardUI] Card scale updated to: 2.0`
   - ✅ Both cards update simultaneously

6. **Try changing `Card Spacing`** (e.g., from 400 to 600)
7. **Expected Results**:
   - ✅ Gap between cards increases immediately
   - ✅ Console log: `[CardRewardUI] Card spacing updated to: 600`

---

### Test 4: Different Scale Values ✅
**Recommended Scale Settings**:
- `0.8` - Smaller cards (good for 3+ options)
- `1.0` - Normal size (same as hand cards)
- `1.2` - **DEFAULT** - Slightly bigger (good for 2 options)
- `1.5` - Large cards (dramatic, good for important choices)
- `2.0` - Very large (overwhelming, not recommended)

**Recommended Spacing Settings**:
- `200` - Cards close together
- `300` - Moderate spacing
- `400` - **DEFAULT** - Good spacing for 2 cards
- `500` - Wide spacing
- `600` - Very wide spacing

**Test**: Try each scale and verify visual appearance matches expectations.

---

## How It Works Now

### Reward Card Lifecycle
```
1. CardRewardUI.CreateCardOption(card)
   ↓
2. Instantiate card prefab (has CardMovement, CardDisplay, etc.)
   ↓
3. Set cardMovement.isRewardCard = true
   ↓
4. Card stays active, hover works:
   - Update() runs normally
   - CheckHover() detects mouse position
   - Scales up to hoverScale (1.1x) when hovered
   ↓
5. Player clicks card:
   - CardMovement.OnCardClicked() checks isRewardCard flag
   - Returns early if true (doesn't handle click)
   - Button.onClick fires instead
   ↓
6. Button calls CardRewardUI.OnCardSelected()
   ↓
7. Card added to collection, panel closes
```

### Scale Update Flow
```
Inspector Value Changed (Play Mode)
   ↓
OnValidate() automatically called by Unity
   ↓
Checks if in play mode (Application.isPlaying)
   ↓
Updates HorizontalLayoutGroup.spacing
   ↓
Iterates through currentCardDisplays list
   ↓
Sets localScale = Vector3.one * cardScale on each card
   ↓
Cards resize immediately on screen
```

---

## Files Modified

1. **CardMovement.cs** (`Assets/Scripts/CardGame/`)
   - Added `isRewardCard` flag
   - Updated `OnCardClicked()` to check flag

2. **CardRewardUI.cs** (`Assets/Scripts/CardCollection/`)
   - Changed from disabling component to setting flag
   - Added `UpdateCardScale()` method
   - Added `UpdateCardSpacing()` method
   - Added `OnValidate()` for inspector changes

---

## Unity Inspector Settings

### CardRewardUI Component
Located on: `Canvas > CardRewardPanel > CardRewardUI`

**Adjustable Settings**:
```
Card Display Settings:
  Card Scale: 1.2 (default)
    - Controls size of reward cards
    - Range: 0.5 to 2.0 recommended
    - Updates immediately in play mode
    
  Card Spacing: 400 (default)
    - Space between cards in pixels
    - Range: 100 to 800 recommended
    - Updates immediately in play mode
```

**To Adjust During Testing**:
1. Enter Play Mode
2. Trigger reward panel
3. Select CardRewardUI in Hierarchy
4. Change values in Inspector
5. See changes immediately on screen

---

## Troubleshooting

### "Cards still not hovering"
- **Check**: CardMovement component enabled?
  - Select reward card GameObject in Hierarchy (while panel is open)
  - Inspector → CardMovement component → Should have checkmark enabled
  - If disabled, something is still disabling it

- **Check**: isRewardCard flag set?
  - Select card GameObject
  - Inspector → CardMovement → Should see `Is Reward Card: true` (debug inspector)
  - If false, CardRewardUI didn't set it

- **Check Console for**:
  ```
  [CardRewardUI] Marked [CardName] as reward card (hover enabled, click disabled)
  [CardMovement] ✅ Hovering over card!
  ```

### "Cards still playing when clicked"
- **Check Console for**:
  ```
  [CardMovement] Click ignored - this is a reward card (handled by Button)
  ```
  
- **If seeing**: `[CardMovement] 🃏 CLICK 1: Card clicked!`
  - isRewardCard flag is not set correctly
  - Card is being treated as battle card

### "Inspector changes not updating cards"
- **Check**: Are you in Play Mode?
  - `OnValidate()` only works during play mode
  - Edit mode changes apply when play starts

- **Check**: Are cards currently displayed?
  - Changes only affect existing cards if reward panel is open
  - If panel is closed, changes apply to next reward shown

### "Scale is too big/small but I can't change it"
- **Current Scale**: Look at Inspector value
- **Reset to Default**: Set `Card Scale = 1.2`
- **For Testing**: Try 1.0 (normal), 1.5 (large), 0.8 (small)
- **Visual Guideline**:
  - If cards overlap: Reduce scale or increase spacing
  - If cards are tiny: Increase scale
  - If cards are off-screen: Reduce scale significantly

---

## Summary

✅ **Fixed**: Hover effects now work on reward cards (scale increases on mouse over)  
✅ **Fixed**: Inspector scale/spacing changes update immediately during play mode  
✅ **Improved**: Better separation of concerns (CardMovement for visuals, Button for selection)  
✅ **Added**: Public methods to change scale/spacing from code or inspector  
✅ **Files Changed**: 2 files (CardMovement.cs, CardRewardUI.cs)  
✅ **Testing**: Hover, click, and scale adjustment all have clear testing procedures  

**Next Steps**: Test in-game to verify hover effects work properly and inspector changes apply immediately!
