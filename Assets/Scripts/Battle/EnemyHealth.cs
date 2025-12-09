using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHP = 20;
    public int currentHP = 20;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    [Header("Status Effects")]
    public int bleedStacks;
    public int weakenPercent;
    public int weakenTurns; // usually 0 or 1 turn so 0 end of effect so it can disappear

    [SerializeField] private EnemyStatusDisplay statusDisplay; 

    public event System.Action<int, int> OnHealthChanged; // (current, max)
    
    private SkeletonAudioController audioController;

    void Awake()
    {
        // Make sure this object has the "Enemy" tag for proper cleanup
        if (gameObject.tag != "Enemy")
        {
            Debug.LogWarning("EnemyHealth object should have the 'Enemy' tag for proper cleanup after battle");
            tag = "Enemy";
        }
        
        // Get audio controller if available
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
    }

    public void TakeDamage(int amount)
    {
        currentHP = Mathf.Max(currentHP - amount, 0);
        Debug.Log($"Enemy took {amount} damage. HP now {currentHP}");

        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0)
        {
            Debug.Log("Enemy died");
            
            // Play death sound if audio controller is available
            if (audioController != null)
            {
                audioController.PlayDeathSound();
            }
            
            BattleState.SetOver(true);
            Destroy(gameObject, 0.5f); // Small delay to let death sound play
        }
    }

    public void AddBleed(int amount)
    {
        if (amount <= 0) return;

        bleedStacks += amount;
        if (statusDisplay != null)
            statusDisplay.SetBleedTurns(bleedStacks);
    }

    public void AddPoison(int percent)
    {
        if (percent <= 0) return;

        // Poison in this design applies a 1-turn weaken effect
        weakenPercent = percent;
        weakenTurns   = 1;  // always 1 turn for now

        if (statusDisplay != null)
            statusDisplay.SetWeakenPercent(weakenPercent);
    }

    public void TickStatuses()
    {
        // Bleed: -1 HP per turn for each remaining stack (stack = turn)
        if (bleedStacks > 0)
        {
            TakeDamage(1);
            bleedStacks--;

            if (statusDisplay != null)
                statusDisplay.SetBleedTurns(bleedStacks);
        }

        // Weaken: lasts a fixed number of turns (usually 1). When it expires,
        // clear the percent and hide the icon.
        if (weakenTurns > 0)
        {
            weakenTurns--;

            if (weakenTurns <= 0)
            {
                weakenPercent = 0;

                if (statusDisplay != null)
                    statusDisplay.SetWeakenPercent(0);
            }
        }
    }

    public int GetWeakenedDamage(int baseDamage)
    {
        if (weakenPercent <= 0) return baseDamage;

        float factor = 1f - (weakenPercent / 100f);
        return Mathf.RoundToInt(baseDamage * factor);
    }
}