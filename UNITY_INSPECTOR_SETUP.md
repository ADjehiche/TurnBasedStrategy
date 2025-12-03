# Unity Inspector Setup Guide

## 🎯 Critical Setup Steps (Do These First!)

This guide shows you exactly what to do in Unity's Inspector to make the targeting system work.

---

## Step 1: Add Collider to Enemy

### In Unity:
```
1. Click on your Enemy GameObject in the Hierarchy
2. Look at the Inspector on the right
3. Click "Add Component" at the bottom
4. Type "Box Collider 2D" (or "Circle Collider 2D")
5. Press Enter to add it
```

### Inspector Settings:
```
┌────────────────────────────────────────────────┐
│ Enemy GameObject                               │
├────────────────────────────────────────────────┤
│ Transform                                      │
│   Position: (whatever)                         │
│   Rotation: (whatever)                         │
│   Scale: (whatever)                            │
├────────────────────────────────────────────────┤
│ Sprite Renderer                                │
│   Sprite: (your enemy sprite)                  │
├────────────────────────────────────────────────┤
│ Enemy Health (Script)                          │
│   Max HP: 20                                   │
│   Current HP: 20                               │
├────────────────────────────────────────────────┤
│ Box Collider 2D          ← ADD THIS!          │
│   ☐ Is Trigger          ← LEAVE UNCHECKED     │
│   Material: None                               │
│   Offset: (0, 0)                               │
│   Size: (adjust to fit sprite)                 │
│                                                 │
│   [Edit Collider] button ← Click this to      │
│                            adjust visually     │
└────────────────────────────────────────────────┘
```

### Visual Check:
- In Scene view, you should see a **green outline** around your enemy
- This is the collider boundary
- Make sure it covers the sprite completely

---

## Step 2: Add Collider to Player

### In Unity:
```
1. Click on your Player GameObject in the Hierarchy
2. Look at the Inspector on the right
3. Click "Add Component" at the bottom
4. Type "Box Collider 2D" (or "Circle Collider 2D")
5. Press Enter to add it
```

### Inspector Settings:
```
┌────────────────────────────────────────────────┐
│ Player GameObject                              │
├────────────────────────────────────────────────┤
│ Transform                                      │
│   Position: (whatever)                         │
│   Rotation: (whatever)                         │
│   Scale: (whatever)                            │
├────────────────────────────────────────────────┤
│ Sprite Renderer                                │
│   Sprite: (your player sprite)                 │
├────────────────────────────────────────────────┤
│ Player Health (Script)                         │
│   Max Health: 20                               │
│   Current Health: 20                           │
├────────────────────────────────────────────────┤
│ Box Collider 2D          ← ADD THIS!          │
│   ☐ Is Trigger          ← LEAVE UNCHECKED     │
│   Material: None                               │
│   Offset: (0, 0)                               │
│   Size: (adjust to fit sprite)                 │
│                                                 │
│   [Edit Collider] button ← Click to adjust    │
└────────────────────────────────────────────────┘
```

---

## Step 3: Assign Camera to TargetingSystem

### In Unity:
```
1. Find "TargetingSystem" GameObject in Hierarchy
   (Usually under BattleSystem or as its own object)
2. Click on it
3. Look at the Inspector
4. Find the "Targeting System (Script)" component
5. Look for the "World Camera" field
6. Drag your Main Camera from Hierarchy into this field
```

### Inspector Settings:
```
┌────────────────────────────────────────────────┐
│ TargetingSystem GameObject                     │
├────────────────────────────────────────────────┤
│ Transform                                      │
│   (doesn't matter)                             │
├────────────────────────────────────────────────┤
│ Targeting System (Script)                      │
│                                                 │
│ Audio Settings                                 │
│   ☐ Play Player Attack Sound                  │
│   Player Attack Sound Name: PlayerAttack       │
│                                                 │
│ Input (DefaultInputActions)                    │
│   UI Click Action:                             │
│     DefaultInputActions/UI/Click   ✓           │
│   UI Cancel Action:                            │
│     DefaultInputActions/UI/Cancel  ✓           │
│   UI Right Click Action:                       │
│     (None) - Optional                          │
│                                                 │
│ Camera                                         │
│   World Camera:                                │
│     [Drag Main Camera here] ← DO THIS!        │
│     OR it will say "Main Camera" if already    │
│     assigned                                   │
│                                                 │
└────────────────────────────────────────────────┘
```

**How to assign:**
- Find "Main Camera" in your Hierarchy
- Click and drag it into the "World Camera" field
- You should see "Main Camera" appear in the field

---

## Step 4: Verify Input Actions (Should Already Be Set Up)

### Check TargetingSystem:
```
┌────────────────────────────────────────────────┐
│ Targeting System (Script)                      │
│                                                 │
│ Input (DefaultInputActions)                    │
│   UI Click Action:                             │
│     ✓ Should show: DefaultInputActions/UI/Click│
│                                                 │
│   UI Cancel Action:                            │
│     ✓ Should show: DefaultInputActions/UI/...  │
│                                                 │
│   If these say "None", click the circle button │
│   and select the action from the list          │
└────────────────────────────────────────────────┘
```

---

## Step 5: Verify HandManager Setup

### Check HandManager:
```
┌────────────────────────────────────────────────┐
│ HandManager GameObject (usually under          │
│                        BattleSystem)            │
├────────────────────────────────────────────────┤
│ Hand Manager (Script)                          │
│                                                 │
│   Deck Manager:                                │
│     [Should reference DeckManager object] ✓    │
│                                                 │
│   Card Prefab:                                 │
│     [Your card prefab from Assets] ✓           │
│     (drag from Project → Prefabs folder)       │
│                                                 │
│   Hand Transform:                              │
│     [Panel in Canvas where cards appear] ✓     │
│     (drag from Canvas → Hand Panel)            │
│                                                 │
│   Fan Spread: 5                                │
│   Card Spacing: 5                              │
│   Vertical Spacing: 100                        │
│                                                 │
│   Cards In Hand: (runtime list)                │
└────────────────────────────────────────────────┘
```

---

## Step 6: Verify Card Prefab

### Your card prefab should have these components:

```
┌────────────────────────────────────────────────┐
│ CardPrefab (in Project → Assets → Prefabs)    │
├────────────────────────────────────────────────┤
│ Components:                                    │
│   ☑ Rect Transform                            │
│   ☑ Canvas Renderer                           │
│   ☑ Image (card background)                   │
│   ☑ Card Instance (Script)         ← CHECK   │
│   ☑ Card Display (Script)          ← CHECK   │
│   ☑ Card Movement (Script)         ← CHECK   │
│                                                 │
│ Child objects:                                 │
│   ☑ NameText (TextMeshPro)                    │
│   ☑ StaminaText (TextMeshPro)                 │
│   ☑ DescriptionText (TextMeshPro)             │
│   ☑ CardImage (Image)                         │
└────────────────────────────────────────────────┘
```

If **CardMovement** script is missing, cards won't respond to clicks!

---

## Step 7: Verify EventSystem

### Check your scene has EventSystem:

```
Hierarchy:
  ├─ EventSystem                    ← Must exist!
  │  └─ Components:
  │     ├─ Event System
  │     └─ Input System UI Input Module  ← Not "Standalone Input Module"
```

If you see "Standalone Input Module" instead:
1. Remove it (right-click → Remove Component)
2. Add Component → Input System UI Input Module

---

## Visual Checklist

### ✅ Scene View Checks

**When you select the Enemy:**
- Should see a **green box** outline (the collider)
- Box should cover the entire sprite

**When you select the Player:**
- Should see a **green box** outline (the collider)
- Box should cover the entire sprite

**Scene hierarchy should include:**
```
✓ EventSystem
✓ Main Camera
✓ Canvas
│  └─ Hand Panel (or similar)
✓ BattleSystem
│  ├─ TurnManager
│  ├─ DeckManager
│  ├─ HandManager
│  ├─ TargetingSystem ← Camera must be assigned here!
│  └─ PlayerStamina
✓ Player (with Collider2D!)
✓ Enemy (with Collider2D!)
```

---

## 🧪 Testing After Setup

### Test 1: Visual Confirmation
1. **Select Enemy** → See green collider outline in Scene view ✓
2. **Select Player** → See green collider outline in Scene view ✓
3. **Select TargetingSystem** → World Camera field shows "Main Camera" ✓

### Test 2: Play Mode
1. **Enter Play Mode**
2. **Check Console** → Should see "[TurnManager] StartPlayerTurn"
3. **See cards in hand** → 3 cards should appear ✓

### Test 3: Card Interaction
1. **Hover over a card** → Card grows slightly ✓
2. **Click on a card** → Card moves up and locks ✓
3. **Check Console** → "Targeting started for card: [Name]" ✓

### Test 4: Playing Attack Card
1. **Click "Slash" card** → Locks in place
2. **Click directly on enemy sprite** → Should work!
3. **Check Console** → "Successfully playing Slash on enemy!"
4. **Check enemy HP** → Should decrease by 2
5. **Check hand** → Card should be gone ✓

### Test 5: Playing Self Card
1. **Click "Block" or "Heal" card** → Locks in place
2. **Click directly on player sprite** → Should work!
3. **Check Console** → "Successfully playing [Card] on player!"
4. **Check player stats** → Block or HP should increase ✓

### Test 6: Invalid Click
1. **Click any card** → Locks in place
2. **Click on empty space** (not on any sprite)
3. **Check Console** → "requires clicking on [target]"
4. **Card stays locked** → Can try clicking again ✓

### Test 7: Cancel
1. **Click any card** → Locks in place
2. **Press ESC key** (or right-click)
3. **Card returns to hand** ✓

---

## 🐛 If Something's Not Working

### No green outline on Enemy/Player in Scene view
❌ **Problem:** Collider not added
✅ **Fix:** Add Box Collider 2D component (see Step 1 & 2)

### Click on target does nothing, Console says "requires clicking"
❌ **Problem:** Clicking doesn't hit the collider
✅ **Fix:** 
- Check collider size (should cover sprite)
- Make sure "Is Trigger" is UNCHECKED
- Camera must be assigned in TargetingSystem

### Card doesn't lock when clicked
❌ **Problem:** Card prefab missing CardMovement script
✅ **Fix:** Add CardMovement script to card prefab

### No cards appear in hand
❌ **Problem:** HandManager not set up
✅ **Fix:** Assign cardPrefab and handTransform in HandManager

### Console error about camera
❌ **Problem:** Camera not assigned
✅ **Fix:** Drag Main Camera into TargetingSystem.worldCamera field (Step 3)

---

## 🎯 Quick Reference: Where Things Are

| What You Need | Where to Find It | What to Do |
|--------------|------------------|------------|
| Add collider to enemy | Enemy GameObject → Inspector → Add Component | Add "Box Collider 2D" |
| Add collider to player | Player GameObject → Inspector → Add Component | Add "Box Collider 2D" |
| Assign camera | TargetingSystem → Inspector → World Camera field | Drag Main Camera here |
| Check card prefab | HandManager → Inspector → Card Prefab field | Should reference your prefab |
| Check hand location | HandManager → Inspector → Hand Transform field | Should reference UI panel |
| Verify EventSystem | Hierarchy → EventSystem | Should have Input System UI Input Module |

---

## ✅ You're Done When...

- [ ] Enemy has Collider2D (green outline in Scene view)
- [ ] Player has Collider2D (green outline in Scene view)
- [ ] TargetingSystem has camera assigned (shows "Main Camera")
- [ ] Play Mode shows 3 cards in hand
- [ ] Clicking cards locks them in place
- [ ] Clicking enemy with attack card works
- [ ] Clicking player with heal/block card works
- [ ] Console shows success messages

**If all checkboxes are ✓, you're ready to go!**

---

## 💡 Pro Tip

**Use Scene view while testing:**
- Keep Scene view visible alongside Game view
- You can see collider outlines in Scene view
- Helps verify you're clicking on the collider area

**Watch the Console:**
- Every action logs a message
- If something doesn't work, Console explains why
- Enable "Collapse" to see unique messages only

---

That's it! Follow these steps and your targeting system will work perfectly! 🎯
