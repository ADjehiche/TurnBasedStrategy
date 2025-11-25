using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Sound Effects")]
    [Tooltip("Array of sounds to manage. Configure each sound with a unique name, audio clip, and settings.")]
    [SerializeField] private Sound[] sounds;
    
    [Header("Settings")]
    [Tooltip("Master volume multiplier for all sounds (0-1)")]
    [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
    
    [Header("Debug")]
    [Tooltip("Enable verbose logging for troubleshooting audio issues")]
    [SerializeField] private bool verboseLogging = true;
    
    private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();
    
    void Awake()
    {
        if (verboseLogging) Debug.Log("=== AudioManager Awake START ===");
        
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            if (verboseLogging) Debug.LogWarning("[AudioManager] Duplicate instance detected! Destroying this one.");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (verboseLogging) Debug.Log("[AudioManager] Singleton instance created and set to DontDestroyOnLoad");
        
        InitializeSounds();
        
        if (verboseLogging) Debug.Log("=== AudioManager READY ===");
    }
    
    void Start()
    {
        // Validate Audio Listener exists in scene
        ValidateAudioListener();
    }
    
    private void ValidateAudioListener()
    {
        AudioListener listener = FindObjectOfType<AudioListener>();
        if (listener == null)
        {
            Debug.LogError("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                          "⚠️  CRITICAL: No Audio Listener found in scene!\n" +
                          "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                          "Without an Audio Listener, NO audio will play!\n\n" +
                          "To fix:\n" +
                          "1. Select your Main Camera in the Hierarchy\n" +
                          "2. Click 'Add Component' in Inspector\n" +
                          "3. Search for 'Audio Listener'\n" +
                          "4. Add it to the camera\n" +
                          "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        }
        else
        {
            if (verboseLogging) Debug.Log($"[AudioManager] ✓ Audio Listener found on: {listener.gameObject.name}");
        }
    }
    
    private void InitializeSounds()
    {
        if (verboseLogging) Debug.Log($"[AudioManager] Starting initialization with {sounds.Length} sounds in array");
        
        if (sounds == null || sounds.Length == 0)
        {
            Debug.LogWarning("[AudioManager] ⚠️  Sounds array is empty! No sounds will be available.");
            return;
        }
        
        int successCount = 0;
        int failCount = 0;
        StringBuilder soundNames = new StringBuilder();
        
        foreach (Sound sound in sounds)
        {
            if (string.IsNullOrEmpty(sound.name))
            {
                Debug.LogWarning($"[AudioManager] ⚠️  Found a sound with no name! Skipping...");
                failCount++;
                continue;
            }
            
            if (sound.clip == null)
            {
                Debug.LogWarning($"[AudioManager] ⚠️  Sound '{sound.name}' has no audio clip assigned!");
                failCount++;
                continue;
            }
            
            if (verboseLogging) Debug.Log($"[AudioManager] Initializing sound: '{sound.name}' - Clip: {sound.clip.name}");
            
            // Create AudioSource component for each sound
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume * masterVolume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.spatialBlend = sound.spatialBlend;
            sound.source.playOnAwake = false;
            
            // Add to dictionary for quick lookup
            if (!soundDictionary.ContainsKey(sound.name))
            {
                soundDictionary.Add(sound.name, sound);
                if (verboseLogging) Debug.Log($"[AudioManager] ✓ Added '{sound.name}' to dictionary");
                soundNames.Append(sound.name).Append(", ");
                successCount++;
            }
            else
            {
                Debug.LogWarning($"[AudioManager] ❌ Duplicate sound name '{sound.name}' found! Only the first one will be used.");
                failCount++;
            }
        }
        
        if (successCount > 0)
        {
            string allSoundNames = soundNames.ToString().TrimEnd(',', ' ');
            Debug.Log($"[AudioManager] ✓ Initialized {successCount} sounds successfully" + 
                     (failCount > 0 ? $" ({failCount} failed)" : ""));
            if (verboseLogging) Debug.Log($"[AudioManager] Available sounds: {allSoundNames}");
        }
        else
        {
            Debug.LogError("[AudioManager] ❌ No sounds were successfully initialized!");
        }
    }
    
    /// <summary>
    /// Play a sound by name
    /// </summary>
    public void Play(string soundName)
    {
        if (string.IsNullOrEmpty(soundName))
        {
            Debug.LogWarning("[AudioManager] ❌ Play() called with null or empty sound name!");
            return;
        }
        
        if (!soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            Debug.LogWarning($"[AudioManager] ❌ Sound '{soundName}' not found in dictionary!\n" +
                           $"Available sounds: {string.Join(", ", soundDictionary.Keys)}");
            return;
        }
        
        if (sound.source == null)
        {
            Debug.LogWarning($"[AudioManager] ❌ Sound '{soundName}' has no AudioSource!");
            return;
        }
        
        sound.source.Play();
        if (verboseLogging) Debug.Log($"[AudioManager] ▶️  Playing sound '{soundName}' - Volume: {sound.source.volume:F2}, Pitch: {sound.source.pitch:F2}");
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
    
    // TESTING: Press T in Play mode to test audio
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestAudio();
        }
    }
    
    /// <summary>
    /// Test function to verify audio system is working
    /// </summary>
    private void TestAudio()
    {
        Debug.Log("[AudioManager] 🧪 Testing audio system...");
        
        if (soundDictionary.Count == 0)
        {
            Debug.LogError("[AudioManager] ❌ No sounds available to test!");
            return;
        }
        
        // Play the first available sound
        foreach (var soundName in soundDictionary.Keys)
        {
            Debug.Log($"[AudioManager] 🔊 Playing test sound: '{soundName}'");
            Play(soundName);
            break;
        }
    }
}
