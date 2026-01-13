using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
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

        Debug.Log($"[TargetingSystem] 🎯 Targeting started for card: {activeCardData.cardName} (cost {activeCardData.staminaCost})");
        Debug.Log($"[TargetingSystem] 👆 Waiting for CLICK 2 on target...");

        // Enable input actions for target selection (CLICK 2)
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

        Debug.Log($"[TargetingSystem] 🎯 CLICK 2 detected at screen position: {screenPos}");

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
        if (activeCardData == null)
        {
            Debug.LogError("[TargetingSystem] TryTargetAtScreenPoint with no active card.");
            CancelTargeting();
            return;
        }

        EnemyHealth enemy   = null;
        PlayerHealth player = null;

        // FIRST: Check UI elements (for Canvas-based player/enemy)
        if (EventSystem.current != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            Debug.Log($"[TargetingSystem] UI Raycast found {results.Count} UI elements at screen position {screenPos}");

            foreach (var result in results)
            {
                if (result.gameObject == null) continue;

                // Check for EnemyHealth - try self, parent, then children
                if (enemy == null)
                {
                    enemy = result.gameObject.GetComponent<EnemyHealth>();
                    if (enemy == null)
                    {
                        enemy = result.gameObject.GetComponentInParent<EnemyHealth>();
                    }
                    if (enemy == null)
                    {
                        enemy = result.gameObject.GetComponentInChildren<EnemyHealth>();
                    }
                    if (enemy != null)
                    {
                        Debug.Log($"[TargetingSystem] ✓ Found EnemyHealth on {enemy.gameObject.name} (clicked {result.gameObject.name})");
                    }
                }

                // Check for PlayerHealth - try self, parent, then children
                if (player == null)
                {
                    player = result.gameObject.GetComponent<PlayerHealth>();
                    if (player == null)
                    {
                        player = result.gameObject.GetComponentInParent<PlayerHealth>();
                    }
                    if (player == null)
                    {
                        player = result.gameObject.GetComponentInChildren<PlayerHealth>();
                    }
                    if (player != null)
                    {
                        Debug.Log($"[TargetingSystem] ✓ Found PlayerHealth on {player.gameObject.name} (clicked {result.gameObject.name})");
                    }
                }

                // If we found both, no need to keep checking
                if (enemy != null && player != null) break;
            }
        }
        else
        {
            Debug.LogWarning("[TargetingSystem] EventSystem.current is NULL! UI raycasting won't work.");
        }

        // SECOND: Check world space objects (Physics2D)
        if (enemy == null && player == null)
        {
            Ray ray = cam.ScreenPointToRay(screenPos);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);

            if (hit.collider != null)
            {
                Debug.Log($"[TargetingSystem] Physics2D raycast hit: {hit.collider.gameObject.name}");

                // Try to get EnemyHealth from what we clicked
                enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy == null)
                {
                    enemy = hit.collider.GetComponentInParent<EnemyHealth>();
                }

                // Try to get PlayerHealth from what we clicked
                player = hit.collider.GetComponent<PlayerHealth>();
                if (player == null)
                {
                    player = hit.collider.GetComponentInParent<PlayerHealth>();
                }
            }
            else
            {
                Debug.Log("[TargetingSystem] No Physics2D hit detected.");
            }
        }

        // Validate the click based on card's target type
        switch (activeCardData.targetType)
        {
            case TargetType.SingleEnemy:
                if (enemy == null)
                {
                    Debug.Log("[TargetingSystem] Attack card requires clicking on an enemy. Click cancelled.");
                    return; // Don't cancel targeting - let them try again
                }
                break;

            case TargetType.Self:
                if (player == null)
                {
                    Debug.Log("[TargetingSystem] Self-target card requires clicking on the player. Click cancelled.");
                    return; // Don't cancel targeting - let them try again
                }
                break;

            case TargetType.None:
            case TargetType.AllEnemies:
            case TargetType.AllAllies:
                // These cards don't require a specific target click, but we need ANY click to confirm
                // Just accept the click and proceed
                Debug.Log($"[TargetingSystem] Non-targeted card {activeCardData.cardName} - click accepted to play card.");
                break;

            default:
                Debug.LogWarning($"[TargetingSystem] TargetType {activeCardData.targetType} not fully implemented yet - playing anyway.");
                break;
        }

        // Stamina check
        if (PlayerStamina.Instance != null &&
            !PlayerStamina.Instance.Spend(activeCardData.staminaCost))
        {
            Debug.Log($"[TargetingSystem] Not enough stamina to play {activeCardData.cardName} (cost: {activeCardData.staminaCost}); cancelling.");
            CancelTargeting();
            return;
        }

        // Success! Valid target clicked
        string targetName = enemy != null ? "enemy" : (player != null ? "player" : "unknown");
        Debug.Log($"[TargetingSystem] Successfully playing {activeCardData.cardName} on {targetName}!");

        // Apply effects
        ResolveCard(activeCardData, enemy, player);

        // Optional audio
        if (enemy != null && playPlayerAttackSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(playerAttackSoundName);
        }

        // Cleanup input actions BEFORE destroying card to prevent assertion errors
        if (uiClickAction != null && uiClickAction.action != null)
            uiClickAction.action.performed -= OnClickPerformed;
        if (uiCancelAction != null && uiCancelAction.action != null)
            uiCancelAction.action.performed -= OnCancelPerformed;
        if (uiRightClickAction != null && uiRightClickAction.action != null)
            uiRightClickAction.action.performed -= OnCancelPerformed;

        if (uiClickAction != null && uiClickAction.action != null)   uiClickAction.action.Disable();
        if (uiCancelAction != null && uiCancelAction.action != null) uiCancelAction.action.Disable();
        if (uiRightClickAction != null && uiRightClickAction.action != null) uiRightClickAction.action.Disable();

        // Remove card from hand (this destroys the GameObject)
        BattleEvents.RaiseCardResolved(activeCardGO);

        // Final cleanup
        onComplete?.Invoke();
        onComplete     = null;
        activeCardData = null;
        activeCardGO   = null;
    }

    public void ResolveCard(Card card, EnemyHealth enemy, PlayerHealth player)
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

                    // Check if this is an AllEnemies card
                    if (card.targetType == TargetType.AllEnemies)
                    {
                        // Deal damage to ALL enemies
                        if (EnemyManager.Instance != null)
                        {
                            var allEnemies = EnemyManager.Instance.GetAllEnemies();
                            foreach (var enemyHealth in allEnemies)
                            {
                                if (enemyHealth != null)
                                {
                                    enemyHealth.TakeDamage(amount);
                                    Debug.Log($"[TargetingSystem] {card.cardName} dealt {amount} damage to {enemyHealth.gameObject.name}.");
                                }
                            }
                            
                            // Bump animation for player
                            Transform playerTransform = PlayerHealth.Instance != null 
                                ? PlayerHealth.Instance.transform 
                                : null;
                            if (playerTransform != null && BattleAnimator.Instance != null)
                            {
                                BattleAnimator.Instance.Bump(playerTransform, Vector3.right);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[TargetingSystem] AllEnemies card used but no EnemyManager found!");
                        }
                    }
                    else if (!eff.applyToSelf && enemy != null)
                    {
                        // Single enemy damage
                        Transform playerTransform = PlayerHealth.Instance != null 
                            ? PlayerHealth.Instance.transform 
                            : null;
                        
                        if (playerTransform != null)
                        {
                            BattleAnimator.Instance.Bump(playerTransform, Vector3.right);
                        }
                        
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

                case EffectType.GainStamina:
                {
                    if (PlayerStamina.Instance != null)
                    {
                        PlayerStamina.Instance.currentStamina = Mathf.Min(
                            PlayerStamina.Instance.maxStamina,
                            PlayerStamina.Instance.currentStamina + eff.amount
                        );
                        Debug.Log($"[TargetingSystem] {card.cardName} restored {eff.amount} stamina.");
                    }
                    break;
                }

                case EffectType.DrawCards:
                {
                    var deckMgr = Object.FindFirstObjectByType<DeckManager>();
                    var handMgr = Object.FindFirstObjectByType<HandManager>();
                    if (deckMgr != null && handMgr != null)
                    {
                        var drawnCards = deckMgr.Draw(eff.amount);
                        foreach (var c in drawnCards)
                        {
                            handMgr.AddCardToHand(c);
                        }
                        Debug.Log($"[TargetingSystem] {card.cardName} drew {drawnCards.Count} card(s).");
                    }
                    break;
                }

                case EffectType.GainNextTurnStamina:
                {
                    if (PlayerStatusEffects.Instance != null)
                    {
                        PlayerStatusEffects.Instance.AddStaminaNextTurn(eff.amount);
                        Debug.Log($"[TargetingSystem] {card.cardName} will grant {eff.amount} stamina next turn.");
                    }
                    break;
                }

                case EffectType.DodgeNextAttack:
                {
                    if (PlayerStatusEffects.Instance != null)
                    {
                        PlayerStatusEffects.Instance.ApplyDodge(eff.durationTurns);
                        Debug.Log($"[TargetingSystem] {card.cardName} applied Dodge for {eff.durationTurns} turn(s).");
                    }
                    break;
                }

                case EffectType.PreventAttack:
                {
                    if (PlayerStatusEffects.Instance != null)
                    {
                        // Check if this is Invisibility (player) or Disarm (enemies)
                        if (card.targetType == TargetType.Self)
                        {
                            // Invisibility
                            PlayerStatusEffects.Instance.ApplyInvisibility(eff.durationTurns);
                            Debug.Log($"[TargetingSystem] {card.cardName} applied Invisibility for {eff.durationTurns} turn(s).");
                        }
                        else if (card.targetType == TargetType.AllEnemies)
                        {
                            // Disarm
                            PlayerStatusEffects.Instance.ApplyDisarm(eff.durationTurns);
                            Debug.Log($"[TargetingSystem] {card.cardName} disarmed all enemies for {eff.durationTurns} turn(s).");
                        }
                    }
                    break;
                }

                case EffectType.RemoveDebuffs:
                {
                    // Remove player debuffs (if you add debuff tracking later)
                    Debug.Log($"[TargetingSystem] {card.cardName} removed debuffs.");
                    break;
                }

                case EffectType.ReflectDamage:
                {
                    if (PlayerStatusEffects.Instance != null)
                    {
                        PlayerStatusEffects.Instance.ApplyReflect(eff.amount, eff.durationTurns);
                        Debug.Log($"[TargetingSystem] {card.cardName} applied Reflect {eff.amount} damage for {eff.durationTurns} turn(s).");
                    }
                    break;
                }

                // Add more effect types as needed
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