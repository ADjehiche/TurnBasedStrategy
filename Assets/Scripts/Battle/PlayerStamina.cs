using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    public static PlayerStamina Instance { get; private set; }
    public static System.Action InstanceChanged;


    [Header("Stamina")]
    public int maxStamina = 10;
    public int currentStamina = 10;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InstanceChanged?.Invoke();
    }

    public bool CanAfford(int cost) => currentStamina >= cost;

    public bool Spend(int cost)
    {
        if (!CanAfford(cost)) return false;
        currentStamina -= cost;
        Debug.Log($"Stamina spent: {cost}. Now: {currentStamina}/{maxStamina}");
        InstanceChanged?.Invoke();
        return true;
    }

    public void Refill()
    {
        currentStamina = maxStamina;
        InstanceChanged?.Invoke();
    }
}