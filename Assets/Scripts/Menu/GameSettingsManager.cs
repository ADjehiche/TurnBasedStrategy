using UnityEngine;
using UnityEngine.Audio;

/// Persistent settings manager
/// - Global volume (AudioMixer param or AudioListener fallback)
/// - Stores text scale + colorblind mode for later commits (not applied yet)
public enum ColorblindMode
{
    Off = 0,
    Protanopia = 1,
    Deuteranopia = 2,
    Tritanopia = 3
}

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    [Header("Audio")]
    [Tooltip("Optional. If not assigned, falls back to AudioListener.volume.")]
    public AudioMixer audioMixer;

    [Tooltip("Exposed AudioMixer parameter name, e.g. 'MasterVolume'.")]
    public string masterVolumeParam = "MasterVolume";

    [Range(0.0001f, 1f)]
    public float defaultVolume = 0.8f;

    [Header("Accessibility (stored for later commits)")]
    [Range(0.5f, 2.0f)]
    public float defaultTextScale = 1.0f;

    public ColorblindMode defaultColorblind = ColorblindMode.Off;

    // PlayerPrefs keys
    private const string K_Volume = "settings.volume";
    private const string K_TextScale = "settings.textScale";
    private const string K_Colorblind = "settings.colorblind";

    private float _volume;
    private float _textScale;
    private ColorblindMode _colorblind;

    public float Volume => _volume;                 // 0..1
    public float TextScale => _textScale;           // stored only in commit 1
    public ColorblindMode Colorblind => _colorblind;// stored only in commit 1

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        ApplyVolume(_volume);
    }

    public void SetVolume01(float value01)
    {
        _volume = Mathf.Clamp(value01, 0.0001f, 1f);

        PlayerPrefs.SetFloat(K_Volume, _volume);
        PlayerPrefs.Save();

        ApplyVolume(_volume);
    }

    // Stored for later commits (Commit 4/6 will actually apply these)
    public void SetTextScale(float scale)
    {
        _textScale = Mathf.Clamp(scale, 0.5f, 2.0f);

        PlayerPrefs.SetFloat(K_TextScale, _textScale);
        PlayerPrefs.Save();
    }

    // Stored for later commits (Commit 6 will apply this visually)
    public void SetColorblind(ColorblindMode mode)
    {
        _colorblind = mode;

        PlayerPrefs.SetInt(K_Colorblind, (int)_colorblind);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        _volume = PlayerPrefs.GetFloat(K_Volume, defaultVolume);
        _textScale = PlayerPrefs.GetFloat(K_TextScale, defaultTextScale);
        _colorblind = (ColorblindMode)PlayerPrefs.GetInt(K_Colorblind, (int)defaultColorblind);
    }

    private void ApplyVolume(float value01)
    {
        // Convert 0..1 to dB. (0.0001 ~ -80dB, 1.0 = 0dB)
        float db = Mathf.Log10(Mathf.Clamp(value01, 0.0001f, 1f)) * 20f;

        if (audioMixer != null)
        {
            audioMixer.SetFloat(masterVolumeParam, db);
        }
        else
        {
            AudioListener.volume = value01;
        }
    }
}
