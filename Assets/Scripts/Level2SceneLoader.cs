using UnityEngine;
using UnityEngine.SceneManagement; // Required for changing levels
using UnityEngine.Playables;
using System.Collections;

public class Level2SceneLoader : MonoBehaviour
{
    [Header("Settings")]
    public PlayableDirector fadeTimeline; // Drag your Exit Timeline here
    public string nextLevelName = "LevelTwo"; // TYPE EXACT SCENE NAME HERE

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(TransitionSequence());
        }
    }

    IEnumerator TransitionSequence()
    {
        // 1. Play the Fade to Black
        if (fadeTimeline != null)
        {
            fadeTimeline.Play();
            // Wait for the timeline to finish (e.g. 1 second)
            yield return new WaitForSeconds((float)fadeTimeline.duration);
        }

        // 2. Load the next level
        SceneManager.LoadScene(nextLevelName);
    }
}