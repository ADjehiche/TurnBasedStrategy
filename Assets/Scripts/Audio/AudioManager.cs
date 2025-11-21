using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Sound Effects")]
    [SerializeField] private Sound[] sounds;
    
    [Header("Settings")]
    [SerializeField] private float masterVolume = 1f;
    
    private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();
    
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeSounds();
    }
    
    private void InitializeSounds()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.clip == null)
            {
                Debug.LogWarning($"[AudioManager] Sound '{sound.name}' has no audio clip assigned!");
                continue;
            }
            
            // Create AudioSource component for each sound
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume * masterVolume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.spatialBlend = sound.spatialBlend;
            
            // Add to dictionary for quick lookup
            if (!soundDictionary.ContainsKey(sound.name))
            {
                soundDictionary.Add(sound.name, sound);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] Duplicate sound name '{sound.name}' found!");
            }
        }
        
        Debug.Log($"[AudioManager] Initialized {soundDictionary.Count} sounds");
    }
    
    /// <summary>
    /// Play a sound by name
    /// </summary>
    public void Play(string soundName)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            Debug.LogWarning($"[AudioManager] Sound '{soundName}' not found!");
            return;
        }
        
        if (sound.source == null)
        {
            Debug.LogWarning($"[AudioManager] Sound '{soundName}' has no AudioSource!");
            return;
        }
        
        sound.source.Play();
    }
    
    /// <summary>
    /// Play a sound at a specific position (3D audio)
    /// </summary>
    public void PlayAtPosition(string soundName, Vector3 position)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            Debug.LogWarning($"[AudioManager] Sound '{soundName}' not found!");
            return;
        }
        
        if (sound.clip == null)
        {
            Debug.LogWarning($"[AudioManager] Sound '{soundName}' has no audio clip!");
            return;
        }
        
        AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume * masterVolume);
    }
    
    /// <summary>
    /// Stop a sound by name
    /// </summary>
    public void Stop(string soundName)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            Debug.LogWarning($"[AudioManager] Sound '{soundName}' not found!");
            return;
        }
        
        if (sound.source != null)
        {
            sound.source.Stop();
        }
    }
    
    /// <summary>
    /// Check if a sound is currently playing
    /// </summary>
    public bool IsPlaying(string soundName)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            return false;
        }
        
        return sound.source != null && sound.source.isPlaying;
    }
    
    /// <summary>
    /// Set master volume (0-1)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        
        // Update all sound volumes
        foreach (var sound in soundDictionary.Values)
        {
            if (sound.source != null)
            {
                sound.source.volume = sound.volume * masterVolume;
            }
        }
    }
    
    /// <summary>
    /// Play a random sound from a list (e.g., for footsteps variation)
    /// </summary>
    public void PlayRandom(string[] soundNames)
    {
        if (soundNames == null || soundNames.Length == 0)
        {
            Debug.LogWarning("[AudioManager] No sound names provided to PlayRandom!");
            return;
        }
        
        string randomSound = soundNames[Random.Range(0, soundNames.Length)];
        Play(randomSound);
    }
}
