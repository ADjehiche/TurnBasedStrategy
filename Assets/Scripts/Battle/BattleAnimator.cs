using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAnimator : MonoBehaviour
{
    public static BattleAnimator Instance { get; private set; }

    [Header("Popup Prefab")]
    [SerializeField] private DamagePopup damagePopupPrefab;

    [Header("Where popups spawn")]
    [SerializeField] private RectTransform popupRoot;

    [Header("Popup Stacking")]
    [SerializeField] private float stackStep = 30f;     // space between numbers
    [SerializeField] private float stackReleaseDelay = 0.25f; // how fast stack frees up

    [Header("Bump Settings")]
    [SerializeField] private float bumpDistance = 40f;
    [SerializeField] private float bumpTime = 0.08f;

    // tracks how many popups are currently stacked for each anchor
    private readonly Dictionary<RectTransform, int> popupStacks = new Dictionary<RectTransform, int>();

    private void Awake()
    {
        Instance = this;
    }

    // EnemyHealth + PlayerHealth call this
    public void ShowDamagePopup(int amount, RectTransform anchor)
    {
        if (damagePopupPrefab == null || popupRoot == null || anchor == null) return;

        DamagePopup popup = Instantiate(damagePopupPrefab, popupRoot);
        RectTransform popupRect = popup.GetComponent<RectTransform>();

        // spawn at anchor position
        popupRect.position = anchor.position;

        // ---- stacking fix (so multiple numbers don't overlap) ----
        if (!popupStacks.ContainsKey(anchor))
            popupStacks[anchor] = 0;

        int index = popupStacks[anchor];
        popupStacks[anchor]++;

        // move each next popup upward a bit
        popupRect.anchoredPosition += new Vector2(0f, index * stackStep);

        // after a short delay, free one stack slot
        StartCoroutine(ReleaseStack(anchor, stackReleaseDelay));
        // ---------------------------------------------------------

        popup.Setup(amount);
    }

    private IEnumerator ReleaseStack(RectTransform anchor, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (anchor == null) yield break;

        if (popupStacks.ContainsKey(anchor))
        {
            popupStacks[anchor] = Mathf.Max(0, popupStacks[anchor] - 1);

            // optional cleanup if you want:
            // if (popupStacks[anchor] == 0) popupStacks.Remove(anchor);
        }
    }

    // EnemyHealth + PlayerHealth call this
    public void Bump(Transform who, Vector3 direction)
    {
        if (who == null) return;
        StartCoroutine(BumpRoutine(who, direction));
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
    }
}
