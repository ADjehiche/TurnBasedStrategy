// CameraZoomFocus.cs
using UnityEngine;
using System.Collections;

/// <summary>
/// Creates a zoom/focus effect on a target object
/// Used for highlighting important objects like the key on the dead skeleton
/// Pans camera to look at target during zoom
/// </summary>
public class CameraZoomFocus : MonoBehaviour
{
    [Header("Zoom Settings")]
    [Tooltip("Target to zoom towards")]
    [SerializeField] private Transform focusTarget;

    [Tooltip("How much to zoom in (lower = more zoom)")]
    [SerializeField] private float zoomFOV = 40f;

    [Tooltip("How long the zoom lasts")]
    [SerializeField] private float zoomDuration = 2f;

    [Tooltip("Vertical offset to aim higher on target")]
    [SerializeField] private float verticalOffset = 0.5f;

    [Header("Trigger Settings")]
    [Tooltip("Auto-trigger when player enters this trigger zone")]
    [SerializeField] private bool autoTrigger = true;

    [Header("Play Once (prevents replay after respawn/load)")]
    [SerializeField] private bool playOnce = true;

    [Tooltip("Unique id for this cutscene/zoom. Leave empty to auto-generate from scene + object name.")]
    [SerializeField] private string playOnceId = "";

    [Header("Caption")]
    [SerializeField] private bool showCaption = true;
    [SerializeField] private string captionMessage = "[You] A key... on that skeleton.";
    [SerializeField] private float captionDuration = 2.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private Camera playerCamera;
    private float originalFOV;
    private Quaternion originalCameraRotation;
    private bool hasTriggered = false;
    private Coroutine _zoomRoutine;

    string PrefKey
    {
        get
        {
            string id = string.IsNullOrWhiteSpace(playOnceId)
                ? $"{gameObject.scene.name}/{gameObject.name}"
                : playOnceId.Trim();
            return $"ZoomFocus_Played_{id}";
        }
    }

    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera != null)
            originalFOV = playerCamera.fieldOfView;

        // Prevent replay after respawn/load
        if (playOnce && PlayerPrefs.GetInt(PrefKey, 0) == 1)
        {
            hasTriggered = true;
            autoTrigger = false;

            // Disable trigger so it never fires again
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            if (showDebugLogs)
                Debug.Log($"[CameraZoomFocus] Already played once ({PrefKey}). Trigger disabled.");
        }
    }

    void OnDisable()
    {
        StopZoomAndRestore();
    }

    void OnDestroy()
    {
        StopZoomAndRestore();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!autoTrigger || hasTriggered || !other.CompareTag("Player"))
            return;

        hasTriggered = true;

        if (playOnce)
        {
            PlayerPrefs.SetInt(PrefKey, 1);
            PlayerPrefs.Save();
        }

        if (showDebugLogs)
            Debug.Log("[CameraZoomFocus] Player triggered zoom effect");

        StartZoom(focusTarget);
    }

    /// <summary>Manually trigger the zoom effect (can be called from other scripts)</summary>
    public void TriggerZoom()
    {
        if (hasTriggered) return;

        hasTriggered = true;

        if (playOnce)
        {
            PlayerPrefs.SetInt(PrefKey, 1);
            PlayerPrefs.Save();
        }

        StartZoom(focusTarget);
    }

    public void StartZoom(Transform target)
    {
        if (_zoomRoutine != null) StopCoroutine(_zoomRoutine);
        _zoomRoutine = StartCoroutine(ZoomSequence(target));
    }

    void StopZoomAndRestore()
    {
        if (_zoomRoutine != null)
        {
            StopCoroutine(_zoomRoutine);
            _zoomRoutine = null;
        }

        // Best-effort restore (avoid exceptions if camera already destroyed)
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = originalFOV;
            playerCamera.transform.rotation = originalCameraRotation;
        }

        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.UnlockMovement("Camera zoom stopped");
    }

    private IEnumerator ZoomSequence(Transform target)
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera != null)
                originalFOV = playerCamera.fieldOfView;
        }

        if (playerCamera == null || target == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("[CameraZoomFocus] Missing camera or focus target!");
            yield break;
        }

        // Lock movement during zoom
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.LockMovement("Camera zoom");

        // Store original camera rotation
        originalCameraRotation = playerCamera.transform.rotation;

        // Show caption
        if (showCaption && CaptionManager.Instance != null)
            CaptionManager.Instance.ShowMonologue(captionMessage, captionDuration);

        float zoomInDuration = Mathf.Max(0.01f, zoomDuration * 0.5f);
        float zoomOutDuration = Mathf.Max(0.01f, zoomDuration * 0.5f);

        try
        {
            // Zoom in + rotate toward target
            float elapsed = 0f;
            while (elapsed < zoomInDuration)
            {
                if (playerCamera == null || target == null) yield break;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / zoomInDuration);

                playerCamera.fieldOfView = Mathf.Lerp(originalFOV, zoomFOV, t);

                Vector3 targetPosition = target.position + Vector3.up * verticalOffset;
                Vector3 directionToTarget = targetPosition - playerCamera.transform.position;
                if (directionToTarget.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    playerCamera.transform.rotation = Quaternion.Slerp(originalCameraRotation, targetRotation, t);
                }

                yield return null;
            }

            // Hold zoom (but bail if target disappears)
            float hold = zoomDuration * 0.5f;
            float holdElapsed = 0f;
            while (holdElapsed < hold)
            {
                if (playerCamera == null || target == null) yield break;
                holdElapsed += Time.deltaTime;
                yield return null;
            }

            // Zoom out + rotate back
            elapsed = 0f;
            Quaternion currentRotation = playerCamera.transform.rotation;

            while (elapsed < zoomOutDuration)
            {
                if (playerCamera == null) yield break;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / zoomOutDuration);

                playerCamera.fieldOfView = Mathf.Lerp(zoomFOV, originalFOV, t);
                playerCamera.transform.rotation = Quaternion.Slerp(currentRotation, originalCameraRotation, t);

                yield return null;
            }

            // Ensure exact restoration
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = originalFOV;
                playerCamera.transform.rotation = originalCameraRotation;
            }

            if (showDebugLogs)
                Debug.Log("[CameraZoomFocus] Zoom sequence complete");
        }
        finally
        {
            // Always unlock movement even if target/camera disappears mid-zoom
            if (PlayerMovementLock.Instance != null)
                PlayerMovementLock.Instance.UnlockMovement("Zoom complete");

            _zoomRoutine = null;
        }
    }
}
