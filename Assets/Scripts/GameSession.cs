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
    }
}