# Player Status Display Setup Guide

## Overview
The player now has a visual status display system for tracking bleed and weakness effects, matching the enemy status display system.

## Components

### 1. PlayerStatusDisplay.cs
Located in: `Assets/Scripts/Battle/PlayerStatusDisplay.cs`

**Purpose**: Visual UI component that displays bleed stacks and weakness percentage on player health bar.

**Methods**:
- `SetBleedTurns(int turnsLeft)` - Shows bleed stack count (or hides if 0)
- `SetWeakenPercent(int percent)` - Shows weakness percentage with "-X%" format (or hides if 0)
- `ClearAll()` - Resets both displays to hidden

### 2. PlayerHealth.cs - Status Effect Integration
**New Fields**:
```csharp
[Header("Status Effects")]
public int bleedStacks = 0;
public int weakenPercent = 0;
public int weakenTurns = 0;

[SerializeField] private PlayerStatusDisplay statusDisplay;
```

**New Methods**:
- `AddBleed(int amount)` - Adds bleed stacks, updates UI
- `AddWeaken(int percent, int turns)` - Applies weakness, updates UI
- `TickStatuses()` - Called each turn to process bleed damage and decrement weakness
- `ClearStatusEffects()` - Resets all debuffs

**TakeDamage() Updates**:
- Now applies weakness damage reduction before block calculation
- Formula: `damage -= (damage * weakenPercent / 100)`

### 3. TurnManager.cs Integration
**StartPlayerTurn() now calls**:
1. `PlayerStatusEffects.Instance.TickStatuses()` - Player buffs (Reflect, Dodge, etc.)
2. `PlayerHealth.Instance.TickStatuses()` - **Player debuffs (Bleed, Weakness)**
3. `EnemyManager.Instance.TickAllEnemyStatuses()` - Enemy statuses
4. `playerStamina.Refill()` - Refill stamina
5. `DrawCardsForPlayerTurn()` - Draw cards

## Unity Scene Setup

### Step 1: Create Player Status Display UI
1. Open your Battle scene (`Battle_Template`)
2. Find the Player Health Bar UI (should be under Canvas)
3. Create UI structure similar to enemy status display:

```
PlayerHealthBar (existing)
├── HealthSlider (existing)
├── HealthText (existing)
├── BlockGroup (existing)
└── PlayerStatusDisplay (NEW GameObject)
    ├── BleedDisplay (GameObject)
    │   └── BleedText (TextMeshProUGUI)
    └── WeakenDisplay (GameObject)
        └── WeakenText (TextMeshProUGUI)
```

### Step 2: Configure PlayerStatusDisplay Component
1. Add `PlayerStatusDisplay` component to the `PlayerStatusDisplay` GameObject
2. Assign references in the inspector:
   - **Bleed Root**: Drag `BleedDisplay` GameObject
   - **Bleed Text**: Drag `BleedText` TextMeshProUGUI component
   - **Weaken Root**: Drag `WeakenDisplay` GameObject
   - **Weaken Text**: Drag `WeakenText` TextMeshProUGUI component

### Step 3: Style the Status Icons
**BleedText** settings:
- Font: Bold
- Color: Red/Dark Red (#8B0000)
- Text: "🩸 X" (will be set programmatically)
- Font Size: 18-24

**WeakenText** settings:
- Font: Bold
- Color: Purple/Dark Purple (#4B0082)
- Text: "💔 -X%" (will be set programmatically)
- Font Size: 18-24

### Step 4: Link to PlayerHealth
1. Select the GameObject with the `PlayerHealth` component (usually on the Player prefab or battle controller)
2. Find the "UI" section in the inspector
3. Drag the `PlayerStatusDisplay` GameObject to the **Status Display** field

## How It Works

### Bleed System
**When Applied** (future implementation for enemy attacks):
```csharp
PlayerHealth.Instance.AddBleed(2); // Applies 2 bleed stacks
```

**Each Turn**:
- `PlayerHealth.TickStatuses()` is called at turn start
- Deals damage equal to `bleedStacks` (bypasses block)
- Updates UI: "🩸 2" means 2 bleed damage per turn
- Bleed doesn't expire (stacks indefinitely until game over)

### Weakness System
**When Applied** (future implementation for enemy attacks):
```csharp
PlayerHealth.Instance.AddWeaken(30, 3); // 30% damage reduction for 3 turns
```

**Each Turn**:
- `PlayerHealth.TakeDamage()` reduces incoming damage by `weakenPercent`
- `PlayerHealth.TickStatuses()` decrements `weakenTurns`
- When `weakenTurns` reaches 0, weakness is removed
- UI: "💔 -30%" means player damage is reduced by 30%

**Example**:
- Enemy attacks for 10 damage
- Player has 30% weakness
- Actual damage: 10 - (10 * 0.30) = 7 damage

## Testing

### Test Bleed (Console Command)
In Unity, during battle, select the GameObject with PlayerHealth and in the Console run:
```csharp
PlayerHealth.Instance.AddBleed(3);
```
You should see:
- "🩸 3" appear on player health bar
- Each turn start, 3 damage is dealt
- Debug log: "[PlayerHealth] Bleed deals 3 damage"

### Test Weakness (Console Command)
```csharp
PlayerHealth.Instance.AddWeaken(50, 2);
```
You should see:
- "💔 -50%" appear on player health bar
- Enemy attacks deal 50% less damage for 2 turns
- Debug log: "[PlayerHealth] Weakness reduced damage by X"
- After 2 player turns: "[PlayerHealth] Weakness expired"

## Future Implementation: Enemy Applies Status Effects

To make enemies apply bleed/weakness to the player, update `EnemyManager.ExecuteSingleEnemyTurn()`:

```csharp
// In EnemyManager.cs, after calculating damage:
int damage = enemy.attackDamage;

// Apply weakness reduction if enemy is weakened
if (enemy.poisonPercent > 0)
{
    int reduction = Mathf.RoundToInt(damage * (enemy.poisonPercent / 100f));
    damage -= reduction;
}

// Deal damage
PlayerHealth.Instance.TakeDamage(damage, enemy);

// NEW: Some enemies can apply status effects
if (enemy.HasBleedAttack) // Add this bool to EnemyHealth
{
    PlayerHealth.Instance.AddBleed(1);
}

if (enemy.HasWeakenAttack) // Add this bool to EnemyHealth
{
    PlayerHealth.Instance.AddWeaken(25, 2);
}
```

## Cards That Apply Status to Player

Currently, all cards apply status effects to **enemies only**. There are no cards that apply bleed/weakness to the player (that would be self-harm).

However, if you create "cursed" or "risky" cards in the future that harm the player for powerful effects:

```csharp
// Example: "Blood Pact" card (deal huge damage, but apply bleed to self)
effects.Add(new CardEffect
{
    effectType = EffectType.ApplyBleed,
    amount = 2,
    targetType = TargetType.Self
});
```

Then update `TargetingSystem.ResolveCard()`:
```csharp
case EffectType.ApplyBleed:
{
    if (enemy != null)
    {
        enemy.AddBleed(eff.amount);
    }
    else if (player != null && card.targetType == TargetType.Self) // NEW
    {
        player.AddBleed(eff.amount);
    }
    break;
}
```

## Summary

✅ **PlayerStatusDisplay.cs** - Created (similar to EnemyStatusDisplay)
✅ **PlayerHealth.cs** - Updated with bleed/weakness tracking and methods
✅ **TurnManager.cs** - Now ticks player status effects each turn
✅ **TakeDamage()** - Applies weakness damage reduction
✅ **UI Setup** - Instructions provided above

**Status**: Fully functional! Just needs Unity scene setup to connect the UI elements.

**Visible Effects**: Only bleed and weakness show visually (as requested). Other effects (Reflect, Dodge, Invisibility, Disarm) work functionally but have no UI display.
