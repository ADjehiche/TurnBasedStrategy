using UnityEngine;

public static class GameSession
{
    // Enemy state
    public static bool EnemyDefeated;

    // Player position state
    public static bool HasReturnPosition;
    public static Vector3 ReturnPosition;
    public static Vector3 BattleTriggerCenter; // Where player spawns after battle
    public static string BattleSceneName = "Battle_Template"; // Which battle scene to load

    // World state preservation
    public static bool DoorOpened;           // Track if first door was opened
    public static bool OriginalKeyCollected; // Track if original key was picked up
    public static bool Symbol1Activated; // Track if first cultist symbol was activated
    public static bool Symbol2Activated; // Track if second cultist symbol was activated
    
    // Helper property to check if both symbols are activated
    public static bool BothSymbolsActivated => Symbol1Activated && Symbol2Activated;
    
    // Companion state
    public static bool CompanionActive;      // Track if companion is following player
    
    // Objective system persistence (so objectives don't reset after battle)
    public static bool ObjectivesStarted;       // Track if objectives have been started
    public static int CurrentObjectiveIndex;    // Track current objective index
    public static bool HasFoundKey;             // Track key pickup completion
    public static bool HasEscapedCell;          // Track cell escape completion
    public static bool HasDefeatedSkeleton;     // Track skeleton defeat completion
    public static bool HasExploredDungeon;      // Track dungeon exploration completion
    public static bool HasEscapedDungeon;       // Track final escape completion
    
    // Level Two objective system persistence
    public static bool LevelTwoObjectivesStarted;       // Track if Level Two objectives have been started
    public static int CurrentLevelTwoObjectiveIndex;    // Track current Level Two objective index
    public static bool HasExploredArchive;              // Track archive exploration completion
    public static bool HasExploredTunnel;               // Track tunnel exploration completion
    public static bool HasExploredMaze;                 // Track maze exploration completion
    public static bool HasReturnedToArchive;            // Track return to archive completion
    
    // Fragment collection (glyphs for boss door)
    public static bool HasCollectedRedFragment;         // Red (Rage) fragment from Combat Wing
    public static bool HasCollectedBlueFragment;        // Blue (Logic) fragment from Maze
    public static bool HasCollectedPurpleFragment;      // Purple (Personality) fragment from Boss
    
    // Helper to count collected fragments (boss door requires Red + Blue)
    public static int CollectedFragmentCount => 
        (HasCollectedRedFragment ? 1 : 0) + 
        (HasCollectedBlueFragment ? 1 : 0);
    
    public static bool CanUnlockBossDoor => CollectedFragmentCount >= 2;
    
    // Caption state persistence (so captions don't repeat after battle)
    public static bool HasShownStartInstruction;
    public static bool HasShownKeyPickup;
    public static bool HasShownDoorOpen;
    public static bool HasShownEnemySpotted;
    
    // Checkpoint system (for respawning after death)
    public static bool HasCheckpoint;
    public static Vector3 CheckpointPosition;
    public static Quaternion CheckpointRotation;
    public static bool IsRespawning; // Flag to indicate we're loading from death

    public static void SetReturnPosition(Vector3 pos)
    {
        HasReturnPosition = true;
        ReturnPosition = pos;
    }

    public static void SetBattleTriggerPosition(Vector3 center)
    {
        BattleTriggerCenter = center;
        Debug.Log($"[GameSession] Battle trigger center saved: {center}");
    }

    public static void SetBattleSceneName(string sceneName)
    {
        BattleSceneName = sceneName;
        Debug.Log($"[GameSession] Battle scene set to: {sceneName}");
    }
    
    public static void SaveCheckpoint(Vector3 pos, Quaternion rot)
    {
        HasCheckpoint = true;
        CheckpointPosition = pos;
        CheckpointRotation = rot;
        Debug.Log($"[GameSession] ✅ Checkpoint saved at {pos}, rotation {rot.eulerAngles}");
    }

    public static void Reset()
    {
        EnemyDefeated = false;
        HasReturnPosition = false;
        ReturnPosition = default;
        BattleTriggerCenter = default;
        BattleSceneName = "Battle_Template"; // Reset to default
        DoorOpened = false;
        OriginalKeyCollected = false;
        Symbol1Activated = false;
        Symbol2Activated = false;
        CompanionActive = false;
        
        // Reset caption states
        HasShownStartInstruction = false;
        HasShownKeyPickup = false;
        HasShownDoorOpen = false;
        HasShownEnemySpotted = false;
        
        // Reset checkpoint
        HasCheckpoint = false;
        CheckpointPosition = default;
        CheckpointRotation = default;
        IsRespawning = false;
        
        // Reset fragment collection
        HasCollectedRedFragment = false;
        HasCollectedBlueFragment = false;
        HasCollectedPurpleFragment = false;
    }
}