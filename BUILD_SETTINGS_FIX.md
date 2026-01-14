# Scene Not in Build Settings - Fix

## The Error
```
Scene 'Battle_Template' couldn't be loaded because it has not been added to the build settings
```

## Root Cause
Unity's `SceneManager.LoadScene()` can only load scenes that are:
1. ✅ Added to Build Settings, OR
2. ✅ Loaded via AssetBundles (advanced)

## Quick Fix (1 minute)

### In Unity Editor:

1. **File** → **Build Settings...** (or `Ctrl+Shift+B` / `Cmd+Shift+B`)

2. **Add Battle_Template scene**:
   - **Option A**: If Battle_Template is open, click **Add Open Scenes** button
   - **Option B**: Drag `Assets/Scenes/Battle_Template.unity` from Project window into the list

3. **Your Scenes In Build should include**:
   ```
   ☑ TitleScene
   ☑ LevelOne
   ☑ Battle_Template  ← Add this!
   ☑ ControlsPage
   ☑ DeathScene
   ```

4. Click **Close**

5. **Press Play** - Works now! ✅

## Verify Scene Name Matches

Make sure the scene file name matches exactly what's in code:

**In Project Window**: `Assets/Scenes/Battle_Template.unity`
**In GameManager.cs**: `private const string BattleScene = "Battle_Template";`

Scene names are **case-sensitive**! Must match exactly.

## Testing Checklist

After adding to Build Settings:

- [ ] Open TitleScene
- [ ] Press Play
- [ ] Click Start Button
- [ ] Battle_Template loads successfully
- [ ] No console errors
- [ ] Fight and win battle
- [ ] Reward panel appears

## Why This Happens

Unity requires all scenes to be registered in Build Settings before loading them at runtime. This is for optimization - Unity only includes registered scenes in builds.

During development, you can open scenes directly in the editor, but `SceneManager.LoadScene()` needs them in Build Settings.

## Other Scenes to Check

While you're in Build Settings, make sure ALL these scenes are added:

```
Required Scenes:
✅ TitleScene (starting scene - should be index 0)
✅ LevelOne (exploration)
✅ Battle_Template (combat)

Optional Scenes (if you use them):
✅ ControlsPage
✅ DeathScene
```

## If You Still Get Errors

### Error: "Scene name is incorrect"
→ Check spelling/capitalization matches file name exactly

### Error: "Scene file not found"  
→ Verify scene exists in `Assets/Scenes/` folder

### Error: "Build Settings window won't open"
→ Use menu: `File` → `Build Settings...`

---

**TL;DR**: 
1. Open Build Settings (`File` → `Build Settings`)
2. Add `Battle_Template` scene to the list
3. Press Play
4. Done! 🎯
