using UnityEngine;
using UnityEngine.SceneManagement;

/// Core menu controller 
/// - ESC toggles pause in gameplay scenes (not TitleScene)
/// - Resume / Quit to Title
/// - NewGame loads a scene by name
/// - Settings panel open/close (panel references optional)
///
public class MenuController : MonoBehaviour
{
    [Header("Scene Names")]
    public string titleSceneName = "TitleScene";

    [Header("UI Panels (optional)")]
    public GameObject pauseRoot;     // pause menu root panel
    public GameObject settingsRoot;  // settings panel root (can be under pause or standalone)

    private bool _paused;

    private void Start()
    {
        if (pauseRoot != null) pauseRoot.SetActive(false);
        if (settingsRoot != null) settingsRoot.SetActive(false);

        SetPaused(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) &&
            SceneManager.GetActiveScene().name != titleSceneName)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        SetPaused(!_paused);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    private void SetPaused(bool pause)
    {
        _paused = pause;
        Time.timeScale = _paused ? 0f : 1f;

        if (pauseRoot != null) pauseRoot.SetActive(_paused);

        // If unpausing, close settings automatically
        if (!_paused && settingsRoot != null) settingsRoot.SetActive(false);
    }

    public void OpenSettings()
    {
        if (settingsRoot != null) settingsRoot.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsRoot != null) settingsRoot.SetActive(false);
    }

    // Title menu button
    public void NewGame(string firstLevelSceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelSceneName);
    }

    // In-game button
    public void QuitToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(titleSceneName);
    }

    // Optional actual app quit (for PC builds etc.)
    public void QuitApplication()
    {
        Application.Quit();
    }
}
