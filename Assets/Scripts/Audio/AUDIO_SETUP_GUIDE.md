# Audio System Setup Guide

## Overview
This audio system provides a centralized AudioManager for playing sounds throughout the game, with specific controllers for:
- Player footsteps
- Item pickups
- Skeleton animations (scream, slash, death)

## Setup Instructions

### 1. Create AudioManager GameObject

1. In your scene (e.g., LevelOne), create an empty GameObject named **"AudioManager"**
2. Add the **AudioManager** script to it
3. The AudioManager persists across scenes (DontDestroyOnLoad)

### 2. Configure Sounds in AudioManager

In the AudioManager Inspector, add your sounds to the **Sounds** array:

#### Required Sounds:
- **Name**: `Footstep1` - Clip: [Your footstep sound 1]
- **Name**: `Footstep2` - Clip: [Your footstep sound 2]  
- **Name**: `Footstep3` - Clip: [Your footstep sound 3]
- **Name**: `ItemPickup` - Clip: [Your item pickup sound]
- **Name**: `SkeletonScream` - Clip: [Skeleton scream sound]
- **Name**: `SkeletonSlash` - Clip: [Skeleton slash sound]
- **Name**: `SkeletonDeath` - Clip: [Skeleton death sound]

#### Sound Properties:
- **Volume**: 0-1 (default 1.0)
- **Pitch**: 0.1-3 (default 1.0)
- **Spatial Blend**: 0 (2D) or 1 (3D)
- **Loop**: Check if sound should loop

### 3. Setup Player Footsteps

1. Find your **Player** GameObject
2. Add the **PlayerFootstepAudio** script
3. Configure in Inspector:
   - **Footstep Sound Names**: `["Footstep1", "Footstep2", "Footstep3"]`
   - **Footstep Interval**: `0.5` (time between steps)
   - **Minimum Velocity**: `0.1` (minimum speed to trigger footsteps)

### 4. Setup Skeleton Audio

1. Find your **Skeleton** GameObject (the one with LevelOneEnemyAutoHide)
2. Add the **SkeletonAudioController** script
3. Configure in Inspector:
   - **Scream Sound Name**: `SkeletonScream`
   - **Slash Sound Name**: `SkeletonSlash`
   - **Death Sound Name**: `SkeletonDeath`

The LevelOneEnemyAutoHide script will automatically:
- Play scream sound when switching to scream animation
- Play slash sound when switching to slash animation
- Play death sound when playing death animation

### 5. Setup Item Pickup (Optional)

To play pickup sounds when items are collected, call in your item pickup code:

```csharp
// Simple pickup
ItemPickupAudio.PlayPickupSound();

// Or pickup at specific position (3D audio)
ItemPickupAudio.PlayPickupSoundAtPosition(itemPosition);
```

## Audio File Recommendations

### Footsteps
- Format: WAV or OGG
- Length: 0.1-0.3 seconds
- Volume: Medium
- Have 2-3 variations for realism

### Item Pickup
- Format: WAV or OGG
- Length: 0.2-0.5 seconds
- Volume: Medium-High
- Short, satisfying "ding" or "collect" sound

### Skeleton Sounds
- **Scream**: 1-2 seconds, spooky/scary
- **Slash**: 0.3-0.7 seconds, weapon swoosh
- **Death**: 1-3 seconds, final wail/collapse

## Finding Free Audio

### Recommended Sources:
1. **Freesound.org** - Community audio library
2. **OpenGameArt.org** - Game assets including audio
3. **Mixkit.co** - Free sound effects
4. **ZapSplat.com** - Sound effects library
5. **BBC Sound Effects** - Professional quality sounds

## Usage Examples

### Playing Sounds from Code:

```csharp
// Play a sound
AudioManager.Instance.Play("SoundName");

// Play at position (3D)
AudioManager.Instance.PlayAtPosition("SoundName", position);

// Stop a sound
AudioManager.Instance.Stop("SoundName");

// Check if playing
bool isPlaying = AudioManager.Instance.IsPlaying("SoundName");

// Adjust master volume
AudioManager.Instance.SetMasterVolume(0.7f);
```

## Troubleshooting

### No Sound Playing:
1. Check AudioManager exists in scene
2. Verify sound names match exactly (case-sensitive)
3. Check AudioClip is assigned in Inspector
4. Ensure Master Volume > 0
5. Check Unity Audio Mixer settings

### Footsteps Not Working:
1. Verify PlayerFootstepAudio is on Player GameObject
2. Check Player has CharacterController component
3. Ensure player is actually moving (velocity > Minimum Velocity)
4. Verify footstep sound names in array

### Skeleton Audio Not Working:
1. Check SkeletonAudioController is on Skeleton GameObject
2. Verify sound names match AudioManager
3. Check that LevelOneEnemyAutoHide has reference to audio controller
4. Look for debug logs in console

## Advanced Features

### Adding New Sounds:
1. Import audio file to Unity project
2. Add new entry in AudioManager's Sounds array
3. Set name, assign clip, configure properties
4. Call `AudioManager.Instance.Play("YourSoundName")`

### Volume Control:
```csharp
// Individual sound volume (set in Inspector)
sound.volume = 0.8f;

// Master volume (affects all sounds)
AudioManager.Instance.SetMasterVolume(0.5f);
```

### 3D Spatial Audio:
- Set **Spatial Blend** to 1.0 for 3D audio
- Sound will attenuate based on distance
- Use `PlayAtPosition()` for one-shot 3D sounds

## Testing Checklist

- [ ] AudioManager exists in scene
- [ ] All required sounds added and clips assigned
- [ ] Player footsteps play when moving
- [ ] Skeleton plays scream sound
- [ ] Skeleton plays slash sound
- [ ] Skeleton plays death sound
- [ ] Item pickup sound works (if implemented)
- [ ] No audio warnings in console

## Notes

- AudioManager persists across scenes (singleton pattern)
- Only one AudioManager should exist at a time
- All sounds are pre-loaded on AudioManager Awake
- Footsteps use random selection for variety
- Skeleton audio is automatically triggered by animations
