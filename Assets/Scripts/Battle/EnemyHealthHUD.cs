using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthHUD : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text hpText;         // drag EnemyHPText here
    [SerializeField] private Slider healthSlider;     // drag Health Slider here
    
    [Header("Target")]
    [SerializeField] private EnemyHealth target;      // drag your Enemy here (or auto-find)

    void Awake()
    {
        if (target == null)
        {
            target = Object.FindFirstObjectByType<EnemyHealth>();
            if (target != null)
            {
                Debug.Log($"[EnemyHealthHUD] Auto-found enemy: {target.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("[EnemyHealthHUD] No EnemyHealth target found in scene!");
            }
        }
        else
        {
            Debug.Log($"[EnemyHealthHUD] Target assigned in Inspector: {target.gameObject.name}");
        }
    }

    void OnEnable()
    {
        if (target != null)
        {
            target.OnHealthChanged += HandleChanged;
            Debug.Log($"[EnemyHealthHUD] Subscribed to {target.gameObject.name}'s OnHealthChanged event");
            
            // initial draw (covers case where Start already ran)
            UpdateHealthUI(target.currentHP, target.maxHP);
        }
        else
        {
            Debug.LogError("[EnemyHealthHUD] Cannot subscribe - target is NULL!");
        }
    }

    void OnDisable()
    {
        if (target != null)
        {
            target.OnHealthChanged -= HandleChanged;
            Debug.Log($"[EnemyHealthHUD] Unsubscribed from {target.gameObject.name}'s OnHealthChanged event");
        }
    }

    private void HandleChanged(int current, int max)
    {
        Debug.Log($"[EnemyHealthHUD] HandleChanged called: {current}/{max}");
        UpdateHealthUI(current, max);
    }

    private void UpdateHealthUI(int current, int max)
    {
        Debug.Log($"[EnemyHealthHUD] UpdateHealthUI: {current}/{max}");
        
        // Update text
        if (hpText != null)
        {
            hpText.text = $"{current}/{max}";
            Debug.Log($"[EnemyHealthHUD] Updated text to: {current}/{max}");
        }
        else
        {
            Debug.LogWarning("[EnemyHealthHUD] hpText is NULL! Assign HP Text in Inspector.");
        }

        // Update slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
            Debug.Log($"[EnemyHealthHUD] Updated slider to: {current}/{max}");
        }
        else
        {
            Debug.LogWarning("[EnemyHealthHUD] healthSlider is NULL! Assign Health Slider in Inspector.");
        }
    }
}