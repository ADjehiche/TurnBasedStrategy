using UnityEngine;

/// <summary>
/// Manages player status effects like Reflect, Dodge, Invisibility, etc.
/// These effects last for a certain number of turns.
/// </summary>
public class PlayerStatusEffects : MonoBehaviour
{
    public static PlayerStatusEffects Instance { get; private set; }

    [Header("Status Effects")]
    [SerializeField] private int reflectDamage = 0;
    [SerializeField] private int reflectTurnsRemaining = 0;
    
    [SerializeField] private bool hasDodge = false;
    [SerializeField] private int dodgeTurnsRemaining = 0;
    
    [SerializeField] private bool isInvisible = false;
    [SerializeField] private int invisibleTurnsRemaining = 0;
    
    [SerializeField] private bool enemiesDisarmed = false;
    [SerializeField] private int disarmTurnsRemaining = 0;

    [SerializeField] private int staminaNextTurn = 0;

    [Header("Visual")]
    [SerializeField] private GameObject playerModel; // Assign player visual in inspector
    private Renderer playerRenderer;
    private Color originalColor;

    public bool IsInvisible => isInvisible;
    public bool EnemiesDisarmed => enemiesDisarmed;

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
        // Get player renderer for invisibility effect
        if (playerModel != null)
        {
            playerRenderer = playerModel.GetComponent<Renderer>();
            if (playerRenderer != null)
            {
                originalColor = playerRenderer.material.color;
            }
        }
    }

    #region Apply Status Effects

    public void ApplyReflect(int damage, int turns)
    {
        reflectDamage = damage;
        reflectTurnsRemaining = turns;
        Debug.Log($"[PlayerStatus] Applied Reflect: {damage} damage for {turns} turn(s)");
    }

    public void ApplyDodge(int turns)
    {
        hasDodge = true;
        dodgeTurnsRemaining = turns;
        Debug.Log($"[PlayerStatus] Applied Dodge for {turns} turn(s)");
    }

    public void ApplyInvisibility(int turns)
    {
        isInvisible = true;
        invisibleTurnsRemaining = turns;
        
        // Make player semi-transparent OR hide completely
        if (playerModel != null)
        {
            // Hide the player model completely for better invisibility effect
            playerModel.SetActive(false);
        }
        
        // Show popup at PLAYER position
        if (BattleAnimator.Instance != null && PlayerHealth.Instance != null)
        {
            RectTransform playerAnchor = PlayerHealth.Instance.GetDamagePopupAnchor();
            if (playerAnchor != null)
            {
                // Show "INVISIBLE!" text popup at player position
                BattleAnimator.Instance.ShowDamagePopup(0, playerAnchor);
            }
        }
        
        Debug.Log($"[PlayerStatus] Applied Invisibility for {turns} turn(s)");
    }

    public void ApplyDisarm(int turns)
    {
        enemiesDisarmed = true;
        disarmTurnsRemaining = turns;
        Debug.Log($"[PlayerStatus] Enemies disarmed for {turns} turn(s)");
    }

    public void AddStaminaNextTurn(int amount)
    {
        staminaNextTurn += amount;
        Debug.Log($"[PlayerStatus] Will gain {amount} TEMPORARY stamina next turn (total: {staminaNextTurn})");
    }

    #endregion

    #region Check Status Effects

    public bool TryReflectDamage(int incomingDamage, EnemyHealth attacker)
    {
        if (reflectTurnsRemaining > 0 && reflectDamage > 0)
        {
            // Reflect damage back to attacker
            if (attacker != null)
            {
                int actualReflect = Mathf.Min(reflectDamage, incomingDamage);
                attacker.TakeDamage(actualReflect);
                Debug.Log($"[PlayerStatus] Reflected {actualReflect} damage back to {attacker.gameObject.name}!");
                return true;
            }
        }
        return false;
    }

    public bool TryDodgeAttack()
    {
        if (hasDodge && dodgeTurnsRemaining > 0)
        {
            // Consume the dodge
            hasDodge = false;
            dodgeTurnsRemaining = 0;
            Debug.Log($"[PlayerStatus] Dodged the attack!");
            return true;
        }
        return false;
    }

    #endregion

    #region Tick Status Effects (called at start of player turn)

    public void TickStatuses()
    {
        // Grant next-turn stamina as TEMPORARY stamina
        if (staminaNextTurn > 0)
        {
            if (PlayerStamina.Instance != null)
            {
                PlayerStamina.Instance.AddTemporaryStamina(staminaNextTurn);
                Debug.Log($"[PlayerStatus] Gained {staminaNextTurn} TEMPORARY stamina from Brace (lasts this turn only)");
            }
            staminaNextTurn = 0;
        }

        // Tick reflect
        if (reflectTurnsRemaining > 0)
        {
            reflectTurnsRemaining--;
            if (reflectTurnsRemaining <= 0)
            {
                reflectDamage = 0;
                Debug.Log($"[PlayerStatus] Reflect expired");
            }
        }

        // Tick dodge
        if (dodgeTurnsRemaining > 0)
        {
            dodgeTurnsRemaining--;
            if (dodgeTurnsRemaining <= 0)
            {
                hasDodge = false;
                Debug.Log($"[PlayerStatus] Dodge expired");
            }
        }

        // Tick invisibility
        if (invisibleTurnsRemaining > 0)
        {
            invisibleTurnsRemaining--;
            if (invisibleTurnsRemaining <= 0)
            {
                isInvisible = false;
                
                // Restore player visibility
                if (playerModel != null)
                {
                    playerModel.SetActive(true);
                }
                
                Debug.Log($"[PlayerStatus] Invisibility expired - player visible again");
            }
        }

        // Tick disarm
        if (disarmTurnsRemaining > 0)
        {
            disarmTurnsRemaining--;
            if (disarmTurnsRemaining <= 0)
            {
                enemiesDisarmed = false;
                Debug.Log($"[PlayerStatus] Disarm expired");
            }
        }
    }

    #endregion

    #region Reset

    public void ResetAllStatuses()
    {
        reflectDamage = 0;
        reflectTurnsRemaining = 0;
        hasDodge = false;
        dodgeTurnsRemaining = 0;
        isInvisible = false;
        invisibleTurnsRemaining = 0;
        enemiesDisarmed = false;
        disarmTurnsRemaining = 0;
        staminaNextTurn = 0;

        // Restore player visibility
        if (playerRenderer != null)
        {
            Color visibleColor = originalColor;
            visibleColor.a = 1f;
            playerRenderer.material.color = visibleColor;
        }

        Debug.Log($"[PlayerStatus] All status effects reset");
    }

    #endregion
}
