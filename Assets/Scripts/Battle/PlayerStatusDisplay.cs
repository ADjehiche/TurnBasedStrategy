using UnityEngine;
using TMPro;

/// <summary>
/// Displays player status effects (Bleed and Weakness only)
/// Similar to EnemyStatusDisplay but for the player
/// </summary>
public class PlayerStatusDisplay : MonoBehaviour
{
    [Header("Bleed")]
    [SerializeField] private GameObject bleedRoot;
    [SerializeField] private TMP_Text bleedText;

    [Header("Weaken")]
    [SerializeField] private GameObject weakenRoot;
    [SerializeField] private TMP_Text weakenText;

    // Bleed: turns remaining (stacks of damage per turn)
    public void SetBleedTurns(int turnsLeft)
    {
        if (bleedRoot != null)
            bleedRoot.SetActive(turnsLeft > 0);

        if (bleedText != null && turnsLeft > 0)
            bleedText.text = turnsLeft.ToString();
    }

    // Weaken: damage reduction percent (how much less damage player deals)
    public void SetWeakenPercent(int percent)
    {
        if (weakenRoot != null)
            weakenRoot.SetActive(percent > 0);

        if (weakenText != null && percent > 0)
            weakenText.text = $"-{percent}%";
    }

    // Clear all status displays
    public void ClearAll()
    {
        SetBleedTurns(0);
        SetWeakenPercent(0);
    }
}
