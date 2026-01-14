using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

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
    [Range(1f, 30f)] public float defaultMouseSensitivity = 1.5f;


    [Header("Difficulty")]
    public Difficulty defaultDifficulty = Difficulty.Normal;

    // Keep your naming: Easy / Normal / Hard
    public enum Difficulty { Easy = 0, Normal = 1, Hard = 2 }

    [Header("Settings")]

    const string K_Volume = "settings.volume";
    const string K_TextScale = "settings.textScale";
    const string K_ColorSensitivity = "settings.colorSensitivity";
    const string K_Difficulty = "settings.difficulty";
    const string K_DifficultyChosen = "settings.difficultyChosen"; 
    const string K_MouseSensitivity = "settings.mouseSensitivity";


    float _volume;
    float _textScale;
    float _colorSensitivity;
    Difficulty _difficulty;
    float _mouseSensitivity;

    public float Volume => _volume;
    public float TextScale => _textScale;
    public float ColorSensitivity => _colorSensitivity;
    public Difficulty CurrentDifficulty => _difficulty;
    public float MouseSensitivity => _mouseSensitivity;

    public bool HasChosenDifficulty => PlayerPrefs.GetInt(K_DifficultyChosen, 0) == 1;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _volume = PlayerPrefs.GetFloat(K_Volume, defaultVolume);
        _textScale = PlayerPrefs.GetFloat(K_TextScale, defaultTextScale);
        _colorSensitivity = PlayerPrefs.GetFloat(K_ColorSensitivity, defaultColorSensitivity);

        int diffInt = PlayerPrefs.GetInt(K_Difficulty, (int)defaultDifficulty);
        _difficulty = (Difficulty)Mathf.Clamp(diffInt, 0, 2);

        _mouseSensitivity = Mathf.Clamp(
            PlayerPrefs.GetFloat(K_MouseSensitivity, defaultMouseSensitivity),
            1f, 30f
        );

        ApplyAll();
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ApplyAll();

    void ApplyAll()
    {
        ApplyVolume(_volume);
        TextScaleUtility.ApplyGlobalTextScale(_textScale);
        ApplyColorSensitivity(_colorSensitivity);
        ApplyMouseSensitivity(_mouseSensitivity);
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
        ApplyColorSensitivity(_colorSensitivity);
    }

    public void SetDifficulty(Difficulty difficulty)
    {
        _difficulty = difficulty;
        PlayerPrefs.SetInt(K_Difficulty, (int)_difficulty);
        PlayerPrefs.Save();
    }

    public void SetDifficultyFromInt(int value)
    {
        value = Mathf.Clamp(value, 0, 2);
        SetDifficulty((Difficulty)value);
    }

    public void ChooseDifficulty(Difficulty difficulty)
    {
        SetDifficulty(difficulty);
        PlayerPrefs.SetInt(K_DifficultyChosen, 1);
        PlayerPrefs.Save();
    }

    public void ChooseDifficultyFromInt(int value)
    {
        value = Mathf.Clamp(value, 0, 2);
        ChooseDifficulty((Difficulty)value);
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

    void ApplyColorSensitivity(float value01)
    {
        PostProcessSensitivityApplier.Apply(value01);
    }
    public void SetMouseSensitivity(float value)
    {
        _mouseSensitivity = Mathf.Clamp(value, 1f, 30f);
        PlayerPrefs.SetFloat(K_MouseSensitivity, _mouseSensitivity);
        PlayerPrefs.Save();

        ApplyMouseSensitivity(_mouseSensitivity); 
    }


    void ApplyMouseSensitivity(float value)
    {
        var pc = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
        if (pc != null)
            pc.sensitivity = value;
    }

}
