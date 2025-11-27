using UnityEngine;
using UnityEngine.InputSystem;
using CardGame;

public class TargetingSystem : MonoBehaviour
{
    public static TargetingSystem Instance;

    private Card activeCardData = null;
    private System.Action onComplete;
    private GameObject activeCardGO;

    [Header("Audio Settings")]
    [SerializeField] private bool   playPlayerAttackSound = false;
    [SerializeField] private string playerAttackSoundName = "PlayerAttack";

    [Header("Input (DefaultInputActions)")]
    [SerializeField] private InputActionReference uiClickAction;
    [SerializeField] private InputActionReference uiCancelAction;
    [SerializeField] private InputActionReference uiRightClickAction;

    [Header("Camera")]
    [SerializeField] private Camera worldCamera;

    public bool IsBusy => activeCardGO != null;

    void Awake()
    {
        Instance = this;
    }

    public void BeginTargeting(Card cardData, GameObject cardGO, System.Action onDone)
    {
        // cancel previous
        if (IsBusy) CancelTargeting();

        activeCardData = cardData;
        activeCardGO   = cardGO;
        onComplete     = onDone;

        Debug.Log($"[TargetingSystem] Targeting started for card: {activeCardData.cardName} (cost {activeCardData.staminaCost})");

        // subscribe + enable actions
        if (uiClickAction != null && uiClickAction.action != null)
        {
            uiClickAction.action.performed += OnClickPerformed;
            uiClickAction.action.Enable();
        }

        if (uiCancelAction != null && uiCancelAction.action != null)
        {
            uiCancelAction.action.performed += OnCancelPerformed;
            uiCancelAction.action.Enable();
        }

        if (uiRightClickAction != null && uiRightClickAction.action != null)
        {
            uiRightClickAction.action.performed += OnCancelPerformed;
            uiRightClickAction.action.Enable();
        }
    }

    public void CancelTargeting()
    {
        if (!IsBusy) return;

        Debug.Log("[TargetingSystem] Targeting cancelled.");

        onComplete?.Invoke();
        onComplete     = null;
        activeCardData = null;
        activeCardGO   = null;

        // unsubscribe + disable
        if (uiClickAction != null && uiClickAction.action != null)
        {
            uiClickAction.action.performed -= OnClickPerformed;
            uiClickAction.action.Disable();
        }
        if (uiCancelAction != null && uiCancelAction.action != null)
        {
            uiCancelAction.action.performed -= OnCancelPerformed;
            uiCancelAction.action.Disable();
        }
        if (uiRightClickAction != null && uiRightClickAction.action != null)
        {
            uiRightClickAction.action.performed -= OnCancelPerformed;
            uiRightClickAction.action.Disable();
        }
    }

    private void OnClickPerformed(InputAction.CallbackContext ctx)
    {
        if (!IsBusy) return;

        Vector2 screenPos =
            Pointer.current != null ? Pointer.current.position.ReadValue() :
            (Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero);

        var cam = worldCamera != null ? worldCamera : Camera.main;
        TryTargetAtScreenPoint(screenPos, cam);
    }

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        if (!IsBusy) return;
        CancelTargeting();
    }

    public void TryTargetAtScreenPoint(Vector2 screenPos, Camera cam)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        if (activeCardData == null)
        {
            Debug.LogError("[TargetingSystem] TryTargetAtScreenPoint with no active card.");
            CancelTargeting();
            return;
        }

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        Vector2 point = world;

        Collider2D hit = Physics2D.OverlapPoint(point);
        EnemyHealth enemy   = hit ? hit.GetComponentInParent<EnemyHealth>() : null;
        PlayerHealth player = hit ? hit.GetComponentInParent<PlayerHealth>() : null;

        // --- SIMPLE RULES ---
        // Attack cards (TargetType.SingleEnemy) only valid if we clicked enemy
        // Heal/block cards (TargetType.Self) only valid if we clicked player

        switch (activeCardData.targetType)
        {
            case TargetType.SingleEnemy:
                if (enemy == null)
                {
                    Debug.Log("[TargetingSystem] Clicked, but no enemy for Attack card.");
                    return; // keep targeting, do nothing
                }
                break;

            case TargetType.Self:
                if (player == null)
                {
                    // fallback: use PlayerHealth.Instance if no collider under cursor
                    if (PlayerHealth.Instance != null)
                    {
                        player = PlayerHealth.Instance;
                    }
                    else
                    {
                        Debug.Log("[TargetingSystem] No player found for Self card.");
                        return;
                    }
                }
                break;

            case TargetType.None:
                // global effect – no specific click needed; we can just apply on player by default if needed
                break;

            default:
                if (enemy == null && player == null)
                {
                    Debug.Log("[TargetingSystem] Clicked, but no valid target.");
                    return;
                }
                break;
        }

        // Stamina check
        if (PlayerStamina.Instance != null &&
            !PlayerStamina.Instance.Spend(activeCardData.staminaCost))
        {
            Debug.Log("[TargetingSystem] Not enough stamina; cancelling.");
            CancelTargeting();
            return;
        }

        // Apply effects
        ResolveCard(activeCardData, enemy, player);

        // Audio (optional)
        if (enemy != null && playPlayerAttackSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(playerAttackSoundName);
        }

        // Remove card from hand
        BattleEvents.RaiseCardResolved(activeCardGO);

        // cleanup
        onComplete?.Invoke();
        onComplete     = null;
        activeCardData = null;
        activeCardGO   = null;

        if (uiClickAction != null && uiClickAction.action != null)
            uiClickAction.action.performed -= OnClickPerformed;
        if (uiCancelAction != null && uiCancelAction.action != null)
            uiCancelAction.action.performed -= OnCancelPerformed;
        if (uiRightClickAction != null && uiRightClickAction.action != null)
            uiRightClickAction.action.performed -= OnCancelPerformed;

        if (uiClickAction != null && uiClickAction.action != null)  uiClickAction.action.Disable();
        if (uiCancelAction != null && uiCancelAction.action != null) uiCancelAction.action.Disable();
        if (uiRightClickAction != null && uiRightClickAction.action != null) uiRightClickAction.action.Disable();
    }

    private void ResolveCard(Card card, EnemyHealth enemy, PlayerHealth player)
    {
        if (card.effects == null || card.effects.Count == 0)
        {
            if (enemy != null)
            {
                enemy.TakeDamage(3);
                Debug.Log($"[TargetingSystem] {card.cardName} had no effects; dealt 3 default damage to enemy.");
            }
            return;
        }

        foreach (var eff in card.effects)
        {
            switch (eff.effectType)
            {
                case EffectType.Damage:
                {
                    int amount = eff.useRandomRange
                        ? Random.Range(eff.minAmount, eff.maxAmount + 1)
                        : eff.amount;

                    amount = Mathf.Max(0, amount);

                    if (!eff.applyToSelf && enemy != null)
                    {
                        enemy.TakeDamage(amount);
                        Debug.Log($"[TargetingSystem] {card.cardName} dealt {amount} damage to enemy.");
                    }
                    else if (eff.applyToSelf && player != null)
                    {
                        player.TakeDamage(amount);
                        Debug.Log($"[TargetingSystem] {card.cardName} dealt {amount} damage to player (self).");
                    }
                    break;
                }

                case EffectType.ApplyBleed:
                {
                    if (enemy != null)
                    {
                        enemy.AddBleed(eff.amount);
                        Debug.Log($"[TargetingSystem] {card.cardName} applied {eff.amount} bleed to enemy.");
                    }
                    break;
                }

                case EffectType.ApplyWeak:
                {
                    if (enemy != null)
                    {
                        enemy.AddPoison(eff.amount);
                        Debug.Log($"[TargetingSystem] {card.cardName} applied {eff.amount}% weaken/poison to enemy.");
                    }
                    break;
                }

                case EffectType.Heal:
                {
                    if (player != null)
                    {
                        player.Heal(eff.amount);
                        Debug.Log($"[TargetingSystem] {card.cardName} healed player for {eff.amount}.");
                    }
                    break;
                }

                case EffectType.ApplyBlock:
                {
                    if (player != null)
                    {
                        player.GainBlock(eff.amount);
                        Debug.Log($"[TargetingSystem] {card.cardName} gave player {eff.amount} block.");
                    }
                    break;
                }

                // add more here later if you want (GainStamina, DrawCards, etc.)
            }
        }
    }

    // OPTIONAL: keep this for other systems if you want
    public void PlayCardOnEnemyNow(Card card, EnemyHealth enemy, GameObject cardGO)
    {
        if (card == null || enemy == null)
        {
            Debug.LogWarning("[TargetingSystem] PlayCardOnEnemyNow called with nulls.");
            return;
        }

        if (PlayerStamina.Instance != null &&
            !PlayerStamina.Instance.Spend(card.staminaCost))
        {
            Debug.Log("[TargetingSystem] Not enough stamina to play card.");
            return;
        }

        ResolveCard(card, enemy, null);
        BattleEvents.RaiseCardResolved(cardGO);
    }
}