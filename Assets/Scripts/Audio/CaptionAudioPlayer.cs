using System;
using System.Collections.Generic;
using UnityEngine;

public class CaptionAudioPlayer : MonoBehaviour
{
    [Serializable]
    public class AudioEntry
    {
        [TextArea(1, 3)]
        public string captionText;   // MUST match what appears on screen (including [You], [Fragment], etc.)
        public AudioClip audioClip;
    }

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Caption -> Audio Mapping")]
    [SerializeField] private List<AudioEntry> audioEntries = new List<AudioEntry>();

    [Header("Settings")]
    [SerializeField] private bool stopPreviousLine = true;
    [SerializeField] private bool debugLogs = true;

    private Dictionary<string, AudioClip> map;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        BuildMap();
    }

    private void OnEnable()
    {
        CaptionManager.OnCaptionShown += HandleCaptionShown;
    }

    private void OnDisable()
    {
        CaptionManager.OnCaptionShown -= HandleCaptionShown;
    }

    private void BuildMap()
    {
        map = new Dictionary<string, AudioClip>();

        foreach (var e in audioEntries)
        {
            if (e == null) continue;

            string key = Normalize(e.captionText);
            if (string.IsNullOrEmpty(key) || e.audioClip == null) continue;

            // last one wins if duplicates
            map[key] = e.audioClip;
        }

        if (debugLogs)
            Debug.Log($"[CaptionAudioPlayer] Map built. Entries={map.Count}");
    }

    private void HandleCaptionShown(string captionText, CaptionType type)
    {
        // OPTIONAL: skip system lines if you didn't record them
        if (type == CaptionType.System) return;

        if (audioSource == null) return;

        string key = Normalize(captionText);

        if (map != null && map.TryGetValue(key, out AudioClip clip) && clip != null)
        {
            if (stopPreviousLine) audioSource.Stop();

            audioSource.clip = clip;
            audioSource.Play();

            if (debugLogs)
                Debug.Log($"[CaptionAudioPlayer] Playing: {clip.name} for caption: {captionText}");
        }
        else
        {
            if (debugLogs)
                Debug.LogWarning($"[CaptionAudioPlayer] No audio mapped for caption: {captionText}");
        }
    }

    private string Normalize(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        return s.Trim().Replace("\r", "").Replace("\n", " ");
    }

    // If you edit entries while playing and want to rebuild quickly
    [ContextMenu("Rebuild Map")]
    private void RebuildMapContext()
    {
        BuildMap();
    }
}
