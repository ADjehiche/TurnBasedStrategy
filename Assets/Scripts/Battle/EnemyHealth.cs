using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 20;
    public int currentHP;

    [Header("Status Effects")]
    public int bleedStacks;
    public int weakenPercent;
    public int weakenTurns;

    [Header("UI")]
    [SerializeField] private RectTransform enemyDamagePopupAnchor;
    [SerializeField] private EnemyStatusDisplay statusDisplay;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    public event System.Action<int, int> OnHealthChanged;

    // ✅ For switched damage popups
    public RectTransform GetDamagePopupAnchor() => enemyDamagePopupAnchor;

    private SkeletonAudioController audioController;

    void Awake()
    {
        if (gameObject.tag != "Enemy")
            tag = "Enemy";

        audioController = GetComponent<SkeletonAudioController>();
    }

    void Start()
    {
        currentHP = maxHP;
        Debug.Log($"[EnemyHealth] {gameObject.name} initialized with HP: {currentHP}/{maxHP}");
        
        OnHealthChanged?.Invoke(currentHP, maxHP);
        Debug.Log($"[EnemyHealth] OnHealthChanged event invoked. Subscribers: {(OnHealthChanged?.GetInvocationList().Length ?? 0)}");

        if (statusDisplay != null)
        {
            statusDisplay.SetBleedTurns(bleedStacks);
            statusDisplay.SetWeakenPercent(weakenPercent);
        }

        // Fallback: try to find the anchor if it's not assigned
        if (enemyDamagePopupAnchor == null)
        {
            var t = GameObject.Find("EnemyDamagePopupAnchor");
            if (t != null) enemyDamagePopupAnchor = t.GetComponent<RectTransform>();
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        int oldHP = currentHP;
        currentHP = Mathf.Max(0, currentHP - amount);
        Debug.Log($"[EnemyHealth] {gameObject.name} took {amount} damage. HP: {oldHP} → {currentHP}/{maxHP}");

        OnHealthChanged?.Invoke(currentHP, maxHP);
        Debug.Log($"[EnemyHealth] OnHealthChanged event invoked. Subscribers: {(OnHealthChanged?.GetInvocationList().Length ?? 0)}");

        // 🔥 SWITCHED POPUP - Shows damage at PLAYER's position (the attacker)
        if (BattleAnimator.Instance != null && PlayerHealth.Instance != null)
        {
            // Show popup at PLAYER's anchor (who is attacking the enemy)
            RectTransform playerAnchor = PlayerHealth.Instance.GetDamagePopupAnchor();
            if (playerAnchor != null)
            {
                BattleAnimator.Instance.ShowDamagePopup(amount, playerAnchor);
            }
            else
            {
                Debug.LogWarning("[EnemyHealth] Player's damage popup anchor is NULL.");
            }
        }
        else
        {
            Debug.LogWarning("[EnemyHealth] BattleAnimator.Instance or PlayerHealth.Instance is NULL.");
        }

        if (currentHP <= 0)
        {
            if (audioController != null)
                audioController.PlayDeathSound();

            // Don't end battle here - let EnemyManager check if ALL enemies are dead
            Debug.Log($"[EnemyHealth] {gameObject.name} defeated!");
            
            // Notify EnemyManager to check battle end condition
            if (EnemyManager.Instance != null)
            {
                StartCoroutine(EnemyManager.Instance.CheckBattleEndAfterDelay());
            }
            
            Destroy(gameObject, 0.5f);
        }
    }

    public void AddBleed(int amount)
    {
        if (amount <= 0) return;

        bleedStacks += amount;
        statusDisplay?.SetBleedTurns(bleedStacks);
    }

    public void AddPoison(int percent)
    {
        if (percent <= 0) return;

        weakenPercent = percent;
        weakenTurns = 1;

        statusDisplay?.SetWeakenPercent(weakenPercent);
    }

    /// <summary>
    /// Tick status effects at start of turn.
    /// NEW: Bleed countdown system - takes N damage, then decreases by 1.
    /// </summary>
    public void TickStatuses()
    {
        // Bleed countdown: Deal damage equal to current stacks, then decrease
        if (bleedStacks > 0)
        {
            int bleedDamage = bleedStacks; // Take damage equal to current bleed value
            Debug.Log($"[EnemyHealth] {gameObject.name} takes {bleedDamage} bleed damage (Bleed {bleedStacks})");
            TakeDamage(bleedDamage);
            
            bleedStacks--; // Decrease bleed counter by 1
            statusDisplay?.SetBleedTurns(bleedStacks);
            
            if (bleedStacks == 0)
            {
                Debug.Log($"[EnemyHealth] {gameObject.name} bleed expired");
            }
        }

        // Weaken countdown
        if (weakenTurns > 0)
        {
            weakenTurns--;
            if (weakenTurns <= 0)
            {
                weakenPercent = 0;
                statusDisplay?.SetWeakenPercent(0);
                Debug.Log($"[EnemyHealth] {gameObject.name} weakness expired");
            }
        }
    }

    public int GetWeakenedDamage(int baseDamage)
    {
        if (weakenPercent <= 0) return baseDamage;
        return Mathf.RoundToInt(baseDamage * (1f - weakenPercent / 100f));
    }
}
