# Reward Panel Not Showing - Fix Guide

## Issue
The reward panel isn't appearing after battle completion. This is due to **duplicate scripts** and incorrect GameObject setup.

## Root Cause
You currently have:
- Multiple `CardRewardUI` scripts on different GameObjects (causing conflicts)
- Multiple `BattleRewardManager` scripts on different GameObjects (causing singleton conflicts)
- Incorrect hierarchy setup

## Correct Setup

### Step 1: Clean Up Duplicates

#### On `BattleRewardPanel` GameObject:
1. **REMOVE** the `BattleRewardManager` script (if attached)
2. **KEEP** only the `CardRewardUI` script
3. Ensure child objects exist:
   - `CardOptionsContainer` (empty Transform/RectTransform)
   - `TitleText` (TextMeshProUGUI)
   - `SkipButton` (Button)

#### On `CardRewardUI` GameObject (if this is a separate GameObject):
1. **DELETE** this GameObject entirely, OR
2. **REMOVE** the `CardRewardUI` script from it

#### On `BattleRewardManager` GameObject:
1. **KEEP** only the `BattleRewardManager` script
2. **REMOVE** any `CardRewardUI` script (if attached)

### Step 2: Correct Hierarchy

Your Battle_Template scene should have:

```
Canvas
└── BattleFieldRoot
    └── BattleRewardPanel
        ├── CardOptionsContainer
        ├── TitleText
        ├── SkipButton
        └── CardRewardUI (script component)

BattleSystem (or other root GameObject)
└── BattleRewardManager (script component)
```

**OR** as separate GameObjects:

```
BattleRewardPanel (GameObject with CardRewardUI script)
├── CardOptionsContainer
├── TitleText
└── SkipButton

BattleRewardManager (Empty GameObject with BattleRewardManager script)
```

### Step 3: Inspector Setup for CardRewardUI

Select `BattleRewardPanel` GameObject, inspect `CardRewardUI` script:

✅ **Reward Panel**: Drag `BattleRewardPanel` itself here
✅ **Card Prefab**: Drag `Assets/Prefabs/CardPrefab` (same one HandManager uses)
✅ **Card Options Container**: Drag the `CardOptionsContainer` child
✅ **Title Text**: Drag the `TitleText` child
✅ **Skip Button**: Drag the `SkipButton` child

### Step 4: Inspector Setup for BattleRewardManager

Select the GameObject with `BattleRewardManager` script:

✅ **Show Card Reward After Battle**: Checked (true)
✅ **Delay Before Reward**: 2 (seconds)
✅ **Card Reward UI**: Drag the `BattleRewardPanel` GameObject (the one with CardRewardUI script)

## Why This Works

### Singleton Pattern
Both scripts use singletons:
```csharp
public static BattleRewardManager Instance { get; private set; }
public static CardRewardUI Instance { get; private set; }
```

If you have **duplicates**, the `Awake()` method destroys one of them:
```csharp
if (Instance != null && Instance != this)
{
    Destroy(gameObject);
    return;
}
```

This causes unpredictable behavior - the wrong instance might be destroyed!

### Script Responsibilities

**BattleRewardManager** (Separate GameObject):
- Listens to `BattleState.OnBattleOverChanged` event
- Waits 2 seconds after victory
- Calls `CardRewardUI.ShowRewardSelection()`

**CardRewardUI** (On BattleRewardPanel):
- Manages the UI panel visibility
- Gets 2 random cards from `CardCollection`
- Creates card display instances
- Handles card selection
- Calls `BattleManager.ReturnToLevelOne()` after selection

## Testing Steps

1. **Delete ALL duplicate GameObjects/scripts**
2. **Set up ONE BattleRewardManager** (separate GameObject in scene)
3. **Set up ONE CardRewardUI** (on BattleRewardPanel GameObject)
4. **Assign all references in Inspector**
5. **Start Battle_Template scene**
6. **Kill all enemies**
7. **Wait 2 seconds** → Reward panel should appear!

## Console Logs to Watch

✅ **Expected (Working)**:
```
[EnemyManager] All enemies defeated! Battle won!
[BattleRewardManager] Battle won! Showing rewards...
[CardRewardUI] Showing reward selection
```

❌ **Error (Not Working)**:
```
[CardRewardUI] CardCollection.Instance is null!
// OR
[BattleRewardManager] CardRewardUI not found! Cannot show card reward.
// OR
No logs at all (scripts destroyed due to duplicate singletons)
```

## Common Issues

### Issue 1: "CardCollection.Instance is null"
**Solution**: You need to set up `CardCollection` first!
1. Create `GameInitializer` GameObject in TitleScene
2. Attach `GameInitializer` script
3. Play from TitleScene (not Battle_Template directly)

### Issue 2: Panel shows but no cards
**Solution**: Check `CardRewardUI` inspector:
- Card Prefab assigned?
- CardOptionsContainer assigned?
- CardCollection initialized with cards?

### Issue 3: Panel never appears
**Solution**: 
1. Check BattleRewardManager has CardRewardUI reference assigned
2. Check BattleRewardPanel starts inactive (`rewardPanel.SetActive(false)` in Start)
3. Check battle actually ends with victory (all enemies dead)
4. Add debug log in `CardRewardUI.ShowRewardSelection()` first line

### Issue 4: Multiple instances destroying each other
**Solution**: 
- Search entire scene for duplicate scripts
- Use Unity menu: Edit → Find References In Scene
- Remove ALL duplicates, keep only ONE of each script

## Quick Fix Checklist

- [ ] Only ONE `BattleRewardManager` script in scene
- [ ] Only ONE `CardRewardUI` script in scene  
- [ ] BattleRewardManager has CardRewardUI reference assigned
- [ ] CardRewardUI has all UI references assigned (panel, prefab, container, text, button)
- [ ] BattleRewardPanel GameObject starts with panel inactive
- [ ] CardCollection exists (play from TitleScene with GameInitializer)
- [ ] Card prefab is the same one HandManager uses
- [ ] Console shows "[BattleRewardManager] Battle won! Showing rewards..."
- [ ] Console shows "[CardRewardUI] Showing reward selection"

## Final Scene Structure

```
Battle_Template Scene
│
├── Canvas
│   └── BattleFieldRoot
│       ├── PlayerHPUI
│       ├── EnemyHPUI
│       ├── HandArea
│       └── BattleRewardPanel ← CardRewardUI script HERE
│           ├── CardOptionsContainer
│           ├── TitleText
│           └── SkipButton
│
├── BattleSystem
│   ├── TurnManager
│   ├── DeckManager
│   ├── HandManager
│   ├── TargetingSystem
│   └── BattleRewardManager ← BattleRewardManager script HERE
│
└── Enemies
    ├── EnemyContainer 1
    └── EnemyContainer 2
```

If you follow this structure exactly, the reward panel WILL show up! 🎯
