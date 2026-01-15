using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    public static PlayerStamina Instance { get; private set; }
    public static System.Action InstanceChanged;


    [Header("Stamina")]
    public int maxStamina = 10;
    public int currentStamina = 10;
    
    [Header("Temporary Stamina")]
    [Tooltip("Extra stamina that disappears at end of turn")]
    public int temporaryStamina = 0;

    public int TotalStamina => currentStamina + temporaryStamina;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InstanceChanged?.Invoke();
    }

    public bool CanAfford(int cost) => TotalStamina >= cost;

    public bool Spend(int cost)
    {
        if (!CanAfford(cost)) return false;
        
        // Spend temporary stamina first, then regular stamina
        if (temporaryStamina > 0)
        {
            int tempSpent = Mathf.Min(temporaryStamina, cost);
            temporaryStamina -= tempSpent;
            cost -= tempSpent;
            
            if (cost > 0)
            {
                currentStamina -= cost;
            }
            
            Debug.Log($"Stamina spent: {tempSpent} temp + {cost} regular. Now: {currentStamina} + {temporaryStamina} temp = {TotalStamina} total");
        }
        else
        {
            currentStamina -= cost;
            Debug.Log($"Stamina spent: {cost}. Now: {currentStamina}/{maxStamina}");
        }
        
        InstanceChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Add temporary stamina that lasts until end of turn
    /// </summary>
    public void AddTemporaryStamina(int amount)
    {
        if (amount <= 0) return;
        
        temporaryStamina += amount;
        Debug.Log($"[PlayerStamina] Gained {amount} temporary stamina. Total: {currentStamina} + {temporaryStamina} temp = {TotalStamina}");
        InstanceChanged?.Invoke();
    }

    /// <summary>
    /// Clear all temporary stamina (called at end of turn)
    /// </summary>
    public void ClearTemporaryStamina()
    {
        if (temporaryStamina > 0)
        {
            Debug.Log($"[PlayerStamina] Clearing {temporaryStamina} temporary stamina");
            temporaryStamina = 0;
            InstanceChanged?.Invoke();
        }
    }

    public void Refill()
    {
        currentStamina = maxStamina;
        // Note: Temporary stamina is NOT cleared here - it's cleared at turn end
        InstanceChanged?.Invoke();
    }
}