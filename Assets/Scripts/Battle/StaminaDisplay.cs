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
        
        // Show temporary stamina in green if present
        if (s.temporaryStamina > 0)
        {
            staminaText.text = $"{s.currentStamina}<color=green>+{s.temporaryStamina}</color>/{s.maxStamina}";
        }
        else
        {
            staminaText.text = $"{s.currentStamina}/{s.maxStamina}";
        }
    }
}