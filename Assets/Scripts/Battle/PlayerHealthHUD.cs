using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthHUD : MonoBehaviour
{
    [Header("Health UI")]
    [SerializeField] private Slider hpSlider;            
    [SerializeField] private TMP_Text hpText;

    [Header("Block UI")]
    [SerializeField] private GameObject blockGroup;
    [SerializeField] private TMP_Text blockText;

    [Header("Target")]
    [SerializeField] private PlayerHealth target;

    void Awake()
    {
        if (target == null) 
        {
            target = PlayerHealth.Instance;
            if (target == null)
                target = Object.FindObjectOfType<PlayerHealth>();
        }
    }

    void Update()
    {
        if (target == null) return;

        // Update health bar + text
        if (hpSlider != null)
        {
            hpSlider.maxValue = target.MaxHealth;
            hpSlider.value = target.CurrentHealth;
        }

        if (hpText != null)
        {
            hpText.text = $"{target.CurrentHealth}/{target.MaxHealth}";
        }

        // Update block UI
        if (blockGroup != null)
        {
            if (target.CurrentBlock > 0)
            {
                blockGroup.SetActive(true);
                blockText.text = target.CurrentBlock.ToString();
            }
            else
            {
                blockGroup.SetActive(false);
            }
        }
    }
}