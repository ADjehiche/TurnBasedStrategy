# Testing Mode: Direct Battle_Template Loading

## What Changed

Modified `GameManager.StartGame()` to load **Battle_Template** directly instead of LevelOne for quick reward system testing.

## Current Setup (TESTING MODE)

```csharp
public void StartGame()
{
    // TESTING: Load Battle_Template directly to test reward system
    SceneManager.LoadScene(BattleScene, LoadSceneMode.Single);
    
    // MAIN GAME: Uncomment below to restore normal flow
    // SceneManager.LoadScene(LevelOne, LoadSceneMode.Single);
}
```

## How to Test the Reward System

### Step 1: Setup TitleScene

1. Open **TitleScene** in Unity
2. Create empty GameObject named "GameInitializer"
3. Add `GameInitializer` script component
4. Configure in Inspector:
   - ☑ **Initialize Collection On Start**: Checked
   - **Card Collection Prefab**: Leave empty (auto-creates)
   - ☑ **Show Debug Logs**: Checked (to see what's happening)

### Step 2: Setup Battle_Template Scene

Make sure you have the correct reward system setup (from previous guides):

**Option A: Two GameObjects**
```
BattleRewardPanel (with CardRewardUI script)
├── CardOptionsContainer
├── TitleText
└── SkipButton

BattleRewardManager (separate GameObject with BattleRewardManager script)
```

**Assign References:**
- CardRewardUI: All 5 fields (panel, prefab, container, text, button)
- BattleRewardManager: cardRewardUI reference points to BattleRewardPanel

### Step 3: Play from TitleScene

1. ✅ Open **TitleScene**
2. ✅ Press **Play**
3. ✅ Click **Start Button** (or whatever your play button is)
4. ✅ Scene loads directly to **Battle_Template**
5. ✅ CardCollection is initialized with 15 starting cards
6. ✅ Kill all enemies in battle
7. ✅ Wait 2 seconds
8. ✅ **Reward panel appears!** 🎉

## Expected Console Output

### On TitleScene Start:
```
[GameInitializer] CardCollection created
[CardCollection] Initialized with 15 starting cards
[CardCollection] - 8 Attack cards
[CardCollection] - 4 Defense cards
[CardCollection] - 3 Utility cards
```

### On Battle_Template Load:
```
[DeckManager] Built deck with 15 cards from CardCollection
[TurnManager] StartPlayerTurn -> PlayerTurn
[EnemyManager] Found 2 enemies in scene
```

### On Battle Victory:
```
[EnemyManager] All enemies defeated! Battle won!
[BattleRewardManager] Battle won! Showing rewards...
[CardRewardUI] Showing reward selection
```

### If Something Goes Wrong:
```
❌ [CardRewardUI] CardCollection.Instance is null!
   → Fix: Make sure GameInitializer exists in TitleScene

❌ [BattleRewardManager] CardRewardUI not found! Cannot show card reward.
   → Fix: Check BattleRewardManager has CardRewardUI reference assigned

❌ No reward logs at all
   → Fix: Check for duplicate scripts (only ONE of each!)
```

## Restoring Normal Game Flow

When you're done testing and want to restore the normal game flow:

### Edit GameManager.cs:

```csharp
public void StartGame()
{
    // TESTING: Load Battle_Template directly to test reward system
    // SceneManager.LoadScene(BattleScene, LoadSceneMode.Single);
    
    // MAIN GAME: Uncomment below to restore normal flow
    SceneManager.LoadScene(LevelOne, LoadSceneMode.Single);
}
```

Just swap the comments - comment out Battle_Template line, uncomment LevelOne line.

## Why This Works

### Normal Flow (Production):
```
TitleScene → LevelOne → (player explores) → Battle_Template → (battle) → LevelOne
```

### Testing Flow (Current):
```
TitleScene → Battle_Template → (test reward) → Can't return to LevelOne (no GameSession data)
```

**Note**: The return to LevelOne won't work in testing mode because there's no saved player position in GameSession. That's okay - you're just testing the reward panel appearance!

## Quick Test Checklist

### Before Testing:
- [ ] TitleScene has GameInitializer GameObject with script
- [ ] Battle_Template has ONE BattleRewardManager script
- [ ] Battle_Template has ONE CardRewardUI script
- [ ] All references assigned in Inspector
- [ ] BattleRewardPanel starts inactive in scene

### During Testing:
- [ ] Play from TitleScene (not Battle_Template directly!)
- [ ] Click Start button
- [ ] Battle_Template loads
- [ ] Check console for CardCollection initialization logs
- [ ] Kill all enemies
- [ ] Wait 2 seconds
- [ ] Reward panel appears with 2 cards

### Success Indicators:
- ✅ Console shows "[CardCollection] Initialized with 15 starting cards"
- ✅ Console shows "[BattleRewardManager] Battle won! Showing rewards..."
- ✅ Console shows "[CardRewardUI] Showing reward selection"
- ✅ Reward panel visible with 2 clickable cards
- ✅ Clicking card adds it to collection

## Troubleshooting

### Problem: "CardCollection.Instance is null"
**Solution**: GameInitializer isn't running. Make sure:
1. GameInitializer GameObject exists in TitleScene
2. GameInitializer script is attached
3. You're playing from TitleScene, not Battle_Template

### Problem: Reward panel never appears
**Solution**: Check references. In Battle_Template:
1. Find BattleRewardManager GameObject
2. Check Inspector: "Card Reward UI" field must have BattleRewardPanel assigned
3. Find BattleRewardPanel GameObject  
4. Check Inspector: All CardRewardUI fields must be assigned

### Problem: Panel shows but no cards
**Solution**: 
1. CardRewardUI needs "Card Prefab" assigned (use same one as HandManager)
2. CardOptionsContainer must be assigned
3. Check console for CardCollection card count

### Problem: Cards show but can't click them
**Solution**:
1. Cards need CardMovement script
2. Cards need Button or clickable components
3. Check CardRewardUI.CreateCardOption() is adding click listeners

## Testing Duplicate Cards (Your Request)

You mentioned "duplicate cards in hands are okay". To test this:

1. Play from TitleScene → Battle_Template
2. Draw cards (should see hand composition rules working)
3. Win battle
4. Select a card from rewards
5. Start another battle (would need manual scene reload in testing mode)
6. Check if duplicate cards can appear in same hand

**Note**: With the current system:
- ✅ You CAN own multiple copies of same card
- ✅ Duplicates CAN appear in different turns
- ✅ Hand composition rules still apply (1+ Attack, 1+ Defense, max 1 Utility)

## Next Steps After Testing

Once you confirm the reward panel works:

1. **Restore normal flow**: Comment out Battle_Template line in StartGame()
2. **Test full game loop**: TitleScene → LevelOne → Battle → Reward → Return to LevelOne
3. **Test multiple battles**: Verify card collection persists across battles
4. **Test deck building**: Check that owned cards appear in future battles

---

**TL;DR**: 
- TitleScene now loads Battle_Template directly
- Just press Play in TitleScene and click Start
- Kill enemies, wait 2 seconds, reward panel appears
- This lets you quickly test the reward system! 🎯
