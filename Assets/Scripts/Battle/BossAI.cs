using System.Collections;
using UnityEngine;

/// <summary>
/// Boss enemy AI with advanced mechanics:
/// - Can apply weakness or bleed to player
/// - Can add block to itself
/// - Has a chance to block + attack in same turn
/// - Auto-heals when health drops below 10
/// </summary>
public class BossAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyHealth enemyHealth;

    [Header("Attack Settings")]
    [SerializeField] private int minDamage = 3;
    [SerializeField] private int maxDamage = 8;
    [SerializeField] private float attackAnimationDelay = 0.5f;

    [Header("Status Effect Chances (0-100)")]
    [SerializeField] private int weakenChance = 30; // 30% chance to apply weakness
    [SerializeField] private int bleedChance = 25;  // 25% chance to apply bleed
    [SerializeField] private int weakenPercent = 25; // Reduces player damage by 25%
    [SerializeField] private int weakenDuration = 2; // Lasts 2 turns
    [SerializeField] private int bleedAmount = 3;    // 3 bleed stacks

    [Header("Block Settings")]
    [SerializeField] private int minBlock = 3;
    [SerializeField] private int maxBlock = 6;
    [SerializeField] private int blockOnlyChance = 30;     // 30% chance to only block
    [SerializeField] private int blockAndAttackChance = 15; // 15% chance to block AND attack

    [Header("Healing Settings")]
    [SerializeField] private int healThreshold = 10; // Heal when HP drops below this
    [SerializeField] private int healAmount = 5;
    private bool hasHealed = false; // Only heal once per battle - MUST be private to prevent reset

    [Header("Audio")]
    [SerializeField] private string attackSoundName = "SkeletonScream";
    [SerializeField] private string slashSoundName = "SkeletonSlash";
    [SerializeField] private string blockSoundName = "Block"; // Add a block sound if you have one

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private bool isHealingInProgress = false; // Prevent multiple heal coroutines

    private void Awake()
    {
        // Auto-find EnemyHealth if not assigned
        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }
    }

    private void Start()
    {
        // Subscribe to health changes to detect healing threshold
        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged += OnHealthChanged;
            
            // Apply difficulty scaling
            if (DifficultyManager.Instance != null)
            {
                DifficultySettings settings = DifficultyManager.Instance.CurrentSettings;
                healThreshold = settings.bossHealThreshold;
                healAmount = settings.bossHealAmount;
                
                if (showDebugLogs)
                    Debug.Log($"[BossAI] Difficulty adjusted - Heal at {healThreshold} HP, heal for {healAmount}");
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged -= OnHealthChanged;
        }
    }

    /// <summary>
    /// Called when boss health changes - check if we need to heal
    /// </summary>
    private void OnHealthChanged(int currentHP, int maxHP)
    {
        // Strict check: Only heal once, only when below threshold, not already healing
        if (!hasHealed && !isHealingInProgress && currentHP > 0 && currentHP < healThreshold)
        {
            if (showDebugLogs)
                Debug.Log($"[BossAI] Heal triggered! HP: {currentHP}/{maxHP} < threshold: {healThreshold}");
            
            StartCoroutine(EmergencyHeal());
        }
    }

    /// <summary>
    /// Boss automatically heals when health is critically low
    /// </summary>
    private IEnumerator EmergencyHeal()
    {
        // Lock to prevent multiple simultaneous heals
        if (isHealingInProgress || hasHealed)
        {
            if (showDebugLogs)
                Debug.Log($"[BossAI] Heal blocked - Already healed: {hasHealed}, In progress: {isHealingInProgress}");
            yield break;
        }

        isHealingInProgress = true;
        hasHealed = true; // Mark as healed immediately to prevent re-triggering

        if (showDebugLogs)
            Debug.Log($"[BossAI] ❤️ EMERGENCY HEAL ACTIVATED! Boss HP critically low.");

        // Play heal sound/animation
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("Heal"); // You can add a heal sound
        }

        // Visual feedback
        if (BattleAnimator.Instance != null)
        {
            BattleAnimator.Instance.Bump(transform, Vector3.up);
        }

        yield return new WaitForSeconds(0.3f);

        // Heal the boss using the proper Heal method
        int oldHP = enemyHealth.CurrentHP;
        enemyHealth.Heal(healAmount);
        int newHP = enemyHealth.CurrentHP;
        
        if (showDebugLogs)
            Debug.Log($"[BossAI] ✅ Boss healed for {healAmount}. HP: {oldHP} → {newHP}/{enemyHealth.MaxHP}");

        isHealingInProgress = false;
    }

    /// <summary>
    /// Execute the boss's turn - called by EnemyManager
    /// </summary>
    public IEnumerator ExecuteTurn()
    {
        if (enemyHealth == null || enemyHealth.CurrentHP <= 0)
            yield break;

        // Check if player is invisible
        if (PlayerStatusEffects.Instance != null && PlayerStatusEffects.Instance.IsInvisible)
        {
            if (showDebugLogs)
                Debug.Log($"[BossAI] Cannot attack - Player is INVISIBLE!");
            yield break;
        }

        if (showDebugLogs)
            Debug.Log($"[BossAI] 👑 Boss turn starting...");

        // Determine action based on probabilities
        int actionRoll = Random.Range(0, 100);

        if (showDebugLogs)
            Debug.Log($"[BossAI] 🎲 Action roll: {actionRoll} (Block+Attack: 0-{blockAndAttackChance-1}, Block: {blockAndAttackChance}-{blockAndAttackChance+blockOnlyChance-1}, Attack: {blockAndAttackChance+blockOnlyChance}+)");

        if (actionRoll < blockAndAttackChance)
        {
            // Block AND attack - powerful combo!
            if (showDebugLogs)
                Debug.Log($"[BossAI] ⚔️🛡️ COMBO! Boss will block AND attack!");

            yield return PerformBlock();
            yield return new WaitForSeconds(0.5f);
            yield return PerformAttack();
        }
        else if (actionRoll < blockAndAttackChance + blockOnlyChance)
        {
            // Only block
            if (showDebugLogs)
                Debug.Log($"[BossAI] 🛡️ Boss is defending! (rolled {actionRoll})");

            yield return PerformBlock();
        }
        else
        {
            // Regular attack (with possible status effects)
            if (showDebugLogs)
                Debug.Log($"[BossAI] ⚔️ Boss is attacking! (rolled {actionRoll})");

            yield return PerformAttack();
        }

        if (showDebugLogs)
            Debug.Log($"[BossAI] Boss turn complete.");
    }

    /// <summary>
    /// Boss adds block to itself
    /// </summary>
    private IEnumerator PerformBlock()
    {
        int blockAmount = Random.Range(minBlock, maxBlock + 1);

        // Play block animation
        if (BattleAnimator.Instance != null)
        {
            BattleAnimator.Instance.Bump(transform, Vector3.back * 0.5f);
        }

        // Play block sound
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(blockSoundName))
        {
            AudioManager.Instance.Play(blockSoundName);
        }

        yield return new WaitForSeconds(0.3f);

        // Add block to boss
        if (enemyHealth != null)
        {
            enemyHealth.AddBlock(blockAmount);
            
            if (showDebugLogs)
                Debug.Log($"[BossAI] 🛡️ Boss gained {blockAmount} block! Total: {enemyHealth.CurrentBlock}");
        }
    }

    /// <summary>
    /// Boss attacks the player with possible status effects
    /// </summary>
    private IEnumerator PerformAttack()
    {
        // Play attack sound
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(attackSoundName))
        {
            AudioManager.Instance.Play(attackSoundName);
        }

        yield return new WaitForSeconds(attackAnimationDelay);

        // Play slash sound
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(slashSoundName))
        {
            AudioManager.Instance.Play(slashSoundName);
        }

        // Play attack animation
        if (BattleAnimator.Instance != null)
        {
            BattleAnimator.Instance.Bump(transform, Vector3.left);
        }

        yield return new WaitForSeconds(0.2f);

        // Calculate damage
        int damage = Random.Range(minDamage, maxDamage + 1);

        // Apply difficulty scaling
        if (DifficultyManager.Instance != null)
        {
            damage = DifficultyManager.Instance.GetScaledDamage(damage);
        }

        // Apply weaken if boss has it
        damage = enemyHealth.GetWeakenedDamage(damage);

        // Deal damage to player
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.TakeDamage(damage, enemyHealth);
            if (showDebugLogs)
                Debug.Log($"[BossAI] Boss dealt {damage} damage to player!");
        }

        yield return new WaitForSeconds(0.3f);

        // Check for status effect application (independent rolls - both can trigger)
        int weakenRoll = Random.Range(0, 100);
        int bleedRoll = Random.Range(0, 100);

        if (weakenRoll < weakenChance)
        {
            // Apply weakness
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.AddWeaken(weakenPercent, weakenDuration);
                if (showDebugLogs)
                    Debug.Log($"[BossAI] 💀 Boss applied {weakenPercent}% weakness for {weakenDuration} turns! (Roll: {weakenRoll}/{weakenChance})");
            }
        }
        else if (showDebugLogs)
        {
            Debug.Log($"[BossAI] ❌ Weakness failed to apply (Roll: {weakenRoll}/{weakenChance})");
        }

        if (bleedRoll < bleedChance)
        {
            // Apply bleed
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.AddBleed(bleedAmount);
                if (showDebugLogs)
                    Debug.Log($"[BossAI] 🩸 Boss applied {bleedAmount} bleed stacks! (Roll: {bleedRoll}/{bleedChance})");
            }
        }
        else if (showDebugLogs)
        {
            Debug.Log($"[BossAI] ❌ Bleed failed to apply (Roll: {bleedRoll}/{bleedChance})");
        }
    }

    /// <summary>
    /// Reset the heal flag (useful if you want to test multiple times)
    /// </summary>
    public void ResetHealFlag()
    {
        hasHealed = false;
    }
}
