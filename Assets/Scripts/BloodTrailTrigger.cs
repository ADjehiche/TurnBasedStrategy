using UnityEngine;
using System.Collections;

/// <summary>
/// Triggers automatic Fragment dialogue when approaching blood trail
/// Only triggers after Fragment has joined the party
/// One-time trigger for atmospheric storytelling
/// </summary>
public class BloodTrailTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string fragmentComment = "[Fragment] Blood. Old blood.";
    [SerializeField] private string playerQuestion = "[You] Someone tried to escape before?";
    [SerializeField] private string fragmentResponse = "[Fragment] Many did. None succeeded.";
    [SerializeField] private string playerRealization = "[You] Until now.";

    [Header("Timing")]
    [SerializeField] private float dialogueDuration = 2f;
    [SerializeField] private float pauseBetweenLines = 0.5f;

    [Header("Voice")]
    [SerializeField] private AudioSource voiceSource;

    [SerializeField] private AudioClip bt_001_fragment_bloodOld;
    [SerializeField] private AudioClip bt_002_you_escapeBefore;
    [SerializeField] private AudioClip bt_003_fragment_manyNone;
    [SerializeField] private AudioClip bt_004_you_untilNow;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private bool hasTriggered = false;

    private void Start()
    {
        // Auto-grab AudioSource if not assigned
        if (voiceSource == null)
        {
            voiceSource = GetComponent<AudioSource>();
        }

        // Prevent any AudioSource from auto-playing at scene start
        if (voiceSource != null)
        {
            voiceSource.playOnAwake = false;
            voiceSource.loop = false;
            voiceSource.Stop();
            voiceSource.clip = null;
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

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player"))
            return;

        // Only trigger if Fragment is with the player
        if (!GameSession.CompanionActive)
        {
            if (showDebugLogs)
                Debug.Log("[BloodTrail] Fragment not active, skipping dialogue");
            return;
        }

        hasTriggered = true;

        if (showDebugLogs)
            Debug.Log("[BloodTrail] Triggering blood trail dialogue");

        StartCoroutine(BloodTrailDialogue());
    }

    private IEnumerator BloodTrailDialogue()
    {
        if (CaptionManager.Instance != null)
        {
            // Fragment notices
            PlayVoice(bt_001_fragment_bloodOld);
            CaptionManager.Instance.ShowMonologue(fragmentComment, dialogueDuration);
            yield return new WaitForSeconds(dialogueDuration + pauseBetweenLines);

            // Player asks
            PlayVoice(bt_002_you_escapeBefore);
            CaptionManager.Instance.ShowMonologue(playerQuestion, dialogueDuration);
            yield return new WaitForSeconds(dialogueDuration + pauseBetweenLines);

            // Fragment responds
            PlayVoice(bt_003_fragment_manyNone);
            CaptionManager.Instance.ShowMonologue(fragmentResponse, dialogueDuration);
            yield return new WaitForSeconds(dialogueDuration + pauseBetweenLines);

            // Player realizes they're different
            PlayVoice(bt_004_you_untilNow);
            CaptionManager.Instance.ShowMonologue(playerRealization, dialogueDuration);
        }
        else
        {
            Debug.Log(fragmentComment);
            Debug.Log(playerQuestion);
            Debug.Log(fragmentResponse);
            Debug.Log(playerRealization);
        }

        // Disable trigger after use
        GetComponent<Collider>().enabled = false;
    }
}
