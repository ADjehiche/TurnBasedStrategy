using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class SettingsUIBinder : MonoBehaviour
{
    public Slider volumeSlider;          // 0..1
    public Slider textScaleSlider;        // 0.5..2
    public Slider colorSensitivitySlider; // 0..1
    public TMP_Dropdown difficultyDropdown;


    void OnEnable()
    {
        if (GameSettingsManager.Instance == null)
        {
            Debug.LogWarning("No GameSettingsManager in scene.");
            return;
        }

        var s = GameSettingsManager.Instance;

        if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(s.Volume);
        if (textScaleSlider != null) textScaleSlider.SetValueWithoutNotify(s.TextScale);
        if (colorSensitivitySlider != null) colorSensitivitySlider.SetValueWithoutNotify(s.ColorSensitivity);
        if (difficultyDropdown != null) difficultyDropdown.SetValueWithoutNotify((int)s.CurrentDifficulty);
    }

    public void OnVolumeChanged(float v) => GameSettingsManager.Instance?.SetVolume01(v);
    public void OnTextScaleChanged(float s) => GameSettingsManager.Instance?.SetTextScale(s);
    public void OnColorSensitivityChanged(float v) => GameSettingsManager.Instance?.SetColorSensitivity(v);
    public void OnDifficultyChanged(int v) => GameSettingsManager.Instance?.SetDifficultyFromInt(v);
}
