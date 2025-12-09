using UnityEngine;
using System.Collections;

/// <summary>
/// Zone 3: Application (Open Door)
/// Player must use the key to open the door
/// Requires Zone 2 completion (key acquisition)
/// </summary>
public class Zone3_Door : TutorialZone
{
    [Header("Zone 3 Specific")]
    [SerializeField] private GameObject door; // The door to open
    [SerializeField] private Collider captionTrigger; // Trigger collider to show caption (assign in Inspector)
    [SerializeField] private DoorRotationAxis rotationAxis = DoorRotationAxis.Y;
    [SerializeField] private float doorOpenAngle = -90f;
    [SerializeField] private float doorOpenSpeed = 2f;
    [SerializeField] private GameObject exitLight; // The blinding white light
    [SerializeField] private Transform exitTarget; // Point to pan camera toward
    [SerializeField] private float exitZoomFOV = 40f;
    [SerializeField] private string titleSceneName = "TitleScene";
    
    private bool doorOpening = false;
    private bool captionShown = false;
    private Transform player;
    private Zone2_Interact zone2; // Reference to check if player has key
    private Camera playerCamera;
    private float originalFOV;
    private Quaternion originalCameraRotation;
    
    protected override void Start()
    {
        base.Start();
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (door == null)
        {
            Debug.LogError("[Zone3_Door] Door not assigned!");
        }
        
        // Find Zone 2 to check if player has key
        zone2 = FindObjectOfType<Zone2_Interact>();
        if (zone2 == null)
        {
            Debug.LogWarning("[Zone3_Door] Zone2_Interact not found! Door will open without key.");
        }
        
        // Find camera
        playerCamera = Camera.main;
        if (playerCamera != null)
        {
            originalFOV = playerCamera.fieldOfView;
        }
        
        // Hide exit light initially
        if (exitLight != null)
        {
            exitLight.SetActive(false);
        }
        
        // Set up caption trigger
        if (captionTrigger != null)
        {
            // Make sure it's a trigger
            captionTrigger.isTrigger = true;
            Debug.Log("[Zone3] Caption trigger set up");
        }
        else
        {
            Debug.LogWarning("[Zone3] Caption trigger not assigned! Assign a trigger collider in Inspector.");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Zone3] Trigger entered by: {other.gameObject.name}, Tag: {other.tag}");
        
        if (!other.CompareTag("Player")) return;
        
        // If captionTrigger is assigned, this GameObject IS the caption trigger
        // Show caption and don't process door opening
        if (captionTrigger != null && !captionShown)
        {
            bool hasKey = zone2 != null && zone2.IsComplete();
            if (hasKey && CaptionManager.Instance != null)
            {
                CaptionManager.Instance.ShowInstruction("Walk into the door to exit", 999f);
                Debug.Log("[Zone3] Caption shown - this is caption trigger GameObject");
            }
            captionShown = true;
            return; // This is just the caption trigger, not the door
        }
        
        // If captionTrigger is NOT assigned, this GameObject IS the door trigger
        // Process door opening
        if (isComplete || doorOpening) return;
        
        // Check if player has the key (Zone 2 must be complete)
        bool hasKey2 = zone2 != null && zone2.IsComplete();
        
        Debug.Log($"[Zone3] Player entered door trigger. Zone2 exists: {zone2 != null}, Has key: {hasKey2}");
        
        if (!hasKey2)
        {
            // Player doesn't have key yet
            if (CaptionManager.Instance != null)
            {
                CaptionManager.Instance.ShowSystemMessage("Door is locked - Find a key", 2f);
                Debug.Log("[Zone3] Door locked message shown");
            }
            // Don't open door - just show message and return
            return;
        }
        
        // Player has key, open door
        Debug.Log("[Zone3] Opening door with key");
        StartCoroutine(ThrowKeyAndOpenDoor(other.transform));
    }
    
    private IEnumerator ThrowKeyAndOpenDoor(Transform player)
    {
        doorOpening = true;
        
        // Lock player movement
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.LockMovement("Door opening");
        }
        
        // Store camera rotation
        if (playerCamera != null)
        {
            originalCameraRotation = playerCamera.transform.rotation;
        }
        
        // Destroy the key immediately
        if (zone2 != null && zone2.keyObject != null)
        {
            Destroy(zone2.keyObject);
            Debug.Log("[Zone3] Key destroyed");
        }
        
        yield return new WaitForSeconds(0.3f);
        
        // Show unlocking message
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowSystemMessage("Door unlocked", 2f);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Destroy the door
        if (door != null)
        {
            Destroy(door);
            Debug.Log("[Zone3] Door destroyed");
        }
        
        // Show exit light
        if (exitLight != null)
        {
            exitLight.SetActive(true);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Zoom camera straight ahead (no panning, just zoom)
        if (playerCamera != null)
        {
            float zoomDuration = 2f;
            float elapsed = 0f;
            
            while (elapsed < zoomDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / zoomDuration;
                
                // Just zoom in, keep looking straight ahead
                playerCamera.fieldOfView = Mathf.Lerp(originalFOV, exitZoomFOV, t);
                
                yield return null;
            }
        }
        
        yield return new WaitForSeconds(2f);
        
        // Show Tutorial Complete message
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.HideCaption(); // Clear any previous captions
            CaptionManager.Instance.ShowInstruction("TUTORIAL COMPLETE", 3f);
        }
        
        yield return new WaitForSeconds(3f);
        
        // Load title scene
        Debug.Log("[Zone3] Loading title scene");
        UnityEngine.SceneManagement.SceneManager.LoadScene(titleSceneName);
        
        CompleteZone();
    }
}

public enum DoorRotationAxis
{
    X,  // Pitch (horizontal door)
    Y,  // Yaw (vertical door, most common)
    Z   // Roll (rare)
}
