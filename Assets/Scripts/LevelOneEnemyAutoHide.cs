using UnityEngine;
using System.Collections;

public class LevelOneEnemyAutoHide : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private RuntimeAnimatorController deathController; // Drag the death controller here
    [SerializeField] private RuntimeAnimatorController screamController; // Drag the scream controller here
    [SerializeField] private RuntimeAnimatorController slash01Controller; // Drag the slash01 controller here
    [SerializeField] private float animationCycleTime = 3f; // Time between animation changes
    
    [Header("Skeleton Key Drop")]
    [Tooltip("Prefab of the skeleton key to spawn when enemy is defeated")]
    [SerializeField] private GameObject skeletonKeyPrefab;
    [Tooltip("Offset from skeleton position to spawn the key")]
    [SerializeField] private Vector3 keySpawnOffset = new Vector3(0f, 0.5f, 0f);
    
    [Header("Level Settings")]
    [Tooltip("If true, checks CombatWingVictory (Level 2). If false, checks LevelOneEnemyDefeated (Level 1).")]
    [SerializeField] private bool checkLevelTwoVictory = false;
    
    private bool isAnimationCycling = false;
    private SkeletonAudioController audioController;
    private bool hasDroppedKey = false; // Prevent duplicate key spawns
    
    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Get audio controller if present
        audioController = GetComponent<SkeletonAudioController>();
        
        // Determine which victory flag to check
        bool isDefeated = checkLevelTwoVictory ? GameSession.CombatWingVictory : GameSession.LevelOneEnemyDefeated;
        
        // Add debug logging to see the state
        Debug.Log($"Skeleton Start - CheckLevel2: {checkLevelTwoVictory}, IsDefeated: {isDefeated}");
        
        if (isDefeated)
        {
            PlayDeathAnimation();
            if (!checkLevelTwoVictory) // Only spawn key for Level 1 skeleton
            {
                SpawnSkeletonKey(); 
            }
        }
        else
        {
            // Start periodic animation cycling if skeleton is alive
            StartAnimationCycling();
        }
    }
    
    private void PlayDeathAnimation()
    {
        Debug.Log("[LevelOneEnemyAutoHide] PlayDeathAnimation called - skeleton will stay visible");
        
        // Play death sound
        if (audioController != null)
        {
            audioController.PlayDeathSound();
        }
        
        if (animator != null)
        {
            if (deathController != null)
            {
                animator.runtimeAnimatorController = deathController;
                Debug.Log("[LevelOneEnemyAutoHide] Playing death animation");
            }
            else
            {
                if (HasParameter("change"))
                {
                    animator.SetTrigger("change");
                }
                else if (HasParameter("death"))
                {
                    animator.SetTrigger("death");
                }
                else if (HasParameter("Death"))
                {
                    animator.SetTrigger("Death");
                }
                else
                {
                    animator.Play("anim");
                }
            }
        }
        else
        {
            Debug.LogWarning("[LevelOneEnemyAutoHide] No animator found - skeleton will remain visible");
        }
    }
    
    private bool HasParameter(string paramName)
    {
        if (animator == null) return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
    
    private void StartAnimationCycling()
    {
        if (screamController != null && slash01Controller != null && animator != null)
        {
            isAnimationCycling = true;
            Debug.Log("Starting skeleton animation cycling");
            
            // Start with scream animation
            animator.runtimeAnimatorController = screamController;
            Debug.Log("Set skeleton to scream animation");
            
            // Schedule the first animation change
            Invoke(nameof(CycleToSlash01), animationCycleTime);
        }
        else
        {
            Debug.LogWarning($"Animation cycling failed - Scream: {screamController != null}, Slash01: {slash01Controller != null}, Animator: {animator != null}");
        }
    }
    
    private void CycleToSlash01()
    {
        if (isAnimationCycling && animator != null)
        {
            Debug.Log("Switching skeleton to slash01 animation");
            
            // Play slash sound
            if (audioController != null)
            {
                audioController.PlaySlashSound();
            }
            
            if (slash01Controller != null)
            {
                // Switch controller and force animation to start from beginning
                animator.runtimeAnimatorController = slash01Controller;
                
                // Wait a frame then force the animation to start from the beginning
                StartCoroutine(ForceAnimationRestart());
            }
            
            // Schedule change back to scream
            Invoke(nameof(CycleToScream), animationCycleTime);
        }
    }
    
    private void CycleToScream()
    {
        if (isAnimationCycling && animator != null)
        {
            Debug.Log("Switching skeleton to scream animation");
            
            // Play scream sound
            if (audioController != null)
            {
                audioController.PlayScreamSound();
            }
            
            if (screamController != null)
            {
                // Switch controller and force animation to start from beginning
                animator.runtimeAnimatorController = screamController;
                
                // Wait a frame then force the animation to start from the beginning
                StartCoroutine(ForceAnimationRestart());
            }
            
            // Schedule change back to slash01
            Invoke(nameof(CycleToSlash01), animationCycleTime);
        }
    }
    
    private System.Collections.IEnumerator ForceAnimationRestart()
    {
        // Wait one frame for the controller to be applied
        yield return null;
        
        // Force the animation to play from the beginning with normalized time 0
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.Play("anim", 0, 0f); // Play state "anim" in layer 0 from time 0
        }
    }
    
    /// <summary>
    /// Spawns the skeleton key at the skeleton's position
    /// Only spawns once, even if called multiple times
    /// </summary>
    private void SpawnSkeletonKey()
    {
        if (hasDroppedKey)
        {
            Debug.Log("[LevelOneEnemyAutoHide] Key already spawned, skipping");
            return;
        }

        if (skeletonKeyPrefab == null)
        {
            Debug.LogWarning("[LevelOneEnemyAutoHide] Skeleton key prefab not assigned! Cannot spawn key.");
            return;
        }

        // Spawn key at skeleton position with offset
        Vector3 spawnPosition = transform.position + keySpawnOffset;
        GameObject spawnedKey = Instantiate(skeletonKeyPrefab, spawnPosition, Quaternion.identity);
        spawnedKey.name = "SkeletonKey"; // Give it a unique name
        
        hasDroppedKey = true;
        Debug.Log($"[LevelOneEnemyAutoHide] Spawned skeleton key at {spawnPosition}");
    }
    
    private void OnDestroy()
    {
        // Cancel any pending animation cycles
        CancelInvoke();
    }
}