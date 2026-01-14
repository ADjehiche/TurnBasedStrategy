# Bug Fixes & New Features Summary

## ✅ Issues Fixed

### 1. Invisibility Popup Location
**Problem**: Popup showed above enemy instead of player
**Solution**: 
- Updated `PlayerStatusEffects.ApplyInvisibility()` to show popup at player's damage anchor
- Now uses `PlayerHealth.Instance.GetDamagePopupAnchor()` for correct position

### 2. Invisibility Visual Effect
**Problem**: Only made player semi-transparent
**Solution**: 
- Now completely hides player model using `playerModel.SetActive(false)`
- Restored after invisibility expires with `playerModel.SetActive(true)`
- **Setup Required**: Assign player's visual model to `PlayerStatusEffects.playerModel` field in inspector

### 3. Whirlwind Not Hitting All Enemies
**Problem**: AllEnemies cards only damaged one enemy
**Solution**:
- Updated `TargetingSystem.ResolveCard()` to check `card.targetType == TargetType.AllEnemies`
- Now loops through `EnemyManager.Instance.GetAllEnemies()` and damages each one
- Applies to all AllEnemies cards (Whirlwind, future AOE cards)

### 4. Battle Ending After One Enemy Death
**Problem**: Battle 2 ended when first enemy died instead of all enemies
**Solution**:
- Removed `BattleState.SetOver(true)` from `EnemyHealth` death
- Added `EnemyManager.CheckBattleEndAfterDelay()` coroutine
- EnemyHealth notifies EnemyManager → waits 0.6s → checks if ALL enemies dead
- Only ends battle when `AllEnemiesDefeated()` returns true

### 5. Cards Per Turn
**Status**: Already set to 4 in `TurnManager.cardsPerTurn` (no changes needed)

---

## 🆕 Card Collection System

### Overview
Players now have a **persistent card collection** that grows throughout the game. Starting with 15 cards, they can earn more through battle victories.

### New Scripts

#### 1. `CardCollection.cs`
- Manages player's owned cards (persists via DontDestroyOnLoad)
- Generates 15 starting cards: 8 Attack, 4 Defense, 3 Utility
- Allows duplicates (can have 3x Quick Slash)
- Provides hand composition rules
- Location: `Assets/Scripts/CardCollection/`

#### 2. `CardRewardUI.cs`
- Post-battle reward selection UI
- Shows 2 random cards from starter pool
- Player clicks one to add to collection
- Location: `Assets/Scripts/CardCollection/`

#### 3. `BattleRewardManager.cs`
- Triggers rewards after battle victory
- Delays 2 seconds for victory animations
- Only shows rewards if all enemies defeated
- Location: `Assets/Scripts/CardCollection/`

#### 4. `GameInitializer.cs`
- Sets up CardCollection at game start (TitleScene)
- Initializes 15-card starting deck
- Location: `Assets/Scripts/CardCollection/`

### Hand Composition Rules

Every hand drawn guarantees:
- ✅ **1+ Attack card** (minimum 1)
- ✅ **1+ Defense card** (minimum 1)
- ✅ **0-1 Utility card** (70% chance, max 1)
- ✅ **4th card**: Random from remaining

**Example Hand**:
- Quick Slash (Attack)
- Block (Defense)
- Battle Focus (Utility)
- Power Strike (Attack) ← 4th slot

### Starting Deck Generation

**Ratios**: 8 Attack / 4 Defense / 3 Utility = 15 cards total

**Example Starting Deck**:
```
Attack (8): Quick Slash, Quick Slash, Power Strike, Slash, Cleave, Quick Slash, Power Strike, Slash
Defense (4): Block, Block, Brace, Shield Bash
Utility (3): Battle Focus, Disarm, Battle Focus
```

Duplicates allowed! Random selection from 23 starter cards.

### Battle Reward Flow

1. **Win battle** → All enemies defeated
2. **Wait 2 seconds** → Victory animations play
3. **Show UI** → 2 random card choices appear
4. **Player clicks one** → Card added to collection
5. **Collection grows** → 15 → 16 → 17 → etc.
6. **Next battle** → Uses updated collection

### Merge Cards & Categories

Merge-only cards follow their component categories for hand rules:

**Attack Merges**:
- Whirlwind, Devastating Blow, Executioner's Axe

**Defense Merges**:
- Iron Fortress, Immovable Object

**Utility Merges**:
- Perfect Parry, Smoke Bomb, Mirror Shield, Bloodlust

### Integration Changes

#### DeckManager
- Now checks `CardCollection.Instance` first
- Uses player's owned cards (with duplicates)
- Falls back to old Resources loading if no collection

#### TurnManager
- Uses `CardCollection.DrawHandWithRules()` for guaranteed composition
- Falls back to normal draw if no collection

#### EnemyHealth
- No longer ends battle on death
- Notifies EnemyManager to check if all dead

#### EnemyManager
- New method: `CheckBattleEndAfterDelay()`
- Only ends battle when ALL enemies defeated

---

## 🎮 Unity Setup Required

### 1. Title Scene Setup
```
Create GameObject: "CardCollectionManager"
Add Component: GameInitializer
Set: Initialize Collection On Start = true
```

### 2. Battle Scene Setup
```
Create UI Panel: BattleRewardPanel
  ├── TitleText (TMP)
  ├── CardOptionsContainer (Horizontal Layout Group)
  └── SkipButton

Create GameObject: "BattleRewardManager"
Add Component: BattleRewardManager
Add Component: CardRewardUI
Assign all UI references
```

### 3. Player Invisibility Setup
```
Find: PlayerStatusEffects component
Assign: Player Model field → Your player's visual mesh/model GameObject
```

---

## 📝 Testing Checklist

### Invisibility
- [ ] Popup shows above player (not enemy)
- [ ] Player model disappears completely
- [ ] Player reappears after 1 turn

### Whirlwind (AllEnemies)
- [ ] Damages ALL enemies in battle
- [ ] Each enemy takes 2 damage
- [ ] Works with 2+ enemies

### Multi-Enemy Battles
- [ ] Battle 2 continues after first enemy dies
- [ ] Battle ends only when ALL enemies dead
- [ ] Reward UI appears after all dead

### Card Collection
- [ ] 15 cards at game start (8/4/3 ratio)
- [ ] Every hand has 1+ Attack, 1+ Defense
- [ ] Max 1 Utility per hand
- [ ] Rewards show after battle victory
- [ ] Chosen card adds to collection
- [ ] Next battle uses updated collection

---

## 📚 Documentation

See these files for details:
- **CARD_COLLECTION_SYSTEM.md** - Complete system guide
- **PLAYER_STATUS_DISPLAY_SETUP.md** - Player status UI setup
- **STATUS_EFFECTS_IMPLEMENTATION.md** - Status effects guide

---

## 🔧 Technical Notes

### Why Duplicates in Collection?
Allows deck evolution - players can get multiple copies of strong cards, creating synergies. Matches roguelike deckbuilder genre (Slay the Spire, Monster Train).

### Why Guaranteed Hand Composition?
Prevents "dead hands" (all defense, no attacks). Ensures every turn is playable. Common in card games to prevent feel-bad RNG.

### Why AllEnemies Fix in TargetingSystem?
Original code only passed single `enemy` parameter. AllEnemies cards need to query EnemyManager for full list and loop through them.

### Why Delay in CheckBattleEnd?
Prevents race condition - wait for GameObject destruction and animations to complete before checking enemy count.

---

## ❓ Questions Answered

**Q: Can merge cards appear as rewards?**
A: No, only the 23 starter cards appear as rewards. Merge cards only obtainable through merging.

**Q: Is collection saved between game sessions?**
A: Not yet - collection resets on game restart. Save system is a future enhancement.

**Q: Can cards be removed from collection?**
A: System supports it (`CardCollection.RemoveCard()`), but no UI/mechanic yet.

**Q: What fills the 4th hand slot?**
A: Random card from remaining deck (after guaranteeing 1 Attack, 1 Defense, 0-1 Utility).

---

## 🚀 Next Steps

1. **Unity Setup** (see sections above)
2. **Test each fix** (use checklist)
3. **Play full battle** (start to reward)
4. **Verify collection persists** (across multiple battles)

Need help with setup or finding issues? Check the detailed documentation files!
