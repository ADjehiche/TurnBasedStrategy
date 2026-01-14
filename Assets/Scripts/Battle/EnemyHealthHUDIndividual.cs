using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual health HUD for a specific enemy.
/// Attach this to each enemy's health bar UI element.
/// Unlike EnemyHealthHUD, this doesn't auto-find - you must assign the target in Inspector.
/// </summary>
public class EnemyHealthHUDIndividual : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text hpText;         // The HP text for this specific enemy
    [SerializeField] private Slider healthSlider;     // The health slider for this specific enemy
    
    [Header("Target Enemy")]
    [SerializeField] private EnemyHealth target;      // MUST assign this specific enemy in Inspector

    void OnEnable()
    {
        if (target != null)
        {
            target.OnHealthChanged += HandleChanged;
            UpdateHealthUI(target.currentHP, target.maxHP);
            Debug.Log($"[EnemyHealthHUDIndividual] {gameObject.name} subscribed to {target.gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[EnemyHealthHUDIndividual] No target assigned for {gameObject.name}! Please assign in Inspector.");
        }
    }

    void OnDisable()
    {
        if (target != null)
        {
            target.OnHealthChanged -= HandleChanged;
            Debug.Log($"[EnemyHealthHUDIndividual] {gameObject.name} unsubscribed from {target.gameObject.name}");
        }
    }

    private void HandleChanged(int current, int max)
    {
        Debug.Log($"[EnemyHealthHUDIndividual] {gameObject.name} HandleChanged: {current}/{max}");
        UpdateHealthUI(current, max);
        
        // Optional: Hide this HUD when enemy dies
        if (current <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    private void UpdateHealthUI(int current, int max)
    {
        Debug.Log($"[EnemyHealthHUDIndividual] {gameObject.name} UpdateHealthUI: {current}/{max}");
        
        // Update text
        if (hpText != null)
        {
            hpText.text = $"{current}/{max}";
        }
        else
        {
            Debug.LogWarning($"[EnemyHealthHUDIndividual] {gameObject.name} hpText is NULL!");
        }

        // Update slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
        else
        {
            Debug.LogWarning($"[EnemyHealthHUDIndividual] {gameObject.name} healthSlider is NULL!");
        }
    }

    /// <summary>
    /// Manually set the target enemy (useful for runtime spawning)
    /// </summary>
    public void SetTarget(EnemyHealth enemy)
    {
        // Unsubscribe from old target
        if (target != null)
            target.OnHealthChanged -= HandleChanged;

        // Subscribe to new target
        target = enemy;
        if (target != null)
        {
            target.OnHealthChanged += HandleChanged;
            UpdateHealthUI(target.currentHP, target.maxHP);
        }
    }
}
