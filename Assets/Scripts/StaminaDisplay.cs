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
        staminaText.text = $"{s.currentStamina}/{s.maxStamina}";
    }
}