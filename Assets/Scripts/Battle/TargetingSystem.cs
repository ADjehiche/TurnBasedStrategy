using UnityEngine;
using UnityEngine.InputSystem;
using CardGame;

public class TargetingSystem : MonoBehaviour
{
    public static TargetingSystem Instance;

    private Card activeCardData = null;      // where the card being played will be stored
    private System.Action onComplete;

    private GameObject activeCardGO;         // the GameObject of the card being played

    [Header("Audio Settings")]
    [Tooltip("Enable this when you have a player attack sound ready")]
    [SerializeField] private bool playPlayerAttackSound = false;
    [Tooltip("Name of the sound to play when player attacks (e.g., 'PlayerSlash', 'SwordHit')")]
    [SerializeField] private string playerAttackSoundName = "PlayerAttack";


    // using the new Input System
    [Header("Input (DefaultInputActions)")]
    [Tooltip("Assign DefaultInputActions → UI/Click")]
    [SerializeField] private InputActionReference uiClickAction;

    [Tooltip("Assign DefaultInputActions → UI/Cancel")]
    [SerializeField] private InputActionReference uiCancelAction;

    [Tooltip("Optional: Assign DefaultInputActions → UI/RightClick (lets right-click cancel as well)")]
    [SerializeField] private InputActionReference uiRightClickAction;

    [Header("Camera")]
    [SerializeField] private Camera worldCamera; 

    public bool IsBusy => activeCardGO != null;

    void Awake() => Instance = this;

    public void BeginTargeting(Card cardData, GameObject cardGO, System.Action onDone)
    {
        // If another card was already locked, cancel it (this resets that card)
        if (IsBusy) CancelTargeting();

        activeCardData = cardData;
        activeCardGO   = cardGO;
        onComplete     = onDone;
        Debug.Log($"Targeting started for card: {activeCardData.cardName} (cost {activeCardData.staminaCost})");

     

             // Subscribe + enable actions
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

        Debug.Log("Targeting cancelled.");
    
        onComplete?.Invoke(); // reset the locked card UI
        onComplete = null;
        activeCardGO = null;

        // Unsubscribe + disable actions
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

    // called when UI/Click fires
    private void OnClickPerformed(InputAction.CallbackContext ctx)
    {
        if (!IsBusy) return;

        // read pointer position from the Input System
        Vector2 screenPos =
            Pointer.current != null ? Pointer.current.position.ReadValue() :
            (Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero);

        var cam = worldCamera != null ? worldCamera : Camera.main;
        TryTargetAtScreenPoint(screenPos, cam);
        // TryTargetAtScreenPoint will apply damage & call onComplete → reset + cleanup below happens there
    }

    // UI/Cancel or UI/RightClick
    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        if (!IsBusy) return;
        CancelTargeting();
    }




    public void TryTargetAtScreenPoint(Vector2 screenPos, Camera cam)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        Vector2 point = world;

        Collider2D hit = Physics2D.OverlapPoint(point);
        if (!hit) { Debug.Log("No 2D target under cursor."); return; }

        var enemy = hit.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            //  no card means nothing to apply
            if (activeCardData == null)
            {
                Debug.LogError("Tried to resolve targeting with no active card data.");
                CancelTargeting();
                return;
            }

            // Spend stamina from PlayerStamina
            if (PlayerStamina.Instance != null && !PlayerStamina.Instance.Spend(activeCardData.staminaCost))
            {
                Debug.Log("Not enough stamina at confirm-time; cancelling.");
                CancelTargeting();
                return;
            }

            // Roll damage from card data
            int min = activeCardData.damageMin;
            int max = activeCardData.damageMax;
            int amount = Mathf.Clamp(Random.Range(min, max + 1), 0, int.MaxValue);

            // Play player attack sound if enabled (add your audio file first!)
            if (playPlayerAttackSound && AudioManager.Instance != null)
            {
                AudioManager.Instance.Play(playerAttackSoundName);
            }

            // Apply damage
            enemy.TakeDamage(amount);

            // Tell HandManager to remove this card
            BattleEvents.RaiseCardResolved(activeCardGO);

            // Cleanup
            onComplete?.Invoke();
            onComplete = null;
            activeCardData = null;
            activeCardGO   = null;

            // cleanup input
            if (uiClickAction != null && uiClickAction.action != null)
                uiClickAction.action.performed -= OnClickPerformed;
            if (uiCancelAction != null && uiCancelAction.action != null)
                uiCancelAction.action.performed -= OnCancelPerformed;
            if (uiRightClickAction != null && uiRightClickAction.action != null)
                uiRightClickAction.action.performed -= OnCancelPerformed;

            if (uiClickAction != null && uiClickAction.action != null) uiClickAction.action.Disable();
            if (uiCancelAction != null && uiCancelAction.action != null) uiCancelAction.action.Disable();
            if (uiRightClickAction != null && uiRightClickAction.action != null) uiRightClickAction.action.Disable();
        }
        else
        {
            Debug.Log("Hit 2D collider, but no EnemyHealth on it.");
        }
    }    
}