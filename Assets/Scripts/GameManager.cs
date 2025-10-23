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
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
}
