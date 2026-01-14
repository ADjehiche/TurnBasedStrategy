# Card System - Unified Starting Pool

## ✅ What Changed

### Old System (Before)
- **21 Starter Cards**: Eligible for starting deck
- **2 Reward Cards**: Found through chests/exploration only
- **9 Merge-Only Cards**: Created by merging

### New System (Current) ✅
- **23 Starting Pool Cards**: ALL available from game start (includes Battle Focus & Disarm)
- **9 Merge-Only Cards**: Created by merging

---

## 🎮 How It Works Now

### Player's Permanent Collection
1. **At Game Start**: Player selects/receives 15 random cards from the 23-card pool
2. **Permanent Deck**: These 15 cards stay with the player for the entire game
3. **Battle to Battle**: Same cards used in every battle
4. **Collection Building**: Player can find/collect more cards during exploration
5. **Merge System**: Combine two specific cards to create powerful merge-only cards

### Card Flags (All 23 Starting Cards)
```csharp
canAppearAsReward: false              // Not in chests (already in starting pool)
canAppearInStartingDecks: true        // ✅ Eligible for initial 15 cards
isStarterCard: true                   // ✅ Part of permanent collection
unlockedByDefault: true               // ✅ Available from game start
```

---

## 📊 Complete Card Breakdown

### STARTING POOL (23 cards total)

#### Attack Cards (11)
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

#### Defense Cards (6)
12. Block - 4 block, 0 cost
13. Parry - 3 block + Reflect 2, 1 cost
14. Dodge - 2 block + Dodge attack, 1 cost
15. Shield Block - 10 block, 2 cost
16. Invisibility - Untargetable, lose 2 HP, 2 cost
17. Brace - 6 block + 1 stamina next turn, 1 cost

#### Utility Cards (6)
18. Quick Draw - Draw 2, 1 cost | Common
19. Energize - +2 stamina, exhaust, 0 cost | Common (max 2)
20. Heal - Restore 5 HP, 2 cost | Common
21. Cleanse - Remove debuffs + 2 block, 1 cost | Common
22. **Battle Focus** - +2 damage (1 turn), 1 cost | **Uncommon** (max 2)
23. **Disarm** - Enemies can't attack, exhaust, 2 cost | **Rare** (max 1)

---

### MERGE-ONLY POOL (9 cards)
**Cannot be found - must be created by merging**

24. Hemorrhage (Quick Slash + Stab) - 2 dmg + Bleed 4, 2 cost
25. Vanguard Strike (Low Sweep + Quick Slash) - 3 dmg + 4 block, 2 cost
26. Whirlwind (Quick Slash + Quick Slash) - 2 dmg to all enemies, 2 cost
27. Skewer (Crossbow Bolt + Poison Arrow) - 6 dmg + Bleed 3 + Weak 15%, 3 cost
28. Weighted Tip (Improvised Bolt + Brawler's Jab) - 3 dmg + Bleed 2 + 1 stamina, 2 cost
29. Deep Cuts (Stab + Rend) - 1 dmg + Bleed 4, 2 cost
30. Counter Sweep (Low Sweep + Parry) - 4 block + 2 dmg + Weak 15%, 2 cost
31. Evasive Maneuver (Dodge + Quick Draw) - Untargetable + Draw 2, 2 cost
32. Execution (Battle Focus + Lunging Thrust) - 8 dmg, exhaust, 3 cost

---

## 🎯 Starting Deck Logic

### DeckManager Implementation
```csharp
// Load all cards from Resources
var allCards = Resources.LoadAll<Card>("Cards");

// Filter for starting pool (23 cards)
var startingPool = allCards.Where(c => c.canAppearInStartingDecks == true).ToList();
// This will include: 11 Attack + 6 Defense + 6 Utility = 23 cards

// Shuffle and select 15 random unique cards
var playerStartingDeck = startingPool
    .OrderBy(x => Random.value)
    .Take(15)
    .ToList();

// These 15 cards are the player's permanent collection
// Used in every battle for the rest of the game
```

---

## 🔑 Key Design Principles

### 1. Persistent Deck
- Player's cards carry over between battles
- No "reset" after each battle
- Deck grows/evolves through gameplay

### 2. Starting Diversity
- 15 random cards from 23-card pool
- Every playthrough has different starting combinations
- No two games are exactly alike

### 3. Rarity Distribution
| Rarity | Count | Notes |
|--------|-------|-------|
| Common | 21 | All attacks, defenses, and 4 utility cards |
| Uncommon | 1 | Battle Focus only |
| Rare | 1 | Disarm only |

### 4. Collection Expansion
- **Chests/Exploration**: Find additional cards from starting pool
- **Merging**: Combine two cards to create powerful merge-only cards
- **Shops** (if implemented): Purchase specific cards
- **Boss Rewards** (if implemented): Rare card drops

---

## 📁 Folder Structure

```
Assets/Resources/Cards/
├── Attack/      (11 cards - all in starting pool)
├── Defense/     (6 cards - all in starting pool)
├── Utility/     (6 cards - all in starting pool, includes Battle Focus & Disarm)
└── MergeOnly/   (9 cards - NOT in starting pool)
```

---

## ⚙️ How to Generate

1. Open Unity Editor
2. Menu: **Tools > Card Game > Generate All Cards**
3. Console output: "✅ All 32 cards generated! (23 starting pool + 9 merge-only)"

---

## 🧪 Testing Checklist

### Starting Pool Verification (23 cards)
- [ ] All 23 cards have `canAppearInStartingDecks = true`
- [ ] All 23 cards have `isStarterCard = true`
- [ ] All 23 cards have `canAppearAsReward = false`
- [ ] Battle Focus is Uncommon, max 2 copies
- [ ] Disarm is Rare, max 1 copy
- [ ] All other starting cards are Common, max 4 copies (except Energize max 2)

### Merge-Only Verification (9 cards)
- [ ] All 9 cards have `canAppearInStartingDecks = false`
- [ ] All 9 cards have `canAppearAsReward = false`
- [ ] All 9 cards have `unlockedByDefault = false`

### Gameplay Verification
- [ ] Player receives 15 random cards at game start
- [ ] All 15 cards are unique (no duplicates)
- [ ] Same 15 cards appear in every battle
- [ ] Deck persists across battles
- [ ] Player can collect more cards during exploration
- [ ] Merging system creates merge-only cards

---

## 💡 Future Expansion Ideas

### Card Acquisition During Gameplay
1. **Treasure Chests**: Find random cards from starting pool
2. **Elite Enemies**: Drop rare cards (Battle Focus, Disarm)
3. **Merchant Shops**: Purchase specific cards
4. **Boss Rewards**: Guaranteed rare card + merge recipe
5. **Secret Areas**: Hidden powerful cards

### Collection Management
- **Deck Size**: Start with 15, expand to 20-30 cards
- **Max Copies**: Respect `maxCopiesInDeck` limits
- **Card Removal**: Let player remove unwanted cards
- **Favorites**: Mark cards to prioritize in hand

### Merge System
- **Merge Station**: Special location to combine cards
- **Recipe Discovery**: Learn merge recipes by finding both cards
- **Merge Cost**: Require resources (gold, materials)
- **Merge Preview**: Show result before committing

---

## 🎮 Example Gameplay Flow

### Game Start
```
Player starts new game
→ System loads 23 starting pool cards
→ Shuffles and selects 15 random unique cards
→ Player sees their starting deck
→ "You have received: Quick Slash, Stab, Block, Parry, Heal, ..."
```

### First Battle
```
Player enters dungeon → triggers battle
→ DeckManager draws from player's 15 cards
→ Player fights using their deck
→ Battle ends → player keeps same deck
```

### Exploration
```
Player finds treasure chest
→ Discovers "Battle Focus" card
→ Added to collection (now 16 cards)
→ Next battle uses all 16 cards
```

### Merging
```
Player has: Quick Slash + Stab
→ Visits merge station
→ Merges both cards
→ Receives: Hemorrhage (2 dmg + Bleed 4)
→ Original cards consumed
→ Deck now has Hemorrhage instead
```

---

## 📚 Summary

**The player's deck is permanent and grows throughout the game.**

- Start: 15 random cards from 23-card pool
- Battle: Use your permanent deck
- Explore: Find more cards, expand collection
- Merge: Combine cards for powerful upgrades
- Persist: Same deck across all battles

This creates a **roguelike deckbuilding experience** where every run is unique based on the initial 15 cards selected! 🎲
