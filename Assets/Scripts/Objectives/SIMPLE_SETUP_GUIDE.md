# SIMPLE Objective System Setup Guide

This is the SIMPLE version - just one text field that updates. No complex management!

## What You Get

- ✅ ONE objective text that changes over time
- ✅ Sequential progression (one objective at a time)
- ✅ Auto-creates its own UI panel
- ✅ Can use your custom background image
- ✅ No prefabs, no complex setup needed

## Super Easy Setup

### Step 1: Setup Your Existing Panel

1. Find your existing objective panel GameObject (the one with your custom image)
2. Add the `SimpleObjectiveUI` script to this GameObject
3. In the SimpleObjectiveUI component:
   - **Objective Text**: Drag your existing text component here
   - **Progress Slider**: Drag your existing slider component here (optional)
   - **Objective Panel**: Drag the panel GameObject itself here
4. Make sure your slider's "Interactable" is set to FALSE (so it's display-only)

### Step 2: Add the Objectives Manager

1. Create an empty GameObject in your Level One scene
2. Name it "SimpleObjectives"
3. Add the `SimpleLevelOneObjectives` script
4. In the SimpleLevelOneObjectives component:
   - **Simple Objective UI**: Drag your panel GameObject (the one with SimpleObjectiveUI script)

### Step 3: Test It

1. Press Play
2. You should see "Wake Up" appear in your custom panel
3. It will automatically progress through the objectives using your existing UI

### Step 3: Optional - Use Progress Slider

If you want some objectives to show progress:

```csharp
SimpleLevelOneObjectives objectives = FindFirstObjectByType<SimpleLevelOneObjectives>();
SimpleObjectiveUI ui = objectives.GetComponent<SimpleObjectiveUI>();
ui.SetProgress(0.5f); // 50% progress
ui.HideProgress(); // Hide slider when not needed
```

## Integration with Your Game

### When player picks up key:

```csharp
SimpleLevelOneObjectives objectives = FindFirstObjectByType<SimpleLevelOneObjectives>();
if (objectives != null) objectives.OnKeyPickedUp();
```

### When cell door opens:

```csharp
SimpleLevelOneObjectives objectives = FindFirstObjectByType<SimpleLevelOneObjectives>();
if (objectives != null) objectives.OnCellDoorOpened();
```

### When player enters hallway:

```csharp
SimpleLevelOneObjectives objectives = FindFirstObjectByType<SimpleLevelOneObjectives>();
if (objectives != null) objectives.OnEnterHallway();
```

### When player meets companion:

```csharp
SimpleLevelOneObjectives objectives = FindFirstObjectByType<SimpleLevelOneObjectives>();
if (objectives != null) objectives.OnMeetCompanion();
```

## Testing

Right-click on the `SimpleLevelOneObjectives` component in inspector:

- "Force Complete Current Objective" - Skip to next objective
- "Skip To Key Objective" - Jump to the key finding part
- "Skip To Escape Cell" - Jump to door opening part
- "Skip To Companion" - Jump to final objective

## Objective Sequence

1. **"Wake Up"** → Auto-completes (2 seconds)
2. **"Explore Your Cell"** → Auto-completes (5 seconds)
3. **"Find a Way Out"** → Auto-completes (3 seconds)
4. **"Find the Cell Key"** → Waits for OnKeyPickedUp()
5. **"Escape the Cell"** → Waits for OnCellDoorOpened()
6. **"Escape the Dungeon"** → Waits for OnEnterHallway()
7. **"Befriend the Glowing Entity"** → Waits for OnMeetCompanion()

## That's It!

No complex setup, no prefabs, no multiple components. Just one script that creates a simple text field and updates it. Perfect for your one-objective-at-a-time system!
