using UnityEngine;
using TMPro;

public class EnemyHealthHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text hpText;         // drag EnemyHPText here
    [SerializeField] private EnemyHealth target;      // drag your Enemy here (or auto-find)

    void Awake()
    {
        if (target == null) target = Object.FindObjectOfType<EnemyHealth>(); // simple 1-enemy fallback
    }

    void OnEnable()
    {
        if (target != null) target.OnHealthChanged += HandleChanged;

        // initial draw (covers case where Start already ran)
        if (target != null && hpText != null)
            hpText.text = $"{target.currentHP}/{target.maxHP}";
    }

    void OnDisable()
    {
        if (target != null) target.OnHealthChanged -= HandleChanged;
    }

    private void HandleChanged(int current, int max)
    {
        if (hpText != null)
            hpText.text = $"{current}/{max}";
    }
}