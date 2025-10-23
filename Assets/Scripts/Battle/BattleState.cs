using System;

public static class BattleState
{
    public static bool IsOver { get; private set; } = false;

    public static event Action<bool> OnBattleOverChanged; // arg = IsOver

    public static void SetOver(bool value)
    {
        if (IsOver == value) return;
        IsOver = value;
        OnBattleOverChanged?.Invoke(IsOver);
    }

    public static void Reset()
    {
        SetOver(false);
    }
}