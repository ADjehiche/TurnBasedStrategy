using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHP = 20;
    public int currentHP = 20;

    public event System.Action<int, int> OnHealthChanged; // (current, max)
    // public int hp = 20;

    void Awake()
    {
        // Make sure this object has the "Enemy" tag for proper cleanup
        if (gameObject.tag != "Enemy")
        {
            Debug.LogWarning("EnemyHealth object should have the 'Enemy' tag for proper cleanup after battle");
        }
    }

    void Start()
    {
        currentHP = maxHP;
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(int amount)
    {
        currentHP = Mathf.Max(currentHP - amount, 0);
        Debug.Log($"Enemy took {amount} damage. HP now {currentHP}");

        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0)
        {
            Debug.Log("Enemy died");
            BattleState.SetOver(true);
            Destroy(gameObject);
        }
    }


}
