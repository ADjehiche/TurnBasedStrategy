using UnityEngine;
using System.Collections;

/// <summary>
/// Controller for the Rage Flashback scene
/// Handles the dialogue sequence, visuals, and timing within the flashback
/// Place this on a GameObject in the RageFlashback scene
/// </summary>
public class FlashbackSceneController : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private FlashbackType flashbackType = FlashbackType.Rage;
    
    [Header("Dialogue")]
    [SerializeField] private string[] dialogueLines = new string[]
    {
        "...Blood. So much blood...",
        "They screamed my name. Not in fear—in reverence.",
        "I held lightning in my hands. I decided who lived.",
        "This place... I built it all.",
        "What... what have I done?"
    };
    [SerializeField] private float lineDuration = 3f;
    [SerializeField] private float pauseBetweenLines = 0.5f;
    
    [Header("Audio (Assign when available)")]
    [SerializeField] private AudioClip[] voiceLines; // One per dialogue line
    [SerializeField] private AudioClip ambienceClip;
    [SerializeField] private AudioClip thunderClip;
    
    [Header("Visual Effects")]
    [Tooltip("Point lights that flash like lightning during dramatic moments")]
    [SerializeField] private Light[] lightningLights;
    [SerializeField] private float lightningIntensity = 5f;
    [SerializeField] private float lightningDuration = 0.1f;
    
    [Header("Lightning VFX")]
    [Tooltip("Lightning particle effect prefab to spawn during dramatic moments")]
    [SerializeField] private GameObject lightningVFXPrefab;
    [Tooltip("Spawn points for lightning VFX (will pick randomly). Leave empty to spawn at camera position.")]
    [SerializeField] private Transform[] lightningSpawnPoints;
    [Tooltip("How long the spawned VFX stays before being destroyed")]
    [SerializeField] private float vfxLifetime = 2f;
    [Tooltip("Spawn multiple lightning bolts for more intensity")]
    [SerializeField] private int lightningBoltsPerTrigger = 1;
    
    [Header("Camera")]
    [SerializeField] private Camera flashbackCamera;
    [SerializeField] private Transform[] cameraWaypoints; // Optional camera movement
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool autoStartOnLoad = true;
    
    private AudioSource audioSource;
    private bool sequenceComplete = false;
    
    public enum FlashbackType
    {
        Rage,   // Red fragment - Tyrant memories
        Logic,  // Blue fragment - Architect memories (future use)
        Peace   // Purple fragment - Personality memories (future use)
    }
    
    void Start()
    {
        // Get or create AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Find camera if not assigned
        if (flashbackCamera == null)
        {
            flashbackCamera = Camera.main;
        }
        
        // Initialize all lightning lights
        if (lightningLights != null)
        {
            foreach (Light light in lightningLights)
            {
                if (light != null) light.intensity = 0f;
            }
        }
        
        // Start sequence automatically
        if (autoStartOnLoad)
        {
            StartCoroutine(RunFlashbackSequence());
        }
    }
    
    /// <summary>
    /// Run the complete flashback dialogue sequence
    /// </summary>
    public IEnumerator RunFlashbackSequence()
    {
        if (debugMode) Debug.Log($"[FlashbackSceneController] Starting {flashbackType} flashback sequence");
        
        // Start ambience
        if (ambienceClip != null && audioSource != null)
        {
            audioSource.clip = ambienceClip;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Initial pause to let scene settle
        yield return new WaitForSeconds(1f);
        
        // Play each dialogue line
        for (int i = 0; i < dialogueLines.Length; i++)
        {
            string line = dialogueLines[i];
            
            if (debugMode) Debug.Log($"[FlashbackSceneController] Line {i + 1}/{dialogueLines.Length}: {line}");
            
            // Show dialogue via CaptionManager
            if (CaptionManager.Instance != null)
            {
                bool isLastLine = (i == dialogueLines.Length - 1);
                
                if (isLastLine)
                {
                    // Last line is player's reaction - use [You] in yellow
                    string formattedLine = $"[You] {line}";
                    CaptionManager.Instance.ShowMonologue(formattedLine, lineDuration);
                }
                else
                {
                    // Flashback memory lines - use [Flashback] in red
                    string formattedLine = $"[Flashback] {line}";
                    CaptionManager.Instance.ShowFlashback(formattedLine, lineDuration);
                }
            }
            
            // Play voice line if available
            if (voiceLines != null && i < voiceLines.Length && voiceLines[i] != null)
            {
                audioSource.PlayOneShot(voiceLines[i]);
            }
            
            // Trigger lightning effect on dramatic lines
            if (ShouldTriggerLightning(line))
            {
                StartCoroutine(LightningFlash());
            }
            
            // Move camera to waypoint if available
            if (cameraWaypoints != null && i < cameraWaypoints.Length && cameraWaypoints[i] != null)
            {
                StartCoroutine(MoveCameraToWaypoint(cameraWaypoints[i], lineDuration));
            }
            
            // Wait for line duration + pause
            yield return new WaitForSeconds(lineDuration + pauseBetweenLines);
        }
        
        // Stop ambience
        if (audioSource != null && audioSource.isPlaying)
        {
            // Fade out ambience
            float fadeTime = 1f;
            float startVolume = audioSource.volume;
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
                yield return null;
            }
            audioSource.Stop();
            audioSource.volume = startVolume;
        }
        
        // Final pause before returning
        yield return new WaitForSeconds(1f);
        
        if (debugMode) Debug.Log("[FlashbackSceneController] Flashback sequence complete - signaling FlashbackManager");
        
        // Signal completion to FlashbackManager
        sequenceComplete = true;
        if (FlashbackManager.Instance != null)
        {
            FlashbackManager.Instance.OnFlashbackSceneComplete();
        }
        else
        {
            Debug.LogWarning("[FlashbackSceneController] FlashbackManager not found! Scene cannot return automatically.");
        }
    }
    
    /// <summary>
    /// Check if a line should trigger lightning effect
    /// </summary>
    private bool ShouldTriggerLightning(string line)
    {
        string lower = line.ToLower();
        return lower.Contains("lightning") || 
               lower.Contains("thunder") || 
               lower.Contains("power") ||
               lower.Contains("killed") ||
               lower.Contains("blood");
    }
    
    /// <summary>
    /// Flash the lightning light and spawn VFX for dramatic effect
    /// </summary>
    private IEnumerator LightningFlash()
    {
        // Play thunder sound
        if (thunderClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(thunderClip);
        }
        
        // Spawn lightning VFX prefabs
        SpawnLightningVFX();
        
        // Flash all lights (if any assigned)
        if (lightningLights != null && lightningLights.Length > 0)
        {
            // Flash on - all lights
            SetAllLightIntensity(lightningIntensity);
            yield return new WaitForSeconds(lightningDuration);
            
            // Flash off
            SetAllLightIntensity(0f);
            yield return new WaitForSeconds(0.1f);
            
            // Second flash (natural lightning pattern)
            SetAllLightIntensity(lightningIntensity * 0.6f);
            yield return new WaitForSeconds(lightningDuration * 0.5f);
            
            SetAllLightIntensity(0f);
        }
        else
        {
            // Still wait if no lights, so VFX can play
            yield return new WaitForSeconds(lightningDuration * 2f);
        }
    }
    
    /// <summary>
    /// Set intensity on all lightning lights
    /// </summary>
    private void SetAllLightIntensity(float intensity)
    {
        if (lightningLights == null) return;
        
        foreach (Light light in lightningLights)
        {
            if (light != null)
            {
                light.intensity = intensity;
            }
        }
    }
    
    /// <summary>
    /// Spawn lightning VFX prefabs at each light's location
    /// </summary>
    private void SpawnLightningVFX()
    {
        if (lightningVFXPrefab == null)
        {
            if (debugMode) Debug.Log("[FlashbackSceneController] No lightning VFX prefab assigned");
            return;
        }
        
        // Spawn VFX at each light's position
        if (lightningLights != null && lightningLights.Length > 0)
        {
            foreach (Light light in lightningLights)
            {
                if (light == null) continue;
                
                Vector3 spawnPos = light.transform.position;
                
                // Z rotation of 90 degrees as specified
                Quaternion spawnRot = Quaternion.Euler(0f, 0f, 90f);
                
                // Spawn the VFX
                GameObject vfxInstance = Instantiate(lightningVFXPrefab, spawnPos, spawnRot);
                
                // Auto-destroy after lifetime
                Destroy(vfxInstance, vfxLifetime);
                
                if (debugMode) Debug.Log($"[FlashbackSceneController] ⚡ Spawned lightning VFX at light position {spawnPos}");
            }
        }
        else
        {
            // Fallback: spawn at default position if no lights
            Vector3 spawnPos = GetDefaultSpawnPosition();
            Quaternion spawnRot = Quaternion.Euler(0f, 0f, 90f);
            
            GameObject vfxInstance = Instantiate(lightningVFXPrefab, spawnPos, spawnRot);
            Destroy(vfxInstance, vfxLifetime);
            
            if (debugMode) Debug.Log($"[FlashbackSceneController] ⚡ Spawned lightning VFX at default position {spawnPos}");
        }
    }
    
    /// <summary>
    /// Get default spawn position (in front of camera)
    /// </summary>
    private Vector3 GetDefaultSpawnPosition()
    {
        if (flashbackCamera != null)
        {
            return flashbackCamera.transform.position + flashbackCamera.transform.forward * 5f;
        }
        return transform.position + Vector3.up * 3f;
    }
    
    /// <summary>
    /// Smoothly move camera to a waypoint
    /// </summary>
    private IEnumerator MoveCameraToWaypoint(Transform target, float duration)
    {
        if (flashbackCamera == null || target == null) yield break;
        
        Vector3 startPos = flashbackCamera.transform.position;
        Quaternion startRot = flashbackCamera.transform.rotation;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            flashbackCamera.transform.position = Vector3.Lerp(startPos, target.position, smoothT);
            flashbackCamera.transform.rotation = Quaternion.Slerp(startRot, target.rotation, smoothT);
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Manually trigger the flashback sequence (for testing)
    /// </summary>
    [ContextMenu("Start Flashback Sequence")]
    public void StartFlashback()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("[FlashbackSceneController] Must be in Play mode!");
            return;
        }
        
        StartCoroutine(RunFlashbackSequence());
    }
    
    /// <summary>
    /// Force complete the flashback (for testing/skip)
    /// </summary>
    [ContextMenu("Force Complete")]
    public void ForceComplete()
    {
        StopAllCoroutines();
        sequenceComplete = true;
        
        if (FlashbackManager.Instance != null)
        {
            FlashbackManager.Instance.OnFlashbackSceneComplete();
        }
    }
}
