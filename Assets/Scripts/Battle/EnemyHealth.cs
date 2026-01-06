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
        OnHealthChanged?.Invoke(currentHP, maxHP);

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

        currentHP = Mathf.Max(0, currentHP - amount);
        Debug.Log($"Enemy took {amount} damage. HP: {currentHP}");

        OnHealthChanged?.Invoke(currentHP, maxHP);

        // 🔥 POPUP + BUMP
        if (BattleAnimator.Instance != null)
        {
            // popup
            if (enemyDamagePopupAnchor != null)
                BattleAnimator.Instance.ShowDamagePopup(amount, enemyDamagePopupAnchor);
            else
                Debug.LogWarning("[EnemyHealth] enemyDamagePopupAnchor is NULL (assign EnemyDamagePopupAnchor in Inspector).");

            // bump (enemy gets pushed a bit to the RIGHT or LEFT depending on your setup)
            // If enemy is on right side, bump left looks better:
            BattleAnimator.Instance.Bump(transform, Vector3.left);
        }
        else
        {
            Debug.LogWarning("[EnemyHealth] BattleAnimator.Instance is NULL (make sure BattleAnimator object exists in scene).");
        }

        if (currentHP <= 0)
        {
            if (audioController != null)
                audioController.PlayDeathSound();

            BattleState.SetOver(true);
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

    public void TickStatuses()
    {
        if (bleedStacks > 0)
        {
            TakeDamage(1);
            bleedStacks--;
            statusDisplay?.SetBleedTurns(bleedStacks);
        }

        if (weakenTurns > 0)
        {
            weakenTurns--;
            if (weakenTurns <= 0)
            {
                weakenPercent = 0;
                statusDisplay?.SetWeakenPercent(0);
            }
        }
    }

    public int GetWeakenedDamage(int baseDamage)
    {
        if (weakenPercent <= 0) return baseDamage;
        return Mathf.RoundToInt(baseDamage * (1f - weakenPercent / 100f));
    }
}
