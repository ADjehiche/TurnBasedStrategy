using UnityEngine;
using System.Collections;

/// <summary>
/// Manages post-boss-battle state in LevelTwo:
/// 1. Warden's warning dialogue (before death animation)
/// 2. Death animation
/// 3. Purple fragment spawn
/// 4. Final door opening
/// </summary>
public class BossRoomManager : MonoBehaviour
{
    [Header("Boss Reference")]
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private RuntimeAnimatorController deathController;
    [SerializeField] private Transform bossTransform;
    [SerializeField] private GameObject bossGameObject; // To hide after defeat
    [SerializeField] private GameObject bossTrigger;    // Trigger to destroy after defeat
    
    [Header("Player Respawn")]
    [Tooltip("Where to spawn player after returning from boss battle")]
    [SerializeField] private Transform playerRespawnPoint;
    
    [Header("Purple Fragment")]
    [SerializeField] private GameObject purpleFragmentPrefab;
    [SerializeField] private Vector3 fragmentSpawnOffset = new Vector3(0, 0.5f, 0);
    
    [Header("Final Door")]
    [SerializeField] private GameObject finalDoor;
    [SerializeField] private float doorOpenAngle = -90f;
    [SerializeField] private float doorOpenSpeed = 45f;
    
    [Header("Warden Dialogue")]
    [SerializeField] private string[] wardenDialogue = {
        "[Warden] You... have bested me...",
        "[Warden] But know this - that fragment...",
        "[Warden] It holds your true self. Your darkness.",
        "[Warden] Take it, and you reclaim your evil.",
        "[Warden] Leave it... and perhaps... you find redemption..."
    };
    [SerializeField] private string[] wardenDialogueAudio; // Parallel array for audio
    [SerializeField] private float dialogueLineDuration = 3f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private bool hasProcessedVictory = false;
    
    void Start()
    {
        if (debugMode) Debug.Log($"[BossRoomManager] Start - BossDefeated: {GameSession.BossDefeated}, WardensWarningShown: {GameSession.WardensWarningShown}");
        
        // Position player at respawn point if returning from boss battle
        if (GameSession.BossDefeated && playerRespawnPoint != null)
        {
            PositionPlayerAtRespawn();
        }
        
        // If boss already defeated AND dialogue already shown, just hide boss and open door
        if (GameSession.BossDefeated && GameSession.WardensWarningShown)
        {
            HideBossAndOpenDoor();
        }
        // If boss just defeated (returning from battle), play dialogue sequence
        else if (GameSession.BossDefeated && !GameSession.WardensWarningShown)
        {
            StartCoroutine(PostVictorySequence());
        }
    }
    
    private void PositionPlayerAtRespawn()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && playerRespawnPoint != null)
        {
            // Use full position including Y
            player.transform.position = new Vector3(
                playerRespawnPoint.position.x,
                playerRespawnPoint.position.y,
                playerRespawnPoint.position.z
            );
            player.transform.rotation = playerRespawnPoint.rotation;
            if (debugMode) Debug.Log($"[BossRoomManager] Positioned player at respawn: {playerRespawnPoint.position}");
        }
    }
    
    private void HideBossAndOpenDoor()
    {
        // Hide boss
        if (bossGameObject != null)
        {
            bossGameObject.SetActive(false);
            if (debugMode) Debug.Log("[BossRoomManager] Boss hidden (already defeated)");
        }
        
        // Destroy boss trigger so it can't fire again
        if (bossTrigger != null)
        {
            Destroy(bossTrigger);
            if (debugMode) Debug.Log("[BossRoomManager] Boss trigger destroyed");
        }
        
        // Instantly open final door
        if (finalDoor != null)
        {
            Vector3 axis = Vector3.up;
            finalDoor.transform.Rotate(axis, doorOpenAngle, Space.World);
            if (debugMode) Debug.Log("[BossRoomManager] Final door opened instantly (already defeated)");
        }
    }
    
    
    private IEnumerator PostVictorySequence()
    {
        if (hasProcessedVictory) yield break;
        hasProcessedVictory = true;
        
        if (debugMode) Debug.Log("[BossRoomManager] 🎉 Processing boss victory!");
        
        // Lock player movement
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.LockMovement("Warden dialogue");
        }
        
        // Wait for scene to settle
        yield return new WaitForSeconds(1f);
        
        // Show Warden's dialogue
        for (int i = 0; i < wardenDialogue.Length; i++)
        {
            string audioName = (wardenDialogueAudio != null && i < wardenDialogueAudio.Length) ? wardenDialogueAudio[i] : null;
            
            if (CaptionManager.Instance != null)
            {
                CaptionManager.Instance.ShowMonologue(wardenDialogue[i], dialogueLineDuration, audioName);
            }
            yield return new WaitForSeconds(dialogueLineDuration + 0.5f);
        }
        
        GameSession.WardensWarningShown = true;
        
        // Play death animation
        if (bossAnimator != null && deathController != null)
        {
            bossAnimator.runtimeAnimatorController = deathController;
            if (debugMode) Debug.Log("[BossRoomManager] Playing boss death animation");
        }
        
        // Play death sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("EnemyDeath");
        }
        
        yield return new WaitForSeconds(2f);
        
        // Hide boss after death animation
        if (bossGameObject != null)
        {
            bossGameObject.SetActive(false);
            if (debugMode) Debug.Log("[BossRoomManager] Boss hidden after death");
        }
        
        // Spawn purple fragment
        SpawnPurpleFragment();
        
        // Open final door
        if (finalDoor != null)
        {
            StartCoroutine(OpenFinalDoor());
        }
        
        // Unlock player
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.UnlockMovement("Boss sequence complete");
        }
        
        // Notify objectives - Warden defeated
        var objectives = FindFirstObjectByType<SimpleLevelTwoObjectives>();
        if (objectives != null)
        {
            objectives.OnWardenDefeated();
        }
        
        if (debugMode) Debug.Log("[BossRoomManager] ✅ Post-victory sequence complete!");
    }
    
    private void SpawnPurpleFragment()
    {
        if (purpleFragmentPrefab == null)
        {
            Debug.LogWarning("[BossRoomManager] Purple fragment prefab not assigned!");
            return;
        }
        
        Vector3 spawnPos = bossTransform != null 
            ? bossTransform.position + fragmentSpawnOffset 
            : transform.position + fragmentSpawnOffset;
        
        Instantiate(purpleFragmentPrefab, spawnPos, Quaternion.identity);
        
        // Play spawn sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("ItemSpawn");
        }
        
        if (debugMode) Debug.Log($"[BossRoomManager] 💜 Purple fragment spawned at {spawnPos}");
    }
    
    private IEnumerator OpenFinalDoor()
    {
        if (debugMode) Debug.Log("[BossRoomManager] 🚪 Opening final door...");
        
        // Play door sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("DoorUnlock");
        }
        
        float totalRotated = 0f;
        float targetRotation = Mathf.Abs(doorOpenAngle);
        Vector3 axis = Vector3.up;
        
        while (totalRotated < targetRotation)
        {
            float step = doorOpenSpeed * Time.deltaTime;
            if (totalRotated + step > targetRotation)
            {
                step = targetRotation - totalRotated;
            }
            
            float rotationStep = Mathf.Sign(doorOpenAngle) * step;
            finalDoor.transform.Rotate(axis, rotationStep, Space.World);
            
            totalRotated += step;
            yield return null;
        }
        
        if (debugMode) Debug.Log("[BossRoomManager] 🚪 Final door opened!");
    }
}
