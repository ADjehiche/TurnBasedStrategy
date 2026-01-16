using UnityEngine;
using System.Collections;

/// <summary>
/// Trigger for boss room entrance. Handles:
/// 1. Boss spawn animation
/// 2. Pre-battle dialogue from Blue Fragment
/// 3. Scene transition to Battle_Boss
/// </summary>
public class BossTrigger : MonoBehaviour
{
    [Header("Boss Reference")]
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private RuntimeAnimatorController spawnController;
    [SerializeField] private GameObject bossGameObject;
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnAnimationDuration = 2f;
    
    [Header("Dialogue")]
    [SerializeField] private string blueFragmentDialogue = "[Blue Fragment] Its the warden!";
    [SerializeField] private float dialogueDuration = 2.5f;
    
    [Header("Battle Scene")]
    [SerializeField] private string battleSceneName = "Battle_Boss";
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private bool hasTriggered = false;
    
    void Start()
    {
        // Hide boss initially if not spawned yet
        if (!GameSession.BossSpawned && bossGameObject != null)
        {
            bossGameObject.SetActive(false);
        }
        else if (GameSession.BossSpawned && bossGameObject != null)
        {
            // Boss already spawned before (returning from battle) - show it
            bossGameObject.SetActive(true);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;
        
        // Don't trigger if boss already defeated
        if (GameSession.BossDefeated)
        {
            if (debugMode) Debug.Log("[BossTrigger] Boss already defeated, skipping");
            return;
        }
        
        hasTriggered = true;
        StartCoroutine(BossEncounterSequence());
    }
    
    private IEnumerator BossEncounterSequence()
    {
        if (debugMode) Debug.Log("[BossTrigger] 👹 Boss encounter starting!");
        
        // Lock player movement
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.LockMovement("Boss encounter");
        }
        
        // Show boss
        if (bossGameObject != null)
        {
            bossGameObject.SetActive(true);
        }
        
        // Play spawn animation
        if (bossAnimator != null && spawnController != null)
        {
            bossAnimator.runtimeAnimatorController = spawnController;
            if (debugMode) Debug.Log("[BossTrigger] Playing boss spawn animation");
        }
        
        // Wait for spawn animation
        yield return new WaitForSeconds(spawnAnimationDuration);
        
        GameSession.BossSpawned = true;
        
        // Notify objectives
        var objectives = FindFirstObjectByType<SimpleLevelTwoObjectives>();
        if (objectives != null)
        {
            objectives.OnBossRoomEntered();
        }
        
        // Show Blue Fragment dialogue
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(blueFragmentDialogue, dialogueDuration);
        }
        
        // Play warning sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("EnemySpotted");
        }
        
        yield return new WaitForSeconds(dialogueDuration + 0.5f);
        
        // Save state for battle return
        SaveBattleState();
        
        // Unlock before scene change
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.UnlockMovement("Starting boss battle");
        }
        
        // Load battle scene
        if (debugMode) Debug.Log($"[BossTrigger] Loading battle: {battleSceneName}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(battleSceneName);
    }
    
    private void SaveBattleState()
    {
        // Save return scene
        GameSession.ReturnSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // Save battle scene name (CRITICAL: BattleManager uses this to set BossDefeated)
        GameSession.SetBattleSceneName(battleSceneName);
        
        // Save trigger position for player respawn
        Vector3 triggerCenter = GetComponent<Collider>().bounds.center;
        triggerCenter.y = GameObject.FindGameObjectWithTag("Player").transform.position.y;
        GameSession.SetBattleTriggerPosition(triggerCenter);
        
        if (debugMode) Debug.Log($"[BossTrigger] Saved state - Return: {GameSession.ReturnSceneName}, Battle: {battleSceneName}");
    }
}
