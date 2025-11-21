using UnityEngine;

/// <summary>
/// Handles audio playback for skeleton animations
/// Call these methods from Animation Events or scripts
/// </summary>
public class SkeletonAudioController : MonoBehaviour
{
    [Header("Sound Names - Must match AudioManager")]
    [SerializeField] private string screamSoundName = "SkeletonScream";
    [SerializeField] private string slashSoundName = "SkeletonSlash";
    [SerializeField] private string deathSoundName = "SkeletonDeath";
    
    [Header("Settings")]
    [SerializeField] private bool debugLogs = true;
    
    /// <summary>
    /// Play skeleton scream sound
    /// </summary>
    public void PlayScreamSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(screamSoundName);
            if (debugLogs) Debug.Log($"[SkeletonAudio] Playing scream sound: {screamSoundName}");
        }
        else
        {
            Debug.LogWarning("[SkeletonAudio] AudioManager instance not found!");
        }
    }
    
    /// <summary>
    /// Play skeleton slash sound
    /// </summary>
    public void PlaySlashSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(slashSoundName);
            if (debugLogs) Debug.Log($"[SkeletonAudio] Playing slash sound: {slashSoundName}");
        }
        else
        {
            Debug.LogWarning("[SkeletonAudio] AudioManager instance not found!");
        }
    }
    
    /// <summary>
    /// Play skeleton death sound
    /// </summary>
    public void PlayDeathSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(deathSoundName);
            if (debugLogs) Debug.Log($"[SkeletonAudio] Playing death sound: {deathSoundName}");
        }
        else
        {
            Debug.LogWarning("[SkeletonAudio] AudioManager instance not found!");
        }
    }
}
