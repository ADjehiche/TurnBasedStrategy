# Quick Fix: Reward Panel Setup

## The Problem
You said: "I have 3 game objects" - **That's the problem!** You should have **2** GameObjects, not 3.

## Current Setup (WRONG ❌)
```
GameObject 1: BattleRewardPanel
- Has: BattleRewardManager script ❌ WRONG!
- Has: CardRewardUI script
- Children: CardOptionsContainer, TitleText, SkipButton

GameObject 2: CardRewardUI
- Has: CardRewardUI script ❌ DUPLICATE!

GameObject 3: BattleRewardManager  
- Has: BattleRewardManager script ❌ DUPLICATE!
```

**Problem**: You have duplicate scripts competing with each other!

## Correct Setup (RIGHT ✅)

### Option A: Two Separate GameObjects (RECOMMENDED)

```
GameObject 1: BattleRewardPanel
- Has: CardRewardUI script ✅ ONLY THIS
- Children: CardOptionsContainer, TitleText, SkipButton

GameObject 2: BattleRewardManager
- Has: BattleRewardManager script ✅ ONLY THIS
- No children needed
```

### Option B: Single GameObject (Alternative)

```
GameObject: BattleRewardPanel
- Has: CardRewardUI script ✅
- Has: BattleRewardManager script ✅
- Children: CardOptionsContainer, TitleText, SkipButton
```

## Step-by-Step Fix (5 minutes)

### 1. Delete Duplicates
- **DELETE** the separate "CardRewardUI" GameObject entirely
- **DELETE** the separate "BattleRewardManager" GameObject if using Option A (or keep if using Option A)

### 2. On BattleRewardPanel GameObject
- **Remove** BattleRewardManager script component (click gear icon → Remove Component)
- **Keep** CardRewardUI script
- Verify children exist: CardOptionsContainer, TitleText, SkipButton

### 3. Create BattleRewardManager GameObject (Option A)
- Right-click in Hierarchy → Create Empty
- Name it "BattleRewardManager"
- Add Component → BattleRewardManager script

### 4. Assign References in CardRewardUI (on BattleRewardPanel)
Inspector for CardRewardUI script:
- **Reward Panel**: Drag BattleRewardPanel itself
- **Card Prefab**: `Assets/Prefabs/CardPrefab`
- **Card Options Container**: Drag CardOptionsContainer child
- **Title Text**: Drag TitleText child
- **Skip Button**: Drag SkipButton child

### 5. Assign References in BattleRewardManager
Inspector for BattleRewardManager script:
- **Show Card Reward After Battle**: ✅ Checked
- **Delay Before Reward**: 2
- **Card Reward UI**: Drag BattleRewardPanel GameObject

## Test It!

1. **Play Battle_Template scene**
2. **Kill all enemies**
3. **Wait 2 seconds**
4. **Reward panel appears!** ✨

## Console Check

You should see:
```
[EnemyManager] All enemies defeated! Battle won!
[BattleRewardManager] Battle won! Showing rewards...
[CardRewardUI] Showing reward selection
```

If you see:
```
[CardRewardUI] CardCollection.Instance is null!
```

Then you need to:
1. Go to **TitleScene**
2. Create GameObject → Add `GameInitializer` script
3. **Play from TitleScene**, not Battle_Template

## Why This Happens

Both scripts use singletons:
```csharp
public static BattleRewardManager Instance { get; private set; }
public static CardRewardUI Instance { get; private set; }
```

When you have duplicates, Unity's `Awake()` destroys one instance:
```csharp
if (Instance != null && Instance != this)
{
    Destroy(gameObject); // ← Destroys the duplicate!
    return;
}
```

But you can't predict WHICH one gets destroyed! So references break and nothing works.

**Solution**: Only ONE of each script in the entire scene! ✅
