# Status Effects System - Implementation Guide

## ✅ Fixed Card Effects

All non-working card effects have been implemented! Here's what was fixed:

### 1. **Reflect Damage** (Parry Card)
- **Before**: Debug log only, no actual reflection
- **After**: ✅ Reflects damage back to attacking enemy
- **How it works**: 
  - Parry applies Reflect status for 1 turn
  - When enemy attacks, damage is reflected back before hitting player
  - Reflected damage shows at enemy's position

### 2. **Stamina Gain** (Energize, Brace, Weighted Tip)
- **Before**: Direct stamina gain worked, but GainNextTurnStamina didn't
- **After**: ✅ Both immediate and next-turn stamina gain work
- **How it works**:
  - Energize: Instant +2 stamina
  - Brace: +1 stamina at START of next turn
  - Weighted Tip: Instant +1 stamina

### 3. **Dodge** (Dodge Card)
- **Before**: Debug log only, player still took damage
- **After**: ✅ Completely avoids next incoming attack
- **How it works**:
  - Dodge applies status for 1 turn
  - Next enemy attack is completely negated
  - Shows "DODGE!" message (0 damage popup)

### 4. **Invisibility** (Invisibility Card)
- **Before**: Debug log only, enemies still attacked
- **After**: ✅ Player becomes untargetable + semi-transparent
- **How it works**:
  - Player becomes 30% transparent visually
  - Enemies cannot attack invisible player
  - Each enemy checks invisibility before attacking
  - Player returns to 100% opacity after duration

### 5. **Disarm** (Disarm Card)
- **Before**: Debug log only, enemies still attacked
- **After**: ✅ Prevents ALL enemies from attacking
- **How it works**:
  - Disarm applies to all enemies for 1 turn
  - EnemyManager checks disarm status before executing turns
  - Enemies skip their entire turn when disarmed

### 6. **Parry** (Full Implementation)
- **Before**: Only gave block, reflect didn't work
- **After**: ✅ Gives block + reflects 2 damage
- **How it works**:
  - Gain 3 block immediately
  - Apply Reflect 2 damage for 1 turn
  - Works with dodge/invisibility simultaneously

---

## 🏗️ New System Architecture

### PlayerStatusEffects.cs (NEW)
Singleton that tracks all player buffs/debuffs:

```csharp
// Status tracking
- reflectDamage (amount)
- reflectTurnsRemaining
- hasDodge (bool)
- dodgeTurnsRemaining
- isInvisible (bool)
- invisibleTurnsRemaining
- enemiesDisarmed (bool)
- disarmTurnsRemaining
- staminaNextTurn (int)
```

**Key Methods:**
- `ApplyReflect(damage, turns)` - Set reflect status
- `ApplyDodge(turns)` - Set dodge status
- `ApplyInvisibility(turns)` - Set invisibility + visual effect
- `ApplyDisarm(turns)` - Disarm all enemies
- `AddStaminaNextTurn(amount)` - Queue stamina for next turn
- `TryReflectDamage(incoming, attacker)` - Check + apply reflect
- `TryDodgeAttack()` - Check + consume dodge
- `TickStatuses()` - Decrement all turn counters (called at start of player turn)

---

## 🔄 Integration Points

### 1. TargetingSystem.cs (UPDATED)
Now properly applies status effects instead of placeholder debug logs:

```csharp
case EffectType.ReflectDamage:
    PlayerStatusEffects.Instance.ApplyReflect(amount, turns);

case EffectType.DodgeNextAttack:
    PlayerStatusEffects.Instance.ApplyDodge(turns);

case EffectType.PreventAttack:
    if (targetType == Self) 
        ApplyInvisibility(turns);  // Invisibility card
    else if (targetType == AllEnemies)
        ApplyDisarm(turns);  // Disarm card

case EffectType.GainNextTurnStamina:
    PlayerStatusEffects.Instance.AddStaminaNextTurn(amount);
```

### 2. PlayerHealth.cs (UPDATED)
Checks status effects before taking damage:

```csharp
public void TakeDamage(int amount, EnemyHealth attacker)
{
    // 1. Check Dodge (completely avoids damage)
    if (PlayerStatusEffects.Instance.TryDodgeAttack())
        return; // No damage taken!
    
    // 2. Check Reflect (reflects damage back)
    PlayerStatusEffects.Instance.TryReflectDamage(amount, attacker);
    
    // 3. Normal damage processing (block, then HP)
    // ...
}
```

### 3. TurnManager.cs (UPDATED)
Ticks player status effects at start of player turn:

```csharp
public void StartPlayerTurn()
{
    // 1. Tick player statuses (grant next-turn stamina, decrement durations)
    PlayerStatusEffects.Instance.TickStatuses();
    
    // 2. Tick enemy statuses (bleed, weaken)
    EnemyManager.Instance.TickAllEnemyStatuses();
    
    // 3. Refill stamina
    // 4. Draw cards
}
```

### 4. EnemyManager.cs (UPDATED)
Checks disarm and invisibility before attacking:

```csharp
public IEnumerator ExecuteAllEnemyTurns()
{
    // Check if enemies are disarmed
    if (PlayerStatusEffects.Instance.EnemiesDisarmed)
    {
        Debug.Log("Enemies are DISARMED! Cannot attack.");
        yield break; // Skip all enemy turns
    }
    
    foreach (enemy in livingEnemies)
    {
        // Check if player is invisible
        if (PlayerStatusEffects.Instance.IsInvisible)
        {
            Debug.Log("Cannot attack - Player is INVISIBLE!");
            continue; // Skip this enemy's turn
        }
        
        // Normal attack
        enemy.Attack(player);
    }
}
```

---

## 🎮 Setup in Unity

### 1. Add PlayerStatusEffects Component
1. Open Battle scene
2. Create empty GameObject: "PlayerStatusManager"
3. Add Component: `PlayerStatusEffects`
4. **IMPORTANT**: Assign `Player Model` field in inspector
   - Drag your player's visual GameObject (body/mesh)
   - This enables transparency effect for Invisibility

### 2. Verify Singleton References
Make sure these exist in Battle scene:
- ✅ TurnManager (with singleton)
- ✅ PlayerHealth (with singleton)
- ✅ PlayerStamina (with singleton)
- ✅ PlayerStatusEffects (NEW - with singleton)
- ✅ EnemyManager (with singleton)

### 3. Test Each Effect

**Test Parry (Reflect):**
1. Play Parry card (gain 3 block + Reflect 2)
2. End turn
3. Enemy attacks
4. ✅ Should see damage popup at ENEMY's position (reflected damage)
5. ✅ Player takes reduced/no damage

**Test Dodge:**
1. Play Dodge card
2. End turn
3. Enemy attacks
4. ✅ Should see "DODGE!" or 0 damage popup
5. ✅ Player takes NO damage

**Test Invisibility:**
1. Play Invisibility card (cost 2, lose 2 HP)
2. ✅ Player becomes semi-transparent (30% opacity)
3. End turn
4. ✅ Enemies cannot attack (skip turn)
5. Next player turn starts
6. ✅ Player returns to full opacity

**Test Disarm:**
1. Play Disarm card (cost 2, exhaust)
2. End turn
3. ✅ Console shows "Enemies are DISARMED!"
4. ✅ No enemy attacks occur

**Test Brace (Next Turn Stamina):**
1. Use all stamina (play cards)
2. Play Brace card (gain 6 block + 1 stamina next turn)
3. End turn
4. New player turn starts
5. ✅ Stamina bar shows +1 stamina (in addition to normal refill)

**Test Energize:**
1. Play cards to reduce stamina
2. Play Energize (cost 0, +2 stamina, exhaust)
3. ✅ Stamina immediately increases by 2
4. ✅ Energize card is removed from combat

---

## 🐛 Troubleshooting

### "PlayerStatusEffects.Instance is null"
**Fix**: Add PlayerStatusEffects component to Battle scene

### Invisibility doesn't make player transparent
**Fix**: Assign Player Model field in PlayerStatusEffects inspector

### Reflect doesn't work
**Fix**: Ensure EnemyHealth passes `this` as attacker parameter:
```csharp
PlayerHealth.Instance.TakeDamage(damage, this); // Pass 'this'
```

### Disarm doesn't work
**Fix**: Ensure EnemyManager is using the updated code with disarm check

### Next-turn stamina doesn't work (Brace)
**Fix**: Ensure TurnManager calls `PlayerStatusEffects.Instance.TickStatuses()` at start of player turn

---

## 📊 Turn Flow with Status Effects

```
PLAYER TURN START:
├─ TurnManager.StartPlayerTurn()
│  ├─ PlayerStatusEffects.TickStatuses()
│  │  ├─ Grant next-turn stamina (Brace effect)
│  │  ├─ Decrement reflect turns
│  │  ├─ Decrement dodge turns
│  │  ├─ Decrement invisibility turns (restore opacity if expired)
│  │  └─ Decrement disarm turns
│  ├─ EnemyManager.TickAllEnemyStatuses()
│  ├─ Refill player stamina to max
│  └─ Draw cards for turn

PLAYER PLAYS CARDS:
├─ TargetingSystem.ResolveCard()
│  ├─ Apply Reflect → PlayerStatusEffects.ApplyReflect()
│  ├─ Apply Dodge → PlayerStatusEffects.ApplyDodge()
│  ├─ Apply Invisibility → PlayerStatusEffects.ApplyInvisibility()
│  ├─ Apply Disarm → PlayerStatusEffects.ApplyDisarm()
│  └─ Queue stamina → PlayerStatusEffects.AddStaminaNextTurn()

ENEMY TURN START:
├─ EnemyManager.ExecuteAllEnemyTurns()
│  ├─ Check if PlayerStatusEffects.EnemiesDisarmed
│  │  └─ If TRUE: Skip all enemy turns
│  └─ For each enemy:
│     ├─ Check if PlayerStatusEffects.IsInvisible
│     │  └─ If TRUE: Skip this enemy's turn
│     └─ Enemy attacks player
│        ├─ PlayerHealth.TakeDamage(damage, attacker)
│        │  ├─ Check PlayerStatusEffects.TryDodgeAttack()
│        │  │  └─ If TRUE: Return (no damage)
│        │  ├─ Check PlayerStatusEffects.TryReflectDamage()
│        │  │  └─ Attacker.TakeDamage(reflectAmount)
│        │  ├─ Block absorbs damage
│        │  └─ Remaining damage to HP
```

---

## 🎉 Summary

All card effects now work properly! The status effects system:
- ✅ Tracks turn-based buffs/debuffs
- ✅ Applies effects when cards are played
- ✅ Checks effects when damage is dealt
- ✅ Ticks/decrements durations each turn
- ✅ Visual feedback (transparency for invisibility)
- ✅ Console logs for debugging

**No more "not fully implemented" messages!** 🚀
