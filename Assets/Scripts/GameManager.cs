using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public Button startButton;
    private const string LevelOne = "LevelOne";
    private const string ControlsPage = "ControlsPage";
    private const string BattleScene = "BattleScene";
    private const string TitleScene = "TitleScene";
    public static GameManager Instance;
    public Vector3 playerPosition;
    public bool hasSavedState = false;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
    public void StartGame()
    {
        // Use LoadSceneMode.Single to properly initialize lighting
        SceneManager.LoadScene(LevelOne, LoadSceneMode.Single);
    }

    public void OpenControls()
    {
        SceneManager.LoadScene(ControlsPage, LoadSceneMode.Single);
    }

    public void OpenMenu()
    {
        SceneManager.LoadScene(TitleScene, LoadSceneMode.Single);
    }

    public void StartBattle()
    {
        SceneManager.LoadScene(BattleScene, LoadSceneMode.Single);
        Cursor.lockState = CursorLockMode.None;
    } 
    
    public void ReturnToLevelOne()
    {
        // Set flag to indicate we're returning from battle
        hasSavedState = true;
        
        // Return to LevelOne scene
        SceneManager.LoadScene(LevelOne);
        Cursor.lockState = CursorLockMode.Locked;
    }
}
