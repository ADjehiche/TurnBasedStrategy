using UnityEngine;
using TMPro;

public class StaminaDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text staminaText;   

    void OnEnable()
    {
        UpdateStamina();
        PlayerStamina.InstanceChanged += UpdateStamina;   // subscribe
    }

    void OnDisable()
    {
        PlayerStamina.InstanceChanged -= UpdateStamina;   // unsubscribe
    }

    public void UpdateStamina()
    {
        if (staminaText == null || PlayerStamina.Instance == null) return;
        var s = PlayerStamina.Instance;
        
        // Show total stamina in green if temporary stamina is present
        if (s.temporaryStamina > 0)
        {
            int total = s.currentStamina + s.temporaryStamina;
            staminaText.text = $"<color=green>{total}</color>/{s.maxStamina}";
        }
        else
        {
            staminaText.text = $"{s.currentStamina}/{s.maxStamina}";
        }
    }
}