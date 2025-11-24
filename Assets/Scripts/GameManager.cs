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
    private const string BattleScene = "Battle_Template";
    private const string TitleScene = "TitleScene";
    public static GameManager Instance;
    public Vector3 playerPosition;
    public bool hasSavedState = false;

    void Awake()
    {
        // if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // keep it across scenes 
    }

    // call this before starting the battle
    public void SavePlayerPosition(Vector3 pos)
    {
        playerPosition = pos;
        hasSavedState = true;
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
        // If we didn't get a position earlier, try to grab the player's current position now
        if (!hasSavedState)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) SavePlayerPosition(p.transform.position);
        }

        // Disable footsteps during battle
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var footstepAudio = player.GetComponent<PlayerFootstepAudio>();
            if (footstepAudio != null)
            {
                footstepAudio.SetEnabled(false);
            }
        }

        // Save return position for after the battle
        GameSession.SetReturnPosition(playerPosition);

        SceneManager.LoadScene(BattleScene, LoadSceneMode.Single);
        Cursor.lockState = CursorLockMode.None;
    }
    
}
