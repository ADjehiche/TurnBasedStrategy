using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHP = 20;
    public int currentHP = 20;

    public event System.Action<int,int> OnHealthChanged; // (current, max)
    // public int hp = 20;

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
            Destroy(gameObject);
        }
    }


}