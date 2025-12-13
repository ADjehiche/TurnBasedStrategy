using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Needed for scene loading

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int currentHealth;

    [Header("Block Settings")]
    [SerializeField] private int currentBlock;
    [SerializeField] private GameObject blockGroup; // BlockGroup UI (shield + text)
    [SerializeField] private Text blockText;        // Block number text

    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentBlock => currentBlock;

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
        currentBlock  = 0;
        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        int remaining = amount;

        // 1) Block absorbs damage first
        if (currentBlock > 0)
        {
            int absorbed = Mathf.Min(currentBlock, remaining);
            currentBlock -= absorbed;
            remaining    -= absorbed;
        }

        // 2) Any leftover hits HP
        if (remaining > 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - remaining);
        }

        Debug.Log($"[PlayerHealth] Player took {amount} damage. HP: {currentHealth}/{maxHealth}, Block: {currentBlock}");

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
        Debug.Log($"[PlayerHealth] Player healed {amount}. Health: {currentHealth}/{maxHealth}");

        UpdateHealthUI();
    }

    // NEW: gain block (from block cards)
    public void GainBlock(int amount)
    {
        if (amount <= 0) return;

        currentBlock += amount;
        Debug.Log($"[PlayerHealth] Player gained {amount} block. Block: {currentBlock}");

        UpdateHealthUI();
    }

    // NEW: reset block (e.g. at start of player turn)
    public void ResetBlock()
    {
        currentBlock = 0;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        // Health bar + text (existing behaviour)
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value    = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }

        // Block shield UI (optional)
        if (blockGroup != null)
            blockGroup.SetActive(currentBlock > 0);

        if (blockText != null)
            blockText.text = currentBlock.ToString("00");
    }

    private void OnPlayerDeath()
    {
        Debug.Log("[PlayerHealth] Player defeated! Loading death screen...");
        
        BattleState.SetOver(true);
        
        // Mark that we're respawning from death (for when Try Again is pressed)
        GameSession.IsRespawning = true;
        
        // Load death screen (Try Again button will reload to checkpoint)
        SceneManager.LoadScene("DeathScene");

        // Disable turn manager
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.enabled = false;
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        currentBlock  = 0;
        UpdateHealthUI();
    }
}