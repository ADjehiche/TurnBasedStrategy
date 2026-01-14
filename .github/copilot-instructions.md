# Dungeon Decks - AI Coding Agent Guide

## Project Overview
Unity 3D single-player dungeon crawler blending third-person exploration with turn-based card combat. Player explores dungeons, triggers battles via collision, fights using card-based actions, then returns to exploration on victory/defeat.

## Core Architecture

### Scene Flow & State Management
- **Scenes**: `TitleScene` → `LevelOne` (exploration) ⇄ `Battle_Template` (combat) → `DeathScene` (on loss)
- **GameManager**: Singleton managing scene transitions. Lives in each scene (not DontDestroyOnLoad). Key method: `StartBattle()` saves player position, loads battle scene
- **GameSession** (static): Cross-scene state container. Stores:
  - `EnemyDefeated`, `DoorOpened`, `Symbol1/2Activated` (world state)
  - `ReturnPosition`, `BattleTriggerCenter` (player spawn after battle)
  - `CheckpointPosition/Rotation` (death respawn system)
  - Caption display flags to prevent re-triggering
- **Scene return pattern**: After battle, `BattleManager` sets `GameSession.EnemyDefeated = true`, loads `LevelOne`. `LevelOneReturnManager` spawns player at `BattleTriggerCenter - 3 units` to avoid re-triggering

### Battle System Architecture (Card-Based Combat)
Three core managers orchestrate turn-based combat:

1. **TurnManager** (Singleton): Turn state machine
   - States: `PlayerTurn`, `EnemyTurn`, `None`
   - `StartPlayerTurn()`: Refills stamina → draws cards → enables EndTurnButton
   - `OnEndTurnButtonPressed()`: Discards hand → starts enemy turn → enemy acts → returns to player turn
   - Depends on: `DeckManager`, `HandManager`, `PlayerStamina`

2. **DeckManager**: Card deck/discard pile system
   - Auto-builds deck from `Resources/Cards/{Attack,Defense,Tactical,Utility}` folders
   - No card duplicates in starting deck (all unique cards)
   - `DrawOne()`: Auto-reshuffles discard → draw pile when empty
   - `Discard(Card)`: Moves card to discard pile

3. **HandManager**: UI hand display
   - `AddCardToHand(Card)`: Instantiates card prefab → sets CardInstance data → enables GameObject → updates visual layout
   - **Critical**: Set CardInstance data BEFORE enabling GameObject (prevents OnEnable errors)
   - Listens to `BattleEvents.OnCardResolved` to remove played cards

4. **TargetingSystem** (Singleton): Mouse-based card targeting
   - `BeginTargeting(Card, GameObject, Action)`: Locks card, enables input listening
   - Multi-layer targeting: UI raycast first (Canvas health bars), then 3D collider raycast
   - Validates stamina cost, target type (Self/SingleEnemy/AllEnemies), resolves card effects
   - `CancelTargeting()`: Right-click or Escape to return card to hand

### Card System (ScriptableObject-Based)
- **Namespace**: `CardGame` (only file with namespace in project)
- **Card** (`CardSystem.cs`): ScriptableObject with `CardCategory` (Attack/Defense/Utility/Tactical), `TargetType`, `effects` list, stamina cost
- **CardInstance** (component): Runtime wrapper with unique GUID, holds Card data reference
- **CardDisplay** (component): UI binding (artwork, name, description, stamina text)
- **CardMovement** (component): Hover/click interactions using Unity EventSystems (IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler)

### Health & Combat Resources
- **PlayerHealth**, **EnemyHealth**: HP tracking + status effects (bleed stacks, weaken percent/turns)
- **PlayerStamina**: Resource system for playing cards (max 10, refills each player turn)
- **BattleAnimator** (Singleton): Damage popups, bump animations, screen shake effects
- **BattleState** (static): Tracks `IsOver` flag, fires `OnBattleOverChanged` event

### Singleton Pattern Usage
Heavy use of singletons for battle systems:
- `TurnManager.Instance`
- `TargetingSystem.Instance`
- `PlayerHealth.Instance`
- `PlayerStamina.Instance`
- `BattleAnimator.Instance`
- `AudioManager.Instance`
- `CaptionManager.Instance`
- `PlayerMovementLock.Instance` (exploration mode lock)

**Pattern**: `public static T Instance { get; private set; }` set in `Awake()`, nulled in `OnDestroy()`

## Development Workflows

### Adding New Cards
1. Create ScriptableObject: `Assets > Create > Card Game > Card`
2. Place in `Assets/Resources/Cards/{Category}/` folder (DeckManager auto-loads from here)
3. Set `cardName`, `artwork`, `description`, `staminaCost`, `category`, `targetType`
4. Add effects to `effects` list (EffectType + value pairs)
5. **No code changes needed** - DeckManager auto-includes unique cards

### Debugging Turn System
- `TurnManager` has `enableDebugLogs` inspector toggle
- Check singleton integrity: "DUPLICATE INSTANCE DETECTED!" warnings mean multiple TurnManager objects exist
- EndTurnButton wired programmatically in `Start()` - inspector bindings often fail in Unity

### Scene Transition Debugging
- Add logs in `GameSession.SetReturnPosition()` / `SetBattleTriggerPosition()`
- Check `LevelOneReturnManager.Start()` for spawn position calculation
- `BattleManager.HandleBattleStateChanged()` drives return to LevelOne

## Unity-Specific Conventions

### Input System
- Uses Unity's **new Input System** (not legacy Input Manager)
- `PlayerController` uses InputActionReferences for movement/look/interact
- Battle system uses `uiClickAction`/`uiCancelAction` InputActionReferences in TargetingSystem
- Enable/disable actions programmatically, not via inspector toggles

### Component Initialization Order
**Critical pattern**: When instantiating UI cards:
```csharp
GameObject card = Instantiate(prefab, parent);
card.SetActive(false);  // Prevent OnEnable firing
card.GetComponent<CardInstance>().SetData(cardData);  // Set data first
card.SetActive(true);  // Now OnEnable has valid data
```

### Resources Loading
- Cards: `Resources.LoadAll<Card>("Cards")` (recursive subfolder scan)
- Audio: AudioManager expects clips in Resources folders
- Avoid runtime Resources loading for performance - prefer inspector assignment

### Scene Management
- Always use `SceneManager.LoadScene(sceneName, LoadSceneMode.Single)` for proper lighting initialization
- GameManager does NOT persist via DontDestroyOnLoad (destroyed on scene change)
- Use static classes (GameSession, BattleState, BattleEvents) for cross-scene data

## Common Pitfalls

### 1. Button Click Not Firing
- Unity inspector button bindings break easily. TurnManager.Start() wires EndTurnButton programmatically:
```csharp
endTurnButton.onClick.RemoveAllListeners();  // Clear stale bindings
endTurnButton.onClick.AddListener(OnEndTurnButtonPressed);
```

### 2. CardDisplay Showing Empty Data
- Card data must be set BEFORE GameObject.SetActive(true)
- CardDisplay.OnEnable() runs before Start(), so data must exist at enable time

### 3. Targeting Not Working
- TargetingSystem requires both UI raycast (Canvas) and 3D colliders
- Check EventSystem exists in scene
- Enemy needs EnemyHealth component + "Enemy" tag + UI health bar for targeting

### 4. Player Re-Triggering Battle After Return
- LevelOneReturnManager spawns player at `BattleTriggerCenter - Vector3(0,0,3)` (3 units back)
- Battle triggers must be one-way or check `GameSession.EnemyDefeated` flag

### 5. Singleton Duplication
- Symptoms: "DUPLICATE INSTANCE DETECTED!" logs, event handlers firing twice
- Cause: Multiple GameObjects with same manager component
- Fix: Ensure only one exists in scene hierarchy

## Key Files Reference
- Battle flow: `TurnManager.cs`, `BattleManager.cs`, `BattleState.cs`
- Card system: `CardSystem.cs` (ScriptableObject defs), `CardInstance.cs` (runtime), `CardDisplay.cs` (UI), `CardMovement.cs` (interactions)
- Deck management: `DeckManager.cs`, `HandManager.cs`
- Scene state: `GameSession.cs` (static global state), `GameManager.cs` (scene transitions)
- Player control: `PlayerController.cs`, `PlayerMovementLock.cs`
- Health/resources: `PlayerHealth.cs`, `EnemyHealth.cs`, `PlayerStamina.cs`
- Targeting: `TargetingSystem.cs` (card target selection)

## Testing
- Play from `TitleScene` (not LevelOne) for proper initialization
- Battle trigger testing: Check BattleTrigger prefab has `OnTriggerEnter` calling `GameManager.Instance.StartBattle()`
- Card testing: Verify card appears in `Resources/Cards/` subfolders, check DeckManager logs on battle start
