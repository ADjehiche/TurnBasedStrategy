using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsAutoApply : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        Apply();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Apply();
    }

    void Apply()
    {
        var s = GameSettingsManager.Instance;
        if (s == null) return;

        TextScaleUtility.ApplyGlobalTextScale(s.TextScale);
        // ColorSensitivity persists already; actual visual effect later.
    }
}
