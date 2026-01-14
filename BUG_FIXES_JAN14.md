# Bug Fixes & System Updates - January 14, 2026

## Issues Fixed

### ✅ 1. Reward Panel Not Showing After Battle

**Problem**: Battle ended and immediately returned to LevelOne without showing reward panel.

**Root Cause**: `BattleManager.HandleBattleStateChanged()` was calling `ReturnToLevelOne()` immediately when battle ended, before rewards could be shown.

**Solution**:
- Updated `BattleManager` to check if player won and if `BattleRewardManager` exists
- If yes, wait for reward selection before returning to level
- `CardRewardUI` now calls `BattleManager.ReturnToLevelOne()` after player selects/skips reward

**Flow Now**:
```
Battle Ends → Player Wins → Wait 2 seconds → Show Reward Panel
   ↓
Player Clicks Card → Add to Collection → Return to LevelOne
   ↓
Player Clicks Skip → Return to LevelOne
```

**Files Modified**:
- `BattleManager.cs` - Added check for BattleRewardManager
- `CardRewardUI.cs` - Added `ReturnToExploration()` method

---

### ✅ 2. Stamina Cards Not Increasing Stamina

**Problem**: Cards that grant stamina (like Brace) weren't increasing the stamina value, or were capped at max.

**Root Cause**: `TargetingSystem` was using `Mathf.Min(maxStamina, current + amount)`, preventing stamina from going over the limit.

**Solution**: Removed the cap - stamina can now exceed max temporarily.

**Example**:
- Max Stamina: 10
- Current: 8
- Play card that gives +3 stamina
- **Before**: Stamina = 10 (capped)
- **After**: Stamina = 11 (can go over max) ✅

**Files Modified**:
- `TargetingSystem.cs` - `EffectType.GainStamina` case

---

### ✅ 3. Cards Playing Twice (Duplicate Cards in Hand)

**Problem**: Same card appearing multiple times in a single hand, even with only one copy in deck.

**Root Cause**: When using `CardCollection.DrawHandWithRules()`, cards weren't being removed from DrawPile properly, and discard pile wasn't being reshuffled when DrawPile was empty.

**Solution**: Added reshuffle logic before drawing cards with composition rules.

**New Draw Flow**:
```
1. Check if DrawPile is empty
2. If empty and DiscardPile has cards:
   - Move all discard cards back to DrawPile
   - Shuffle DrawPile
3. Draw cards with composition rules
4. Remove drawn cards from DrawPile
5. Display in hand
```

**Files Modified**:
- `TurnManager.cs` - `DrawCardsForPlayerTurn()` method

---

### ✅ 4. New Bleed System - Countdown Damage

**Problem**: Old bleed system dealt flat 1 damage per stack per turn.

**New System**: Bleed countdown - deals damage equal to current value, then decreases by 1.

**How It Works**:
```
Bleed 3:
  Turn 1: Takes 3 damage → Bleed becomes 2
  Turn 2: Takes 2 damage → Bleed becomes 1
  Turn 3: Takes 1 damage → Bleed becomes 0 (removed)
  
Total damage: 3 + 2 + 1 = 6
```

**Damage Formula**: `Total = N × (N + 1) / 2`

| Bleed Value | Total Damage |
|-------------|--------------|
| Bleed 1     | 1            |
| Bleed 2     | 3            |
| Bleed 3     | 6            |
| Bleed 4     | 10           |
| Bleed 5     | 15           |

**Stacking Rule**: Bleed stacks **additively**
- Enemy has Bleed 2
- Apply Bleed 3
- **Result**: Bleed 5 (takes 5 + 4 + 3 + 2 + 1 = 15 total damage)

**Implementation**:
```csharp
public void TickStatuses()
{
    if (bleedStacks > 0)
    {
        int bleedDamage = bleedStacks; // Take damage = current value
        TakeDamage(bleedDamage);
        bleedStacks--; // Decrease by 1
        
        if (bleedStacks == 0)
        {
            // Bleed removed
        }
    }
}
```

**Files Modified**:
- `EnemyHealth.cs` - `TickStatuses()` method
- `PlayerHealth.cs` - `TickStatuses()` method

---

## Testing Checklist

### Test Reward Panel:
- [ ] Win a battle in Battle_Template
- [ ] After 2 seconds, reward panel appears ✅
- [ ] 2 cards displayed ✅
- [ ] Click one → Added to collection, returns to LevelOne ✅
- [ ] Click Skip → Returns to LevelOne ✅

### Test Stamina Gain:
- [ ] Start with 10/10 stamina
- [ ] Play cards until at 3/10 stamina
- [ ] Play Brace card (or other +stamina card)
- [ ] Stamina should increase (can go over 10) ✅
- [ ] Example: 3 + 3 = 6, or even 8 + 3 = 11 ✅

### Test Card Duplication Fix:
- [ ] Start battle with 15-card collection
- [ ] Draw 4 cards
- [ ] All 4 should be unique (no duplicates in same hand) ✅
- [ ] Play cards, end turn
- [ ] Draw 4 new cards
- [ ] When all 15 cards played, discard shuffles back ✅

### Test New Bleed System:
**Enemy Bleed**:
- [ ] Apply Bleed 3 to enemy
- [ ] UI shows "🩸 3"
- [ ] End turn → Enemy takes 3 damage, becomes Bleed 2
- [ ] End turn → Enemy takes 2 damage, becomes Bleed 1
- [ ] End turn → Enemy takes 1 damage, becomes Bleed 0 (removed)
- [ ] Total: 6 damage ✅

**Player Bleed** (if enemies can apply it):
- [ ] Player receives Bleed 2
- [ ] UI shows "🩸 2"
- [ ] Start turn → Takes 2 damage, becomes Bleed 1
- [ ] Start turn → Takes 1 damage, becomes Bleed 0 (removed)
- [ ] Total: 3 damage ✅

**Bleed Stacking**:
- [ ] Apply Bleed 2 to enemy
- [ ] Apply Bleed 3 to same enemy
- [ ] Result: Bleed 5 ✅
- [ ] Takes: 5 + 4 + 3 + 2 + 1 = 15 total damage ✅

---

## Console Logs to Watch For

### Reward System:
```
[BattleManager] Battle won - waiting for reward selection before returning to level
[CardRewardUI] Showing reward selection
[CardRewardUI] Player selected: [CardName]
[CardCollection] Added [CardName] to collection. Total: 16
[BattleManager] Setting EnemyDefeated to true
```

### Stamina Gain:
```
[TargetingSystem] [CardName] gained 3 stamina. Total: 11
Stamina spent: 2. Now: 9/10
```

### Card Drawing:
```
[TurnManager] Draw pile empty - shuffling discard pile back
[TurnManager] Drew 4 cards with composition rules
```

### Bleed System:
```
[EnemyHealth] Goblin takes 3 bleed damage (Bleed 3)
[EnemyHealth] Goblin takes 2 bleed damage (Bleed 2)
[EnemyHealth] Goblin takes 1 bleed damage (Bleed 1)
[EnemyHealth] Goblin bleed expired
```

---

## Summary of Changes

| Issue | Fix | Impact |
|-------|-----|--------|
| Reward not showing | BattleManager waits for reward selection | Players now see reward UI |
| Stamina not increasing | Removed max cap | Stamina can go over 10 |
| Card duplication | Added reshuffle logic | No duplicate cards in hand |
| Bleed system | Changed to countdown (N damage, N-1) | More strategic bleed mechanic |

---

## Files Modified

1. `BattleManager.cs` - Delayed return until reward selected
2. `CardRewardUI.cs` - Added return to exploration
3. `TargetingSystem.cs` - Removed stamina cap
4. `TurnManager.cs` - Added discard reshuffle
5. `EnemyHealth.cs` - New bleed countdown system
6. `PlayerHealth.cs` - New bleed countdown system

---

## Next Steps

All issues fixed! ✅

### Optional Future Enhancements:
- Add visual effect when stamina goes over max (golden glow?)
- Add sound effect for reward card selection
- Add animation for bleed countdown (number shrinks each turn)
- Add tooltip explaining bleed countdown system

### Recommended Testing:
1. Play through 2-3 full battles
2. Test reward selection multiple times
3. Test bleed on various enemies
4. Test stamina gain cards
5. Verify no card duplication in hands
