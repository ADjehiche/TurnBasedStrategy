# Card Merge UI Setup Guide (Minimal)

This guide sets up the simplest possible Card Merge UI: a visible panel, a recipe list, a details area, and a Merge button. No animations, no CardSlotUI, no tooltips.

## 📋 Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [UI Hierarchy Setup](#ui-hierarchy-setup)
4. [Component Configuration](#component-configuration)
5. [Prefab Creation](#prefab-creation)
6. [Testing](#testing)
7. [Customization](#customization)

---

## Overview

The minimal Card Merge UI system uses:
- **CardMergeUI** - Main controller for the merge panel
- **RecipeItemUI** - Individual recipe items in the scroll list
- **CardMergeManager** - Backend logic (already implemented)
- **CardCollection / CardRecipe** - Data and player inventory

---

## Prerequisites

✅ **Required Components:**
- TextMeshPro package installed
- CardMergeManager in the scene
- CardCollection system set up
- Card ScriptableObjects created
- Recipe ScriptableObjects generated (use Tools > Card Game > Generate All Recipes)

---

## UI Hierarchy Setup

### Step 1: Create Main Merge Panel

1. **Right-click in Hierarchy** → UI → Panel
2. **Rename** to `CardMergePanel`
3. **Set anchor** to stretch (full screen)
4. **Background:** Semi-transparent dark color (R:0, G:0, B:0, A:200)

### Step 2: Create Content Container

Inside `CardMergePanel`:

```
CardMergePanel (Panel)
├── Header (Panel)
│   ├── TitleText (TextMeshProUGUI) - "Card Merge"
│   └── CloseButton (Button)
│
├── RecipeList (ScrollView)
│   ├── Viewport
│   │   └── Content (Vertical Layout Group)
│   │       └── [Recipe Items spawned here]
│   └── Scrollbar Vertical
│
└── SelectedRecipePanel (Panel)
   ├── Ingredient1Image (Image)
   ├── Ingredient1Text (TextMeshProUGUI)
   ├── PlusIcon (Image) - "+"
   ├── Ingredient2Image (Image)
   ├── Ingredient2Text (TextMeshProUGUI)
   ├── ArrowIcon (Image) - "→"
   ├── ResultImage (Image)
   ├── ResultText (TextMeshProUGUI)
   ├── DescriptionText (TextMeshProUGUI)
   └── MergeButton (Button)
      └── ButtonText (TextMeshProUGUI) - "MERGE CARDS"
```

### Step 3: Create Recipe Item Prefab

Create a new prefab called `RecipeItemPrefab`:

```
RecipeItemPrefab (Button)
├── Background (Image)
├── Ingredient1Image (Image)
├── PlusIcon (Image) - "+"
├── Ingredient2Image (Image)
├── ArrowIcon (Image) - "→"
├── ResultImage (Image)
└── RecipeName (TextMeshProUGUI)
```

**Add RecipeItemUI component** to this prefab and assign all references.

---

## Component Configuration

### CardMergeUI Configuration

Add the `CardMergeUI` script to `CardMergePanel` and assign:

**UI References:**
- `mergePanel` → CardMergePanel itself
- `recipeListContainer` → RecipeList/Viewport/Content
- `recipeItemPrefab` → Your RecipeItemPrefab
- `titleText` → Header/TitleText
- `closeButton` → Header/CloseButton

**Selected Recipe Display:**
- `selectedRecipePanel` → SelectedRecipePanel
- `ingredient1Image` → Ingredient1Image
- `ingredient2Image` → Ingredient2Image
- `resultImage` → ResultImage
- `ingredient1Text` → Optional text below ingredient1
- `ingredient2Text` → Optional text below ingredient2
- `resultText` → Optional text below result
- `recipeDescriptionText` → DescriptionText
- `mergeButton` → MergeButton

### RecipeItemUI Configuration

For each recipe item prefab:

Assign whatever references exist on your `RecipeItemUI` prefab (at minimum: the ingredient/result images and the recipe name text).

---

## Prefab Creation

### Recipe Item Prefab Details

1. **Create prefab** from Hierarchy
2. **Size:** Width: 600, Height: 100
3. **Layout:** Horizontal Layout Group
   - Padding: 10
   - Spacing: 15
   - Child Alignment: Middle Center

4. **Button Component:**
   - Transition: Color Tint
   - Normal Color: White
   - Highlighted: Light Blue
   - Pressed: Gray
   - Disabled: Dark Gray

5. **Images:**
   - Ingredient images: 70x70
   - Result image: 80x80
   - Icons: 30x30

6. **Save as prefab** in `Assets/Prefabs/UI/`

---

## Testing

### Testing Checklist

1. **Scene Setup:**
   - [ ] CardMergePanel exists with all components
   - [ ] CardMergeManager in scene
   - [ ] CardCollection in scene
   - [ ] At least one recipe generated

2. **Opening Panel:**
   - [ ] Call `CardMergeUI.OpenMerge()` from inspector or script
   - [ ] Panel shows up
   - [ ] Recipes display in scroll view
   - [ ] Clicking recipes selects them

3. **Recipe Selection:**
   - [ ] Clicking recipe shows details
   - [ ] Card counts display correctly
   - [ ] Locked recipes show as grayed out
   - [ ] Craftable recipes highlighted

4. **Merging:**
   - [ ] Merge button enabled when ingredients available
   - [ ] Animation plays (if configured)
   - [ ] Cards consumed from collection
   - [ ] Result card added to collection
   - [ ] UI refreshes after merge

5. **Enhanced Features:**
   - [ ] Search filters recipes
   - [ ] "Show Only Craftable" toggle works
   - [ ] Sorting dropdown works
   - [ ] Hover effects work

---

## Customization

### Color Schemes

**Fantasy Theme:**
```csharp
craftableColor = new Color(0.8f, 0.6f, 0.2f, 0.4f); // Gold
lockedColor = new Color(0.3f, 0.3f, 0.4f, 0.5f);     // Dark blue-gray
```

**Sci-Fi Theme:**
```csharp
craftableColor = new Color(0.0f, 0.8f, 1.0f, 0.4f); // Cyan
lockedColor = new Color(0.2f, 0.2f, 0.3f, 0.6f);     // Dark gray
```

### Animation Speeds

**Fast animations:**
```csharp
ingredientMoveTime = 0.3f;
mergeHoldTime = 0.2f;
resultRevealTime = 0.3f;
```

**Slow/dramatic animations:**
```csharp
ingredientMoveTime = 0.8f;
mergeHoldTime = 0.5f;
resultRevealTime = 0.7f;
```

### Layout Variations

**Compact Layout:**
- Recipe items: Height 80
- Card slots: 60x60
- Smaller fonts

**Large Layout:**
- Recipe items: Height 120
- Card slots: 100x100
- Bigger fonts, more spacing

---

## Common Issues & Solutions

### Issue: Recipes not showing
**Solution:** 
- Check that recipes are in `Resources/Recipes/` folder
- Run `Tools > Card Game > Generate All Recipes`
- Verify CardMergeManager is in scene

### Issue: Cards not merging
**Solution:**
- Check CardCollection has the ingredient cards
- Verify recipe ingredient references are assigned
- Check console for error messages

### Issue: UI not responding
**Solution:**
- Ensure EventSystem exists in scene
- Check Canvas has GraphicRaycaster
- Verify button components are properly configured

### Issue: Animations not playing
**Solution:**
- Assign MergeAnimationController reference in CardMergeUI
- Check particle system references
- Verify AudioManager has correct sound names

---

## Opening the Merge Panel

### Method 1: From Code
```csharp
CardMergeUI.OpenMerge();
```

### Method 2: Button Click
Add a button in your game UI:
- OnClick → CardMergeUI (Instance) → OpenMergePanel()

### Method 3: Keyboard Shortcut
Add to a player controller:
```csharp
if (Input.GetKeyDown(KeyCode.M))
{
    CardMergeUI.OpenMerge();
}
```

---

## Additional Features

### Keyboard Navigation
Add this to CardMergeUI:
```csharp
private void Update()
{
    if (mergePanel != null && mergePanel.activeSelf)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }
}
```

### Recipe Discovery Notifications
When a new recipe becomes craftable, show a notification:
```csharp
public void ShowNewRecipeNotification(CardRecipe recipe)
{
    // Show popup: "New recipe unlocked: [Recipe Name]"
}
```

### Batch Merging
Allow merging multiple copies at once (modify CardMergeManager):
```csharp
public bool TryMergeMultiple(CardRecipe recipe, int count)
{
    for (int i = 0; i < count; i++)
    {
        if (!TryMergeCards(recipe))
            return false;
    }
    return true;
}
```

---

## Best Practices

1. **Performance:**
   - Use object pooling for recipe items if you have 50+ recipes
   - Disable animations on low-end devices
   - Cache component references

2. **UX:**
   - Show tooltips on hover
   - Add confirmation for expensive merges
   - Provide undo option (advanced)

3. **Accessibility:**
   - Use clear, readable fonts (minimum 14pt)
   - High contrast colors
   - Keyboard navigation support

4. **Mobile Support:**
   - Larger touch targets (minimum 80x80)
   - Pinch to zoom on recipe list
   - Swipe to close panel

---

## Credits & References

- **Scripts Created:**
  - CardMergeUI.cs (enhanced)
  - RecipeItemUI.cs
  - CardSlotUI.cs
  - MergeAnimationController.cs

- **Existing Scripts Used:**
  - CardMergeManager.cs
  - CardRecipe.cs
  - CardCollection.cs
  - Card.cs (CardSystem.cs)

---

**Happy Merging! ✨**

For questions or issues, check the Unity console for debug logs.
All scripts include detailed comments and debug output.
