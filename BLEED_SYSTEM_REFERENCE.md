# Bleed System - Quick Reference

## New Countdown System ⚡

### How It Works
```
Bleed is a countdown timer that deals INCREASING damage as it expires.

Each turn:
1. Take damage = current Bleed value (N)
2. Reduce Bleed by 1 (N → N-1)
3. Repeat until Bleed = 0
```

---

## Damage Tables

### Single Application
| Bleed Applied | Turn 1 | Turn 2 | Turn 3 | Turn 4 | Turn 5 | Total Damage |
|---------------|--------|--------|--------|--------|--------|--------------|
| Bleed 1       | 1      | -      | -      | -      | -      | **1**        |
| Bleed 2       | 2      | 1      | -      | -      | -      | **3**        |
| Bleed 3       | 3      | 2      | 1      | -      | -      | **6**        |
| Bleed 4       | 4      | 3      | 2      | 1      | -      | **10**       |
| Bleed 5       | 5      | 4      | 3      | 2      | 1      | **15**       |

**Formula**: Total Damage = N × (N + 1) / 2

---

### Stacking Examples

#### Example 1: Double Application
```
Turn 0: Apply Bleed 2 → Enemy has Bleed 2
Turn 1: Enemy takes 2 damage → Bleed becomes 1
Turn 2: Apply Bleed 3 → Enemy has Bleed 1 + 3 = Bleed 4
Turn 3: Enemy takes 4 damage → Bleed becomes 3
Turn 4: Enemy takes 3 damage → Bleed becomes 2
Turn 5: Enemy takes 2 damage → Bleed becomes 1
Turn 6: Enemy takes 1 damage → Bleed becomes 0 (removed)

Total from first application: 2 + 1 = 3
Total from second application: 4 + 3 + 2 + 1 = 10
Grand Total: 13 damage
```

#### Example 2: Rapid Stacking
```
Turn 1: Apply Bleed 1 → Bleed = 1
Turn 2: Apply Bleed 2 → Bleed = 1 + 2 = 3
Turn 3: Apply Bleed 1 → Bleed = 3 + 1 = 4
Turn 4: Enemy takes 4 damage → Bleed = 3
Turn 5: Enemy takes 3 damage → Bleed = 2
Turn 6: Enemy takes 2 damage → Bleed = 1
Turn 7: Enemy takes 1 damage → Bleed = 0

Total: 4 + 3 + 2 + 1 = 10 damage
```

---

## Visual Indicators

### In-Game Display
```
Enemy Health Bar:
┌─────────────────────────┐
│ Goblin        HP: 15/20 │
│ 🩸 3                    │  ← Bleed 3 = Will take 3 damage this turn
│ 💔 -25%                 │  ← Weakness (separate status)
└─────────────────────────┘

After enemy turn:
┌─────────────────────────┐
│ Goblin        HP: 12/20 │  ← Took 3 damage
│ 🩸 2                    │  ← Bleed reduced to 2
│ 💔 -25%                 │
└─────────────────────────┘
```

---

## Strategic Implications

### Why This System Is Better

**Old System** (Flat 1 per stack):
- Bleed 3 = 1 + 1 + 1 = 3 total damage
- Not very impactful

**New System** (Countdown):
- Bleed 3 = 3 + 2 + 1 = 6 total damage
- **2x more damage!**
- Rewards stacking bleed early

### Optimal Strategy

#### Early Game (High HP enemies):
- Stack bleed as high as possible
- Bleed 5 deals 15 total damage over time
- Then focus on other tactics

#### Late Game (Low HP enemies):
- Small bleeds for finishing blows
- Bleed 1 or 2 is enough to finish weakened enemies

#### Multi-Enemy Battles:
- Apply Bleed 2-3 to all enemies
- Let DoT whittle them down while focusing on one target

---

## Card Design Implications

### Balanced Bleed Values

| Card Power | Bleed Amount | Total Damage | Stamina Cost Suggestion |
|------------|--------------|--------------|-------------------------|
| Weak       | Bleed 1      | 1            | 1-2 stamina             |
| Common     | Bleed 2      | 3            | 2-3 stamina             |
| Strong     | Bleed 3      | 6            | 3-4 stamina             |
| Rare       | Bleed 4      | 10           | 4-5 stamina             |
| Epic       | Bleed 5      | 15           | 5-6 stamina             |

### Example Cards

**Knife Slash** (Common):
- Cost: 2 stamina
- Effect: Apply Bleed 2
- Total potential: 3 damage over 2 turns

**Deep Cut** (Uncommon):
- Cost: 3 stamina
- Effect: Deal 2 damage + Apply Bleed 2
- Total potential: 2 + 3 = 5 damage

**Arterial Strike** (Rare):
- Cost: 5 stamina
- Effect: Deal 3 damage + Apply Bleed 4
- Total potential: 3 + 10 = 13 damage (very powerful!)

**Hemorrhage** (Epic):
- Cost: 6 stamina
- Effect: Apply Bleed 5 to ALL enemies
- Total potential: 15 damage per enemy over 5 turns

---

## Code Reference

### Applying Bleed
```csharp
// In TargetingSystem or card effect:
enemyHealth.AddBleed(3); // Adds 3 to current bleed counter
```

### Ticking Bleed (Automatic)
```csharp
// In EnemyHealth.TickStatuses() - called each turn:
if (bleedStacks > 0)
{
    int bleedDamage = bleedStacks;     // Current value
    TakeDamage(bleedDamage);           // Deal damage
    bleedStacks--;                     // Decrease by 1
}
```

---

## Console Debug Logs

```
[EnemyHealth] Goblin takes 3 bleed damage (Bleed 3)
[EnemyHealth] Goblin takes 2 bleed damage (Bleed 2)
[EnemyHealth] Goblin takes 1 bleed damage (Bleed 1)
[EnemyHealth] Goblin bleed expired
```

---

## Comparison: Old vs New

| Scenario           | Old System | New System | Difference |
|--------------------|------------|------------|------------|
| Apply Bleed 3      | 3 damage   | 6 damage   | +3 (2x)    |
| Apply Bleed 5      | 5 damage   | 15 damage  | +10 (3x)   |
| Stack Bleed 2 + 2  | 4 damage   | 10 damage  | +6 (2.5x)  |
| Stack Bleed 1 × 3  | 3 damage   | 6 damage   | +3 (2x)    |

**Average Improvement**: **~2.5x more damage** from bleed effects! 🔥

---

## Tips for Players

1. **Stack Early**: Apply bleed at start of battle for maximum value
2. **Multiple Small > One Big**: Bleed 2 + Bleed 2 = Bleed 4 (10 damage) is better than waiting
3. **Combo with Block**: Apply bleed, then defend - let DoT do the work
4. **Multi-Target**: Great for battles with multiple enemies
5. **Finish Weak Enemies**: Bleed 1 is enough to finish low HP targets

---

## Balance Notes

If bleed feels too strong:
- Reduce bleed values on cards (Bleed 3 → Bleed 2)
- Increase stamina costs
- Make bleed cards rarer

If bleed feels too weak:
- Increase bleed values (Bleed 2 → Bleed 3)
- Add cards that synergize with bleed
- Add "Double Bleed" mechanics

Current system is **2.5x stronger** than old flat-damage system, so may need card rebalancing! ⚖️
