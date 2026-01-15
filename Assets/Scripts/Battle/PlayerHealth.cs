using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Health")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int currentHealth;

    [Header("Block")]
    [SerializeField] private int currentBlock;
    [SerializeField] private GameObject blockGroup;
    [SerializeField] private Text blockText;

    [Header("Status Effects")]
    public int bleedStacks;
    public int weakenPercent;
    public int weakenTurns;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;
    [SerializeField] private PlayerStatusDisplay statusDisplay;

    [Header("Damage Popup")]
    [SerializeField] private RectTransform playerDamagePopupAnchor;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    // ✅ IMPORTANT: your HUD needs this
    public int CurrentBlock => currentBlock;

    // ✅ For switched damage popups
    public RectTransform GetDamagePopupAnchor() => playerDamagePopupAnchor;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentBlock = 0;
        UpdateHealthUI();

        // Fallback: try to find the anchor if it's not assigned
        if (playerDamagePopupAnchor == null)
        {
            var t = GameObject.Find("PlayerDamagePopupAnchor");
            if (t != null) playerDamagePopupAnchor = t.GetComponent<RectTransform>();
        }
    }

    public void TakeDamage(int amount, EnemyHealth attacker = null)
    {
        if (amount <= 0) return;

        // Check for Dodge (completely avoids damage)
        if (PlayerStatusEffects.Instance != null && PlayerStatusEffects.Instance.TryDodgeAttack())
        {
            Debug.Log($"[PlayerHealth] Player DODGED the attack! No damage taken.");
            
            // Show "DODGE!" popup
            if (BattleAnimator.Instance != null && playerDamagePopupAnchor != null)
            {
                // You could create a special dodge popup method in BattleAnimator
                // For now, show 0 damage
                BattleAnimator.Instance.ShowDamagePopup(0, playerDamagePopupAnchor);
            }
            return; // No damage taken
        }

        // Check for Reflect (reflects damage back to attacker)
        if (PlayerStatusEffects.Instance != null && attacker != null)
        {
            PlayerStatusEffects.Instance.TryReflectDamage(amount, attacker);
        }

        // NOTE: Weakness is applied to OUTGOING damage (when player attacks), not incoming damage
        // Enemy weakness reduces enemy's outgoing damage in EnemyManager/BossAI

        int remaining = amount;

        // Block absorbs first
        if (currentBlock > 0)
        {
            int absorbed = Mathf.Min(currentBlock, remaining);
            currentBlock -= absorbed;
            remaining -= absorbed;
        }

        // leftover hits HP
        if (remaining > 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - remaining);
        }

        Debug.Log($"[PlayerHealth] Player took {amount} damage. HP: {currentHealth}/{maxHealth}, Block: {currentBlock}");

        // 🔥 SWITCHED POPUP - Shows damage at ENEMY's position (the attacker)
        if (BattleAnimator.Instance != null)
        {
            if (attacker != null)
            {
                // Show popup at ENEMY's anchor (who is attacking the player)
                RectTransform enemyAnchor = attacker.GetDamagePopupAnchor();
                if (enemyAnchor != null)
                {
                    BattleAnimator.Instance.ShowDamagePopup(amount, enemyAnchor);
                }
                else
                {
                    Debug.LogWarning("[PlayerHealth] Enemy attacker's damage popup anchor is NULL.");
                }
            }
            else
            {
                // Fallback: If no attacker specified, use player's own anchor (backward compatibility)
                if (playerDamagePopupAnchor != null)
                {
                    BattleAnimator.Instance.ShowDamagePopup(amount, playerDamagePopupAnchor);
                }
                else
                {
                    Debug.LogWarning("[PlayerHealth] playerDamagePopupAnchor is NULL (assign PlayerDamagePopupAnchor in Inspector).");
                }
            }
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] BattleAnimator.Instance is NULL.");
        }

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            OnPlayerDeath();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"[PlayerHealth] Player healed {amount}. HP: {currentHealth}/{maxHealth}");

        UpdateHealthUI();
    }

    public void GainBlock(int amount)
    {
        if (amount <= 0) return;

        currentBlock += amount;
        Debug.Log($"[PlayerHealth] Player gained {amount} block. Block: {currentBlock}");

        UpdateHealthUI();
    }

    public void ResetBlock()
    {
        currentBlock = 0;
        UpdateHealthUI();
    }

    // ✅ IMPORTANT: LevelOneManager needs this
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        currentBlock = 0;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }

        if (blockGroup != null)
            blockGroup.SetActive(currentBlock > 0);

        if (blockText != null)
            blockText.text = currentBlock.ToString("00");
    }

    // Status Effect Methods
    public void AddBleed(int amount)
    {
        bleedStacks += amount;
        
        Debug.Log($"[PlayerHealth] 🩸 PLAYER BLEED ADDED! Amount: {amount}, Total stacks: {bleedStacks}");
        
        if (statusDisplay != null)
        {
            Debug.Log($"[PlayerHealth] ✅ Calling SetBleedTurns({bleedStacks}) on {statusDisplay.gameObject.name}");
            statusDisplay.SetBleedTurns(bleedStacks);
        }
        else
        {
            Debug.LogWarning($"[PlayerHealth] ⚠️ Status display is NULL! Bleed not shown in UI.");
        }
    }

    public void AddWeaken(int percent, int turns)
    {
        weakenPercent = percent;
        weakenTurns = turns;
        
        Debug.Log($"[PlayerHealth] 💀 PLAYER WEAKNESS ADDED! Percent: {percent}%, Turns: {turns}");
        
        if (statusDisplay != null)
        {
            Debug.Log($"[PlayerHealth] ✅ Calling SetWeakenPercent({percent}) on {statusDisplay.gameObject.name}");
            statusDisplay.SetWeakenPercent(weakenPercent);
        }
        else
        {
            Debug.LogWarning($"[PlayerHealth] ⚠️ Status display is NULL! Weakness not shown in UI.");
        }
    }

    /// <summary>
    /// Tick status effects at start of player turn.
    /// NEW: Bleed countdown system - takes N damage, then decreases by 1.
    /// </summary>
    public void TickStatuses()
    {
        // Bleed countdown: Deal damage equal to current stacks, then decrease
        if (bleedStacks > 0)
        {
            int bleedDamage = bleedStacks; // Take damage equal to current bleed value
            Debug.Log($"[PlayerHealth] Player takes {bleedDamage} bleed damage (Bleed {bleedStacks})");
            TakeDamage(bleedDamage, null);
            
            bleedStacks--; // Decrease bleed counter by 1
            
            // Update bleed display
            if (statusDisplay != null)
            {
                statusDisplay.SetBleedTurns(bleedStacks);
            }
            
            if (bleedStacks == 0)
            {
                Debug.Log("[PlayerHealth] Player bleed expired");
            }
        }

        // Decrement weakness
        if (weakenTurns > 0)
        {
            weakenTurns--;
            if (weakenTurns <= 0)
            {
                weakenPercent = 0;
                if (statusDisplay != null)
                {
                    statusDisplay.SetWeakenPercent(0);
                }
                Debug.Log("[PlayerHealth] Weakness expired");
            }
            else
            {
                Debug.Log($"[PlayerHealth] Weakness: {weakenPercent}% for {weakenTurns} more turn(s)");
            }
        }
    }

    public void ClearStatusEffects()
    {
        bleedStacks = 0;
        weakenPercent = 0;
        weakenTurns = 0;
        if (statusDisplay != null)
        {
            statusDisplay.ClearAll();
        }
    }

    private void OnPlayerDeath()
    {
        Debug.Log("[PlayerHealth] Player defeated! Loading death screen...");

        BattleState.SetOver(true);
        GameSession.IsRespawning = true;

        SceneManager.LoadScene("DeathScene");

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.enabled = false;
        }
    }
}
