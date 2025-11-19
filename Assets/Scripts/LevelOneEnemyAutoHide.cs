using UnityEngine;
using System.Collections;

public class LevelOneEnemyAutoHide : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private RuntimeAnimatorController deathController; // Drag the death controller here
    [SerializeField] private RuntimeAnimatorController screamController; // Drag the scream controller here
    [SerializeField] private RuntimeAnimatorController slash01Controller; // Drag the slash01 controller here
    [SerializeField] private float deathAnimationDuration = 2f; // Adjust based on your animation length
    [SerializeField] private float animationCycleTime = 3f; // Time between animation changes
    
    private bool isAnimationCycling = false;
    
    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (GameSession.EnemyDefeated)
        {
            PlayDeathAnimation();
        }
        else
        {
            // Start periodic animation cycling if skeleton is alive
            StartAnimationCycling();
        }
    }
    
    private void PlayDeathAnimation()
    {
        if (animator != null)
        {
            if (deathController != null)
            {
                animator.runtimeAnimatorController = deathController;
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
            
            // Disable the GameObject after the animation completes
            Invoke(nameof(HideAfterAnimation), deathAnimationDuration);
        }
        else
        {
            // Fallback: just hide immediately if no animator found
            gameObject.SetActive(false);
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
    
    private void HideAfterAnimation()
    {
        gameObject.SetActive(false);
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
    
    private void OnDestroy()
    {
        // Cancel any pending animation cycles
        CancelInvoke();
    }
}