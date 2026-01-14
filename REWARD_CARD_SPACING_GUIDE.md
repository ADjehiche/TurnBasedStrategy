# Reward Card Spacing & Layout Guide

## Overview
The reward card display uses Unity's HorizontalLayoutGroup for automatic spacing and alignment.

## Quick Adjustments

### In Unity Inspector (CardRewardUI component):

**Card Display Settings:**
- **Card Scale** (default: 1.2)
  - Make cards bigger/smaller
  - 1.0 = same size as hand cards
  - 1.5 = 50% larger
  - Recommended: 1.2 - 1.5

- **Card Spacing** (default: 50)
  - Space between the 2 cards
  - 0 = cards touching
  - 100 = large gap
  - Recommended: 40-80

### Via Code (CardRewardUI.cs):

**SetupCardContainerLayout() method:**
```csharp
layoutGroup.spacing = cardSpacing;              // Space between cards
layoutGroup.padding = new RectOffset(20, 20, 20, 20); // Padding around container (left, right, top, bottom)
layoutGroup.childAlignment = TextAnchor.MiddleCenter;  // Alignment
```

## Layout Options

### Horizontal Layout Group Settings:
- **Spacing**: Gap between cards (pixels)
- **Child Alignment**: Where cards sit in container
  - `MiddleCenter` - Center horizontally and vertically
  - `UpperCenter` - Top center
  - `LowerCenter` - Bottom center
- **Padding**: Space around all cards
  - Left, Right, Top, Bottom margins

### Card Transform:
- **Scale**: Applied to each card individually (`cardScale`)
- **Rotation**: Reset to identity (no rotation)
- **Position**: Auto-handled by layout group

## Common Adjustments

### Make cards bigger and further apart:
```csharp
[SerializeField] private float cardScale = 1.5f;
[SerializeField] private float cardSpacing = 80f;
```

### Smaller, compact display:
```csharp
[SerializeField] private float cardScale = 1.0f;
[SerializeField] private float cardSpacing = 30f;
```

### Add more padding around edges:
```csharp
layoutGroup.padding = new RectOffset(50, 50, 30, 30); // More side padding
```

## Advanced Customization

### Vertical Layout Instead:
Replace `HorizontalLayoutGroup` with `VerticalLayoutGroup`:
```csharp
var layoutGroup = cardOptionsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
```

### Grid Layout (for 3+ cards):
```csharp
var gridLayout = cardOptionsContainer.gameObject.AddComponent<GridLayoutGroup>();
gridLayout.spacing = new Vector2(cardSpacing, cardSpacing);
gridLayout.cellSize = new Vector2(200, 300); // Card size
```

### Manual Positioning (no layout group):
Remove the layout group and position cards manually:
```csharp
cardObj.transform.localPosition = new Vector3(i * cardSpacing, 0, 0);
```

## Troubleshooting

### Cards overlapping:
- Increase `cardSpacing`
- Decrease `cardScale`
- Check container size (RectTransform)

### Cards too small:
- Increase `cardScale`
- Check cardPrefab base size

### Cards not centered:
- Set `childAlignment = TextAnchor.MiddleCenter`
- Check container anchors/pivot

### Cards cut off:
- Increase container width/height
- Reduce padding
- Reduce card scale

## Inspector Settings Reference

**Recommended Starting Values:**
- Card Scale: `1.2` to `1.5`
- Card Spacing: `50` to `80`
- Padding: `20-50` pixels each side

**For 2 cards at scale 1.2 with 50px spacing:**
- Minimum container width: ~500px
- Minimum container height: ~400px

## Testing Tips

1. Adjust values in Inspector **while game is running** (changes revert on stop)
2. Note good values, then set them when game is stopped
3. Test on different screen resolutions
4. Consider UI scaling settings in Canvas Scaler
