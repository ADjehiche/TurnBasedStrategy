# Reward System Architecture

## The Two Components

### Component 1: BattleRewardManager (The Listener)
**Purpose**: Detects when battle ends and triggers reward display
**Location**: Separate GameObject anywhere in scene

```csharp
BattleRewardManager.cs
├── Listens to: BattleState.OnBattleOverChanged
├── Checks: Did player win? (all enemies dead)
├── Waits: 2 seconds
└── Calls: CardRewardUI.ShowRewardSelection()
```

### Component 2: CardRewardUI (The Display)
**Purpose**: Shows the reward panel with clickable cards
**Location**: On the BattleRewardPanel GameObject

```csharp
CardRewardUI.cs
├── Gets: 2 random cards from CardCollection
├── Creates: Card display instances
├── Shows: Reward panel
├── Handles: Card selection clicks
└── Calls: BattleManager.ReturnToLevelOne() after selection
```

## How They Work Together

```
Battle Ends (All Enemies Dead)
        ↓
BattleState.SetOver(true)
        ↓
BattleRewardManager hears event
        ↓
Waits 2 seconds (victory animation time)
        ↓
BattleRewardManager.ShowCardReward()
        ↓
Finds CardRewardUI instance
        ↓
CardRewardUI.ShowRewardSelection()
        ↓
Gets 2 cards from CardCollection
        ↓
Creates card GameObjects in CardOptionsContainer
        ↓
Shows BattleRewardPanel (SetActive(true))
        ↓
Player clicks a card
        ↓
CardRewardUI.OnCardSelected()
        ↓
Adds card to CardCollection.OwnedCards
        ↓
Hides panel
        ↓
BattleManager.ReturnToLevelOne()
        ↓
Scene loads back to LevelOne
```

## Hierarchy Setup

### Recommended Structure (Clean Separation)

```
Battle_Template Scene
│
├── Canvas/BattleFieldRoot/
│   └── BattleRewardPanel ← CardRewardUI script
│       ├── CardOptionsContainer (empty transform for cards)
│       ├── TitleText (TextMeshProUGUI)
│       └── SkipButton (Button)
│
└── BattleSystem/ (or separate GameObject)
    └── BattleRewardManager ← BattleRewardManager script
```

### Inspector References

#### CardRewardUI (on BattleRewardPanel)
```
┌─────────────────────────────────────┐
│ CardRewardUI (Script)               │
├─────────────────────────────────────┤
│ Reward Panel: [BattleRewardPanel]  │ ← Self reference
│ Card Prefab: [CardPrefab]          │ ← Same as HandManager
│ Card Options Container: [Container]│ ← Child object
│ Title Text: [TitleText]            │ ← Child object
│ Skip Button: [SkipButton]          │ ← Child object
└─────────────────────────────────────┘
```

#### BattleRewardManager (on separate GameObject)
```
┌─────────────────────────────────────┐
│ BattleRewardManager (Script)        │
├─────────────────────────────────────┤
│ ☑ Show Card Reward After Battle    │
│ Delay Before Reward: 2              │
│ Card Reward UI: [BattleRewardPanel]│ ← The GameObject with CardRewardUI
└─────────────────────────────────────┘
```

## Common Mistakes

### ❌ Mistake 1: Duplicate Scripts
```
BattleRewardPanel
├── BattleRewardManager ❌ WRONG!
└── CardRewardUI ✅

BattleRewardManager GameObject  
└── BattleRewardManager ❌ DUPLICATE!
```
**Fix**: Remove from BattleRewardPanel, keep only on separate GameObject

### ❌ Mistake 2: Wrong Reference
```
BattleRewardManager script:
Card Reward UI: [CardRewardUI GameObject] ❌ WRONG!
```
**Fix**: Should reference the GameObject with CardRewardUI script (BattleRewardPanel)

### ❌ Mistake 3: Missing CardCollection
```
[CardRewardUI] CardCollection.Instance is null!
```
**Fix**: Must play from TitleScene with GameInitializer, not directly from Battle_Template

### ❌ Mistake 4: Panel Stays Visible
```
BattleRewardPanel is active in editor/scene
```
**Fix**: Panel must start inactive. CardRewardUI.Start() sets it inactive, but verify in Inspector

## Testing Checklist

- [ ] **Only ONE** BattleRewardManager script exists in scene
- [ ] **Only ONE** CardRewardUI script exists in scene
- [ ] BattleRewardManager reference points to correct GameObject
- [ ] CardRewardUI has all 5 fields assigned
- [ ] BattleRewardPanel starts inactive (unchecked in Inspector)
- [ ] CardPrefab is assigned (same one HandManager uses)
- [ ] Play from **TitleScene** (not Battle_Template directly)
- [ ] Console shows "Battle won! Showing rewards..."
- [ ] Console shows "Showing reward selection"
- [ ] Panel appears 2 seconds after killing last enemy
- [ ] 2 cards are visible and clickable
- [ ] Clicking card adds it to collection and returns to level

## Debug Commands

If panel doesn't show, add this temporary code to test:

### Test CardRewardUI Directly
```csharp
// In Update() of any script, press K to force show
if (Input.GetKeyDown(KeyCode.K))
{
    CardRewardUI.Instance?.ShowRewardSelection();
}
```

### Test BattleRewardManager
```csharp
// Add to BattleRewardManager.HandleBattleEnd()
Debug.Log($"[BattleRewardManager] isOver={isOver}, Instance={Instance != null}, cardRewardUI={cardRewardUI != null}");
```

### Check CardCollection
```csharp
// Add to CardRewardUI.ShowRewardSelection()
Debug.Log($"[CardRewardUI] CardCollection exists: {CardCollection.Instance != null}");
if (CardCollection.Instance != null)
{
    Debug.Log($"[CardRewardUI] Owned cards: {CardCollection.Instance.OwnedCards.Count}");
}
```

## The Root Cause

From your error log:
```
[CardRewardUI] CardCollection.Instance is null!
```

This means **either**:
1. You're playing Battle_Template scene directly (not from TitleScene)
2. GameInitializer doesn't exist in TitleScene
3. CardCollection GameObject was destroyed somehow

**Solution**: Always play from TitleScene, which runs GameInitializer.Start(), which creates CardCollection with DontDestroyOnLoad!

---

**TL;DR**: 
- Delete duplicate GameObjects
- Keep ONE BattleRewardManager, ONE CardRewardUI
- Assign all references correctly
- Play from TitleScene (not Battle_Template)
- Profit! 🎉
