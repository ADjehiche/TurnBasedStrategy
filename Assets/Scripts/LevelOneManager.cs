using UnityEngine;

public class LevelOneReturnManager : MonoBehaviour
{
    [SerializeField] private Transform player;     //  Player here
    [SerializeField] private GameObject enemyRoot; 

    void Start()
    {
        // Put the player back to where they entered the battle from
        if (GameSession.HasReturnPosition && player != null)
        {
            var p = GameSession.ReturnPosition;
            p.y = player.position.y;   
            player.position = p;

            GameSession.HasReturnPosition = false; 
        }

        if (enemyRoot != null && GameSession.EnemyDefeated)
        {
            enemyRoot.SetActive(false);
        }
    }
}