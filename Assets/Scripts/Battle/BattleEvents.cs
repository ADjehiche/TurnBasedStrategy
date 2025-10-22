using System;
using UnityEngine;

public static class BattleEvents
{
    // Fired when a card is successfully played & resolved
    public static event Action<GameObject> OnCardResolved;

    public static void RaiseCardResolved(GameObject cardGO)
        => OnCardResolved?.Invoke(cardGO);
}