using UnityEngine;

public class LevelOneEnemyAutoHide : MonoBehaviour
{
    void Start()
    {
        if (GameSession.EnemyDefeated)
        {
            gameObject.SetActive(false);
        }
    }
}