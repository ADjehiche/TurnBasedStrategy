using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class LevelEndTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string sceneToLoad = "Battle_Template";
    private const string PLAYER_TAG = "Player";

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(PLAYER_TAG)) return;

        // Save player position directly to the cross-scene session container
        GameSession.SetReturnPosition(other.transform.position);

        // Go straight to battle (no GameManager dependency)
        UnityEngine.SceneManagement.SceneManager.LoadScene("Battle_Template", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}