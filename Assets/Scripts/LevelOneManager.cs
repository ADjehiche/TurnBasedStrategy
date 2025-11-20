using UnityEngine;

public class LevelOneReturnManager : MonoBehaviour
{
    [SerializeField] private Transform player;     //  Player here
    [SerializeField] private GameObject enemyRoot;
    [SerializeField] private LevelOneCaptionController captionController; // Reference to caption controller

    void Start()
    {
        // Put the player back to where they entered the battle from
        if (GameSession.HasReturnPosition && player != null)
        {
            var p = GameSession.ReturnPosition;
            p.y = player.position.y;   
            player.position = p;
            
            player.rotation = Quaternion.Euler(0, 30, 0);

            GameSession.HasReturnPosition = false; 
        }

        if (enemyRoot != null && GameSession.EnemyDefeated)
        {
            enemyRoot.SetActive(false);
        }

        // Ensure caption controller is available if not assigned in inspector
        if (captionController == null)
        {
            captionController = FindFirstObjectByType<LevelOneCaptionController>();
        }
    }
}