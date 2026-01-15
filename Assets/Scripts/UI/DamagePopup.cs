using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TMP_Text text;

    [Header("Motion")]
    [SerializeField] private float floatUpSpeed = 60f;
    [SerializeField] private float lifetime = 0.8f;

    private float timer;

    // BattleAnimator expects this:
    public void Setup(int amount)
    {
        if (text != null)
            text.text = "-" + amount.ToString();
    }

    private void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TMP_Text>();
    }

    private void Update()
    {
        // float upward
        transform.localPosition += Vector3.up * floatUpSpeed * Time.deltaTime;

        // die after time
        timer += Time.deltaTime;
        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
