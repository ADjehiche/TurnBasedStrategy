using UnityEngine;
using System.Collections;

public class LevelOneCaptionController : MonoBehaviour
{
    [Header("Caption Messages")]
    [SerializeField] private string wakeUpInstruction = "[You] Where am I?";
    [SerializeField] private string wakeUpMonologue = "[You] My head... I can't remember anything.";
    [SerializeField] private string hallwayObservation = "[You] These ancient pillars...";
    [SerializeField] private string keyPickupMessage = "[System] Cell key";
    [SerializeField] private string keyPickupMonologue = "[You] A way out.";
    [SerializeField] private string doorOpenCelebration = "[You] Free. For now.";
    [SerializeField] private string skeletonDefeated = "[System] Guardian defeated";
    [SerializeField] private string skeletonKeyPickup = "[You] This key... it glows.";

    [Header("Voice Clips (Level One)")]
    [SerializeField] private AudioSource voiceSource;

    [SerializeField] private AudioClip st001_whereAmI;
    [SerializeField] private AudioClip st002_myHead;
    [SerializeField] private AudioClip st003_pillars;
    [SerializeField] private AudioClip st005_keyDoor;
    [SerializeField] private AudioClip st006_freeAtLast;
    [SerializeField] private AudioClip st008_keyGlows;

    [Header("Timing")]
    [SerializeField] private float startDelay = 2f;
    [SerializeField] private float monologueDuration = 2.5f;

    void Start()
    {
        // Make sure voiceSource can't auto-play the wrong thing
        if (voiceSource == null)
        {
            voiceSource = GetComponent<AudioSource>();
        }

        if (voiceSource != null)
        {
            voiceSource.playOnAwake = false;
            voiceSource.loop = false;
            voiceSource.Stop();
            voiceSource.clip = null;
        }

        if (!GameSession.HasShownStartInstruction)
        {
            StartCoroutine(WakeUpSequence());
        }
    }

    private void PlayVoice(AudioClip clip)
    {
        if (voiceSource == null || clip == null) return;

        voiceSource.Stop();
        voiceSource.clip = null;
        voiceSource.clip = clip;
        voiceSource.Play();
    }

    private IEnumerator WakeUpSequence()
    {
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.LockMovement("Wake-up sequence");

        yield return new WaitForSeconds(startDelay);

        if (!GameSession.HasShownStartInstruction && CaptionManager.Instance != null)
        {
            // Confusion
            PlayVoice(st001_whereAmI);
            CaptionManager.Instance.ShowInstruction(wakeUpInstruction, 2f);
            yield return new WaitForSeconds(2.5f);

            // Camera pan (optional)
            if (CameraPanner.Instance != null)
            {
                StartCoroutine(CameraPanner.Instance.PanLookAround(3f, 45f));
            }

            // Blink (optional)
            if (ScreenBlinker.Instance != null)
            {
                StartCoroutine(ScreenBlinker.Instance.BlinkMultiple(2, 0.3f, 0.3f));
            }

            // Amnesia
            PlayVoice(st002_myHead);
            CaptionManager.Instance.ShowMonologue(wakeUpMonologue, monologueDuration);
            yield return new WaitForSeconds(monologueDuration + 1f);

            GameSession.HasShownStartInstruction = true;
        }

        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.UnlockMovement("Wake-up complete");
    }

    public void OnEnterHallway()
    {
        if (CaptionManager.Instance != null)
        {
            PlayVoice(st003_pillars);
            CaptionManager.Instance.ShowMonologue(hallwayObservation, 2f);
        }
    }

    public void OnKeyPickedUp()
    {
        if (!GameSession.HasShownKeyPickup && CaptionManager.Instance != null)
        {
            StartCoroutine(ShowKeyPickupSequence());
            GameSession.HasShownKeyPickup = true;
        }
    }

    private IEnumerator ShowKeyPickupSequence()
    {
        // System message (no voice)
        CaptionManager.Instance.ShowSystemMessage(keyPickupMessage, 1.5f);
        yield return new WaitForSeconds(2f);

        // Player thought (voice)
        PlayVoice(st005_keyDoor);
        CaptionManager.Instance.ShowMonologue(keyPickupMonologue, monologueDuration);
    }

    public void OnDoorOpened()
    {
        if (!GameSession.HasShownDoorOpen && CaptionManager.Instance != null)
        {
            PlayVoice(st006_freeAtLast);
            CaptionManager.Instance.ShowMonologue(doorOpenCelebration, monologueDuration);
            GameSession.HasShownDoorOpen = true;
        }
    }

    public void OnSkeletonDefeated()
    {
        if (!GameSession.HasShownEnemySpotted && CaptionManager.Instance != null)
        {
            StartCoroutine(ShowSkeletonDefeatedSequence());
            GameSession.HasShownEnemySpotted = true;
        }
    }

    private IEnumerator ShowSkeletonDefeatedSequence()
    {
        // System: Defeated (no voice)
        CaptionManager.Instance.ShowSystemMessage(skeletonDefeated, 1.5f);
        yield return new WaitForSeconds(2f);

        // Player line (voice)
        PlayVoice(st008_keyGlows);
        CaptionManager.Instance.ShowMonologue(skeletonKeyPickup, 2.5f);
    }

    // ✅ REQUIRED by EnemyLookDetector (fixes your CS1061 error)
    public void OnEnemySpotted()
    {
        Debug.Log("[LevelOneCaptionController] Enemy spotted (compatibility method).");
    }

    [ContextMenu("Reset Caption States")]
    public void ResetStates()
    {
        GameSession.HasShownStartInstruction = false;
        GameSession.HasShownKeyPickup = false;
        GameSession.HasShownDoorOpen = false;
        GameSession.HasShownEnemySpotted = false;
    }
}
