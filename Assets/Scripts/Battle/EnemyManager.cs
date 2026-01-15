using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages all enemies in a battle scene.
/// Handles enemy turns, status ticking, and provides access to all living enemies.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Enemy Management")]
    [SerializeField] private List<EnemyHealth> enemies = new List<EnemyHealth>();
    [SerializeField] private bool autoFindEnemies = true;

    [Header("Turn Settings")]
    [SerializeField] private float delayBetweenEnemyAttacks = 1.5f;

    /// <summary>
    /// Returns all enemies that are currently alive (HP > 0)
    /// </summary>
    public List<EnemyHealth> GetLivingEnemies()
    {
        return enemies.Where(e => e != null && e.CurrentHP > 0).ToList();
    }

    /// <summary>
    /// Returns all registered enemies (dead or alive)
    /// </summary>
    public List<EnemyHealth> GetAllEnemies()
    {
        return enemies;
    }

    /// <summary>
    /// Check if all enemies are defeated
    /// </summary>
    public bool AllEnemiesDefeated()
    {
        return GetLivingEnemies().Count == 0;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[EnemyManager] DUPLICATE INSTANCE DETECTED! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (autoFindEnemies)
        {
            FindAllEnemies();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        // Subscribe to enemy death events
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                // Listen for when enemy dies
                enemy.OnHealthChanged += (current, max) =>
                {
                    if (current <= 0)
                    {
                        CheckBattleEnd();
                    }
                };
            }
        }
    }

    /// <summary>
    /// Find all EnemyHealth components in the scene
    /// </summary>
    public void FindAllEnemies()
    {
        enemies.Clear();
        enemies.AddRange(FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None));
        Debug.Log($"[EnemyManager] Found {enemies.Count} enemies in scene.");
    }

    /// <summary>
    /// Manually register an enemy (useful for spawning enemies at runtime)
    /// </summary>
    public void RegisterEnemy(EnemyHealth enemy)
    {
        if (enemy != null && !enemies.Contains(enemy))
        {
            enemies.Add(enemy);
            Debug.Log($"[EnemyManager] Registered enemy: {enemy.gameObject.name}");
        }
    }

    /// <summary>
    /// Remove an enemy from the list (called when enemy is destroyed)
    /// </summary>
    public void UnregisterEnemy(EnemyHealth enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
            Debug.Log($"[EnemyManager] Unregistered enemy: {enemy.gameObject.name}");
        }
    }

    /// <summary>
    /// Tick status effects for all living enemies (called at start of player turn)
    /// </summary>
    public void TickAllEnemyStatuses()
    {
        List<EnemyHealth> living = GetLivingEnemies();
        foreach (var enemy in living)
        {
            enemy.TickStatuses();
        }
        Debug.Log($"[EnemyManager] Ticked status effects for {living.Count} enemies.");
    }

    /// <summary>
    /// Have all living enemies take their turn (attack the player)
    /// </summary>
    public IEnumerator ExecuteAllEnemyTurns()
    {
        // Clear all enemy blocks at the START of their turn
        // This makes block last through the player's turn (providing defense)
        List<EnemyHealth> allEnemies = GetLivingEnemies();
        foreach (var enemy in allEnemies)
        {
            if (enemy != null && enemy.CurrentBlock > 0)
            {
                enemy.ClearBlock();
            }
        }

        // Check if enemies are disarmed
        if (PlayerStatusEffects.Instance != null && PlayerStatusEffects.Instance.EnemiesDisarmed)
        {
            Debug.Log("[EnemyManager] Enemies are DISARMED! Cannot attack this turn.");
            yield break;
        }

        List<EnemyHealth> living = GetLivingEnemies();
        
        if (living.Count == 0)
        {
            Debug.Log("[EnemyManager] No living enemies to execute turns.");
            yield break;
        }

        Debug.Log($"[EnemyManager] Executing turns for {living.Count} enemies...");

        foreach (var enemy in living)
        {
            yield return ExecuteSingleEnemyTurn(enemy);
            
            // Delay between enemy attacks
            yield return new WaitForSeconds(delayBetweenEnemyAttacks);
        }

        Debug.Log("[EnemyManager] All enemy turns complete.");
    }

    /// <summary>
    /// Single enemy attacks the player
    /// </summary>
    private IEnumerator ExecuteSingleEnemyTurn(EnemyHealth enemy)
    {
        if (enemy == null || enemy.CurrentHP <= 0)
            yield break;

        // Check if player is invisible (cannot be targeted)
        if (PlayerStatusEffects.Instance != null && PlayerStatusEffects.Instance.IsInvisible)
        {
            Debug.Log($"[EnemyManager] {enemy.gameObject.name} cannot attack - Player is INVISIBLE!");
            yield break;
        }

        // Check if enemy has custom AI (like BossAI)
        BossAI bossAI = enemy.GetComponent<BossAI>();
        if (bossAI != null)
        {
            // Boss has custom AI - let it handle its turn
            yield return bossAI.ExecuteTurn();
            yield break;
        }

        // Regular enemy attack behavior
        Debug.Log($"[EnemyManager] {enemy.gameObject.name} is attacking...");

        // Play skeleton scream before attack
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("SkeletonScream");
        }

        // Small delay after scream
        yield return new WaitForSeconds(0.5f);

        // Play slash sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("SkeletonSlash");
        }

        // Play bump animation
        if (BattleAnimator.Instance != null)
        {
            BattleAnimator.Instance.Bump(enemy.transform, Vector3.left);
        }

        // Brief delay for animation
        yield return new WaitForSeconds(0.2f);

        // Apply damage
        if (PlayerHealth.Instance != null)
        {
            int damage = Random.Range(1, 6);
            
            // Apply difficulty scaling
            if (DifficultyManager.Instance != null)
            {
                damage = DifficultyManager.Instance.GetScaledDamage(damage);
            }
            
            // Apply weaken if enemy has it
            damage = enemy.GetWeakenedDamage(damage);
            
            // Pass the enemy as attacker for proper damage popup location
            PlayerHealth.Instance.TakeDamage(damage, enemy);
            Debug.Log($"[EnemyManager] {enemy.gameObject.name} attacked player for {damage} damage!");
        }
        else
        {
            Debug.LogWarning($"[EnemyManager] {enemy.gameObject.name} tried to attack, but no PlayerHealth instance found!");
        }
    }

    /// <summary>
    /// Check if all enemies are dead and end battle if so
    /// </summary>
    private void CheckBattleEnd()
    {
        if (AllEnemiesDefeated())
        {
            Debug.Log("[EnemyManager] All enemies defeated! Battle won!");
            BattleState.SetOver(true);
        }
    }

    /// <summary>
    /// Called with delay after an enemy dies to check if battle should end
    /// </summary>
    public IEnumerator CheckBattleEndAfterDelay()
    {
        yield return new WaitForSeconds(0.6f); // Wait for death animation
        CheckBattleEnd();
    }
}
