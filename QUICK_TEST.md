# Quick Test Guide - Reward Panel

## Setup (One-Time, 2 minutes)

### In TitleScene:
1. Create Empty GameObject → Name it "GameInitializer"
2. Add Component → `GameInitializer` script
3. In Inspector: Check both boxes (Initialize Collection, Show Debug Logs)

### In Battle_Template:
1. **BattleRewardPanel** GameObject:
   - Has `CardRewardUI` script ONLY (remove duplicates!)
   - Assign all 5 fields in Inspector:
     * Reward Panel: (self)
     * Card Prefab: CardPrefab from Assets/Prefabs
     * Card Options Container: (child)
     * Title Text: (child)  
     * Skip Button: (child)

2. **BattleRewardManager** GameObject (separate):
   - Has `BattleRewardManager` script ONLY
   - Assign in Inspector:
     * Check "Show Card Reward After Battle"
     * Delay Before Reward: 2
     * Card Reward UI: BattleRewardPanel GameObject

## Test (30 seconds)

1. Open **TitleScene**
2. Press **Play** ▶️
3. Click **Start Button**
4. Kill enemies in Battle_Template
5. Wait 2 seconds
6. **Reward panel appears!** ✨

## What You'll See

```
Console Logs:
[GameInitializer] CardCollection created
[CardCollection] Initialized with 15 starting cards
[EnemyManager] All enemies defeated! Battle won!
[BattleRewardManager] Battle won! Showing rewards...
[CardRewardUI] Showing reward selection
```

## If It Doesn't Work

### No reward panel?
→ Check BattleRewardManager has CardRewardUI reference assigned

### "CardCollection.Instance is null"?
→ Make sure you played from TitleScene (not Battle_Template)

### Panel shows but no cards?
→ Check CardRewardUI has Card Prefab assigned

## Restore Normal Game Later

In `GameManager.cs`, swap the comments:

```csharp
// TESTING: Load Battle_Template directly
// SceneManager.LoadScene(BattleScene, LoadSceneMode.Single);

// MAIN GAME: Normal flow
SceneManager.LoadScene(LevelOne, LoadSceneMode.Single);
```

Done! 🎯
