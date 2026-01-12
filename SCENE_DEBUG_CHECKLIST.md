# Scene Setup Debug Checklist - Two-Click System Not Working

## Problem: Cards Playing Immediately (Not Waiting for CLICK 2)

If cards are playing immediately without waiting for you to click a target, the issue is likely in the **scene configuration** or **Input System setup**.

---

## 🔍 Checklist: Things to Check in Unity Inspector

### 1. TargetingSystem GameObject
**Location**: Find the GameObject with the `TargetingSystem` component (likely named "BattleSystem" or "TargetingSystem")

**Inspector Settings to Check:**

#### A. InputActionReferences (Critical!)
Look for these fields in the TargetingSystem component:
- **uiClickAction** → Should be **ASSIGNED** to "UI/Click" or "UI/Point/Press"
- **uiCancelAction** → Should be **ASSIGNED** to "UI/Cancel" or "Player/Cancel"
- **uiRightClickAction** → Should be **ASSIGNED** to "UI/RightClick" or similar

**⚠️ CRITICAL**: These MUST be assigned for the two-click system to work!

**If they're NOT assigned:**
1. Select the TargetingSystem GameObject
2. Find the "Ui Click Action" field
3. Click the circle next to it
4. Select "DefaultInputActions" → "UI" → "Click" (or "Point" → "Press")
5. Repeat for "Ui Cancel Action" and "Ui Right Click Action"

#### B. Camera Reference
- **worldCamera** → Should be assigned to your main camera

---

### 2. Check for Duplicate TargetingSystem Components
**Problem**: If there are multiple TargetingSystem objects in the scene, they might conflict.

**How to Check:**
1. In Hierarchy, search for "TargetingSystem" or "BattleSystem"
2. Make sure there's only ONE GameObject with the TargetingSystem component
3. Check the console on Play for "DUPLICATE INSTANCE DETECTED!" warnings

**If duplicates found:**
- Delete the extra TargetingSystem GameObjects
- Keep only one

---

### 3. EventSystem Check
**Problem**: Without an EventSystem, UI raycasts won't work and targeting breaks.

**How to Check:**
1. In Hierarchy, look for "EventSystem" GameObject
2. It should have these components:
   - EventSystem
   - StandaloneInputModule

**If missing:**
1. Right-click in Hierarchy → UI → Event System
2. This will create the EventSystem automatically

---

### 4. Input Actions Asset
**Problem**: If Input Actions aren't enabled or configured wrong, clicks won't register.

**How to Check:**
1. In Project window, search for "DefaultInputActions" or "InputActions"
2. Double-click to open the Input Actions window
3. Check that these actions exist:
   - **UI/Click** or **UI/Point/Press**
   - **UI/Cancel**
   - **UI/RightClick**

**If actions are missing or disabled:**
1. Click "Enable" at the top of the Input Actions window
2. Make sure "UI/Click" is bound to "Left Mouse Button"

---

### 5. Card Prefab Check
**Problem**: CardMovement component might have wrong settings.

**How to Check:**
1. In Project, find "Assets/Prefabs/CardPrefab"
2. Select it and look at CardMovement component
3. Check these fields:
   - **hoverScale** → Should be around 1.1
   - **selectedScale** → Should be around 1.3
   - **selectedPosition** → Should be something like (0, 100, 0)

---

## 🐛 Debug Console Checklist

When you play a SingleEnemy card (like Punch), you should see this sequence:

### Expected Console Output (Two-Click System Working):
```
[CardMovement] ✅ Hovering over card!
[CardMovement] Mouse clicked! State: 1, IsMouseOver: True
[CardMovement] 🃏 CLICK 1: Card clicked!
[TargetingSystem] 🎯 Targeting started for card: Punch (cost 0)
[TargetingSystem] 👆 Waiting for CLICK 2 on target...
[TargetingSystem] ✅ Input actions enabled and ready for CLICK 2  <-- NEW LOG
[CardMovement] ✅ Card selected: Punch. Now waiting for CLICK 2 on target...

... (you click on enemy) ...

[TargetingSystem] 🎯 CLICK 2 detected at screen position: (259.00, 91.00)
[TargetingSystem] Successfully playing Punch on enemy!
[EnemyHealth] EnemyContainer 1 took 1 damage. HP: 20 → 19/20
```

### ❌ Bad Console Output (System Not Working):
If you see CLICK 2 **immediately** after CLICK 1 without you clicking:
```
[CardMovement] 🃏 CLICK 1: Card clicked!
[TargetingSystem] 🎯 Targeting started for card: Punch (cost 0)
[TargetingSystem] 🎯 CLICK 2 detected at screen position: (259.00, 91.00)  <-- TOO FAST!
```

**This means:** Input actions are firing immediately, likely because:
1. InputActionReferences are not assigned in inspector
2. There are duplicate event handlers subscribed
3. The action is catching the mouse release from CLICK 1

---

## 🔧 Recent Code Fixes Applied

### Fix #1: Unsubscribe Before Subscribe
**File**: `TargetingSystem.cs` → `BeginTargeting()`
```csharp
// CRITICAL: Unsubscribe first to prevent duplicate subscriptions
if (uiClickAction != null && uiClickAction.action != null)
{
    uiClickAction.action.performed -= OnClickPerformed;
}
```

### Fix #2: Wait 1 Frame Before Enabling Input
**File**: `TargetingSystem.cs` → `EnableInputActionsNextFrame()`
```csharp
// Wait one frame to avoid catching the mouse release from CLICK 1
yield return null;
```

This prevents the Input System from catching the mouse **release** event from CLICK 1 as if it were CLICK 2.

### Fix #3: Clean Unsubscribe in Awake
**File**: `TargetingSystem.cs` → `Awake()`
```csharp
// Unsubscribe stale handlers from scene reloads
uiClickAction.action.performed -= OnClickPerformed;
```

---

## 🎯 Quick Test Steps

1. **Start Play Mode** in Unity
2. **Click on a Punch card** (SingleEnemy type)
3. **Check console** - Do you see "✅ Input actions enabled and ready for CLICK 2"?
4. **Wait a moment**, then **click on an enemy**
5. **Expected**: Enemy takes damage
6. **Not Expected**: Card plays immediately without letting you target

If it still doesn't work, check:
- Are the InputActionReferences assigned in the TargetingSystem inspector?
- Is there more than one TargetingSystem in the scene?
- Is there an EventSystem in the scene?

---

## 🆘 If Nothing Works

**Nuclear Option**: Clear and re-assign Input Actions
1. Select TargetingSystem GameObject
2. Set all InputActionReference fields to "None"
3. Click "Apply" or save the scene
4. Re-assign them one by one:
   - Ui Click Action → DefaultInputActions → UI → Click
   - Ui Cancel Action → DefaultInputActions → UI → Cancel
   - Ui Right Click Action → DefaultInputActions → UI → RightClick (if exists)
5. Save scene and test again

**Last Resort**: In inspector, try **unchecking** all InputActionReferences and rely only on `Input.GetMouseButtonDown()` in CardMovement (which doesn't need Input Actions)
