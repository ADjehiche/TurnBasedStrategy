using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CardGame;

/// <summary>
/// Controls visual animations and effects during the card merge process
/// Provides particle effects, card movement, and success feedback
/// </summary>
public class MergeAnimationController : MonoBehaviour
{
    [Header("Animation References")]
    [SerializeField] private CardSlotUI ingredient1Slot;
    [SerializeField] private CardSlotUI ingredient2Slot;
    [SerializeField] private CardSlotUI resultSlot;
    [SerializeField] private Transform mergeCenter;
    
    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem mergeParticles;
    [SerializeField] private ParticleSystem successBurst;
    [SerializeField] private ParticleSystem ingredientConsumeEffect;
    [SerializeField] private GameObject magicCircleEffect;
    
    [Header("Visual Effects")]
    [SerializeField] private Image flashOverlay;
    [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float flashDuration = 0.5f;
    [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 0.5f);
    
    [Header("Card Animation")]
    [SerializeField] private float cardMoveSpeed = 2f;
    [SerializeField] private float cardRotationSpeed = 360f;
    [SerializeField] private float cardScalePulse = 1.3f;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Timing")]
    [SerializeField] private float ingredientMoveTime = 0.5f;
    [SerializeField] private float mergeHoldTime = 0.3f;
    [SerializeField] private float resultRevealTime = 0.5f;
    [SerializeField] private float totalAnimationTime = 2f;
    
    [Header("Audio")]
    [SerializeField] private string mergeStartSound = "MergeStart";
    [SerializeField] private string mergeCompleteSound = "MergeComplete";
    [SerializeField] private string cardConsumeSound = "CardConsume";
    [SerializeField] private string resultRevealSound = "CardReveal";
    
    private bool isAnimating = false;
    
    /// <summary>
    /// Play the full merge animation sequence
    /// </summary>
    public void PlayMergeAnimation(CardRecipe recipe, System.Action onComplete = null)
    {
        if (isAnimating)
        {
            Debug.LogWarning("[MergeAnimationController] Animation already in progress!");
            return;
        }
        
        if (recipe == null || !recipe.IsValid())
        {
            Debug.LogError("[MergeAnimationController] Invalid recipe!");
            onComplete?.Invoke();
            return;
        }
        
        StartCoroutine(MergeSequence(recipe, onComplete));
    }
    
    /// <summary>
    /// The full merge animation sequence
    /// </summary>
    private IEnumerator MergeSequence(CardRecipe recipe, System.Action onComplete)
    {
        isAnimating = true;
        
        // Phase 1: Move ingredient cards to center
        yield return StartCoroutine(MoveIngredientsToCenter());
        
        // Phase 2: Consume ingredients (particles)
        yield return StartCoroutine(ConsumeIngredients());
        
        // Phase 3: Merge effect at center
        yield return StartCoroutine(MergeEffect());
        
        // Phase 4: Reveal result card
        yield return StartCoroutine(RevealResult(recipe.result));
        
        // Phase 5: Success celebration
        yield return StartCoroutine(SuccessCelebration());
        
        isAnimating = false;
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Phase 1: Move ingredient cards toward merge center
    /// </summary>
    private IEnumerator MoveIngredientsToCenter()
    {
        // Play sound
        PlaySound(mergeStartSound);
        
        // Create temporary visual copies of ingredient cards (optional)
        // For now, just scale/highlight the slots
        
        if (ingredient1Slot != null)
        {
            StartCoroutine(PulseSlot(ingredient1Slot, ingredientMoveTime));
        }
        
        if (ingredient2Slot != null)
        {
            StartCoroutine(PulseSlot(ingredient2Slot, ingredientMoveTime));
        }
        
        yield return new WaitForSeconds(ingredientMoveTime);
    }
    
    /// <summary>
    /// Phase 2: Play consume effects on ingredient slots
    /// </summary>
    private IEnumerator ConsumeIngredients()
    {
        // Play consume sound
        PlaySound(cardConsumeSound);
        
        // Particle effects at ingredient positions
        if (ingredientConsumeEffect != null)
        {
            if (ingredient1Slot != null)
            {
                PlayParticleAt(ingredientConsumeEffect, ingredient1Slot.transform.position);
            }
            
            if (ingredient2Slot != null)
            {
                PlayParticleAt(ingredientConsumeEffect, ingredient2Slot.transform.position);
            }
        }
        
        // Flash effect
        if (flashOverlay != null)
        {
            StartCoroutine(FlashEffect(flashColor, flashDuration * 0.5f));
        }
        
        yield return new WaitForSeconds(mergeHoldTime);
    }
    
    /// <summary>
    /// Phase 3: Main merge effect at center
    /// </summary>
    private IEnumerator MergeEffect()
    {
        // Show magic circle
        if (magicCircleEffect != null)
        {
            magicCircleEffect.SetActive(true);
            
            // Rotate magic circle
            StartCoroutine(RotateObject(magicCircleEffect.transform, mergeHoldTime, 360f));
        }
        
        // Play merge particles at center
        if (mergeParticles != null && mergeCenter != null)
        {
            PlayParticleAt(mergeParticles, mergeCenter.position);
        }
        
        yield return new WaitForSeconds(mergeHoldTime);
        
        // Hide magic circle
        if (magicCircleEffect != null)
        {
            magicCircleEffect.SetActive(false);
        }
    }
    
    /// <summary>
    /// Phase 4: Reveal the result card
    /// </summary>
    private IEnumerator RevealResult(Card resultCard)
    {
        // Play reveal sound
        PlaySound(resultRevealSound);
        
        // Update result slot (will be done by CardMergeUI, but we can trigger effects)
        if (resultSlot != null)
        {
            resultSlot.PlayShineEffect();
            StartCoroutine(PulseSlot(resultSlot, resultRevealTime, cardScalePulse * 1.2f));
        }
        
        // Flash effect
        if (flashOverlay != null)
        {
            StartCoroutine(FlashEffect(flashColor, flashDuration));
        }
        
        yield return new WaitForSeconds(resultRevealTime);
    }
    
    /// <summary>
    /// Phase 5: Success celebration effects
    /// </summary>
    private IEnumerator SuccessCelebration()
    {
        // Play success sound
        PlaySound(mergeCompleteSound);
        
        // Success burst particles
        if (successBurst != null && resultSlot != null)
        {
            PlayParticleAt(successBurst, resultSlot.transform.position);
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    // ========== HELPER FUNCTIONS ==========
    
    /// <summary>
    /// Pulse a slot (scale animation)
    /// </summary>
    private IEnumerator PulseSlot(CardSlotUI slot, float duration, float maxScale = 1.2f)
    {
        if (slot == null) yield break;
        
        Transform slotTransform = slot.transform;
        Vector3 originalScale = slotTransform.localScale;
        Vector3 targetScale = originalScale * maxScale;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = Mathf.Lerp(1f, maxScale, Mathf.Sin(t * Mathf.PI));
            slotTransform.localScale = originalScale * scale;
            yield return null;
        }
        
        slotTransform.localScale = originalScale;
    }
    
    /// <summary>
    /// Flash screen overlay
    /// </summary>
    private IEnumerator FlashEffect(Color color, float duration)
    {
        if (flashOverlay == null) yield break;
        
        flashOverlay.color = color;
        flashOverlay.enabled = true;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = flashCurve.Evaluate(1f - t) * color.a;
            
            Color c = color;
            c.a = alpha;
            flashOverlay.color = c;
            
            yield return null;
        }
        
        flashOverlay.enabled = false;
    }
    
    /// <summary>
    /// Rotate an object
    /// </summary>
    private IEnumerator RotateObject(Transform obj, float duration, float degrees)
    {
        if (obj == null) yield break;
        
        float elapsed = 0f;
        Quaternion startRotation = obj.rotation;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float angle = (elapsed / duration) * degrees;
            obj.Rotate(Vector3.forward, angle * Time.deltaTime / duration);
            yield return null;
        }
    }
    
    /// <summary>
    /// Play particle effect at position
    /// </summary>
    private void PlayParticleAt(ParticleSystem particles, Vector3 position)
    {
        if (particles == null) return;
        
        ParticleSystem instance = Instantiate(particles, position, Quaternion.identity);
        instance.Play();
        Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
    }
    
    /// <summary>
    /// Play sound effect
    /// </summary>
    private void PlaySound(string soundName)
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(soundName))
        {
            AudioManager.Instance.Play(soundName);
        }
    }
    
    /// <summary>
    /// Quick merge without animation (instant feedback)
    /// </summary>
    public void PlayQuickMerge()
    {
        if (flashOverlay != null)
        {
            StartCoroutine(FlashEffect(flashColor, 0.2f));
        }
        
        PlaySound(mergeCompleteSound);
        
        if (resultSlot != null)
        {
            resultSlot.PlayShineEffect();
        }
    }
    
    public bool IsAnimating()
    {
        return isAnimating;
    }
}
