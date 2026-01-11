using UnityEngine;
using UnityEngine.Audio;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    [Header("Audio")]
    public AudioMixer audioMixer;
    public string masterVolumeParam = "MasterVolume";
    [Range(0.0001f, 1f)] public float defaultVolume = 0.8f;

    [Header("Accessibility")]
    [Range(0.5f, 2.0f)] public float defaultTextScale = 1.0f;
    [Range(0f, 1f)] public float defaultColorSensitivity = 0f;

    const string K_Volume = "settings.volume";
    const string K_TextScale = "settings.textScale";
    const string K_ColorSensitivity = "settings.colorSensitivity";

    float _volume;
    float _textScale;
    float _colorSensitivity;

    public float Volume => _volume;
    public float TextScale => _textScale;
    public float ColorSensitivity => _colorSensitivity;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _volume = PlayerPrefs.GetFloat(K_Volume, defaultVolume);
        _textScale = PlayerPrefs.GetFloat(K_TextScale, defaultTextScale);
        _colorSensitivity = PlayerPrefs.GetFloat(K_ColorSensitivity, defaultColorSensitivity);

        ApplyVolume(_volume);
        TextScaleUtility.ApplyGlobalTextScale(_textScale);
    }

    public void SetVolume01(float v)
    {
        _volume = Mathf.Clamp(v, 0.0001f, 1f);
        PlayerPrefs.SetFloat(K_Volume, _volume);
        PlayerPrefs.Save();
        ApplyVolume(_volume);
    }

    public void SetTextScale(float scale)
    {
        _textScale = Mathf.Clamp(scale, 0.5f, 2.0f);
        PlayerPrefs.SetFloat(K_TextScale, _textScale);
        PlayerPrefs.Save();

        TextScaleUtility.ApplyGlobalTextScale(_textScale);
    }

    public void SetColorSensitivity(float v)
    {
        _colorSensitivity = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(K_ColorSensitivity, _colorSensitivity);
        PlayerPrefs.Save();
    }

    void ApplyVolume(float value01)
    {
        if (audioMixer != null)
        {
            float db = Mathf.Log10(Mathf.Clamp(value01, 0.0001f, 1f)) * 20f;
            audioMixer.SetFloat(masterVolumeParam, db);
        }
        else
        {
            AudioListener.volume = value01;
        }
    }
}
