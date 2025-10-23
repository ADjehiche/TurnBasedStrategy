using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStateHandler : MonoBehaviour
{
    private const string BATTLE_TRIGGER_TAG = "battleTrigger";
    private const string ENEMY_TAG = "Enemy";

    void Start()
    {
        if (GameManager.Instance.hasSavedState)
        {
            // Restore player position
            transform.position = GameManager.Instance.playerPosition;

            DestroyBattleTriggerAndEnemy();
        }
    }

    void OnDisable()
    {
        if (SceneManager.GetActiveScene().name == "LevelOne")
        {
            GameManager.Instance.playerPosition = transform.position;
            GameManager.Instance.hasSavedState = true;
        }
    }

    private void DestroyBattleTriggerAndEnemy()
    {
        GameObject[] battleTriggers = GameObject.FindGameObjectsWithTag(BATTLE_TRIGGER_TAG);
        foreach (GameObject trigger in battleTriggers)
        {
            Debug.Log("Destroyed battle trigger after battle");
            Destroy(trigger);
        }

        try
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                Debug.Log("Destroyed enemy after battle");
                Destroy(enemy);
            }
        }
        catch (UnityException)
        {
            Debug.LogWarning("Enemy tag not found in project. Consider adding it to use this feature.");
        }
    }
}