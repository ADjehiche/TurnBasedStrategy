# Card Reward Panel - Complete Construction Guide

## Overview
This guide shows you how to build the post-battle card reward UI from scratch in Unity.

---

## Step-by-Step Construction

### Step 1: Create the Main Panel

1. In your **Battle_Template** scene, find the Canvas
2. Right-click Canvas → **UI → Panel**
3. Rename it to **"BattleRewardPanel"**

**Configure BattleRewardPanel**:
- **Rect Transform**:
  - Anchor Presets: **Stretch-Stretch** (click the square in top-left, hold Alt+Shift, click center)
  - Left/Right/Top/Bottom: All set to **0**
  - This makes it cover the entire screen
- **Image Component**:
  - Color: Black with **Alpha 200** (semi-transparent overlay)
- **Set Active**: **Unchecked** (starts hidden)

---

### Step 2: Create Title Text

1. Right-click **BattleRewardPanel** → **UI → Text - TextMeshPro**
2. Rename to **"TitleText"**

**Configure TitleText**:
- **Rect Transform**:
  - Anchor: **Top-Center**
  - Pos X: **0**, Pos Y: **-100**
  - Width: **800**, Height: **100**
- **TextMeshProUGUI**:
  - Text: **"Choose Your Reward"**
  - Font Size: **48**
  - Alignment: **Center, Middle**
  - Color: **White or Gold**
  - Font Style: **Bold**

---

### Step 3: Create Card Options Container

1. Right-click **BattleRewardPanel** → **UI → Empty** (Create Empty GameObject)
2. Rename to **"CardOptionsContainer"**

**Configure CardOptionsContainer**:
- **Rect Transform**:
  - Anchor: **Middle-Center**
  - Pos X: **0**, Pos Y: **0**
  - Width: **1000**, Height: **400**
- **Add Component**: **Horizontal Layout Group**
  - Child Alignment: **Middle Center**
  - Spacing: **50**
  - Child Force Expand: **Width: Off, Height: Off**
- **Add Component**: **Content Size Fitter**
  - Horizontal Fit: **Preferred Size**
  - Vertical Fit: **Preferred Size**

---

### Step 4: Create Card Display Prefab

This is the template for each reward card option.

#### 4.1: Create the Card Object
1. Right-click **CardOptionsContainer** → **UI → Image**
2. Rename to **"CardOption"**

**Configure CardOption**:
- **Rect Transform**:
  - Width: **250**
  - Height: **350**
- **Image Component**:
  - Color: **White**
  - Source Image: Leave empty (will be set by card artwork)
- **Add Component**: **Button**
  - Transition: **Color Tint**
  - Normal Color: **White**
  - Highlighted Color: **Light Yellow (255, 255, 200)**
  - Pressed Color: **Light Gray (200, 200, 200)**
  - Selected Color: **Yellow**

#### 4.2: Add Card Background
1. Right-click **CardOption** → **UI → Image**
2. Rename to **"CardBackground"**

**Configure CardBackground**:
- **Rect Transform**:
  - Anchor: **Stretch-Stretch**
  - Left/Right/Top/Bottom: All **0**
- **Image**:
  - Color: **Light Gray or Beige** (215, 215, 200)
  - Raycast Target: **Off**

#### 4.3: Add Card Artwork
1. Right-click **CardOption** → **UI → Image**
2. Rename to **"Artwork"**

**Configure Artwork**:
- **Rect Transform**:
  - Anchor: **Top-Center**
  - Pos Y: **-20**
  - Width: **210**, Height: **150**
- **Image**:
  - Preserve Aspect: **Checked**
  - Raycast Target: **Off**

#### 4.4: Add Card Name
1. Right-click **CardOption** → **UI → Text - TextMeshPro**
2. Rename to **"CardName"**

**Configure CardName**:
- **Rect Transform**:
  - Anchor: **Top-Center**
  - Pos Y: **-180**
  - Width: **230**, Height: **40**
- **TextMeshProUGUI**:
  - Font Size: **24**
  - Alignment: **Center, Middle**
  - Color: **Black**
  - Font Style: **Bold**
  - Wrapping: **Enabled**
  - Overflow: **Truncate**

#### 4.5: Add Card Description
1. Right-click **CardOption** → **UI → Text - TextMeshPro**
2. Rename to **"Description"**

**Configure Description**:
- **Rect Transform**:
  - Anchor: **Top-Center**
  - Pos Y: **-230**
  - Width: **230**, Height: **80**
- **TextMeshProUGUI**:
  - Font Size: **16**
  - Alignment: **Center, Top**
  - Color: **Dark Gray (50, 50, 50)**
  - Wrapping: **Enabled**
  - Overflow: **Truncate**

#### 4.6: Add Stamina Cost
1. Right-click **CardOption** → **UI → Text - TextMeshPro**
2. Rename to **"StaminaCost"**

**Configure StaminaCost**:
- **Rect Transform**:
  - Anchor: **Bottom-Center**
  - Pos Y: **15**
  - Width: **80**, Height: **40**
- **TextMeshProUGUI**:
  - Font Size: **28**
  - Alignment: **Center, Middle**
  - Color: **Blue (0, 100, 255)**
  - Font Style: **Bold**

#### 4.7: Add CardDisplay Component
1. Select **CardOption**
2. **Add Component**: **CardDisplay** (your existing script)
3. **Add Component**: **CardInstance** (your existing script)
4. Assign references in CardDisplay:
   - **Artwork Image**: Drag Artwork
   - **Card Name Text**: Drag CardName
   - **Description Text**: Drag Description
   - **Stamina Text**: Drag StaminaCost

#### 4.8: Create Prefab
1. Drag **CardOption** from hierarchy to your **Prefabs** folder
2. Delete the CardOption from the hierarchy (we'll spawn it via code)

---

### Step 5: Create Skip Button

1. Right-click **BattleRewardPanel** → **UI → Button - TextMeshPro**
2. Rename to **"SkipButton"**

**Configure SkipButton**:
- **Rect Transform**:
  - Anchor: **Bottom-Center**
  - Pos X: **0**, Pos Y: **50**
  - Width: **200**, Height: **60**
- **Image Component**:
  - Color: **Dark Gray (100, 100, 100)**
- **Button**:
  - Transition: **Color Tint**
  - Normal: **Gray**
  - Highlighted: **Light Gray**
  - Pressed: **Dark Gray**

**Configure Skip Button Text**:
- Select the child Text object
- **TextMeshProUGUI**:
  - Text: **"Skip Reward"**
  - Font Size: **24**
  - Alignment: **Center, Middle**
  - Color: **White**

---

### Step 6: Setup CardRewardUI Component

1. Create empty GameObject in your scene (outside Canvas): **"CardRewardUI"**
2. **Add Component**: **CardRewardUI** (the script we created)

**Assign References**:
- **Reward Panel**: Drag **BattleRewardPanel**
- **Card Option Prefab**: Drag **CardOption** prefab from Prefabs folder
- **Card Options Container**: Drag **CardOptionsContainer**
- **Title Text**: Drag **TitleText**
- **Skip Button**: Drag **SkipButton**

---

### Step 7: Setup BattleRewardManager

1. Create empty GameObject: **"BattleRewardManager"**
2. **Add Component**: **BattleRewardManager**

**Assign References**:
- **Card Reward UI**: Drag **CardRewardUI** GameObject

**Configure Settings**:
- **Show Card Reward After Battle**: ✅ Checked
- **Delay Before Reward**: **2.0** seconds

---

## Final Hierarchy Structure

```
Canvas
├── (Other UI elements)
└── BattleRewardPanel (Panel, starts inactive)
    ├── TitleText (TMP)
    ├── CardOptionsContainer (Empty + Horizontal Layout Group)
    │   └── (Card options spawn here at runtime)
    └── SkipButton (Button)

(Outside Canvas)
├── CardRewardUI (GameObject with CardRewardUI component)
└── BattleRewardManager (GameObject with BattleRewardManager component)
```

---

## Prefabs Folder

```
Prefabs/
└── CardOption (Card display prefab with CardDisplay + CardInstance components)
```

---

## Visual Styling Tips

### For a Professional Look:

**Background Panel**:
- Use a gradient texture (dark at edges, lighter in center)
- Add subtle border image

**Card Options**:
- Add drop shadow effect (Outline component with offset)
- Add rounded corners (use 9-sliced sprite)
- Add rarity borders (gold for rare, silver for common)

**Animations**:
Add **Animator** component to CardOption prefab:
- Scale up on hover (1.0 → 1.1)
- Bounce when spawned
- Glow effect when selected

---

## Testing the UI

### Test in Unity Editor:
1. Play the game
2. Win a battle (defeat all enemies)
3. After 2 seconds, reward panel should appear
4. Click a card → Should log selection and close panel
5. Click Skip → Should close panel without adding card

### Debug Test (Force Show Rewards):
Create a test button in your UI:
```csharp
// Attach to a test button
public void TestShowRewards()
{
    if (CardRewardUI.Instance != null)
    {
        CardRewardUI.Instance.ShowRewardSelection();
    }
}
```

---

## Common Issues & Solutions

### Issue: Cards not displaying properly
**Solution**: Make sure CardDisplay component has all references assigned (Artwork, CardName, Description, StaminaCost)

### Issue: Buttons not clickable
**Solution**: 
- Check Canvas has **GraphicRaycaster** component
- Check EventSystem exists in scene
- Verify Button components are enabled

### Issue: Cards spawning in wrong position
**Solution**: 
- Check CardOptionsContainer has Horizontal Layout Group
- Verify Content Size Fitter is set to Preferred Size
- Make sure CardOption prefab has correct anchors

### Issue: Reward panel doesn't show
**Solution**:
- Check BattleRewardPanel starts inactive
- Verify BattleRewardManager is in the scene
- Check CardRewardUI has all references assigned
- Make sure you WIN the battle (not lose)

---

## Optional Enhancements

### Add Sound Effects:
```csharp
// In CardRewardUI.cs, in OnCardSelected():
if (AudioManager.Instance != null)
{
    AudioManager.Instance.Play("CardSelect");
}
```

### Add Card Hover Preview:
```csharp
// Enlarge card on hover
void OnPointerEnter()
{
    transform.localScale = Vector3.one * 1.15f;
}

void OnPointerExit()
{
    transform.localScale = Vector3.one;
}
```

### Add Rarity Indicators:
- Common: Gray border
- Uncommon: Green border
- Rare: Blue border
- Epic: Purple border
- Legendary: Gold border

---

## Quick Setup Checklist

- [ ] Create BattleRewardPanel (covers screen, starts inactive)
- [ ] Add TitleText to panel
- [ ] Create CardOptionsContainer with Horizontal Layout Group
- [ ] Create CardOption prefab with all child elements
- [ ] Add CardDisplay + CardInstance components to prefab
- [ ] Assign all references in CardDisplay
- [ ] Create prefab in Prefabs folder
- [ ] Add SkipButton to panel
- [ ] Create CardRewardUI GameObject with component
- [ ] Assign all references in CardRewardUI
- [ ] Create BattleRewardManager GameObject with component
- [ ] Assign CardRewardUI reference in BattleRewardManager
- [ ] Test by winning a battle

---

## Result

After setup, winning a battle will:
1. Screen fades with semi-transparent overlay
2. "Choose Your Reward" appears at top
3. 2 random cards display in center (with artwork, name, description)
4. Player clicks one → Added to collection
5. Player clicks Skip → No card added
6. Panel disappears, continue playing

Enjoy your card collection system! 🎴
