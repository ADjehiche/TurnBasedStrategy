// DifficultySelectUI.cs
using UnityEngine;

public class DifficultySelectUI : MonoBehaviour
{
    [Header("UI Roots")]
    public GameObject difficultyRoot;
    public GameObject mainMenuRoot;   

    // runtime flag: why are we showing the picker?
    enum Reason { None, NewGame, ContinueNoSave }
    Reason _reason = Reason.None;

    void Start()
    {
        HidePicker();
    }

    void ShowPicker(Reason reason)
    {
        _reason = reason;
        if (difficultyRoot) difficultyRoot.SetActive(true);
        if (mainMenuRoot) mainMenuRoot.SetActive(false);
    }

    void HidePicker()
    {
        _reason = Reason.None;
        if (difficultyRoot) difficultyRoot.SetActive(false);
        if (mainMenuRoot) mainMenuRoot.SetActive(true);
    }

    public void StartNewGameWithDifficulty()
    {
        ShowPicker(Reason.NewGame);
    }

    public void ContinueNoSavePickDifficulty()
    {
        ShowPicker(Reason.ContinueNoSave);
    }

    public void PickEasy()   => Pick((int)GameSettingsManager.Difficulty.Easy);
    public void PickNormal() => Pick((int)GameSettingsManager.Difficulty.Normal);
    public void PickHard()   => Pick((int)GameSettingsManager.Difficulty.Hard);

    void Pick(int value)
    {
        // Save chosen difficulty
        GameSettingsManager.Instance.ChooseDifficultyFromInt(value);

        // After picking, do the action that triggered the picker
        if (_reason == Reason.NewGame)
        {
            // Find MenuController in title scene and start game
            var mc = FindFirstObjectByType<MenuController>(FindObjectsInactive.Include);
            if (mc != null) mc.NewGame();
        }
        else if (_reason == Reason.ContinueNoSave)
        {
            // No save exists; after picking, just return to menu (or start new game if you prefer)
            HidePicker();
            return;
        }

        HidePicker();
    }
}
