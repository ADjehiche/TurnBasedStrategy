using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Scenes")]
    public string titleSceneName = "TitleScene";
    public string firstLevelSceneName = "LevelOne";

    [Header("Pause Input")]
    public Key pauseKey = Key.Space;

    [Header("UI Panels")]
    public GameObject pauseRoot;
    public GameObject settingsRoot;
    public GameObject menuRoot; // optional (title menu root)

    [Header("Title UI (optional)")]
    public Button continueButton;

    [Header("Save/Load")]
    public InventoryItemDatabase itemDatabase;

    [Header("Player Control")]
    public MonoBehaviour playerMovementScript; // drag your PlayerController (or whatever controls look/move)

    [Header("Cursor")]
    public bool lockCursorInGameplay = true;
    public bool hideCursorInGameplay = true;

    bool _paused;

    void Start()
    {
        _paused = false;
        Time.timeScale = 1f;

        if (IsTitle())
        {
            if (menuRoot != null) menuRoot.SetActive(true);
            if (pauseRoot != null) pauseRoot.SetActive(false);
            if (settingsRoot != null) settingsRoot.SetActive(false);

            if (playerMovementScript != null) playerMovementScript.enabled = false;
            ApplyCursorUI();
        }
        else
        {
            if (pauseRoot != null) pauseRoot.SetActive(false);
            if (settingsRoot != null) settingsRoot.SetActive(false);

            if (playerMovementScript != null) playerMovementScript.enabled = true;
            ApplyCursorGameplay();
        }

        RefreshContinueButton();
    }

    void Update()
    {
        if (IsTitle()) return;

        var kb = Keyboard.current;
        if (kb != null && kb[pauseKey].wasPressedThisFrame)
            SetPaused(!_paused);
    }

    public void TogglePause() => SetPaused(!_paused);
    public void Resume() => SetPaused(false);

    void SetPaused(bool pause)
    {
        _paused = pause;
        Time.timeScale = _paused ? 0f : 1f;

        if (pauseRoot != null) pauseRoot.SetActive(_paused);
        if (settingsRoot != null) settingsRoot.SetActive(false);

        if (playerMovementScript != null) playerMovementScript.enabled = !_paused;

        if (_paused) ApplyCursorUI();
        else ApplyCursorGameplay();
    }

    // ---------- UI Buttons ----------
    public void OpenSettings()
    {
        if (settingsRoot != null) settingsRoot.SetActive(true);
        if (pauseRoot != null) pauseRoot.SetActive(false);
        if (menuRoot != null) menuRoot.SetActive(false);

        if (!IsTitle() && !_paused) SetPaused(true);
        else ApplyCursorUI();
    }

    public void BackFromSettings()
    {
        if (settingsRoot != null) settingsRoot.SetActive(false);

        if (IsTitle())
        {
            if (menuRoot != null) menuRoot.SetActive(true);
            ApplyCursorUI();
        }
        else
        {
            if (pauseRoot != null) pauseRoot.SetActive(true);
            ApplyCursorUI();
        }
    }

    // ---------- Title buttons ----------
    public void NewGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void Continue()
    {
        var data = SaveSystem.LoadGame();
        if (data == null)
        {
            Debug.LogWarning("Continue failed: no save found.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.sceneLoaded += OnSceneLoadedApplySave;
        SceneManager.LoadScene(data.sceneName);
    }

    void OnSceneLoadedApplySave(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedApplySave;

        var data = SaveSystem.LoadGame();
        if (data == null) return;

        var holder = FindFirstObjectByType<PlayerInventoryHolder>(FindObjectsInactive.Include);
        if (holder == null)
        {
            Debug.LogWarning("Load failed: no PlayerInventoryHolder found.");
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogWarning("Load failed: itemDatabase not assigned on MenuController.");
            return;
        }

        SaveSystem.ApplyLoadedData(data, holder.transform, holder, itemDatabase);

        // Return to gameplay state
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        ApplyCursorGameplay();
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    // ---------- In-game buttons ----------
    public void Save()
    {
        var holder = FindFirstObjectByType<PlayerInventoryHolder>(FindObjectsInactive.Include);
        if (holder == null)
        {
            Debug.LogWarning("Save failed: no PlayerInventoryHolder found.");
            return;
        }

        SaveSystem.SaveGame(holder.transform, holder);
        RefreshContinueButton();
    }

    public void SaveAndQuitToTitle()
    {
        Save();
        QuitToTitle();
    }

    public void QuitToTitle()
    {
        _paused = false;
        Time.timeScale = 1f;

        if (playerMovementScript != null) playerMovementScript.enabled = false;

        ApplyCursorUI();
        SceneManager.LoadScene(titleSceneName);
    }

    public void RefreshContinueButton()
    {
        if (continueButton != null)
            continueButton.interactable = SaveSystem.HasSave();
    }
    

    // ---------- Cursor ----------
    void ApplyCursorUI()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ApplyCursorGameplay()
    {
        Cursor.lockState = lockCursorInGameplay ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = hideCursorInGameplay ? false : true;
    }

    bool IsTitle()
    {
        return SceneManager.GetActiveScene().name == titleSceneName;
    }
}
