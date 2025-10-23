using UnityEngine;
using UnityEngine.SceneManagement; // Needed for scene loading

public class TryAgainButton : MonoBehaviour
{
    // This method will be called when the button is clicked
    public void RestartLevel()
    {
        SceneManager.LoadScene("TitleScene");
    }
}