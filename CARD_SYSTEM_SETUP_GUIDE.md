# Card System Setup & Implementation Guide

## Overview
Your turn-based card battle system is well-structured! I've reviewed all the key scripts and made critical fixes to ensure the "click card → click target" flow works correctly.

---

## ✅ What Was Fixed

### **Critical Bug in TargetingSystem.cs**
**Problem:** The system was auto-targeting without requiring you to click on the actual target. It just grabbed the first enemy/player in the scene when you clicked anywhere.

**Solution:** Implemented proper raycast-based targeting:
- Now performs a `Physics2D.Raycast` to detect what you clicked on
- Only plays the card if you click on a valid target (enemy for attacks, player for self-cards)
- Invalid clicks are ignored, allowing you to try again
- Added clear debug messages to show what's happening

### **Additional Improvements:**
1. Added support for more effect types:
   - `GainStamina` - Energize cards now work
   - `DrawCards` - Draw card effects now work
   - Placeholders for `PreventAttack`, `RemoveDebuffs`, `ReflectDamage` (need status effect system)

2. Better debug logging throughout
3. Fixed Unity API deprecation warnings (FindObjectOfType → FindFirstObjectByType)

---

## 🎯 Current Card Flow (Working)

1. **Cards spawn in hand** via `HandManager.AddCardToHand()`
2. **Hover a card** → `CardMovement.OnPointerEnter()` → card scales up
3. **Click the card** → `CardMovement.OnPointerDown()` → card locks in targeting mode:
   - Moves up (y + 150)
   - Scales to 1.15x
   - Brought to front (SetAsLastSibling)
   - Calls `TargetingSystem.BeginTargeting()`
4. **Click on valid target:**
   - Attack cards (SingleEnemy) → Must click on enemy's collider
   - Self cards (Self) → Must click on player's collider
5. **If valid & enough stamina:**
   - Effects apply via `ResolveCard()`
   - Stamina deducted
   - Card removed from hand
   - Card sent to discard pile
   - Targeting ends, card visuals reset

6. **If invalid click:** Nothing happens, you can try again or press ESC/right-click to cancel

---

## 🔧 Setup Requirements

### **For Targeting to Work, You MUST Have:**

1. **Colliders on your game objects:**
   ```
   Enemy GameObject:
   ├─ EnemyHealth component
   └─ Collider2D (BoxCollider2D, CircleCollider2D, etc.)
   
   Player GameObject:
   ├─ PlayerHealth component
   └─ Collider2D
   ```

2. **Camera assigned:**
   - Assign your main camera to `TargetingSystem.worldCamera` in the Inspector
   - Or ensure your camera is tagged as "MainCamera"

3. **Input Actions configured:**
   - `uiClickAction` → bound to UI/Click (usually left mouse button)
   - `uiCancelAction` → bound to UI/Cancel (ESC key)
   - `uiRightClickAction` → bound to right mouse button (optional)

4. **Physics2D Raycaster:**
   - Your camera should have a `Physics2DRaycaster` component
   - OR your Canvas should be in World Space with a raycaster

---

## 📋 Card Data Structure

### **Card ScriptableObject** (`CardSystem.cs`)
```csharp
- cardName: string
- artwork: Sprite
- description: string
- category: Attack/Defense/Utility/Tactical
- staminaCost: int
- targetType: Self/SingleEnemy/AllEnemies/SingleAlly/AllAllies/None
- effects: List<CardEffectData>
- rarity: Common/Uncommon/Rare/Epic
```

### **Effect Types Implemented:**
- ✅ **Damage** - Direct damage to enemy/self
- ✅ **ApplyBleed** - Bleed damage over time on enemy
- ✅ **ApplyWeak** - Reduce enemy damage output
- ✅ **Heal** - Restore player HP
- ✅ **ApplyBlock** - Give player shield/armor
- ✅ **GainStamina** - Restore stamina (Energize)
- ✅ **DrawCards** - Draw additional cards
- ⚠️ **PreventAttack** - Needs status effect system
- ⚠️ **RemoveDebuffs** - Needs debuff tracking
- ⚠️ **ReflectDamage** - Needs status effect system

---

## 🎮 Scene Setup Checklist

### **Required GameObjects in Scene:**

1. **BattleSystem**
   - TurnManager component
   - DeckManager component
   - HandManager component
   - TargetingSystem component
   - PlayerStamina component

2. **Canvas_BattleHUD**
   - Hand Transform (where cards appear)
   - End Turn Button

3. **Player**
   - PlayerHealth component
   - PlayerHealthHUD component
   - Collider2D ⚠️ **REQUIRED FOR TARGETING**

4. **Enemy**
   - EnemyHealth component
   - EnemyStatusDisplay component
   - Collider2D ⚠️ **REQUIRED FOR TARGETING**

5. **EventSystem**
   - Input System UI Input Module

---

## 🐛 Common Issues & Solutions

### **"Card plays when I click anywhere"**
❌ **Old behavior** - This was the bug I fixed!
✅ **Now:** Card only plays if you click on the correct target

### **"Nothing happens when I click the target"**
**Check:**
1. Does the target have a Collider2D?
2. Is the collider enabled?
3. Is the TargetingSystem's camera assigned?
4. Check the Console for debug messages - they tell you exactly what's happening

### **"Card doesn't show in hand"**
**Check:**
1. Is `cardPrefab` assigned in HandManager?
2. Does the prefab have CardInstance + CardDisplay + CardMovement?
3. Is `handTransform` assigned?

### **"Card doesn't lock when clicked"**
**Check:**
1. Does the card prefab have an EventTrigger or the CardMovement script?
2. Is there a GraphicRaycaster on the Canvas?
3. Is the EventSystem active in the scene?

### **"Can't cancel targeting"**
**Check:**
1. Are the uiCancelAction and uiRightClickAction assigned in TargetingSystem?
2. Press ESC or right-click to cancel

---

## 🎨 Card Prefab Structure

Your card prefab should look like this:
```
CardPrefab (GameObject)
├─ RectTransform
├─ CanvasGroup (optional, for fading)
├─ Image (card background)
├─ CardInstance (script - holds Card data)
├─ CardDisplay (script - shows card info)
├─ CardMovement (script - handles hover/click)
└─ Child UI elements:
    ├─ NameText (TextMeshPro)
    ├─ StaminaText (TextMeshPro)
    ├─ DescriptionText (TextMeshPro)
    └─ CardImage (Image for artwork)
```

---

## 🔄 Turn Flow

1. **Start of Player Turn:**
   - Stamina refilled to max
   - 3 cards drawn from deck
   - End Turn button enabled

2. **During Player Turn:**
   - Play cards by clicking them, then clicking targets
   - Each card costs stamina
   - Cards removed from hand after playing

3. **End Turn Button Pressed:**
   - Remaining cards in hand discarded
   - Transition to Enemy Turn

4. **Enemy Turn:**
   - Enemy attacks (1-5 damage)
   - Enemy status effects tick (bleed, weaken)
   - Return to Player Turn

---

## 📝 Debug Console Messages

When playing cards, you'll see:
```
[TargetingSystem] Targeting started for card: Slash (cost 1)
[TargetingSystem] Attack card requires clicking on an enemy. Click cancelled.
[TargetingSystem] Successfully playing Slash on enemy!
[TargetingSystem] Slash dealt 2 damage to enemy.
[PlayerStamina] Stamina spent: 1. Now: 9/10
[HandManager] HandleCardResolved called...
```

These messages help you understand exactly what's happening!

---

## 🚀 Testing Your Setup

1. **Test Attack Card:**
   - Click "Slash" card
   - Click on the enemy
   - Should see damage applied and card removed

2. **Test Self Card:**
   - Click "Heal" or "Block" card
   - Click on the player
   - Should see effect applied

3. **Test Invalid Click:**
   - Click attack card
   - Click on empty space
   - Should see "requires clicking on enemy" message
   - Card stays in targeting mode

4. **Test Cancel:**
   - Click any card
   - Press ESC or right-click
   - Card returns to hand position

---

## ⚙️ Configuration

### **Turn Manager Settings:**
- `cardsPerTurn` = 3 (how many cards to draw each turn)
- `refillStaminaEachTurn` = true

### **Deck Manager Settings:**
- `autoDeckSize` = 20 cards
- Pattern: 2 Attack → 1 Defense → 1 Utility/Tactical

### **Player Settings:**
- Max Health: 20
- Max Stamina: 10

---

## 🎯 Next Steps (Optional Enhancements)

1. **Visual Feedback:**
   - Add targeting arrows/lines
   - Highlight valid targets when card is locked
   - Add card play animations

2. **More Target Types:**
   - AllEnemies support (multi-target)
   - AllAllies support
   - Random targeting

3. **Status Effect System:**
   - Track buffs/debuffs with duration
   - Implement PreventAttack properly
   - Add visual indicators for active effects

4. **Card Upgrades:**
   - Use the CardLevelData system
   - Track card usage and level up

5. **Better Cancel Feedback:**
   - Visual highlight when hovering valid targets
   - Cross icon on invalid targets

---

## 📞 Questions?

Check the Console for debug messages - they're very detailed and will tell you exactly what's happening at each step. If something doesn't work, the logs will guide you to the issue!

**Key Files Modified:**
- `Assets/Scripts/Battle/TargetingSystem.cs` - Fixed targeting logic + added more effects

**All other files are working correctly and don't need changes!**
