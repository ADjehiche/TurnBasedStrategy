using UnityEngine;
using TMPro;

public class EnemyStatusDisplay : MonoBehaviour
{
    [Header("Bleed")]
    [SerializeField] private GameObject bleedRoot;
    [SerializeField] private TMP_Text bleedText;

    [Header("Weaken")]
    [SerializeField] private GameObject weakenRoot;
    [SerializeField] private TMP_Text weakenText;

    // Bleed: turns remaining
    public void SetBleedTurns(int turnsLeft)
    {
        if (bleedRoot != null)
            bleedRoot.SetActive(turnsLeft > 0);

        if (bleedText != null && turnsLeft > 0)
            bleedText.text = turnsLeft.ToString();
    }

    // Weaken: damage reduction percent (always 1 turn)
    public void SetWeakenPercent(int percent)
    {
        if (weakenRoot != null)
            weakenRoot.SetActive(percent > 0);

        if (weakenText != null && percent > 0)
            weakenText.text = $"-{percent}%";
    }
}