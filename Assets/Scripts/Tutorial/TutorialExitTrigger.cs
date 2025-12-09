using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Triggers when player walks into the exit light
/// Fades to white and returns to main menu
/// </summary>
public class TutorialExitTrigger : MonoBehaviour
{
    [Header("Exit Settings")]
    [SerializeField] private string mainMenuSceneName = "TitleScene";
    [SerializeField] private float fadeToWhiteDuration = 2f;
    
    [Header("Optional Fade Panel")]
    [SerializeField] private GameObject whiteFadePanel; // UI panel that fades to white
    
    private bool hasTriggered = false;
    
    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;
        
        hasTriggered = true;
        StartCoroutine(ExitTutorial());
    }
    
    private IEnumerator ExitTutorial()
    {
        // Optional: Fade to white using UI panel
        if (whiteFadePanel != null)
        {
            whiteFadePanel.SetActive(true);
            var canvasGroup = whiteFadePanel.GetComponent<CanvasGroup>();
            
            if (canvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < fadeToWhiteDuration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeToWhiteDuration);
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(fadeToWhiteDuration);
            }
        }
        else
        {
            yield return new WaitForSeconds(fadeToWhiteDuration);
        }
        
        // Load main menu
        Debug.Log("[TutorialExit] Loading main menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
