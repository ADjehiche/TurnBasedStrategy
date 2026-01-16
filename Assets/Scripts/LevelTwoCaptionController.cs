using UnityEngine;
using System.Collections;

public class LevelTwoCaptionController : MonoBehaviour
{
    [Header("Archive Introduction")]
    [SerializeField] private string[] archiveArrivalDialogue = new string[]
    {
        "[Fragment] This place... it's an archive.",
        "[Fragment] Ancient texts. Records of the forgotten.",
        "[You] What is this place?"
    };

    [SerializeField] private string[] explorationDialogue = new string[]
    {
        "[Fragment] Be careful. These halls hold secrets.",
        "[You] I can feel something watching..."
    };

    [Header("Hallway Discovery")]
    [SerializeField] private string hallwayPrompt = "[Fragment] Through there... I sense something.";
    [SerializeField] private string hallwayApproach = "[You] A passage. Where does it lead?";

    [Header("Voice Clips (Level Two)")]
    [SerializeField] private AudioSource voiceSource;

    [SerializeField] private AudioClip l2_001_archive;
    [SerializeField] private AudioClip l2_002_texts;
    [SerializeField] private AudioClip l2_003_whatIsThis;
    [SerializeField] private AudioClip l2_004_beCareful;
    [SerializeField] private AudioClip l2_005_watching;
    [SerializeField] private AudioClip l2_006_throughThere;
    [SerializeField] private AudioClip l2_007_passage;

    [Header("Timing")]
    [SerializeField] private float startDelay = 1.5f;
    [SerializeField] private float dialoguePauseDuration = 2.5f;
    [SerializeField] private float monologueDuration = 2.5f;
    [SerializeField] private float hallwayPanDuration = 2f;

    [Header("Hallway Camera Pan")]
    [SerializeField] private Transform hallwayTarget;
    [SerializeField] private float panHoldDuration = 1f;

    [Header("Trigger Settings")]
    [SerializeField] private Transform hallwayTriggerArea;
    [SerializeField] private float hallwayTriggerDistance = 5f;

    private bool hasShownArrival = false;
    private bool hasShownHallwayPrompt = false;
    private bool isPanningToHallway = false;

    private Camera mainCamera;
    private Transform playerTransform;

    void Start()
    {
        mainCamera = Camera.main;
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Make sure we have an AudioSource
        if (voiceSource == null)
        {
            voiceSource = GetComponent<AudioSource>();
        }
        
        // Load saved state from GameSession
        hasShownArrival = GameSession.HasShownLevelTwoArrival;
        hasShownHallwayPrompt = GameSession.HasShownLevelTwoHallway;

        // ✅ Prevent any wrong clip from playing automatically at scene start
        if (voiceSource != null)
        {
            voiceSource.playOnAwake = false;
            voiceSource.loop = false;
            voiceSource.Stop();
            voiceSource.clip = null;
        }

        if (!hasShownArrival)
        {
            StartCoroutine(ArchiveArrivalSequence());
        }
    }

    void Update()
    {
        if (!hasShownHallwayPrompt && hallwayTriggerArea != null && playerTransform != null)
        {
            float distance = Vector3.Distance(playerTransform.position, hallwayTriggerArea.position);
            if (distance <= hallwayTriggerDistance)
            {
                TriggerHallwayDiscovery();
            }
        }
    }

    private IEnumerator ArchiveArrivalSequence()
    {
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.LockMovement("Archive arrival sequence");

        yield return new WaitForSeconds(startDelay);
        hasShownArrival = true;
        GameSession.HasShownLevelTwoArrival = true; // Persist to survive scene reload

        // Arrival dialogue (correct order)
        yield return StartCoroutine(ShowLineWithVoice(archiveArrivalDialogue[0], l2_001_archive));
        yield return new WaitForSeconds(dialoguePauseDuration);

        yield return StartCoroutine(ShowLineWithVoice(archiveArrivalDialogue[1], l2_002_texts));
        yield return new WaitForSeconds(dialoguePauseDuration);

        yield return StartCoroutine(ShowLineWithVoice(archiveArrivalDialogue[2], l2_003_whatIsThis));
        yield return new WaitForSeconds(dialoguePauseDuration);

        yield return new WaitForSeconds(1f);

        // Exploration dialogue
        yield return StartCoroutine(ShowLineWithVoice(explorationDialogue[0], l2_004_beCareful));
        yield return new WaitForSeconds(dialoguePauseDuration);

        yield return StartCoroutine(ShowLineWithVoice(explorationDialogue[1], l2_005_watching));
        yield return new WaitForSeconds(dialoguePauseDuration);

        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.UnlockMovement("Archive intro complete");
    }

    public void TriggerHallwayDiscovery()
    {
        if (hasShownHallwayPrompt) return;

        hasShownHallwayPrompt = true;
        GameSession.HasShownLevelTwoHallway = true; // Persist to survive scene reload
        StartCoroutine(HallwayDiscoverySequence());
    }

    private IEnumerator HallwayDiscoverySequence()
    {
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.LockMovement("Hallway discovery");

        // Fragment prompt + voice
        yield return StartCoroutine(ShowLineWithVoice(hallwayPrompt, l2_006_throughThere));

        // Pan camera
        if (hallwayTarget != null && mainCamera != null)
        {
            StartCoroutine(PanToHallway());
        }

        yield return new WaitForSeconds(hallwayPanDuration + panHoldDuration + hallwayPanDuration);

        // Player response + voice
        yield return StartCoroutine(ShowLineWithVoice(hallwayApproach, l2_007_passage));

        yield return new WaitForSeconds(monologueDuration);

        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.UnlockMovement("Hallway discovery complete");
    }

    private IEnumerator ShowLineWithVoice(string line, AudioClip clip)
    {
        // Show subtitle
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(line, monologueDuration);
        }

        // Play audio
        if (voiceSource != null)
        {
            voiceSource.Stop();
            voiceSource.clip = null;

            if (clip != null)
            {
                voiceSource.clip = clip;
                voiceSource.Play();
            }
        }

        yield return new WaitForSeconds(monologueDuration);
    }

    private IEnumerator PanToHallway()
    {
        if (isPanningToHallway) yield break;
        isPanningToHallway = true;

        Transform camTransform = mainCamera.transform;
        Transform camParent = camTransform.parent;

        Transform targetTransform = camParent != null ? camParent : camTransform;
        Quaternion originalRotation = targetTransform.rotation;

        Vector3 directionToHallway = (hallwayTarget.position - camTransform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToHallway);

        float elapsed = 0f;
        while (elapsed < hallwayPanDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hallwayPanDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            targetTransform.rotation = Quaternion.Slerp(originalRotation, targetRotation, smoothT);
            yield return null;
        }

        yield return new WaitForSeconds(panHoldDuration);

        elapsed = 0f;
        Quaternion currentRotation = targetTransform.rotation;
        while (elapsed < hallwayPanDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hallwayPanDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            targetTransform.rotation = Quaternion.Slerp(currentRotation, originalRotation, smoothT);
            yield return null;
        }

        targetTransform.rotation = originalRotation;
        isPanningToHallway = false;
    }

    public void OnPlayerEnterHallwayArea()
    {
        TriggerHallwayDiscovery();
    }

    // ✅ REQUIRED by MazeGuidanceController + TeleportBlueFragmentCollectable (fixes your CS1061 errors)
    public IEnumerator ShowDialogue(string speaker, string message, float duration)
    {
        if (CaptionManager.Instance != null)
        {
            string formattedMessage = $"[{speaker}] {message}";
            CaptionManager.Instance.ShowMonologue(formattedMessage, duration);
        }
        else
        {
            Debug.LogWarning("[LevelTwoCaptionController] CaptionManager not found!");
        }

        yield return new WaitForSeconds(duration);
    }

    /// <summary>
    /// Check if dialogue audio is currently playing
    /// </summary>
    public bool IsDialoguePlaying()
    {
        return voiceSource != null && voiceSource.isPlaying;
    }

    /// <summary>
    /// Get the AudioSource used for dialogue (for external systems to check)
    /// </summary>
    public AudioSource GetVoiceSource()
    {
        return voiceSource;
    }
}
