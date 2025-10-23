using UnityEngine;

public static class GameSession
{
    public static bool EnemyDefeated;

    // New: return position info
    public static bool HasReturnPosition;
    public static UnityEngine.Vector3 ReturnPosition;

    public static void SetReturnPosition(UnityEngine.Vector3 pos)
    {
        HasReturnPosition = true;
        ReturnPosition = pos;
    }

    public static void Reset()
    {
        EnemyDefeated = false;
        HasReturnPosition = false;
        ReturnPosition = default;
    }
}