using UnityEngine;
using System;
using System.Collections.Generic;

public class CaptionVoicePlayer : MonoBehaviour
{
    [Header("Audio Output")]
    [SerializeField] private AudioSource voiceSource;

    [Header("Play Rules")]
    [SerializeField] private bool voiceInstructions = false; // usually no voice
    [SerializeField] private bool voiceMonologues = true;     // yes
    [SerializeField] private bool voiceSystemMessages = true; // ✅ you said YES

    [Header("Caption -> Audio Mapping (EXACT MATCH)")]
    [Tooltip("Caption text must match EXACTLY what you pass into CaptionManager (including [You] / [Fragment] / [System]).")]
    [SerializeField] private List<CaptionAudioEntry> mappings = new List<CaptionAudioEntry>();

    [Header("Debug")]
    [SerializeField] private bool logWhenNoClipFound = true;
    [SerializeField] private bool logWhenClipPlays = false;

    private Dictionary<string, AudioClip> lookup = new Dictionary<string, AudioClip>();

    void Awake()
    {
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

        BuildLookup();
    }

    void OnEnable()
    {
        CaptionManager.OnCaptionShown += HandleCaptionShown;
    }

    void OnDisable()
    {
        CaptionManager.OnCaptionShown -= HandleCaptionShown;
    }

    private void BuildLookup()
    {
        lookup.Clear();

        for (int i = 0; i < mappings.Count; i++)
        {
            if (mappings[i] == null) continue;

            string key = Normalize(mappings[i].captionExact);
            AudioClip clip = mappings[i].clip;

            if (string.IsNullOrWhiteSpace(key)) continue;
            if (clip == null) continue;

            // if duplicates exist, last one wins
            lookup[key] = clip;
        }
    }

    private void HandleCaptionShown(string captionText, CaptionType type)
    {
        if (!ShouldVoiceThisType(type)) return;
        if (voiceSource == null) return;
        if (string.IsNullOrWhiteSpace(captionText)) return;

        string key = Normalize(captionText);

        if (lookup.TryGetValue(key, out AudioClip clip) && clip != null)
        {
            voiceSource.Stop();
            voiceSource.clip = null;
            voiceSource.clip = clip;
            voiceSource.Play();

            if (logWhenClipPlays)
                Debug.Log($"[CaptionVoicePlayer] Playing: {clip.name} for caption: {captionText}");
        }
        else
        {
            if (logWhenNoClipFound)
                Debug.LogWarning($"[CaptionVoicePlayer] No audio mapped for: {captionText}");
        }
    }

    private bool ShouldVoiceThisType(CaptionType type)
    {
        if (type == CaptionType.Instruction) return voiceInstructions;
        if (type == CaptionType.Monologue) return voiceMonologues;
        if (type == CaptionType.System) return voiceSystemMessages;
        return true;
    }

    private string Normalize(string s)
    {
        if (s == null) return "";
        return s.Trim().Replace("\r", "").Replace("\n", "");
    }

    // If you add/modify mappings in inspector at runtime, this keeps lookup updated
    void OnValidate()
    {
        BuildLookup();
    }
}

[Serializable]
public class CaptionAudioEntry
{
    [TextArea(1, 3)]
    public string captionExact;

    public AudioClip clip;
}
