using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHP = 20;
    public int currentHP = 20;

    public event System.Action<int, int> OnHealthChanged; // (current, max)
    
    private SkeletonAudioController audioController;

    void Awake()
    {
        // Make sure this object has the "Enemy" tag for proper cleanup
        if (gameObject.tag != "Enemy")
        {
            Debug.LogWarning("EnemyHealth object should have the 'Enemy' tag for proper cleanup after battle");
            tag = "Enemy";
        }
        
        // Get audio controller if available
        audioController = GetComponent<SkeletonAudioController>();
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
            
            // Play death sound if audio controller is available
            if (audioController != null)
            {
                audioController.PlayDeathSound();
            }
            
            BattleState.SetOver(true);
            Destroy(gameObject, 0.5f); // Small delay to let death sound play
        }
    }

}
