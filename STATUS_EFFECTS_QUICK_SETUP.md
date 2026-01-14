# Quick Setup - Status Effects System

## ✅ What Was Fixed

All non-working card effects are now fully implemented:
- ✅ **Reflect** - Reflects damage back to attacker (Parry)
- ✅ **Dodge** - Completely avoids next attack (Dodge)
- ✅ **Invisibility** - Player untargetable + semi-transparent (Invisibility)
- ✅ **Disarm** - Enemies cannot attack (Disarm)
- ✅ **Stamina Gain** - Immediate (Energize) and next-turn (Brace, Weighted Tip)

---

## 🚀 Setup in Unity (5 Minutes)

### Step 1: Add PlayerStatusEffects Component
1. Open **Battle_Template** scene (or your battle scene)
2. Right-click Hierarchy → Create Empty
3. Name it: `PlayerStatusManager`
4. Add Component → `PlayerStatusE
ffects`

### Step 2: Assign Player Model
1. Select `PlayerStatusManager` in Hierarchy
2. In Inspector, find **PlayerStatusEffects** component
3. Look for **Player Model** field
4. Drag your **player's visual GameObject** into this field
   - This is usually the character mesh/body
   - Example: `Player/PlayerModel` or `Player/Body`
   - **This enables transparency effect for Invisibility**

### Step 3: Test a Card
1. Play the scene
2. Draw **Parry** card (gives block + reflect)
3. Play it
4. End turn
5. Enemy attacks
6. ✅ You should see damage popup at **enemy's position** (reflected damage)

---

## 🧪 Testing All Effects

### Test 1: Parry (Reflect) ⚔️
```
1. Play Parry card
2. End turn
3. Enemy attacks
✅ Damage shows at enemy's position
✅ Enemy takes 2 damage back
```

### Test 2: Dodge 💨
```
1. Play Dodge card
2. End turn
3. Enemy attacks
✅ Shows "DODGE!" or 0 damage
✅ Player takes NO damage
```

### Test 3: Invisibility 👻
```
1. Play Invisibility card
✅ Player becomes semi-transparent (30% opacity)
2. End turn
✅ Console: "Cannot attack - Player is INVISIBLE!"
✅ Enemies skip their turns
3. Next player turn
✅ Player returns to full opacity
```

### Test 4: Disarm 🚫
```
1. Play Disarm card
2. End turn
✅ Console: "Enemies are DISARMED!"
✅ No enemy attacks this turn
```

### Test 5: Brace (Next Turn Stamina) 💪
```
1. Play cards until stamina is low
2. Play Brace (gain 6 block + 1 stamina next turn)
3. End turn
4. New player turn starts
✅ Stamina = 10 + 1 = 11 (or max + 1)
✅ Console: "Gained X stamina from last turn's Brace"
```

### Test 6: Energize ⚡
```
1. Play cards until stamina is low (e.g., 3/10)
2. Play Energize (cost 0, +2 stamina, exhaust)
✅ Stamina instantly = 5/10
✅ Energize card disappears (exhausted)
```

---

## 📋 File Changes Summary

### New Files Created:
1. ✅ `PlayerStatusEffects.cs` - Status effect tracking system

### Modified Files:
1. ✅ `TargetingSystem.cs` - Now applies status effects properly
2. ✅ `PlayerHealth.cs` - Checks dodge/reflect before taking damage
3. ✅ `TurnManager.cs` - Ticks player status effects each turn
4. ✅ `EnemyManager.cs` - Checks invisibility/disarm before attacking

---

## ⚠️ Common Issues & Fixes

### Issue: "PlayerStatusEffects.Instance is null"
**Fix**: Make sure you added the PlayerStatusEffects component to Battle scene

### Issue: Invisibility doesn't make player transparent
**Fix**: 
1. Select PlayerStatusManager in Hierarchy
2. Assign Player Model field in Inspector
3. Must be the actual visual mesh/renderer

### Issue: Reflect doesn't reflect damage
**Fix**: Check console for debug logs. Should see:
```
[PlayerStatus] Applied Reflect: 2 damage for 1 turn(s)
[PlayerStatus] Reflected X damage back to Enemy!
```

### Issue: Disarm doesn't work
**Fix**: Make sure EnemyManager is updated with disarm check

### Issue: Cards disappear but effects don't work
**Fix**: Check console for error messages. All status effects should log when applied.

---

## 🎯 What to Look For

### Console Logs (Success)
When playing cards, you should see:
```
[TargetingSystem] Parry applied Reflect 2 damage for 1 turn(s).
[PlayerStatus] Applied Reflect: 2 damage for 1 turn(s)

[TargetingSystem] Dodge applied Dodge for 1 turn(s).
[PlayerStatus] Applied Dodge for 1 turn(s)

[TargetingSystem] Invisibility applied Invisibility for 1 turn(s).
[PlayerStatus] Applied Invisibility for 1 turn(s)

[TargetingSystem] Disarm disarmed all enemies for 1 turn(s).
[PlayerStatus] Enemies disarmed for 1 turn(s)

[TargetingSystem] Brace will grant 1 stamina next turn.
[PlayerStatus] Will gain 1 stamina next turn (total: 1)
```

### Visual Feedback
- **Invisibility**: Player becomes 30% transparent
- **Reflect**: Damage popup appears at enemy (not player)
- **Dodge**: 0 or "DODGE!" popup
- **Disarm**: No enemy attacks occur

---

## 📚 Full Documentation

See `STATUS_EFFECTS_IMPLEMENTATION.md` for:
- Complete architecture details
- Turn flow diagrams
- Troubleshooting guide
- All method signatures

---

## 🎉 You're Done!

All card effects now work! Just:
1. Add PlayerStatusEffects to Battle scene
2. Assign Player Model field
3. Test the cards!

**Enjoy your fully functional card system!** 🎮✨
