using UnityEngine;
using UnityEngine.SceneManagement; // Needed for scene loading

public class TryAgainButton : MonoBehaviour
{
    // This method will be called when the button is clicked
    public void RestartLevel()
    {
        // Option 1: Load by scene name
        SceneManager.LoadScene("LevelOne");

        // Option 2 (alternative): Load by scene index
        // SceneManager.LoadScene(1);
    }
}