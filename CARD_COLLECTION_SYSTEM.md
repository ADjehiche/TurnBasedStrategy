# Card Collection System - Complete Implementation Guide

## Overview
Players now have a **persistent card collection** that travels with them throughout the game. Cards are earned at game start and through battle rewards.

## System Components

### 1. CardCollection.cs
**Purpose**: Manages player's owned cards throughout the game session

**Key Features**:
- Stores 15 starting cards (8 Attack, 4 Defense, 3 Utility)
- Allows duplicates in collection
- Persists across scenes via DontDestroyOnLoad
- Provides card reward options after battles
- Enforces hand composition rules

**Methods**:
- `InitializeStartingCollection()` - Creates 15-card starting deck
- `AddCard(Card)` - Add card from rewards
- `GetRandomRewardOptions(int)` - Get choices for player
- `DrawHandWithRules(drawPile, handSize)` - Draw with guaranteed composition

### 2. CardRewardUI.cs
**Purpose**: Shows post-battle card selection interface

**Features**:
- Displays 2 random card choices
- Player clicks one to add to collection
- Can skip rewards
- Automatically hides after selection

### 3. BattleRewardManager.cs
**Purpose**: Triggers reward UI after battle victory

**Features**:
- Listens to BattleState.OnBattleOverChanged
- Only shows rewards if player wins (all enemies defeated)
- Delays reward UI to let victory animations play

### 4. GameInitializer.cs
**Purpose**: Sets up card collection at game start

**Features**:
- Creates CardCollection singleton on first scene
- Initializes 15 starting cards
- Can reset collection for new game

## Hand Composition Rules

Every hand drawn follows these rules:
- ✅ **Guaranteed 1 Attack card** (minimum)
- ✅ **Guaranteed 1 Defense card** (minimum)
- ✅ **Maximum 1 Utility/Tactical card** (70% chance)
- ✅ **4th slot**: Random card from remaining deck

**Merge cards follow their category**:
- Whirlwind (Attack + Attack) = Attack category → counts as Attack
- Iron Fortress (Defense + Defense) = Defense category → counts as Defense

## Integration with Existing Systems

### DeckManager Updates
**Before**: Loaded ALL unique cards from Resources (no duplicates)
**After**: 
1. First checks if CardCollection exists
2. Uses player's owned cards (with duplicates)
3. Falls back to old behavior if no collection

### TurnManager Updates
**Before**: Drew random cards from deck
**After**:
1. Calls `CardCollection.DrawHandWithRules()`
2. Guarantees composition (1 Attack, 1 Defense, max 1 Utility)
3. Falls back to normal draw if no collection

### EnemyHealth Updates
**Before**: Set BattleState.SetOver(true) immediately when ONE enemy died
**After**: Notifies EnemyManager to check if ALL enemies are dead first

### EnemyManager Updates
**New Method**: `CheckBattleEndAfterDelay()` - Waits 0.6s then checks if all enemies defeated

## Unity Setup Instructions

### Step 1: Create CardCollection in Title Scene
1. Open `TitleScene` (your first scene)
2. Create empty GameObject: "CardCollectionManager"
3. Add `GameInitializer` component
4. Check "Initialize Collection On Start"

### Step 2: Create Battle Reward UI
1. Open `Battle_Template` scene
2. In Canvas, create UI structure:
```
Canvas
└── BattleRewardPanel (Panel)
    ├── TitleText (TextMeshProUGUI) - "Choose Your Reward"
    ├── CardOptionsContainer (Horizontal Layout Group)
    │   └── (Card options spawn here)
    └── SkipButton (Button) - "Skip Reward"
```

3. Create empty GameObject: "BattleRewardManager"
4. Add `BattleRewardManager` component

5. Create empty GameObject under Canvas: "CardRewardUI"
6. Add `CardRewardUI` component
7. Assign references:
   - **Reward Panel**: Drag BattleRewardPanel
   - **Card Options Container**: Drag CardOptionsContainer
   - **Title Text**: Drag TitleText
   - **Skip Button**: Drag SkipButton

8. In BattleRewardManager, assign:
   - **Card Reward UI**: Drag CardRewardUI GameObject

### Step 3: Assign Player Model for Invisibility
1. Find `PlayerStatusEffects` component in Battle scene
2. Assign **Player Model** field to your player's visual mesh/model
3. This allows invisibility to hide the player

## Starting Deck Generation

When game starts:
1. `GameInitializer` creates CardCollection singleton
2. Calls `InitializeStartingCollection()`
3. Randomly selects from 23 starter cards:
   - 8 Attack cards (can have duplicates, e.g. 3x Quick Slash)
   - 4 Defense cards (can have duplicates, e.g. 2x Block)
   - 3 Utility cards (can have duplicates)

**Example Starting Deck**:
```
Attack (8):
- Quick Slash
- Quick Slash
- Power Strike
- Quick Slash
- Power Strike
- Slash
- Slash
- Cleave

Defense (4):
- Block
- Block
- Brace
- Shield Bash

Utility (3):
- Battle Focus
- Disarm
- Battle Focus
```

Total: 15 cards (with duplicates)

## Battle Rewards Flow

### After Battle Victory:
1. Last enemy dies → EnemyHealth calls `EnemyManager.CheckBattleEndAfterDelay()`
2. After 0.6s → EnemyManager checks if all enemies dead
3. If all dead → BattleState.SetOver(true)
4. BattleRewardManager detects victory
5. After 2s delay → CardRewardUI shows 2 random cards
6. Player clicks one → Card added to collection
7. Reward panel closes

### Card Reward Selection:
- 2 random cards from the 23 starter pool
- No duplicates in the choice (both cards are different)
- Player clicks one to add to collection
- Can skip if they don't want either card

## Testing

### Test Starting Collection
1. Play from TitleScene
2. Check Console logs:
   ```
   [CardCollection] Loaded 23 starter cards from pool
   [CardCollection] Starting collection initialized with 15 cards
   [CardCollection] Collection: 8 Attack, 4 Defense, 3 Utility, 0 Tactical
   ```
3. Start a battle
4. Check Console:
   ```
   [DeckManager] Using player's collection: 15 cards
   ```

### Test Hand Composition
1. Start player turn
2. Check hand has:
   - At least 1 Attack card
   - At least 1 Defense card
   - 0-1 Utility card
   - Total 4 cards
3. End turn and draw again - composition should be maintained

### Test Battle Rewards
1. Win a battle (defeat all enemies)
2. After 2 seconds, reward UI should appear
3. See 2 random cards
4. Click one
5. Check Console:
   ```
   [CardRewardUI] Player selected: [CardName]
   [CardCollection] Added [CardName] to collection. Total: 16
   ```
6. Next battle should use 16 cards

### Test Multi-Enemy Battle End
1. Start Battle 2 (multiple enemies)
2. Kill first enemy → Battle should NOT end
3. Kill second enemy → Battle should NOT end
4. Kill all enemies → Battle should end, show rewards

## Merge Cards & Categories

All 9 merge-only cards follow their component categories:

**Attack Merges** (count as Attack for hand rules):
- Whirlwind (Quick Slash + Quick Slash)
- Devastating Blow (Power Strike + Power Strike)
- Executioner's Axe (Cleave + Power Strike)

**Defense Merges** (count as Defense for hand rules):
- Iron Fortress (Block + Block)
- Immovable Object (Block + Brace)

**Utility Merges** (count as Utility for hand rules):
- Perfect Parry (Parry + Parry)
- Smoke Bomb (Dodge + Dodge)
- Mirror Shield (Reflect + Reflect)
- Bloodlust (Battle Focus + Power Strike)

## Future Enhancements

### Card Removal
Add ability to remove cards from collection:
```csharp
CardCollection.Instance.RemoveCard(cardToRemove);
```

### Rarity-Based Rewards
Modify `GetRandomRewardOptions()` to filter by rarity:
```csharp
var rareCards = starterPool.Where(c => c.rarity == CardRarity.Rare).ToList();
```

### Save/Load System
Add persistence between game sessions:
```csharp
public void SaveCollection()
{
    // Serialize ownedCards to JSON
    // Save to PlayerPrefs or file
}

public void LoadCollection()
{
    // Deserialize from JSON
    // Restore ownedCards list
}
```

### Chest Rewards
Add chests in exploration that give card rewards:
```csharp
// In chest interaction script:
if (CardRewardUI.Instance != null)
{
    CardRewardUI.Instance.ShowRewardSelection();
}
```

## Bug Fixes Included

✅ **Fixed**: Invisibility popup now shows at PLAYER position (not enemy)
✅ **Fixed**: Invisibility now HIDES player model completely for 1 turn
✅ **Fixed**: Whirlwind (AllEnemies) now hits ALL enemies, not just one
✅ **Fixed**: Battle 2 now ends when ALL enemies die, not just one
✅ **Fixed**: Cards per turn already set to 4 (in TurnManager inspector)

## Summary

**What Players Experience**:
1. Game start: Get 15 random cards (8 Attack, 4 Defense, 3 Utility)
2. Every hand: Guaranteed 1 Attack, 1 Defense, max 1 Utility
3. Win battles: Choose 1 of 2 random cards to add
4. Collection grows throughout playthrough
5. Duplicates allowed - can have 5x Quick Slash if unlucky/lucky

**What Developers Do**:
1. Add GameInitializer to TitleScene
2. Add BattleRewardManager + CardRewardUI to Battle scenes
3. Assign Player Model to PlayerStatusEffects
4. Done! System works automatically.
