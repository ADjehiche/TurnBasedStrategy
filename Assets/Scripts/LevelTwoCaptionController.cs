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

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] archiveArrivalAudio;
    [SerializeField] private AudioClip[] explorationAudio;
    [SerializeField] private AudioClip hallwayPromptAudio;
    [SerializeField] private AudioClip hallwayApproachAudio;
    
    [Header("Audio Source")]
    [SerializeField] private AudioSource dialogueAudioSource;

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

        // Initialize AudioSource if not assigned
        if (dialogueAudioSource == null)
        {
            dialogueAudioSource = gameObject.AddComponent<AudioSource>();
            dialogueAudioSource.playOnAwake = false;
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
        for (int i = 0; i < archiveArrivalDialogue.Length; i++)
        {
            AudioClip audioClip = (archiveArrivalAudio != null && i < archiveArrivalAudio.Length) ? archiveArrivalAudio[i] : null;
            yield return StartCoroutine(ShowLineWithAudio(archiveArrivalDialogue[i], audioClip));
            yield return new WaitForSeconds(dialoguePauseDuration);
        }

        yield return new WaitForSeconds(1f);

        // Exploration dialogue (2)
        for (int i = 0; i < explorationDialogue.Length; i++)
        {
            AudioClip audioClip = (explorationAudio != null && i < explorationAudio.Length) ? explorationAudio[i] : null;
            yield return StartCoroutine(ShowLineWithAudio(explorationDialogue[i], audioClip));
            yield return new WaitForSeconds(dialoguePauseDuration);
        }

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

        // Fragment prompt + audio
        yield return StartCoroutine(ShowLineWithAudio(hallwayPrompt, hallwayPromptAudio));

        // Pan camera
        if (hallwayTarget != null && mainCamera != null)
        {
            StartCoroutine(PanToHallway());
        }

        yield return new WaitForSeconds(hallwayPanDuration + panHoldDuration + hallwayPanDuration);

        // Player response + audio
        yield return StartCoroutine(ShowLineWithAudio(hallwayApproach, hallwayApproachAudio));

        yield return new WaitForSeconds(monologueDuration);

        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.UnlockMovement("Hallway discovery complete");

        Debug.Log("[LevelTwoCaptionController] Hallway discovery sequence complete");
    }

    private IEnumerator ShowLineWithAudio(string line, AudioClip audioClip)
    {
        // Show caption text
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(line, monologueDuration, null);
        }

        // Play audio if provided
        if (audioClip != null && dialogueAudioSource != null)
        {
            dialogueAudioSource.clip = audioClip;
            dialogueAudioSource.Play();
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

    public IEnumerator ShowDialogue(string speaker, string message, float duration, AudioClip audioClip = null)
    {
        // Show caption text
        if (CaptionManager.Instance != null)
        {
            string formattedMessage = $"[{speaker}] {message}";
            CaptionManager.Instance.ShowMonologue(formattedMessage, duration, null);
        }
        else
        {
            Debug.LogWarning("[LevelTwoCaptionController] CaptionManager not found!");
        }

        // Play audio if provided
        if (audioClip != null && dialogueAudioSource != null)
        {
            dialogueAudioSource.clip = audioClip;
            dialogueAudioSource.Play();
        }

        yield return new WaitForSeconds(duration);
    }

    /// <summary>
    /// Check if dialogue audio is currently playing
    /// </summary>
    public bool IsDialoguePlaying()
    {
        return dialogueAudioSource != null && dialogueAudioSource.isPlaying;
    }

}
