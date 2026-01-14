# What Changed in Card Generator

## 🔄 Major Changes

### Old System
- Menu: "Generate Starter Cards"
- Created ~13 cards total
- All cards were starter cards
- Used folder: Attack, Defense, Utility, Tactical
- All cards had `canAppearAsReward = true`
- Simple starter generation

### New System ✅
- Menu: "Generate All Cards"
- Creates 32 cards total (21 starter + 2 reward + 9 merge-only)
- Three distinct card pools with proper flags
- Folders: Attack, Defense, Utility, MergeOnly
- Proper reward/starter/merge-only flag setup
- Advanced card generation with merge recipes

---

## 📊 Card Count Comparison

| Category | Old System | New System |
|----------|------------|------------|
| Attack | 9 cards | 11 starter + 9 merge-only = 20 total |
| Defense | 5 cards | 6 starter + 3 merge-only = 9 total |
| Utility | 4 cards | 4 starter + 2 reward = 6 total |
| Tactical | 3 cards | ❌ Removed (merged into Utility) |
| **TOTAL** | **21 cards** | **32 cards** |

---

## 🗑️ Removed/Renamed Cards

### Removed Cards
- ❌ **Slash** → Renamed to **Quick Slash**
- ❌ **Punch** → Renamed to **Brawler's Jab**
- ❌ **Kick** → Removed (not in spec)
- ❌ **Throw Arrow** → Renamed to **Improvised Bolt**
- ❌ **Draw Card** → Renamed to **Quick Draw**
- ❌ **Remove Debuff** → Renamed to **Cleanse**
- ❌ **Lunging Attack** → Renamed to **Lunging Thrust**
- ❌ **Slash and Stab** → Removed (not in spec)
- ❌ **Inspire** → Removed (Tactical category eliminated)
- ❌ **Scout** → Removed (Tactical category eliminated)

### Renamed Cards
| Old Name | New Name |
|----------|----------|
| Slash | Quick Slash |
| Punch | Brawler's Jab |
| Throw Arrow | Improvised Bolt |
| Lunging Attack | Lunging Thrust |
| Draw Card | Quick Draw |
| Remove Debuff | Cleanse |

---

## ✨ New Cards Added

### New Starter Cards (21 total)
1. ✅ Quick Slash (renamed from Slash)
2. ✅ Stab (updated effects)
3. ✅ Brawler's Jab (renamed from Punch)
4. ✨ **Open-Hand Slap** (NEW - 0 cost, weak debuff)
5. ✨ **Low Sweep** (NEW - 2 dmg + weak)
6. ✅ Improvised Bolt (renamed from Throw Arrow)
7. ✅ Crossbow Bolt (updated: 4 dmg, no bleed)
8. ✅ Lunging Thrust (renamed)
9. ✅ Poison Arrow (updated effects)
10. ✨ **Rend** (NEW - 1 dmg + Bleed 3)
11. ✨ **Aimed Shot** (NEW - 2 dmg ranged)
12. ✅ Block (updated: 4 block instead of 3)
13. ✅ Parry (updated: Reflect 2 instead of 1)
14. ✅ Dodge (updated description)
15. ✅ Shield Block (same)
16. ✅ Invisibility (same)
17. ✨ **Brace** (NEW - 6 block + stamina)
18. ✅ Quick Draw (renamed, draws 2 instead of 1)
19. ✅ Energize (updated: 2 stamina + exhaust, max 2 copies)
20. ✅ Heal (same)
21. ✅ Cleanse (renamed, adds 2 block)

### New Reward Cards (2 total)
22. ✨ **Battle Focus** (NEW - damage buff)
23. ✅ **Disarm** (moved from Tactical, now exhausts)

### New Merge-Only Cards (9 total)
24. ✨ **Hemorrhage** (Quick Slash + Stab)
25. ✨ **Vanguard Strike** (Low Sweep + Quick Slash)
26. ✨ **Whirlwind** (Quick Slash + Quick Slash)
27. ✨ **Skewer** (Crossbow Bolt + Poison Arrow)
28. ✨ **Weighted Tip** (Improvised Bolt + Brawler's Jab)
29. ✨ **Deep Cuts** (Stab + Rend)
30. ✨ **Counter Sweep** (Low Sweep + Parry)
31. ✨ **Evasive Maneuver** (Dodge + Quick Draw)
32. ✨ **Execution** (Battle Focus + Lunging Thrust)

---

## 🔧 Code Changes

### New Helper Methods

#### Old System
```csharp
FinishStarterCommon(Card c, params string[] tags)
{
    canAppearAsReward = true;
    canAppearInStartingDecks = true;
    // All cards could appear anywhere
}
```

#### New System
```csharp
// Three different finish methods:

FinishStarterCommon(Card c, params string[] tags)
{
    canAppearAsReward = false;           // ❌ Not in chests
    canAppearInStartingDecks = true;     // ✅ In starting 15
    isStarterCard = true;
}

FinishRewardCard(Card c, rarity, maxCopies, exhaust, tags)
{
    canAppearAsReward = true;            // ✅ In chests
    canAppearInStartingDecks = false;    // ❌ Not in starting 15
    isStarterCard = false;
}

FinishMergeOnly(Card c, rarity, maxCopies, exhaust, tags)
{
    canAppearAsReward = false;           // ❌ Not in chests
    canAppearInStartingDecks = false;    // ❌ Not in starting 15
    unlockedByDefault = false;           // 🔒 Must be unlocked by merge
}
```

### Menu Item Changed
```csharp
// Old
[MenuItem("Tools/Card Game/Generate Starter Cards")]

// New
[MenuItem("Tools/Card Game/Generate All Cards")]
```

---

## 🎯 Key Flag Differences

| Card Type | canAppearAsReward | canAppearInStartingDecks | unlockedByDefault |
|-----------|-------------------|--------------------------|-------------------|
| **Starter (old system)** | ✅ true | ✅ true | ✅ true |
| **Starter (new system)** | ❌ false | ✅ true | ✅ true |
| **Reward Pool** | ✅ true | ❌ false | ✅ true |
| **Merge-Only** | ❌ false | ❌ false | ❌ false |

---

## 📁 Folder Structure Changes

### Old Structure
```
Assets/Resources/Cards/
├── Attack/      (9 cards)
├── Defense/     (5 cards)
├── Utility/     (4 cards)
└── Tactical/    (3 cards)  ← REMOVED
```

### New Structure
```
Assets/Resources/Cards/
├── Attack/      (11 starter cards)
├── Defense/     (6 starter cards)
├── Utility/     (4 starter + 2 reward = 6 cards)
└── MergeOnly/   (9 merge-only cards)  ← NEW
```

---

## ⚠️ Breaking Changes

1. **Tactical folder removed** - Disarm moved to Utility
2. **Card names changed** - Old saved games may break if referencing by name
3. **Card count increased** - DeckManager must handle 32 cards instead of 21
4. **New flags** - Reward system must check `canAppearAsReward`
5. **Merge system required** - 9 cards can only be obtained via merging

---

## 🔄 Migration Path

If you have existing cards in Unity:

### Option 1: Clean Slate (Recommended)
1. Delete all cards in `Assets/Resources/Cards/`
2. Run new generator: Tools > Card Game > Generate All Cards
3. All 32 cards created fresh

### Option 2: Keep Old Cards
1. Rename `Assets/Resources/Cards/` to `Assets/Resources/Cards_OLD/`
2. Run new generator
3. Compare old vs new manually
4. Delete old folder when satisfied

---

## ✅ What to Test After Generation

1. **Card Count**: Should see 32 card assets total
2. **Folder Structure**: Attack (11), Defense (6), Utility (6), MergeOnly (9)
3. **Starter Flags**: Cards 1-21 should have `canAppearInStartingDecks = true`
4. **Reward Flags**: Battle Focus & Disarm should have `canAppearAsReward = true`
5. **Merge Flags**: All MergeOnly cards should have both flags = false
6. **DeckManager**: Should build starting deck from only 21 starter cards
7. **Reward System**: Should offer Battle Focus/Disarm but not starter cards

---

## 🎮 Ready to Generate!

Open Unity and run:
**Tools > Card Game > Generate All Cards**

All 32 cards will be created according to the new specification! 🎉
