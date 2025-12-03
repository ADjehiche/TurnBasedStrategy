# Quick Setup Checklist ✓

## Immediate Action Items

### 1. Add Colliders to Game Objects ⚠️ **CRITICAL**

**Enemy GameObject:**
```
- Select your enemy in the Hierarchy
- Add Component → Physics 2D → Box Collider 2D (or Circle Collider 2D)
- Make sure "Is Trigger" is UNCHECKED
- Adjust collider size to cover the enemy sprite
```

**Player GameObject:**
```
- Select your player in the Hierarchy
- Add Component → Physics 2D → Box Collider 2D (or Circle Collider 2D)
- Make sure "Is Trigger" is UNCHECKED
- Adjust collider size to cover the player sprite
```

### 2. Assign Camera in TargetingSystem

```
- Select the TargetingSystem GameObject
- In Inspector, find the "World Camera" field
- Drag your Main Camera into this field
```

### 3. Verify Input Actions Are Assigned

```
- Select the TargetingSystem GameObject
- In Inspector, verify these are assigned:
  ✓ UI Click Action → DefaultInputActions/UI/Click
  ✓ UI Cancel Action → DefaultInputActions/UI/Cancel
  ✓ UI Right Click Action → (optional, can leave empty)
```

### 4. Test the Flow

1. Enter Play Mode
2. Click on any card in your hand
   - Card should lock in place (move up, scale larger)
   - Console should say: "Targeting started for card: [CardName]"

3. For Attack cards (Slash, Kick, etc.):
   - Click directly on the enemy sprite
   - Should see: "Successfully playing [CardName] on enemy!"
   - Card should disappear from hand
   - Enemy should take damage

4. For Self cards (Heal, Block, etc.):
   - Click directly on the player sprite
   - Should see: "Successfully playing [CardName] on player!"
   - Card should disappear from hand
   - Effect should apply

5. Test invalid click:
   - Click a card
   - Click on empty space (not on enemy/player)
   - Should see: "requires clicking on [target]"
   - Card should stay in targeting mode

6. Test cancel:
   - Click a card
   - Press ESC or right-click
   - Card should return to normal position
   - Console: "Targeting cancelled"

---

## If It's Not Working

### Card doesn't lock when clicked
❌ **Missing:** EventSystem or Canvas GraphicRaycaster
✅ **Fix:** 
- Check if EventSystem exists in scene (should have Input System UI Input Module)
- Check if your Canvas has a GraphicRaycaster component

### Click on target does nothing
❌ **Missing:** Collider2D on enemy/player
✅ **Fix:** Add Box Collider 2D or Circle Collider 2D to both

### "No camera assigned" error
❌ **Missing:** Camera reference
✅ **Fix:** Assign Main Camera to TargetingSystem.worldCamera field

### Cards don't appear in hand
❌ **Missing:** HandManager setup
✅ **Fix:**
- Assign cardPrefab in HandManager
- Assign handTransform (the UI panel where cards appear)
- Check DeckManager has cards loaded

---

## Expected Console Output (Success)

```
[TurnManager] StartPlayerTurn -> PlayerTurn
[PlayerStamina] Stamina refilled to 10
[TurnManager] Actually drew 3 cards
[HandManager] AddCardToHand called for card: Slash
[HandManager] AddCardToHand called for card: Block
[HandManager] AddCardToHand called for card: Heal

[User clicks Slash card]
[CardMovement] Locked card for targeting: Slash
[TargetingSystem] Targeting started for card: Slash (cost 1)

[User clicks on enemy]
[TargetingSystem] Successfully playing Slash on enemy!
[TargetingSystem] Slash dealt 2 damage to enemy.
[PlayerStamina] Stamina spent: 1. Now: 9/10
Enemy took 2 damage. HP now 18
[HandManager] Card removed from hand
```

---

## Quick Reference: Card Target Types

| Card Name | Category | Target Type | Where to Click |
|-----------|----------|-------------|----------------|
| Slash | Attack | SingleEnemy | Enemy sprite |
| Stab | Attack | SingleEnemy | Enemy sprite |
| Punch | Attack | SingleEnemy | Enemy sprite |
| Block | Defense | Self | Player sprite |
| Heal | Utility | Self | Player sprite |
| Energize | Utility | Self | Player sprite |
| Draw Card | Utility | Self | Player sprite |

---

## Physics2D Settings

If raycasts aren't working, check:

1. **Project Settings → Physics 2D:**
   - Queries Hit Triggers: Your choice (usually OFF)
   - Queries Start In Colliders: Your choice (usually OFF)

2. **Layer Collision Matrix:**
   - Make sure layers can interact (usually Default layer is fine)

3. **Camera:**
   - If using a 2D camera, should be Orthographic
   - Position should allow it to see the battle scene

---

## One More Thing: Card Prefab

Your card prefab MUST have these components:
```
CardPrefab (GameObject)
├─ RectTransform ✓
├─ Image ✓ (for background)
├─ CardInstance ✓ (script)
├─ CardDisplay ✓ (script)
└─ CardMovement ✓ (script) ← This handles the click!
```

If CardMovement is missing, the card won't respond to clicks!

---

## Still Having Issues?

1. Check the Console - every action logs a message
2. Look for red errors first
3. Yellow warnings are usually okay
4. Enable "Collapse" in Console to see unique messages

**Most common issue:** Missing colliders on enemy/player! Add them first!
