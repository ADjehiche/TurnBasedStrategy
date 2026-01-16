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

        // Auto-grab AudioSource if not assigned
        if (voiceSource == null)
        {
            voiceSource = GetComponent<AudioSource>();
        }
        
        // Load saved state from GameSession
        hasShownArrival = GameSession.HasShownLevelTwoArrival;
        hasShownHallwayPrompt = GameSession.HasShownLevelTwoHallway;

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

        // Arrival dialogue (3)
        yield return StartCoroutine(ShowLineWithVoice(archiveArrivalDialogue[0], l2_001_archive));
        yield return new WaitForSeconds(dialoguePauseDuration);

        yield return StartCoroutine(ShowLineWithVoice(archiveArrivalDialogue[1], l2_002_texts));
        yield return new WaitForSeconds(dialoguePauseDuration);

        yield return StartCoroutine(ShowLineWithVoice(archiveArrivalDialogue[2], l2_003_whatIsThis));
        yield return new WaitForSeconds(dialoguePauseDuration);

        yield return new WaitForSeconds(1f);

        // Exploration dialogue (2)
        yield return StartCoroutine(ShowLineWithVoice(explorationDialogue[0], l2_004_beCareful));
        yield return new WaitForSeconds(dialoguePauseDuration);

        yield return StartCoroutine(ShowLineWithVoice(explorationDialogue[1], l2_005_watching));
        yield return new WaitForSeconds(dialoguePauseDuration);

        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.UnlockMovement("Archive intro complete");

        Debug.Log("[LevelTwoCaptionController] Archive arrival sequence complete");
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

        Debug.Log("[LevelTwoCaptionController] Hallway discovery sequence complete");
    }

    private IEnumerator ShowLineWithVoice(string line, AudioClip clip)
    {
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(line, monologueDuration);
        }

        // DEBUG: Check audio setup
        Debug.Log($"[LevelTwoCaptionController] Audio Debug - voiceSource: {(voiceSource != null ? "Found" : "NULL")}, clip: {(clip != null ? clip.name : "NULL")}");

        if (voiceSource != null && clip != null)
        {
            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.Play();
            Debug.Log($"[LevelTwoCaptionController] Playing audio: {clip.name}");
        }
        else
        {
            if (voiceSource == null) Debug.LogError("[LevelTwoCaptionController] voiceSource is NULL! Add AudioSource component to this GameObject.");
            if (clip == null) Debug.LogError("[LevelTwoCaptionController] AudioClip is NULL! Assign audio clips in the inspector.");
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

    public IEnumerator ShowDialogue(string speaker, string message, float duration)
    {
        if (CaptionManager.Instance != null)
        {
            string formattedMessage = $"[{speaker}] {message}";
            CaptionManager.Instance.ShowMonologue(formattedMessage, duration);

            // optional: no voice here unless you add mapping
            yield return new WaitForSeconds(duration);
        }
        else
        {
            Debug.LogWarning("[LevelTwoCaptionController] CaptionManager not found!");
            yield return new WaitForSeconds(duration);
        }
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
