using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAnimator : MonoBehaviour
{
    public static BattleAnimator Instance { get; private set; }

    [Header("Popup Prefab")]
    [SerializeField] private DamagePopup damagePopupPrefab;

    [Header("Where popups spawn (must be under the same Canvas as the popup prefab)")]
    [SerializeField] private RectTransform popupRoot;

    [Header("Popup Stacking")]
    [SerializeField] private float stackStep = 30f;

    [Header("Popup Offset")]
    [SerializeField] private Vector2 popupOffset = new Vector2(0f, 20f); // small lift above anchor

    [Header("Bump Settings")]
    [SerializeField] private float bumpDistance = 0.4f; // world units (adjust)
    [SerializeField] private float bumpTime = 0.08f;

    // tracks how many popups are currently stacked for each anchor
    private readonly Dictionary<RectTransform, int> popupStacks = new Dictionary<RectTransform, int>();

    // prevent overlapping bump coroutines per transform
    private readonly Dictionary<Transform, Coroutine> bumpRoutines = new Dictionary<Transform, Coroutine>();

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Call this when damage happens. Pass the *target's* UI anchor (ex: skeleton hit anchor).
    /// If you use ScreenSpace-Camera or WorldSpace canvas, pass that canvas camera.
    /// </summary>
    public void ShowDamagePopup(int amount, RectTransform targetAnchor, Camera uiCamera = null, Vector2? extraOffset = null)
    {
        if (damagePopupPrefab == null || popupRoot == null || targetAnchor == null) return;

        DamagePopup popup = Instantiate(damagePopupPrefab, popupRoot);
        RectTransform popupRect = popup.GetComponent<RectTransform>();

        // Convert the anchor position to popupRoot local (anchored) position
        Vector2 localPoint;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, targetAnchor.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(popupRoot, screenPoint, uiCamera, out localPoint);

        // stacking
        if (!popupStacks.ContainsKey(targetAnchor))
            popupStacks[targetAnchor] = 0;

        int index = popupStacks[targetAnchor];
        popupStacks[targetAnchor]++;

        Vector2 finalOffset = popupOffset + (extraOffset ?? Vector2.zero);
        localPoint += finalOffset;
        localPoint += Vector2.up * (index * stackStep);

        popupRect.anchoredPosition = localPoint;

        // release stack when this popup is gone (best), otherwise fallback delay
        // StartCoroutine(ReleaseStackAfterSeconds(targetAnchor, popup.GetLifetimeFallbackSeconds()));

        popup.Setup(amount);
    }

    private IEnumerator ReleaseStackAfterSeconds(RectTransform anchor, float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (anchor == null) yield break;

        if (popupStacks.ContainsKey(anchor))
        {
            popupStacks[anchor] = Mathf.Max(0, popupStacks[anchor] - 1);
            // optional cleanup:
            // if (popupStacks[anchor] == 0) popupStacks.Remove(anchor);
        }
    }

    /// <summary>
    /// Bumps the attacker toward the target (so player goes right if enemy is to the right, etc.).
    /// </summary>
    public void BumpToward(Transform attacker, Transform target)
    {
        if (attacker == null || target == null) return;

        Vector3 dir = (target.position - attacker.position);
        dir.z = 0f; // if 2D-ish setup
        if (dir.sqrMagnitude < 0.0001f) return;

        Bump(attacker, dir.normalized);
    }

    /// <summary>
    /// Manual bump with an explicit direction (Vector3.right for player, Vector3.left for enemy).
    /// </summary>
    public void Bump(Transform who, Vector3 direction)
    {
        if (who == null) return;

        // stop existing bump on this transform so it doesn't jitter/feel weird
        if (bumpRoutines.TryGetValue(who, out var running) && running != null)
            StopCoroutine(running);

        var c = StartCoroutine(BumpRoutine(who, direction));
        bumpRoutines[who] = c;
    }

    private IEnumerator BumpRoutine(Transform who, Vector3 direction)
    {
        Vector3 start = who.position;
        Vector3 target = start + direction.normalized * bumpDistance;

        float t = 0f;

        // forward
        while (t < 1f)
        {
            t += Time.deltaTime / bumpTime;
            who.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        t = 0f;

        // back
        while (t < 1f)
        {
            t += Time.deltaTime / bumpTime;
            who.position = Vector3.Lerp(target, start, t);
            yield return null;
        }

        who.position = start;
        bumpRoutines.Remove(who);
    }
}