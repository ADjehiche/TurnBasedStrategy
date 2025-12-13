# Card Play Flow Diagram

## Visual Flow Chart

```
┌─────────────────────────────────────────────────────────────────┐
│                    CARD PLAY FLOW                                │
└─────────────────────────────────────────────────────────────────┘

1. START OF PLAYER TURN
   │
   ├─► PlayerStamina.Refill()
   ├─► DeckManager.Draw(3 cards)
   └─► HandManager.AddCardToHand() × 3
        │
        ▼

2. CARDS IN HAND (Visual State: Normal)
   │
   ├─► Hover over card
   │   └─► CardMovement.OnPointerEnter()
   │       └─► Scale up to 1.1x
   │
   └─► Mouse exits card
       └─► CardMovement.OnPointerExit()
           └─► Scale back to normal
   

3. CLICK ON CARD
   │
   └─► CardMovement.OnPointerDown()
       │
       ├─► currentState = 2 (locked)
       ├─► Visual changes:
       │   ├─► SetAsLastSibling() (bring to front)
       │   ├─► Scale to 1.15x
       │   ├─► Move up +150 pixels
       │   └─► Remove rotation
       │
       └─► TargetingSystem.BeginTargeting(cardData, cardGO, onComplete)
           │
           ├─► Enable input actions (click, cancel)
           └─► Wait for player to click target...


4. TARGETING MODE (Card Locked)
   │
   ├─► Player clicks on screen
   │   │
   │   └─► OnClickPerformed()
   │       │
   │       └─► TryTargetAtScreenPoint(screenPos, camera)
   │           │
   │           ├─► Raycast from camera to world
   │           │   └─► Physics2D.Raycast()
   │           │
   │           ├─► Get clicked object's components
   │           │   ├─► EnemyHealth?
   │           │   └─► PlayerHealth?
   │           │
   │           └─► Validate based on card.targetType
   │               │
   │               ├─► IF TargetType.SingleEnemy:
   │               │   │
   │               │   ├─► Enemy found? → Continue
   │               │   └─► No enemy? → Log "requires clicking on enemy"
   │               │                   → Stay in targeting mode
   │               │
   │               └─► IF TargetType.Self:
   │                   │
   │                   ├─► Player found? → Continue
   │                   └─► No player? → Log "requires clicking on player"
   │                                    → Stay in targeting mode
   │
   ├─► Player presses ESC or Right Click
   │   │
   │   └─► OnCancelPerformed()
   │       │
   │       └─► CancelTargeting()
   │           ├─► Disable input actions
   │           ├─► Clear activeCardData
   │           ├─► Call onComplete callback
   │           └─► Card returns to normal (via callback)
   │
   └─► Valid target clicked! Continue below...


5. VALID TARGET CLICKED
   │
   ├─► Check stamina
   │   │
   │   ├─► Enough stamina?
   │   │   └─► PlayerStamina.Spend(cost)
   │   │       └─► Continue
   │   │
   │   └─► Not enough?
   │       └─► CancelTargeting()
   │           └─► Card returns to hand
   │
   ├─► Apply card effects
   │   │
   │   └─► ResolveCard(card, enemy, player)
   │       │
   │       └─► For each effect in card.effects:
   │           │
   │           ├─► EffectType.Damage
   │           │   └─► enemy.TakeDamage() OR player.TakeDamage()
   │           │
   │           ├─► EffectType.ApplyBleed
   │           │   └─► enemy.AddBleed()
   │           │
   │           ├─► EffectType.ApplyWeak
   │           │   └─► enemy.AddPoison()
   │           │
   │           ├─► EffectType.Heal
   │           │   └─► player.Heal()
   │           │
   │           ├─► EffectType.ApplyBlock
   │           │   └─► player.GainBlock()
   │           │
   │           ├─► EffectType.GainStamina
   │           │   └─► PlayerStamina.currentStamina += amount
   │           │
   │           └─► EffectType.DrawCards
   │               └─► DeckManager.Draw()
   │                   └─► HandManager.AddCardToHand()
   │
   ├─► Play audio (optional)
   │   └─► AudioManager.Play("PlayerAttack")
   │
   └─► Remove card from hand
       │
       └─► BattleEvents.RaiseCardResolved(cardGO)
           │
           └─► HandManager.HandleCardResolved()
               │
               ├─► Extract card data
               ├─► Remove from cardsInHand list
               ├─► Destroy the card GameObject
               ├─► DeckManager.Discard(cardData)
               └─► UpdateHandVisuals()


6. CLEANUP
   │
   ├─► Disable input actions
   ├─► Clear activeCardData
   ├─► Clear activeCardGO
   └─► Card is gone from hand!


═══════════════════════════════════════════════════════════════════

## State Transitions

CardMovement.currentState values:
┌─────────────────────────────────────────────────────────────────┐
│ 0 = IDLE     → Card at rest position, normal scale              │
│ 1 = HOVER    → Mouse over card, scaled to 1.1x                  │
│ 2 = LOCKED   → Card clicked, moved up, in targeting mode        │
└─────────────────────────────────────────────────────────────────┘

State flow:
  0 (IDLE) ──[Mouse Enter]──► 1 (HOVER) ──[Mouse Exit]──► 0 (IDLE)
                                   │
                           [Mouse Click]
                                   │
                                   ▼
                              2 (LOCKED)
                                   │
                       [Play Card or Cancel]
                                   │
                                   ▼
                              0 (IDLE)


═══════════════════════════════════════════════════════════════════

## Raycast Detection Flow

User clicks on screen at position (x, y)
          │
          ▼
Camera.ScreenPointToRay(x, y)
          │
          ▼
Physics2D.Raycast(ray.origin, ray.direction)
          │
          ├─► HIT! ──► hit.collider.GetComponent<EnemyHealth>()
          │             │
          │             ├─► Found? ──► enemy = component
          │             └─► Not found? ──► Try GetComponentInParent<EnemyHealth>()
          │                                  │
          │                                  └─► enemy = parent component or null
          │
          ├─► HIT! ──► hit.collider.GetComponent<PlayerHealth>()
          │             │
          │             └─► Similar logic...
          │
          └─► MISS ──► hit.collider = null
                       └─► enemy = null, player = null


═══════════════════════════════════════════════════════════════════

## What Happens When...

┌─────────────────────────────────────────────────────────────────┐
│ Scenario: Click attack card, then click on enemy               │
├─────────────────────────────────────────────────────────────────┤
│ 1. Card locks (visual change)                                   │
│ 2. TargetingSystem activates                                    │
│ 3. Click on enemy sprite                                        │
│ 4. Raycast hits enemy collider                                  │
│ 5. GetComponent<EnemyHealth>() returns enemy                    │
│ 6. Validate: TargetType.SingleEnemy + enemy != null ✓          │
│ 7. Check stamina ✓                                              │
│ 8. Apply damage to enemy                                        │
│ 9. Spend stamina                                                │
│ 10. Remove card from hand                                       │
│ 11. Card discarded                                              │
│ Result: SUCCESS ✓                                               │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ Scenario: Click attack card, then click on empty space         │
├─────────────────────────────────────────────────────────────────┤
│ 1. Card locks (visual change)                                   │
│ 2. TargetingSystem activates                                    │
│ 3. Click on empty space                                         │
│ 4. Raycast hits nothing (or background without components)     │
│ 5. GetComponent<EnemyHealth>() returns null                     │
│ 6. Validate: TargetType.SingleEnemy + enemy == null ✗          │
│ 7. Log "Attack card requires clicking on an enemy"             │
│ 8. Return (don't cancel targeting)                             │
│ 9. Card stays locked, waiting for valid click                  │
│ Result: RETRY (card still in targeting mode)                   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ Scenario: Click heal card, then click on player                │
├─────────────────────────────────────────────────────────────────┤
│ 1. Card locks                                                   │
│ 2. TargetingSystem activates                                    │
│ 3. Click on player sprite                                       │
│ 4. Raycast hits player collider                                │
│ 5. GetComponent<PlayerHealth>() returns player                  │
│ 6. Validate: TargetType.Self + player != null ✓                │
│ 7. Check stamina ✓                                              │
│ 8. Apply heal to player                                         │
│ 9. Spend stamina                                                │
│ 10. Remove card from hand                                       │
│ Result: SUCCESS ✓                                               │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ Scenario: Click card but not enough stamina                    │
├─────────────────────────────────────────────────────────────────┤
│ 1. Card locks                                                   │
│ 2. TargetingSystem activates                                    │
│ 3. Click on valid target                                        │
│ 4. Raycast finds correct component                             │
│ 5. Validation passes                                            │
│ 6. Check stamina: currentStamina < card.staminaCost ✗          │
│ 7. Log "Not enough stamina to play [CardName]"                 │
│ 8. CancelTargeting()                                            │
│ 9. Card returns to hand (via onComplete callback)              │
│ Result: CANCELLED (card back in hand)                          │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ Scenario: Click card then press ESC                            │
├─────────────────────────────────────────────────────────────────┤
│ 1. Card locks                                                   │
│ 2. TargetingSystem activates                                    │
│ 3. Press ESC (or right-click)                                   │
│ 4. OnCancelPerformed() triggered                                │
│ 5. CancelTargeting()                                            │
│ 6. Disable input actions                                        │
│ 7. Call onComplete callback                                     │
│ 8. CardMovement.ResetVisual() restores card position           │
│ Result: CANCELLED (card back in hand)                          │
└─────────────────────────────────────────────────────────────────┘


═══════════════════════════════════════════════════════════════════

## Component Dependencies

┌─────────────────────────────────────────────────────────────────┐
│                    REQUIRED COMPONENTS                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Card Prefab:                                                    │
│  ├─ CardInstance     (holds Card ScriptableObject data)        │
│  ├─ CardDisplay      (shows card visuals)                      │
│  └─ CardMovement     (handles hover/click/lock)                │
│                                                                  │
│  Enemy GameObject:                                               │
│  ├─ EnemyHealth      (health + status effects)                 │
│  ├─ Collider2D       ⚠️ REQUIRED FOR RAYCAST                   │
│  └─ EnemyStatusDisplay (optional UI)                           │
│                                                                  │
│  Player GameObject:                                              │
│  ├─ PlayerHealth     (health + block system)                   │
│  ├─ Collider2D       ⚠️ REQUIRED FOR RAYCAST                   │
│  └─ PlayerHealthHUD  (optional UI)                             │
│                                                                  │
│  BattleSystem:                                                   │
│  ├─ TurnManager      (turn flow)                               │
│  ├─ DeckManager      (draw/discard piles)                      │
│  ├─ HandManager      (card spawning/removal)                   │
│  ├─ TargetingSystem  (click-to-play logic) ← FIXED!           │
│  └─ PlayerStamina    (resource management)                     │
│                                                                  │
│  Scene:                                                          │
│  ├─ EventSystem      (with Input System UI Input Module)       │
│  ├─ Canvas           (with GraphicRaycaster)                   │
│  └─ Main Camera      (assigned to TargetingSystem)             │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

This detailed flow shows exactly how your system works now!
