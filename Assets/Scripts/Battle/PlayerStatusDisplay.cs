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

    private void Start()
    {
        // Initialize all status icons as hidden
        ClearAll();
    }

    // Bleed: turns remaining (stacks of damage per turn)
    public void SetBleedTurns(int turnsLeft)
    {
        Debug.Log($"[PlayerStatusDisplay] 🩸 SetBleedTurns({turnsLeft}) called. BleedRoot active: {bleedRoot != null}, BleedText exists: {bleedText != null}");
        
        if (bleedRoot != null)
        {
            bleedRoot.SetActive(turnsLeft > 0);
            Debug.Log($"[PlayerStatusDisplay] BleedRoot.SetActive({turnsLeft > 0})");
        }

        if (bleedText != null)
        {
            if (turnsLeft > 0)
            {
                bleedText.text = turnsLeft.ToString();
                Debug.Log($"[PlayerStatusDisplay] BleedText set to: \"{turnsLeft}\"");
            }
            else
            {
                bleedText.text = ""; // Clear text when hidden
                Debug.Log($"[PlayerStatusDisplay] BleedText cleared");
            }
        }
    }

    // Weaken: damage reduction percent (how much less damage player deals)
    public void SetWeakenPercent(int percent)
    {
        Debug.Log($"[PlayerStatusDisplay] 💀 SetWeakenPercent({percent}) called. WeakenRoot active: {weakenRoot != null}, WeakenText exists: {weakenText != null}");
        
        if (weakenRoot != null)
        {
            weakenRoot.SetActive(percent > 0);
            Debug.Log($"[PlayerStatusDisplay] WeakenRoot.SetActive({percent > 0})");
        }

        if (weakenText != null)
        {
            if (percent > 0)
            {
                weakenText.text = $"-{percent}%";
                Debug.Log($"[PlayerStatusDisplay] WeakenText set to: \"-{percent}%\"");
            }
            else
            {
                weakenText.text = ""; // Clear text when hidden
                Debug.Log($"[PlayerStatusDisplay] WeakenText cleared");
            }
        }
    }

    // Clear all status displays
    public void ClearAll()
    {
        SetBleedTurns(0);
        SetWeakenPercent(0);
    }
}
