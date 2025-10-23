using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    public string sceneToLoad;
    private const String PLAYER_TAG = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(PLAYER_TAG))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
