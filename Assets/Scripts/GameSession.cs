using UnityEngine;

public static class GameSession
{
    // Enemy state
    public static bool EnemyDefeated;

    // Player position state
    public static bool HasReturnPosition;
    public static Vector3 ReturnPosition;
    public static Vector3 BattleTriggerCenter; // Where player spawns after battle

    // World state preservation
    public static bool DoorOpened;           // Track if first door was opened
    public static bool OriginalKeyCollected; // Track if original key was picked up
    
    // Companion state
    public static bool CompanionActive;      // Track if companion is following player
    
    // Caption state persistence (so captions don't repeat after battle)
    public static bool HasShownStartInstruction;
    public static bool HasShownKeyPickup;
    public static bool HasShownDoorOpen;
    public static bool HasShownEnemySpotted;

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

    public static void Reset()
    {
        EnemyDefeated = false;
        HasReturnPosition = false;
        ReturnPosition = default;
        BattleTriggerCenter = default;
        DoorOpened = false;
        OriginalKeyCollected = false;
        CompanionActive = false;
        
        // Reset caption states
        HasShownStartInstruction = false;
        HasShownKeyPickup = false;
        HasShownDoorOpen = false;
        HasShownEnemySpotted = false;
    }
}