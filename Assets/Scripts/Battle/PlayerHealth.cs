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

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;

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
