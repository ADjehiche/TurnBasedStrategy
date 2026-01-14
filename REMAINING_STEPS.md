# Ultra-Quick Setup (You're at Step 4.2 ✅)

## What You Already Have ✅
- ✅ BattleRewardPanel (semi-transparent black panel)
- ✅ TitleText ("Choose Your Reward")
- ✅ CardOptionsContainer (empty GameObject)

## 5 Steps Left (2 minutes!)

### Step 1: Add Layout to CardOptionsContainer
Select **CardOptionsContainer**, then:
1. Add Component → **Horizontal Layout Group**
   - Spacing: **80**
   - Child Alignment: **Middle Center**
2. Add Component → **Content Size Fitter**
   - Horizontal: **Preferred Size**
   - Vertical: **Preferred Size**

---

### Step 2: Add Skip Button
1. Right-click **BattleRewardPanel** → UI → Button - TextMeshPro
2. Rename: **"SkipButton"**
3. Position: Bottom-Center, Y: 50
4. Change text to: **"Skip Reward"**

---

### Step 3: Create CardRewardUI
1. Create empty GameObject: **"CardRewardUI"**
2. Add Component: **CardRewardUI**
3. Assign references:
   - **Reward Panel**: BattleRewardPanel
   - **Card Prefab**: ⚠️ **Drag the SAME prefab HandManager uses**
   - **Card Options Container**: CardOptionsContainer
   - **Title Text**: TitleText
   - **Skip Button**: SkipButton

**Finding Card Prefab**:
- Look at HandManager component → "Card Prefab" field
- Drag that same prefab to CardRewardUI

---

### Step 4: Create BattleRewardManager
1. Create empty GameObject: **"BattleRewardManager"**
2. Add Component: **BattleRewardManager**
3. Assign:
   - **Card Reward UI**: CardRewardUI GameObject
4. Settings:
   - Show Card Reward: ✅
   - Delay: **2.0**

---

### Step 5: Test!
1. Play from TitleScene
2. Win battle
3. Reward panel appears with 2 cards
4. Click one
5. Done! 🎉

---

## Reference Files

- **Detailed UI Guide**: `REWARD_PANEL_CONSTRUCTION.md`
- **Simplified Guide**: `SIMPLIFIED_REWARD_SETUP.md`
- **System Comparison**: `CARD_SYSTEM_COMPARISON.md`
- **Quick Start**: `QUICK_START_SETUP.md`

You got this! 💪
