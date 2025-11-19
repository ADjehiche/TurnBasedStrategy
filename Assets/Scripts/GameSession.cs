using UnityEngine;

public static class GameSession
{
    public static bool EnemyDefeated;

    public static bool HasReturnPosition;
    public static Vector3 ReturnPosition;

    public static void SetReturnPosition(Vector3 pos)
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