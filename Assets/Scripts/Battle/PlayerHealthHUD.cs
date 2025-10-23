using UnityEngine;
using TMPro;

public class PlayerHealthHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text hpText;         
    [SerializeField] private PlayerHealth target;     

    void Awake()
    {
        if (target == null) target = PlayerHealth.Instance;
        if (target == null) target = FindObjectOfType<PlayerHealth>();
    }

    void Update()
    {
        // Continuously update the text display
        if (hpText != null && target != null)
        {
            hpText.text = $"Player HP {target.CurrentHealth}/{target.MaxHealth}";
        }
    }
}
