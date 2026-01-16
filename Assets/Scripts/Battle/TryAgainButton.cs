using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the Try Again button on the death screen
/// Reloads to checkpoint instead of full level restart
/// </summary>
public class TryAgainButton : MonoBehaviour
{
    // Called by Unity button OnClick event
    public void RestartLevel()
    {
        Debug.Log("[TryAgainButton] Try Again clicked - loading checkpoint");
        
        // GameSession.IsRespawning was already set by PlayerHealth.OnPlayerDeath()
        // This tells LevelOneReturnManager to use the checkpoint
        
        // Load the correct level (will spawn at checkpoint)
        SceneManager.LoadScene(GameSession.ReturnSceneName);
    }
}
