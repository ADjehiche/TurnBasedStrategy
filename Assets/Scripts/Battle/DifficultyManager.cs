using UnityEngine;

/// <summary>
/// Manages difficulty settings for battles.
/// Uses static global difficulty that persists across all scenes.
/// Can be set at game start and will affect all battles.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    // GLOBAL difficulty setting - persists across scenes
    private static DifficultyMode globalDifficulty = DifficultyMode.Normal;
    private static bool hasBeenSet = false; // Track if difficulty was explicitly set

    [Header("Difficulty Settings")]
    [SerializeField] private DifficultyMode currentDifficulty = DifficultyMode.Normal;
    [SerializeField] private bool persistAcrossScenes = true; // DontDestroyOnLoad

    /// <summary>
    /// Difficulty modes that affect enemy behavior and rewards
    /// </summary>
    public enum DifficultyMode
    {
        Easy,       // Enemies deal less damage, player gets more rewards
        Normal,     // Balanced gameplay
        Hard,       // Enemies deal more damage, have more HP
        Nightmare   // Boss-level difficulty for all enemies
    }

    // Difficulty multipliers
    public DifficultySettings CurrentSettings => GetDifficultySettings(GetDifficulty());

    private void Awake()
    {
        // Singleton with DontDestroyOnLoad support
        if (Instance == null)
        {
            Instance = this;
            
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            // If global difficulty has been set elsewhere, use it
            if (hasBeenSet)
            {
                currentDifficulty = globalDifficulty;
            }
            else
            {
                // First time - set global from inspector value
                globalDifficulty = currentDifficulty;
                hasBeenSet = true;
            }
            
            Debug.Log($"[DifficultyManager] Initialized with difficulty: {globalDifficulty}");
        }
        else
        {
            Debug.LogWarning("[DifficultyManager] Duplicate instance destroyed.");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Get current difficulty mode (uses global static setting)
    /// </summary>
    public DifficultyMode GetDifficulty()
    {
        return globalDifficulty;
    }

    /// <summary>
    /// Set difficulty mode globally (affects all current and future battles)
    /// </summary>
    public void SetDifficulty(DifficultyMode difficulty)
    {
        globalDifficulty = difficulty;
        currentDifficulty = difficulty;
        hasBeenSet = true;
        Debug.Log($"[DifficultyManager] Global difficulty set to: {difficulty}");
    }

    /// <summary>
    /// Set difficulty by name (useful for UI buttons)
    /// </summary>
    public void SetDifficultyByName(string difficultyName)
    {
        if (System.Enum.TryParse<DifficultyMode>(difficultyName, true, out DifficultyMode mode))
        {
            SetDifficulty(mode);
        }
        else
        {
            Debug.LogError($"[DifficultyManager] Invalid difficulty name: {difficultyName}");
        }
    }

    /// <summary>
    /// Get difficulty settings for a specific mode
    /// </summary>
    public DifficultySettings GetDifficultySettings(DifficultyMode mode)
    {
        switch (mode)
        {
            case DifficultyMode.Easy:
                return new DifficultySettings
                {
                    enemyDamageMultiplier = 0.75f,
                    enemyHealthMultiplier = 0.8f,
                    playerRewardMultiplier = 1.5f,
                    bossHealThreshold = 15,
                    bossHealAmount = 8
                };

            case DifficultyMode.Normal:
                return new DifficultySettings
                {
                    enemyDamageMultiplier = 1f,
                    enemyHealthMultiplier = 1f,
                    playerRewardMultiplier = 1f,
                    bossHealThreshold = 10,
                    bossHealAmount = 5
                };

            case DifficultyMode.Hard:
                return new DifficultySettings
                {
                    enemyDamageMultiplier = 1.25f,
                    enemyHealthMultiplier = 1.5f,
                    playerRewardMultiplier = 1.25f,
                    bossHealThreshold = 8,
                    bossHealAmount = 10
                };

            case DifficultyMode.Nightmare:
                return new DifficultySettings
                {
                    enemyDamageMultiplier = 1.5f,
                    enemyHealthMultiplier = 2f,
                    playerRewardMultiplier = 2f,
                    bossHealThreshold = 5,
                    bossHealAmount = 15
                };

            default:
                return GetDifficultySettings(DifficultyMode.Normal);
        }
    }

    /// <summary>
    /// Apply difficulty multiplier to enemy damage
    /// </summary>
    public int GetScaledDamage(int baseDamage)
    {
        float multiplier = CurrentSettings.enemyDamageMultiplier;
        return Mathf.RoundToInt(baseDamage * multiplier);
    }

    /// <summary>
    /// Apply difficulty multiplier to enemy health
    /// </summary>
    public int GetScaledHealth(int baseHealth)
    {
        float multiplier = CurrentSettings.enemyHealthMultiplier;
        return Mathf.RoundToInt(baseHealth * multiplier);
    }

    /// <summary>
    /// Check if current difficulty is at least the specified level
    /// </summary>
    public bool IsAtLeastDifficulty(DifficultyMode minDifficulty)
    {
        return currentDifficulty >= minDifficulty;
    }
}

/// <summary>
/// Settings for each difficulty mode
/// </summary>
[System.Serializable]
public class DifficultySettings
{
    public float enemyDamageMultiplier = 1f;
    public float enemyHealthMultiplier = 1f;
    public float playerRewardMultiplier = 1f;
    public int bossHealThreshold = 10;
    public int bossHealAmount = 5;
}
