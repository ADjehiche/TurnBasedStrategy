# Card Generation System - Complete Card List

## Overview
- **Total Cards**: 32
- **Starter Pool** (eligible for 15 random start): 21 cards (1-21)
- **Reward Pool** (exploration/chests only): 2 cards (22-23)
- **Merge-Only** (created by merging): 9 cards (24-32)

## How to Generate
1. Open Unity Editor
2. Go to menu: **Tools > Card Game > Generate All Cards**
3. All 32 cards will be created/updated in `Assets/Resources/Cards/`

## Folder Structure
```
Assets/Resources/Cards/
├── Attack/         (Attack cards 1-11)
├── Defense/        (Defense cards 12-17)
├── Utility/        (Utility cards 18-23)
└── MergeOnly/      (Merge-only cards 24-32)
```

---

## STARTER CARD POOL (21 cards)
*These cards are eligible for the 15 random starting deck*

### ATTACK — Starter (1-11)

**1) Quick Slash** ⚔️
- Cost: 1 | Target: SingleEnemy
- Effects: Deal 2 damage
- Common | Max 4 copies
- Tags: attack, melee

**2) Stab** 🗡️
- Cost: 1 | Target: SingleEnemy
- Effects: Deal 1 damage, Apply Bleed 2 (2 turns)
- Common | Max 4 copies
- Tags: attack, melee, bleed

**3) Brawler's Jab** 👊
- Cost: 0 | Target: SingleEnemy
- Effects: Deal 1 damage
- Common | Max 4 copies
- Tags: attack, melee, free

**4) Open-Hand Slap** 🖐️
- Cost: 0 | Target: SingleEnemy
- Effects: Deal 1 damage, Apply Weak 10% (1 turn)
- Common | Max 4 copies
- Tags: attack, melee, free, debuff, weak

**5) Low Sweep** 🦵
- Cost: 1 | Target: SingleEnemy
- Effects: Deal 2 damage, Apply Weak 15% (1 turn)
- Common | Max 4 copies
- Tags: attack, melee, debuff, weak

**6) Improvised Bolt** 🏹
- Cost: 1 | Target: SingleEnemy
- Effects: Deal 2 damage, Apply Bleed 1 (1 turn)
- Common | Max 4 copies
- Tags: attack, ranged, bleed

**7) Crossbow Bolt** 🎯
- Cost: 2 | Target: SingleEnemy
- Effects: Deal 4 damage
- Common | Max 4 copies
- Tags: attack, ranged

**8) Lunging Thrust** ⚔️
- Cost: 2 | Target: SingleEnemy
- Effects: Deal 5 damage
- Common | Max 4 copies
- Tags: attack, melee, heavy

**9) Poison Arrow** ☠️
- Cost: 2 | Target: SingleEnemy
- Effects: Deal 2 damage, Apply Bleed 3 (3 turns), Apply Weak 10% (2 turns)
- Common | Max 4 copies
- Tags: attack, ranged, bleed, debuff, weak

**10) Rend** 🩸
- Cost: 1 | Target: SingleEnemy
- Effects: Deal 1 damage, Apply Bleed 3 (3 turns)
- Common | Max 4 copies
- Tags: attack, melee, bleed

**11) Aimed Shot** 🎯
- Cost: 1 | Target: SingleEnemy
- Effects: Deal 2 damage
- Common | Max 4 copies
- Tags: attack, ranged

### DEFENSE — Starter (12-17)

**12) Block** 🛡️
- Cost: 0 | Target: Self
- Effects: Gain 4 Block
- Common | Max 4 copies
- Tags: defense, block, free

**13) Parry** ⚔️
- Cost: 1 | Target: Self
- Effects: Gain 3 Block, Reflect 2 damage (1 turn)
- Common | Max 4 copies
- Tags: defense, block, reflect

**14) Dodge** 💨
- Cost: 1 | Target: Self
- Effects: Gain 2 Block, Dodge next attack (1 turn)
- Common | Max 4 copies
- Tags: defense, block, evasion

**15) Shield Block** 🛡️
- Cost: 2 | Target: Self
- Effects: Gain 10 Block
- Common | Max 4 copies
- Tags: defense, block

**16) Invisibility** 👻
- Cost: 2 | Target: Self
- Effects: Become untargetable (1 turn), Lose 2 HP
- Common | Max 4 copies
- Tags: defense, evasion, self-damage

**17) Brace** 💪
- Cost: 1 | Target: Self
- Effects: Gain 6 Block, Gain 1 stamina next turn
- Common | Max 4 copies
- Tags: defense, block, tempo

### UTILITY — Starter (18-21)

**18) Quick Draw** 🎴
- Cost: 1 | Target: Self
- Effects: Draw 2 cards
- Common | Max 4 copies
- Tags: utility, draw

**19) Energize** ⚡
- Cost: 0 | Target: Self
- Effects: Gain 2 stamina, **Exhaust**
- Common | Max 2 copies
- Tags: utility, stamina, free, exhaust

**20) Heal** ❤️
- Cost: 2 | Target: Self
- Effects: Restore 5 HP
- Common | Max 4 copies
- Tags: utility, heal

**21) Cleanse** ✨
- Cost: 1 | Target: Self
- Effects: Remove all debuffs, Gain 2 Block
- Common | Max 4 copies
- Tags: utility, cleanse, block

---

## REWARD POOL (2 cards)
*Found through chests and exploration, NOT in starting decks*

**22) Battle Focus** 💥
- Cost: 1 | Target: Self
- Effects: Gain +2 damage (1 turn)
- Uncommon | Max 2 copies
- Tags: utility, buff, damage
- **canAppearAsReward: true | canAppearInStartingDecks: false**

**23) Disarm** 🚫
- Cost: 2 | Target: AllEnemies
- Effects: Enemies cannot attack (1 turn), **Exhaust**
- Rare | Max 1 copy
- Tags: utility, control, exhaust
- **canAppearAsReward: true | canAppearInStartingDecks: false**

---

## MERGE-ONLY CARDS (9 cards)
*Created by merging two specific cards, cannot be found normally*

**24) Hemorrhage** 🩸🩸 (Quick Slash + Stab)
- Cost: 2 | Target: SingleEnemy
- Effects: Deal 2 damage, Apply Bleed 4 (4 turns)
- Uncommon | Max 2 copies
- Tags: attack, bleed, finisher
- **MERGE-ONLY: Cannot be found as reward**

**25) Vanguard Strike** ⚔️🛡️ (Low Sweep + Quick Slash)
- Cost: 2 | Target: SingleEnemy
- Effects: Deal 3 damage, Gain 4 Block (self)
- Uncommon | Max 2 copies
- Tags: attack, defense, block
- **MERGE-ONLY: Cannot be found as reward**

**26) Whirlwind** 🌪️ (Quick Slash + Quick Slash)
- Cost: 2 | Target: AllEnemies
- Effects: Deal 2 damage to all enemies
- Uncommon | Max 2 copies
- Tags: attack, aoe
- **MERGE-ONLY: Cannot be found as reward**

**27) Skewer** 🏹💀 (Crossbow Bolt + Poison Arrow)
- Cost: 3 | Target: SingleEnemy
- Effects: Deal 6 damage, Apply Bleed 3 (3 turns), Apply Weak 15% (2 turns)
- Rare | Max 1 copy
- Tags: attack, ranged, bleed, debuff
- **MERGE-ONLY: Cannot be found as reward**

**28) Weighted Tip** 🏹⚡ (Improvised Bolt + Brawler's Jab)
- Cost: 2 | Target: SingleEnemy
- Effects: Deal 3 damage, Apply Bleed 2 (2 turns), Gain 1 stamina (self)
- Uncommon | Max 2 copies
- Tags: attack, bleed, tempo
- **MERGE-ONLY: Cannot be found as reward**

**29) Deep Cuts** 🗡️🩸 (Stab + Rend)
- Cost: 2 | Target: SingleEnemy
- Effects: Deal 1 damage, Apply Bleed 4 (4 turns)
- Uncommon | Max 2 copies
- Tags: attack, bleed
- **MERGE-ONLY: Cannot be found as reward**

**30) Counter Sweep** 🛡️⚔️ (Low Sweep + Parry)
- Cost: 2 | Target: SingleEnemy
- Effects: Gain 4 Block (self), Deal 2 damage, Apply Weak 15% (1 turn)
- Uncommon | Max 2 copies
- Tags: defense, counter, block, debuff
- **MERGE-ONLY: Cannot be found as reward**

**31) Evasive Maneuver** 💨🎴 (Dodge + Quick Draw)
- Cost: 2 | Target: Self
- Effects: Become untargetable (1 turn), Draw 2 cards
- Uncommon | Max 2 copies
- Tags: defense, evasion, draw
- **MERGE-ONLY: Cannot be found as reward**

**32) Execution** ⚔️💀 (Battle Focus + Lunging Thrust)
- Cost: 3 | Target: SingleEnemy
- Effects: Deal 8 damage, **Exhaust**
- Rare | Max 1 copy
- Tags: attack, finisher, heavy, exhaust
- **MERGE-ONLY: Cannot be found as reward**

---

## Card Flags Reference

### canAppearAsReward
- **true**: Can appear in post-combat rewards, chests, shops
- **false**: Cannot be found normally (starter pool or merge-only)

### canAppearInStartingDecks
- **true**: Eligible for the 15 random starting cards
- **false**: Not in starting pool (reward pool or merge-only)

### isStarterCard
- **true**: Part of the starter pool (cards 1-21)
- **false**: Not in starter pool (cards 22-32)

### Merge-Only Setup
```csharp
canAppearAsReward: false
canAppearInStartingDecks: false
unlockedByDefault: false
```

---

## Starting Deck Logic

**DeckManager should:**
1. Load all cards from `Resources/Cards/` recursively
2. Filter for `canAppearInStartingDecks == true` (cards 1-21)
3. Shuffle and select 15 random cards
4. Ensure uniqueness (no duplicate card types in starting deck)

**Example Code:**
```csharp
var allCards = Resources.LoadAll<Card>("Cards");
var starterPool = allCards.Where(c => c.canAppearInStartingDecks).ToList();
var startingDeck = starterPool.OrderBy(x => Random.value).Take(15).ToList();
```

---

## Testing Checklist

✅ **Starter Pool (21 cards)**
- [ ] All 21 cards have `canAppearInStartingDecks = true`
- [ ] All 21 cards have `canAppearAsReward = false`
- [ ] All 21 cards have `isStarterCard = true`

✅ **Reward Pool (2 cards)**
- [ ] Battle Focus: `canAppearAsReward = true, canAppearInStartingDecks = false`
- [ ] Disarm: `canAppearAsReward = true, canAppearInStartingDecks = false`

✅ **Merge-Only (9 cards)**
- [ ] All 9 cards have `canAppearAsReward = false`
- [ ] All 9 cards have `canAppearInStartingDecks = false`
- [ ] All 9 cards have `unlockedByDefault = false`

✅ **Gameplay**
- [ ] Starting deck contains 15 random cards from starter pool
- [ ] No duplicate card types in starting deck
- [ ] Merge-only cards don't appear in chests/rewards
- [ ] Reward pool cards appear in chests but not in starting decks
