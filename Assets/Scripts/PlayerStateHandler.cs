using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStateHandler : MonoBehaviour {
    void Start() {
        if (GameManager.Instance.hasSavedState)
            transform.position = GameManager.Instance.playerPosition;
    }

    void OnDisable() {
        if (SceneManager.GetActiveScene().name == "LevelOne") {
            GameManager.Instance.playerPosition = transform.position;
            GameManager.Instance.hasSavedState = true;
        }
    }
}