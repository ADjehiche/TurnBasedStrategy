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
            
            // Trigger final dungeon escape objective
            var objectiveManager = FindFirstObjectByType<SimpleLevelOneObjectives>();
            if (objectiveManager != null)
            {
                objectiveManager.OnDungeonEscaped();
                Debug.Log("[Level2SceneLoader] Final dungeon escape objective triggered");
            }
            
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

        // 2. Clear previous level state (checkpoints, return positions)
        GameSession.ClearPositionalFlags();
        
        // 3. Reset Level 2 specific state (Combat Wing battle, fragments)
        // These should only persist WITHIN Level 2, not from Level 1
        GameSession.CombatWingVictory = false;
        GameSession.RedFragmentSpawnPosition = Vector3.zero;
        GameSession.HasCollectedRedFragment = false;
        GameSession.HasCollectedBlueFragment = false;
        GameSession.RedCompanionActive = false;
        GameSession.BlueCompanionActive = false;
        Debug.Log("[Level2SceneLoader] Reset Level 2 battle/fragment state for fresh start");
        
        // 4. Load the next level
        SceneManager.LoadScene(nextLevelName);
    }
}