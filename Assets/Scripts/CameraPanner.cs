using UnityEngine;
using System.Collections;

/// <summary>
/// Handles cinematic camera panning for cutscenes
/// Used for looking around the environment during wake-up sequence
/// </summary>
public class CameraPanner : MonoBehaviour
{
    public static CameraPanner Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                cameraTransform = mainCam.transform;
        }
    }
    
    /// <summary>
    /// Pan camera to look around (left to right)
    /// </summary>
    public IEnumerator PanLookAround(float duration = 3f, float angle = 60f)
    {
        if (cameraTransform == null)
        {
            Debug.LogError("[CameraPanner] No camera transform!");
            yield break;
        }
        
        if (showDebugLogs)
            Debug.Log($"[CameraPanner] Starting pan - Duration: {duration}s, Angle: {angle}°");
        
        Quaternion startRotation = cameraTransform.localRotation;
        float halfDuration = duration / 2f;
        
        // Pan left
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float currentAngle = Mathf.Lerp(0, -angle / 2, t);
            cameraTransform.localRotation = startRotation * Quaternion.Euler(0, currentAngle, 0);
            yield return null;
        }
        
        // Pan right
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float currentAngle = Mathf.Lerp(-angle / 2, angle / 2, t);
            cameraTransform.localRotation = startRotation * Quaternion.Euler(0, currentAngle, 0);
            yield return null;
        }
        
        // Return to center
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float currentAngle = Mathf.Lerp(angle / 2, 0, t);
            cameraTransform.localRotation = startRotation * Quaternion.Euler(0, currentAngle, 0);
            yield return null;
        }
        
        // Ensure we're back to start
        cameraTransform.localRotation = startRotation;
        
        if (showDebugLogs)
            Debug.Log("[CameraPanner] Pan complete");
    }
}
