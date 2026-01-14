using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages all maze-related dialogue and fragment guidance mechanics
/// </summary>
public class MazeGuidanceController : MonoBehaviour
{
    [Header("Fragment References")]
    [SerializeField] private CompanionFollower yellowFragment;
    [SerializeField] private CompanionFollower blueFragmentFollower; // Blue fragment as follower after collection
    
    // Blue fragment will be found automatically since it spawns randomly
    private GameObject blueFragment;
    
    [Header("Maze References")]
    [SerializeField] private Transform mazeEntrance; // Position of maze entrance
    [SerializeField] private MazeAreaTrigger mazeAreaTrigger; // Trigger that detects maze entry/exit
    
    [Header("Dialogue References")]
    [SerializeField] private LevelTwoCaptionController captionController;
    
    [Header("Timer Settings")]
    [SerializeField] private float helpTimerDuration = 600f; // 10 minutes in seconds
    [SerializeField] private bool debugMode = false;
    
    [Header("Guidance Settings")]
    [SerializeField] private float guidanceSpeed = 8f; // Speed when guiding player
    [SerializeField] private float guidanceStopDistance = 2f; // Distance to stop from target
    [SerializeField] private Color guidanceGlowColor = Color.cyan; // Visual indicator when guiding
    
    // State tracking
    private bool playerInMaze = false;
    private bool hasEnteredMazeOnce = false;
    private float timeInMaze = 0f;
    private bool helpDialogueTriggered = false;
    private bool blueFragmentFound = false;
    private bool isGuidingToBlue = false;
    private bool isGuidingToEntrance = false;
    
    // Player reference
    private Transform player;
    
    // Coroutine references
    private Coroutine guidanceCoroutine;
    
    void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("[MazeGuidanceController] Player not found!");
        }
        
        // Auto-find blue fragment if not found (since it spawns randomly)
        FindBlueFragment();
        
        // Validate references
        ValidateReferences();
        
        if (debugMode)
        {
            Debug.Log($"[MazeGuidanceController] Initialized. Help timer: {helpTimerDuration}s");
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        UpdateMazeTimer();
    }
    
    /// <summary>
    /// Called by MazeAreaTrigger when player enters maze
    /// </summary>
    public void OnPlayerEnteredMazeArea()
    {
        if (playerInMaze) return; // Already in maze
        
        playerInMaze = true;
        
        if (debugMode)
        {
            Debug.Log("[MazeGuidanceController] Player entered maze - starting timer");
        }
        
        // Show entry dialogue if first time
        if (!hasEnteredMazeOnce)
        {
            ShowMazeEntryDialogue();
            hasEnteredMazeOnce = true;
        }
        
        // Reset timer
        timeInMaze = 0f;
        helpDialogueTriggered = false;
    }
    
    /// <summary>
    /// Called by MazeAreaTrigger when player exits maze
    /// </summary>
    public void OnPlayerExitedMazeArea()
    {
        if (!playerInMaze) return; // Already outside maze
        
        playerInMaze = false;
        
        if (debugMode)
        {
            Debug.Log("[MazeGuidanceController] Player exited maze - stopping timer");
        }
        
        // Stop any active guidance
        StopGuidance();
        
        // Reset timer
        timeInMaze = 0f;
        helpDialogueTriggered = false;
    }
    
    /// <summary>
    /// Find the blue fragment in the scene (since it spawns randomly)
    /// </summary>
    private void FindBlueFragment()
    {
        // Look for BlueFragmentCollectable script
        BlueFragmentCollectable collectableScript = FindFirstObjectByType<BlueFragmentCollectable>();
        if (collectableScript != null)
        {
            blueFragment = collectableScript.gameObject;
            if (debugMode)
            {
                Debug.Log($"[MazeGuidanceController] Found blue fragment at: {blueFragment.transform.position}");
            }
        }
        else
        {
            // If no BlueFragmentCollectable found, look for objects with specific tags or names
            GameObject foundBlob = GameObject.FindGameObjectWithTag("BlueFragment");
            if (foundBlob == null)
            {
                foundBlob = GameObject.Find("BlueFragment");
            }
            if (foundBlob == null)
            {
                foundBlob = GameObject.Find("Blob");
            }
            
            if (foundBlob != null)
            {
                blueFragment = foundBlob;
                
                // Add the collectable script if it doesn't exist
                if (foundBlob.GetComponent<BlueFragmentCollectable>() == null)
                {
                    foundBlob.AddComponent<BlueFragmentCollectable>();
                    if (debugMode)
                    {
                        Debug.Log("[MazeGuidanceController] Added BlueFragmentCollectable to found blob");
                    }
                }
                
                if (debugMode)
                {
                    Debug.Log($"[MazeGuidanceController] Found blue fragment by search at: {blueFragment.transform.position}");
                }
            }
            else
            {
                if (debugMode)
                {
                    Debug.LogWarning("[MazeGuidanceController] Blue fragment not found! Make sure it's spawned and has BlueFragmentCollectable script or appropriate tag/name.");
                }
            }
        }
    }
    
    /// <summary>
    /// Called by external scripts (like MazeGenerator) when blue fragment is spawned
    /// </summary>
    public void OnBlueFragmentSpawned(GameObject spawnedBlueFragment)
    {
        if (spawnedBlueFragment != null)
        {
            blueFragment = spawnedBlueFragment;
            
            // Ensure it has the collectable script
            if (blueFragment.GetComponent<BlueFragmentCollectable>() == null)
            {
                blueFragment.AddComponent<BlueFragmentCollectable>();
            }
            
            if (debugMode)
            {
                Debug.Log($"[MazeGuidanceController] Blue fragment registered at: {blueFragment.transform.position}");
            }
        }
    }
    
    /// <summary>
    /// Refresh the blue fragment reference (useful if it respawns)
    /// </summary>
    public void RefreshBlueFragmentReference()
    {
        FindBlueFragment();
    }
    
    /// <summary>
    /// Update the maze timer and trigger help if needed
    /// Timer runs continuously while playerInMaze is true (set by entry/exit events)
    /// </summary>
    private void UpdateMazeTimer()
    {
        // Only run timer if player is marked as inside maze (via entry/exit events)
        if (!playerInMaze || helpDialogueTriggered || blueFragmentFound) return;
        
        // If we don't have a blue fragment reference yet, try to find it
        if (blueFragment == null)
        {
            FindBlueFragment();
        }
        
        timeInMaze += Time.deltaTime;
        
        if (debugMode && Time.frameCount % 60 == 0) // Log every second (assuming 60 FPS)
        {
            Debug.Log($"[MazeGuidanceController] Time in maze: {timeInMaze:F1}s / {helpTimerDuration}s");
        }
        
        // Check if help should be triggered
        if (timeInMaze >= helpTimerDuration)
        {
            TriggerYellowFragmentHelp();
        }
    }
    
    /// <summary>
    /// Show dialogue when player first enters maze
    /// </summary>
    private void ShowMazeEntryDialogue()
    {
        if (captionController == null) return;
        
        StartCoroutine(MazeEntryDialogueSequence());
    }
    
    /// <summary>
    /// Maze entry dialogue sequence
    /// </summary>
    private IEnumerator MazeEntryDialogueSequence()
    {
        yield return captionController.ShowDialogue("Fragment", "This place... it's like a labyrinth of knowledge.", 3f);
        yield return new WaitForSeconds(0.5f);
        yield return captionController.ShowDialogue("Player", "Stay close. We need to find what we're looking for in here.", 3f);
        yield return new WaitForSeconds(0.5f);
        yield return captionController.ShowDialogue("Fragment", "I sense something important deeper within. But this maze is vast...", 3f);
    }
    
    /// <summary>
    /// Trigger yellow fragment help dialogue and guidance
    /// </summary>
    private void TriggerYellowFragmentHelp()
    {
        if (helpDialogueTriggered) return;
        
        helpDialogueTriggered = true;
        
        if (debugMode)
        {
            Debug.Log("[MazeGuidanceController] Triggering yellow fragment help");
        }
        
        StartCoroutine(YellowFragmentHelpSequence());
    }
    
    /// <summary>
    /// Yellow fragment help dialogue and guidance sequence
    /// </summary>
    private IEnumerator YellowFragmentHelpSequence()
    {
        yield return captionController.ShowDialogue("Fragment", "You've been wandering for quite some time...", 3f);
        yield return new WaitForSeconds(0.5f);
        yield return captionController.ShowDialogue("Fragment", "I can sense another fragment nearby. Let me guide you to it.", 3f);
        yield return new WaitForSeconds(0.5f);
        yield return captionController.ShowDialogue("Player", "That would be helpful. Lead the way.", 2f);
        
        // Start guidance to blue fragment
        StartGuidanceToBlueFragment();
    }
    
    /// <summary>
    /// Start yellow fragment guiding player to blue fragment
    /// </summary>
    private void StartGuidanceToBlueFragment()
    {
        if (blueFragment == null || yellowFragment == null)
        {
            Debug.LogError("[MazeGuidanceController] Cannot start guidance - missing references");
            return;
        }
        
        isGuidingToBlue = true;
        
        // Stop normal following behavior
        yellowFragment.SetFollowing(false);
        
        if (debugMode)
        {
            Debug.Log("[MazeGuidanceController] Starting guidance to blue fragment");
        }
        
        // Start guidance coroutine
        if (guidanceCoroutine != null)
        {
            StopCoroutine(guidanceCoroutine);
        }
        guidanceCoroutine = StartCoroutine(GuideToTarget(blueFragment.transform, OnReachedBlueFragment));
    }
    
    /// <summary>
    /// Called when blue fragment is found and collected
    /// </summary>
    public void OnBlueFragmentCollected()
    {
        if (debugMode)
        {
            Debug.Log("[MazeGuidanceController] Blue fragment collected");
        }
        
        blueFragmentFound = true;
        
        // Stop guidance to blue fragment
        if (isGuidingToBlue)
        {
            StopGuidance();
            isGuidingToBlue = false;
        }
        
        StartCoroutine(BlueFragmentCollectedSequence());
    }
    
    /// <summary>
    /// Dialogue sequence when blue fragment is collected
    /// </summary>
    private IEnumerator BlueFragmentCollectedSequence()
    {
        yield return new WaitForSeconds(1f); // Wait for collection animation
        
        yield return captionController.ShowDialogue("Blue Fragment", "Thank you for freeing me from this maze!", 3f);
        yield return new WaitForSeconds(0.5f);
        yield return captionController.ShowDialogue("Player", "We should get out of here. Can you guide us back?", 3f);
        yield return new WaitForSeconds(0.5f);
        yield return captionController.ShowDialogue("Blue Fragment", "Of course! I know these paths well. Follow me!", 3f);
        
        // Start guidance back to entrance
        StartGuidanceToMazeEntrance();
    }
    
    /// <summary>
    /// Start blue fragment guiding player back to maze entrance
    /// </summary>
    private void StartGuidanceToMazeEntrance()
    {
        if (mazeEntrance == null || blueFragmentFollower == null)
        {
            Debug.LogError("[MazeGuidanceController] Cannot start guidance to entrance - missing references");
            return;
        }
        
        isGuidingToEntrance = true;
        
        // Stop normal following behavior for blue fragment
        blueFragmentFollower.SetFollowing(false);
        
        if (debugMode)
        {
            Debug.Log("[MazeGuidanceController] Starting guidance to maze entrance");
        }
        
        // Start guidance coroutine
        if (guidanceCoroutine != null)
        {
            StopCoroutine(guidanceCoroutine);
        }
        guidanceCoroutine = StartCoroutine(GuideToTarget(mazeEntrance, OnReachedMazeEntrance));
    }
    
    /// <summary>
    /// Generic guidance coroutine
    /// </summary>
    private IEnumerator GuideToTarget(Transform target, System.Action onReached)
    {
        CompanionFollower activeGuide = isGuidingToBlue ? yellowFragment : blueFragmentFollower;
        
        if (activeGuide == null || target == null)
        {
            yield break;
        }
        
        // Visual indicator that fragment is guiding
        // TODO: Add glow effect or other visual indicator
        
        while (Vector3.Distance(player.position, target.position) > guidanceStopDistance)
        {
            // Move guide towards target
            Vector3 targetPosition = Vector3.MoveTowards(
                activeGuide.transform.position,
                target.position,
                guidanceSpeed * Time.deltaTime
            );
            
            // Keep guide between player and target, but closer to target
            Vector3 playerToTarget = (target.position - player.position).normalized;
            Vector3 idealPosition = player.position + playerToTarget * 3f; // 3 units ahead of player
            
            // Blend between direct path to target and staying near player
            targetPosition = Vector3.Lerp(targetPosition, idealPosition, 0.3f);
            
            activeGuide.transform.position = targetPosition;
            
            // Make guide face the target
            Vector3 lookDirection = (target.position - activeGuide.transform.position).normalized;
            if (lookDirection != Vector3.zero)
            {
                activeGuide.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
            
            yield return null;
        }
        
        // Reached target
        onReached?.Invoke();
    }
    
    /// <summary>
    /// Called when guide reaches blue fragment
    /// </summary>
    private void OnReachedBlueFragment()
    {
        if (debugMode)
        {
            Debug.Log("[MazeGuidanceController] Reached blue fragment location");
        }
        
        // Resume normal following
        yellowFragment.SetFollowing(true);
        isGuidingToBlue = false;
        
        // Show dialogue
        StartCoroutine(ReachedBlueFragmentDialogue());
    }
    
    /// <summary>
    /// Dialogue when reaching blue fragment
    /// </summary>
    private IEnumerator ReachedBlueFragmentDialogue()
    {
        yield return captionController.ShowDialogue("Fragment", "There! I can sense it strongly now.", 2f);
        yield return new WaitForSeconds(0.5f);
        yield return captionController.ShowDialogue("Fragment", "The blue fragment should be very close. Look around!", 3f);
    }
    
    /// <summary>
    /// Called when guide reaches maze entrance
    /// </summary>
    private void OnReachedMazeEntrance()
    {
        if (debugMode)
        {
            Debug.Log("[MazeGuidanceController] Reached maze entrance");
        }
        
        // Resume normal following
        if (blueFragmentFollower != null)
        {
            blueFragmentFollower.SetFollowing(true);
        }
        isGuidingToEntrance = false;
        
        // Show dialogue
        StartCoroutine(ReachedEntranceDialogue());
    }
    
    /// <summary>
    /// Dialogue when reaching maze entrance
    /// </summary>
    private IEnumerator ReachedEntranceDialogue()
    {
        yield return captionController.ShowDialogue("Blue Fragment", "Here we are - back to the entrance!", 2f);
        yield return new WaitForSeconds(0.5f);
        yield return captionController.ShowDialogue("Player", "Thanks for the help. That maze was more confusing than I thought.", 3f);
        yield return new WaitForSeconds(0.5f);
        yield return captionController.ShowDialogue("Blue Fragment", "Happy to help! Now we can continue our journey together.", 3f);
    }
    
    /// <summary>
    /// Stop any active guidance
    /// </summary>
    private void StopGuidance()
    {
        if (guidanceCoroutine != null)
        {
            StopCoroutine(guidanceCoroutine);
            guidanceCoroutine = null;
        }
        
        // Resume normal following for both fragments
        if (yellowFragment != null && isGuidingToBlue)
        {
            yellowFragment.SetFollowing(true);
            isGuidingToBlue = false;
        }
        
        if (blueFragmentFollower != null && isGuidingToEntrance)
        {
            blueFragmentFollower.SetFollowing(true);
            isGuidingToEntrance = false;
        }
    }
    
    /// <summary>
    /// Validate all required references
    /// </summary>
    private void ValidateReferences()
    {
        if (yellowFragment == null)
            Debug.LogError("[MazeGuidanceController] Yellow Fragment reference not assigned!");
        
        // Blue fragment is found automatically, so we only warn if it's not found after searching
        if (blueFragment == null)
            Debug.LogWarning("[MazeGuidanceController] Blue Fragment not found automatically. It may not be spawned yet or may need to be tagged properly.");
        
        if (blueFragmentFollower == null)
            Debug.LogError("[MazeGuidanceController] Blue Fragment Follower reference not assigned!");
        
        if (mazeEntrance == null)
            Debug.LogError("[MazeGuidanceController] Maze Entrance reference not assigned!");
        
        if (mazeAreaTrigger == null)
            Debug.LogError("[MazeGuidanceController] Maze Area Trigger reference not assigned!");
        
        if (captionController == null)
            Debug.LogError("[MazeGuidanceController] Caption Controller reference not assigned!");
    }
    
    // Public methods for external scripts
    public bool IsPlayerInMazeArea() => playerInMaze;
    public float GetTimeInMaze() => timeInMaze;
    public bool HasTriggeredHelp() => helpDialogueTriggered;
    public bool IsGuidingPlayer() => isGuidingToBlue || isGuidingToEntrance;
    
    // Debug methods
    [ContextMenu("Force Trigger Help")]
    private void ForceTriggerHelp()
    {
        if (Application.isPlaying)
        {
            TriggerYellowFragmentHelp();
        }
    }
    
    [ContextMenu("Simulate Blue Fragment Collection")]
    private void SimulateBlueFragmentCollection()
    {
        if (Application.isPlaying)
        {
            OnBlueFragmentCollected();
        }
    }
}