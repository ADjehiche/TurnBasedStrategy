# Quick Start - Generate All 32 Cards

## ✅ What Was Done

1. **Rewrote CardGenerator** (`Assets/Editor/CardGenerator.cs`)
   - Created all 32 cards according to your specifications
   - Proper card flags for starter/reward/merge-only pools
   - Organized into 4 folders: Attack, Defense, Utility, MergeOnly

2. **Card Breakdown**
   - **Starter Pool (21 cards)**: 11 Attack + 6 Defense + 4 Utility
   - **Reward Pool (2 cards)**: Battle Focus, Disarm
   - **Merge-Only (9 cards)**: Special combo cards

---

## 🎮 How to Generate Cards in Unity

### Method 1: Unity Menu (Recommended)
1. Open Unity Editor
2. Click menu: **Tools > Card Game > Generate All Cards**
3. Wait for console message: "✅ All 32 cards generated!"
4. Check folders in Project window:
   - `Assets/Resources/Cards/Attack/`
   - `Assets/Resources/Cards/Defense/`
   - `Assets/Resources/Cards/Utility/`
   - `Assets/Resources/Cards/MergeOnly/`

### Method 2: Manual Asset Creation
If you want to inspect first:
1. Open `Assets/Editor/CardGenerator.cs` in Unity
2. Read the code to see exactly what will be created
3. Run via menu when ready

---

## 📋 What Each Card Flag Means

### Starter Pool Cards (1-21)
```csharp
canAppearAsReward: false          // Not in chests (already have them)
canAppearInStartingDecks: true    // Eligible for 15 random start
isStarterCard: true               // Part of starter collection
```

### Reward Pool Cards (22-23)
```csharp
canAppearAsReward: true           // Found in chests/exploration
canAppearInStartingDecks: false   // NOT in starting 15
isStarterCard: false              // Must be found
```

### Merge-Only Cards (24-32)
```csharp
canAppearAsReward: false          // Cannot be found
canAppearInStartingDecks: false   // Cannot start with
unlockedByDefault: false          // Must be created by merging
```

---

## 🔄 Starting Deck System

Your DeckManager should:
1. Load all cards: `Resources.LoadAll<Card>("Cards")`
2. Filter: `cards.Where(c => c.canAppearInStartingDecks == true)`
3. Shuffle: `.OrderBy(x => Random.value)`
4. Take 15: `.Take(15).ToList()`
5. **Important**: No duplicates (all 15 cards must be unique types)

---

## 📝 Cards Created

### Attack Cards (11)
1. Quick Slash - 2 dmg, 1 cost
2. Stab - 1 dmg + Bleed 2, 1 cost
3. Brawler's Jab - 1 dmg, 0 cost
4. Open-Hand Slap - 1 dmg + Weak 10%, 0 cost
5. Low Sweep - 2 dmg + Weak 15%, 1 cost
6. Improvised Bolt - 2 dmg + Bleed 1, 1 cost
7. Crossbow Bolt - 4 dmg, 2 cost
8. Lunging Thrust - 5 dmg, 2 cost
9. Poison Arrow - 2 dmg + Bleed 3 + Weak 10%, 2 cost
10. Rend - 1 dmg + Bleed 3, 1 cost
11. Aimed Shot - 2 dmg, 1 cost

### Defense Cards (6)
12. Block - 4 block, 0 cost
13. Parry - 3 block + Reflect 2, 1 cost
14. Dodge - 2 block + Dodge attack, 1 cost
15. Shield Block - 10 block, 2 cost
16. Invisibility - Untargetable, lose 2 HP, 2 cost
17. Brace - 6 block + 1 stamina next turn, 1 cost

### Utility Cards (4 + 2 reward)
18. Quick Draw - Draw 2, 1 cost
19. Energize - +2 stamina, exhaust, 0 cost (max 2 copies)
20. Heal - Restore 5 HP, 2 cost
21. Cleanse - Remove debuffs + 2 block, 1 cost
22. **Battle Focus** - +2 damage (1 turn), 1 cost [REWARD ONLY]
23. **Disarm** - Enemies can't attack, exhaust, 2 cost [REWARD ONLY]

### Merge-Only Cards (9)
24. Hemorrhage (Quick Slash + Stab)
25. Vanguard Strike (Low Sweep + Quick Slash)
26. Whirlwind (Quick Slash + Quick Slash)
27. Skewer (Crossbow Bolt + Poison Arrow)
28. Weighted Tip (Improvised Bolt + Brawler's Jab)
29. Deep Cuts (Stab + Rend)
30. Counter Sweep (Low Sweep + Parry)
31. Evasive Maneuver (Dodge + Quick Draw)
32. Execution (Battle Focus + Lunging Thrust)

---

## ⚠️ Important Notes

1. **Energize** has `maxCopiesInDeck = 2` (not 4 like others)
2. **Exhaust cards**: Energize, Disarm, Execution
3. **Free cards (0 cost)**: Brawler's Jab, Open-Hand Slap, Block, Energize
4. **All starter cards are Common rarity** with max 4 copies (except Energize)
5. **Merge-only cards are hidden** until player merges the required cards

---

## 🧪 Testing

After generating, test in Unity:
1. Check card count: Should have 32 cards total
2. Inspect starter pool: 21 cards should have green checkmark for `canAppearInStartingDecks`
3. Inspect Battle Focus/Disarm: Should have `canAppearAsReward = true`
4. Inspect MergeOnly folder: All 9 cards should have both flags set to false

---

## 🎯 Next Steps

1. **Generate the cards** (Tools > Card Game > Generate All Cards)
2. **Verify in Project window** (check all folders exist with cards)
3. **Test starting deck** (play game, ensure 15 random starter cards)
4. **Implement merge system** (combine two cards to create merge-only cards)
5. **Add reward system** (chests should offer Battle Focus/Disarm but not starter cards)

---

## 📚 Full Documentation

See `CARD_GENERATION_COMPLETE.md` for complete card details with effects, tags, and merge recipes!
