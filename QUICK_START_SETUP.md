# Quick Start Setup Checklist ✅

## Immediate Setup Steps (5-10 minutes)

### 1️⃣ TitleScene Setup
- [ ] Open `TitleScene`
- [ ] Create empty GameObject: **"CardCollectionManager"**
- [ ] Add Component: **GameInitializer**
- [ ] Check: **"Initialize Collection On Start"** ✅
- [ ] Save scene

### 2️⃣ Battle Scene - Create Reward Panel
- [ ] Open `Battle_Template` scene
- [ ] Right-click Canvas → UI → Panel
- [ ] Rename: **"BattleRewardPanel"**
- [ ] Rect Transform: Anchor Stretch-Stretch, all offsets 0
- [ ] Image: Black, Alpha 200
- [ ] **Set Active: OFF** (uncheck in inspector)

### 3️⃣ Add Title Text
- [ ] Right-click BattleRewardPanel → UI → Text - TextMeshPro
- [ ] Rename: **"TitleText"**
- [ ] Text: **"Choose Your Reward"**
- [ ] Font Size: 48, Center aligned
- [ ] Position: Top-Center, Y: -100

### 4️⃣ Create Card Container
- [ ] Right-click BattleRewardPanel → UI → Empty
- [ ] Rename: **"CardOptionsContainer"**
- [ ] Add Component: **Horizontal Layout Group**
  - Spacing: 50
  - Child Alignment: Middle Center
- [ ] Add Component: **Content Size Fitter**
  - Horizontal: Preferred Size
  - Vertical: Preferred Size

### 5️⃣ Create Skip Button
- [ ] Right-click BattleRewardPanel → UI → Button - TextMeshPro
- [ ] Rename: **"SkipButton"**
- [ ] Text: **"Skip Reward"**
- [ ] Position: Bottom-Center, Y: 50

### 6️⃣ Setup Card Prefab (Use Existing)
- [ ] Copy your existing **Card prefab** from battle hand
- [ ] Place copy in **Prefabs** folder
- [ ] Rename: **"CardOptionPrefab"**
- [ ] Make sure it has:
  - CardDisplay component ✅
  - CardInstance component ✅
  - Button component ✅
  - All references assigned ✅

### 7️⃣ Setup CardRewardUI
- [ ] Create empty GameObject: **"CardRewardUI"** (outside Canvas)
- [ ] Add Component: **CardRewardUI**
- [ ] Assign References:
  - **Reward Panel** → BattleRewardPanel
  - **Card Option Prefab** → CardOptionPrefab
  - **Card Options Container** → CardOptionsContainer
  - **Title Text** → TitleText
  - **Skip Button** → SkipButton

### 8️⃣ Setup BattleRewardManager
- [ ] Create empty GameObject: **"BattleRewardManager"** (outside Canvas)
- [ ] Add Component: **BattleRewardManager**
- [ ] Assign Reference:
  - **Card Reward UI** → CardRewardUI GameObject
- [ ] Settings:
  - Show Card Reward After Battle: ✅
  - Delay Before Reward: 2.0

### 9️⃣ Setup Player Invisibility
- [ ] Find **PlayerStatusEffects** component in scene
- [ ] Assign **Player Model** field:
  - Drag your player's visual mesh/model GameObject

### 🔟 Test Everything
- [ ] Play from **TitleScene** (not Battle scene)
- [ ] Check Console for:
  ```
  [CardCollection] Starting collection initialized with 15 cards
  [CardCollection] Collection: 8 Attack, 4 Defense, 3 Utility
  ```
- [ ] Start a battle
- [ ] Check hand has 1+ Attack, 1+ Defense
- [ ] Win battle (kill all enemies)
- [ ] Reward panel should appear after 2 seconds
- [ ] Click a card → Collection grows to 16 cards
- [ ] Next battle should use 16 cards

---

## Common Quick Fixes

### Panel doesn't show after battle?
1. Check BattleRewardPanel is set to **inactive** initially
2. Verify BattleRewardManager exists in Battle scene
3. Make sure you **WIN** the battle (not lose)

### Cards not displaying in reward?
1. Check CardOptionPrefab has CardDisplay component
2. Verify all references in CardDisplay are assigned
3. Make sure CardRewardUI has prefab assigned

### Hand doesn't have guaranteed cards?
1. Make sure CardCollection initialized in TitleScene
2. Check Console for CardCollection logs
3. Verify TurnManager is using DrawHandWithRules

### Player doesn't hide during invisibility?
1. Assign Player Model field in PlayerStatusEffects
2. Should be the visual mesh, not the root GameObject

---

## Files You Created

### Scripts (Already created for you):
✅ `CardCollection.cs`
✅ `CardRewardUI.cs`
✅ `BattleRewardManager.cs`
✅ `GameInitializer.cs`

### Documentation:
📄 `REWARD_PANEL_CONSTRUCTION.md` - Detailed UI guide
📄 `CARD_COLLECTION_SYSTEM.md` - System overview
📄 `IMPLEMENTATION_SUMMARY.md` - Technical summary
📄 `THIS FILE` - Quick checklist

---

## Ready to Play! 🎮

Once checklist is complete:
1. Play from **TitleScene**
2. Win battles
3. Choose reward cards
4. Watch your collection grow
5. Enjoy the card collection system!

---

## Need More Details?

- **UI Construction**: See `REWARD_PANEL_CONSTRUCTION.md`
- **System Behavior**: See `IMPLEMENTATION_SUMMARY.md`
- **Full Documentation**: See `CARD_COLLECTION_SYSTEM.md`
