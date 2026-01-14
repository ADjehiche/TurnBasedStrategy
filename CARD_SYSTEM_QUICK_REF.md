# Quick Reference - Card System Changes

## ✅ What Changed

### Before
- 21 starter cards + 2 reward cards (separate pools)
- `canAppearAsReward = true` for Battle Focus & Disarm
- `canAppearInStartingDecks = false` for Battle Focus & Disarm

### After ✅
- **23 cards in unified starting pool** (all cards available from game start)
- `canAppearAsReward = false` for ALL starting cards
- `canAppearInStartingDecks = true` for ALL 23 cards (including Battle Focus & Disarm)

---

## 🎮 Player Experience

### Old System
```
Game Start: Get 15 cards from pool of 21
During Game: Find Battle Focus & Disarm in chests
Result: Two-tier card acquisition
```

### New System ✅
```
Game Start: Get 15 cards from pool of 23
During Game: Expand collection, merge cards
Result: All cards available from start, permanent deck
```

---

## 📊 Card Distribution

| Pool | Count | Rarity | Available When |
|------|-------|--------|----------------|
| **Starting Pool** | 23 | Common (21), Uncommon (1), Rare (1) | ✅ Game start |
| **Merge-Only** | 9 | Uncommon (7), Rare (2) | ❌ Must merge |

---

## 🔑 Key Card Flags

### All 23 Starting Cards
```csharp
canAppearAsReward: false
canAppearInStartingDecks: true
isStarterCard: true
```

### All 9 Merge-Only Cards
```csharp
canAppearAsReward: false
canAppearInStartingDecks: false
unlockedByDefault: false
```

---

## 🎯 Starting Deck Logic

```csharp
// 1. Load all cards
var allCards = Resources.LoadAll<Card>("Cards");

// 2. Filter for starting pool (gets 23 cards)
var startingPool = allCards.Where(c => c.canAppearInStartingDecks).ToList();

// 3. Select 15 random unique cards
var playerDeck = startingPool.OrderBy(x => Random.value).Take(15).ToList();

// 4. These 15 cards are permanent (persist across battles)
```

---

## 📋 Complete Card List

### Starting Pool (23 cards)

**Attack (11)**
1. Quick Slash
2. Stab
3. Brawler's Jab
4. Open-Hand Slap
5. Low Sweep
6. Improvised Bolt
7. Crossbow Bolt
8. Lunging Thrust
9. Poison Arrow
10. Rend
11. Aimed Shot

**Defense (6)**
12. Block
13. Parry
14. Dodge
15. Shield Block
16. Invisibility
17. Brace

**Utility (6)**
18. Quick Draw
19. Energize
20. Heal
21. Cleanse
22. **Battle Focus** ⭐ (Uncommon)
23. **Disarm** ⭐ (Rare)

**Merge-Only (9)**
24. Hemorrhage
25. Vanguard Strike
26. Whirlwind
27. Skewer
28. Weighted Tip
29. Deep Cuts
30. Counter Sweep
31. Evasive Maneuver
32. Execution

---

## ⚡ Quick Facts

- **Total Cards**: 32 (23 starting + 9 merge-only)
- **Starting Deck Size**: 15 cards
- **Deck Persistence**: Same deck used in every battle
- **Rarity Chances**: 91% Common, 4% Uncommon, 4% Rare
- **Free Cards (0 cost)**: Brawler's Jab, Open-Hand Slap, Block, Energize
- **Exhaust Cards**: Energize, Disarm, Execution

---

## 🔄 How to Generate

1. Unity Menu: **Tools > Card Game > Generate All Cards**
2. Output: "✅ All 32 cards generated! (23 starting pool + 9 merge-only)"
3. Done!

---

## 📚 Full Documentation

See `UNIFIED_CARD_SYSTEM.md` for complete details!
