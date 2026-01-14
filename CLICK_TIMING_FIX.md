# Click Timing Fix - Click-and-Drag vs Two Separate Clicks

## Problem Identified
User was experiencing **click-and-drag** behavior where they had to:
1. Click and **HOLD** on a card
2. While holding, drag to an enemy
3. Release to attack

Instead of the desired **two separate clicks**:
1. Click card (release)
2. Click enemy (release)

## Root Cause

### The Timing Mismatch
CardMovement and TargetingSystem were using **different input detection methods** that fire at different times:

**CardMovement (CLICK 1):**
```csharp
if (Input.GetMouseButtonDown(0))  // Fires when button PRESSED DOWN
{
    OnCardClicked();
}
```

**TargetingSystem (CLICK 2):**
```csharp
uiClickAction.action.performed += OnClickPerformed;  // Fires when button RELEASED
```

### The Problem Flow
1. User **presses** mouse button on card → `GetMouseButtonDown(0)` fires → CLICK 1 detected
2. User **releases** mouse button → Input System fires → CLICK 2 detected **immediately**
3. If user held the button down and moved to enemy → enemy was targeted while holding

This created a **drag-to-target** behavior instead of **click-then-click** behavior!

## Solution Applied

Changed CardMovement to use `GetMouseButtonUp(0)` instead of `GetMouseButtonDown(0)`:

```csharp
// BEFORE (BROKEN)
if (currentState == 1 && Input.GetMouseButtonDown(0))  // Button press
{
    if (IsMouseOverCard())
    {
        OnCardClicked();
    }
}

// AFTER (FIXED)
if (currentState == 1 && Input.GetMouseButtonUp(0))  // Button release
{
    if (IsMouseOverCard())
    {
        OnCardClicked();
    }
}
```

## How It Works Now

### Unified Timing
Both CLICK 1 and CLICK 2 now fire on button **release**:

1. **CLICK 1**: User clicks card → releases → `GetMouseButtonUp(0)` fires → card selected
2. **CLICK 2**: User clicks enemy → releases → Input System fires → enemy targeted

### Expected User Experience
1. **Hover** over card → card scales up
2. **Click and release** on card → card moves to center, highlights
3. **Click and release** on enemy → card plays, damage dealt
4. Card returns to hand (or is destroyed)

## Testing Verification

### ✅ Correct Behavior (Two Separate Clicks)
```
User Action: Click card → release
[CardMovement] Mouse clicked! State: 1, IsMouseOver: True
[CardMovement] 🃏 CLICK 1: Card clicked!
[TargetingSystem] 🎯 Targeting started
[CardMovement] ✅ Card selected: Punch. Now waiting for CLICK 2...

User Action: Click enemy → release
[TargetingSystem] 🎯 CLICK 2 detected
[TargetingSystem] Successfully playing Punch on enemy!
[EnemyHealth] Enemy took 1 damage
```

### ❌ Old Broken Behavior (Click-and-Drag)
```
User Action: Click card → hold → move to enemy → release
[CardMovement] Mouse clicked! State: 1  (button down)
[CardMovement] 🃏 CLICK 1: Card clicked!
[TargetingSystem] 🎯 Targeting started
[TargetingSystem] 🎯 CLICK 2 detected  (same button release)
[TargetingSystem] Successfully playing Punch on enemy!
```

## Why GetMouseButtonUp() Is Correct

### Input.GetMouseButtonDown(0)
- Fires when button is **pressed down**
- Fires **once** at the moment of press
- Does NOT wait for release

### Input.GetMouseButtonUp(0)
- Fires when button is **released**
- Fires **once** at the moment of release
- Matches Unity's Input System behavior for "Click" actions

### Unity Input System Click Action
- Internally detects button **press AND release**
- The `performed` callback fires on **release**
- This is the standard "click" behavior in UI systems

## Summary

**Before:** CLICK 1 on press, CLICK 2 on release → Same click detected as both actions → Click-and-drag behavior

**After:** CLICK 1 on release, CLICK 2 on release → Two separate clicks required → Proper two-click system ✅

This fix ensures both input methods work in harmony by detecting clicks at the **same moment** (button release).
