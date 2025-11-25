using UnityEngine;

/// <summary>
/// Handles audio for skeleton enemy
/// Plays scream and slash sounds during animations
/// </summary>
public class SkeletonAudioController : MonoBehaviour
{
    [Header("Sound Names")]
    [SerializeField] private string screamSoundName = "SkeletonScream";
    [SerializeField] private string slashSoundName = "SkeletonSlash";
    [SerializeField] private string deathSoundName = "SkeletonDeath";
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    /// <summary>
    /// Play scream sound immediately
    /// </summary>
    public void PlayScreamSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(screamSoundName);
            if (showDebugLogs)
                Debug.Log($"[SkeletonAudio] Playing scream: {screamSoundName}");
        }
        else
        {
            Debug.LogWarning("[SkeletonAudio] AudioManager not found!");
        }
    }
    
    /// <summary>
    /// Play slash sound immediately
    /// </summary>
    public void PlaySlashSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(slashSoundName);
            if (showDebugLogs)
                Debug.Log($"[SkeletonAudio] Playing slash: {slashSoundName}");
        }
        else
        {
            Debug.LogWarning("[SkeletonAudio] AudioManager not found!");
        }
    }
    
    /// <summary>
    /// Play death sound immediately
    /// </summary>
    public void PlayDeathSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(deathSoundName);
            if (showDebugLogs)
                Debug.Log($"[SkeletonAudio] Playing death: {deathSoundName}");
        }
        else
        {
            Debug.LogWarning("[SkeletonAudio] AudioManager not found!");
        }
    }
}
