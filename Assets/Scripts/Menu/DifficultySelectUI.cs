using UnityEngine;

public class DifficultySelectUI : MonoBehaviour
{
    [Header("UI Roots")]
    public GameObject difficultyRoot;
    public GameObject mainMenuRoot;

    void Awake()
    {
        // Start hidden
        if (difficultyRoot) difficultyRoot.SetActive(false);
    }

    public void Show()
    {
        if (difficultyRoot) difficultyRoot.SetActive(true);
        if (mainMenuRoot) mainMenuRoot.SetActive(false);
    }

    void HideAll()
    {
        // Prevent any menu flash while scene is loading
        if (difficultyRoot) difficultyRoot.SetActive(false);
        if (mainMenuRoot) mainMenuRoot.SetActive(false);
    }

    // Hook these to your Easy/Normal/Hard buttons
    public void PickEasy()   => Pick(GameSettingsManager.Difficulty.Easy);
    public void PickNormal() => Pick(GameSettingsManager.Difficulty.Normal);
    public void PickHard()   => Pick(GameSettingsManager.Difficulty.Hard);

    void Pick(GameSettingsManager.Difficulty diff)
    {
        GameSettingsManager.Instance?.ChooseDifficultyFromInt((int)diff);

        HideAll();

        var mc = FindFirstObjectByType<MenuController>(FindObjectsInactive.Include);
        if (mc != null)
            mc.StartFirstLevel(); // IMPORTANT: start level, NOT NewGame()
        else
            Debug.LogWarning("DifficultySelectUI: MenuController not found.");
    }
}
