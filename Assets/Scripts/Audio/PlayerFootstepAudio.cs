using UnityEngine;

/// <summary>
/// Handles footstep sounds for player movement
/// Attach to the player GameObject
/// </summary>
public class PlayerFootstepAudio : MonoBehaviour
{
    [Header("Footstep Sound Name")]
    [Tooltip("Single footstep sound name. Must match AudioManager sound name.")]
    [SerializeField] private string footstepSoundName = "Footstep1";
    
    [Header("Settings")]
    [Tooltip("Time between footstep sounds when moving")]
    [SerializeField] private float footstepInterval = 0.5f;
    [Tooltip("Minimum movement speed required to play footsteps")]
    [SerializeField] private float minimumVelocity = 0.5f;
    [Tooltip("Enable debug logging")]
    [SerializeField] private bool debugLogs = false;
    
    private Rigidbody rb;
    private float footstepTimer = 0f;
    private bool wasMoving = false;
    private bool isEnabled = true; // Can be disabled during battles
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            Debug.LogError("[PlayerFootstepAudio] Rigidbody component not found! This script requires a Rigidbody on the player.");
            enabled = false;
            return;
        }
        
        if (debugLogs)
        {
            Debug.Log($"[PlayerFootstepAudio] Initialized with footstep sound: {footstepSoundName}");
        }
    }
    
    void Update()
    {
        if (rb == null || !isEnabled) return;
        
        // Get horizontal velocity (ignore vertical for jumping/falling)
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float speed = horizontalVelocity.magnitude;
        
        // Check if player is moving fast enough
        bool isMoving = speed > minimumVelocity;
        
        if (isMoving)
        {
            footstepTimer += Time.deltaTime;
            
            if (footstepTimer >= footstepInterval)
            {
                PlayFootstepSound();
                footstepTimer = 0f;
            }
            
            if (!wasMoving && debugLogs)
            {
                Debug.Log("[PlayerFootstepAudio] Started walking");
            }
        }
        else
        {
            // Reset timer when not moving
            footstepTimer = 0f;
            
            if (wasMoving && debugLogs)
            {
                Debug.Log("[PlayerFootstepAudio] Stopped walking");
            }
        }
        
        wasMoving = isMoving;
    }
    
    private void PlayFootstepSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(footstepSoundName);
            
            if (debugLogs)
            {
                Debug.Log("[PlayerFootstepAudio] Playing footstep sound");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerFootstepAudio] AudioManager instance not found!");
        }
    }
    
    /// <summary>
    /// Manually trigger a footstep sound (e.g., from animation events)
    /// </summary>
    public void TriggerFootstep()
    {
        if (isEnabled)
        {
            PlayFootstepSound();
        }
    }
    
    /// <summary>
    /// Enable or disable footstep sounds (useful for battle scenes)
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
        if (!enabled)
        {
            footstepTimer = 0f; // Reset timer when disabled
            if (debugLogs)
            {
                Debug.Log("[PlayerFootstepAudio] Footsteps disabled");
            }
        }
        else if (debugLogs)
        {
            Debug.Log("[PlayerFootstepAudio] Footsteps enabled");
        }
    }
    
    /// <summary>
    /// Adjust footstep interval at runtime (useful for different movement speeds)
    /// </summary>
    public void SetFootstepInterval(float interval)
    {
        footstepInterval = Mathf.Max(0.1f, interval);
    }
}
