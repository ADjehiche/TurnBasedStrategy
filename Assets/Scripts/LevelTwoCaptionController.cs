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
    
    [Header("Timing")]
    [SerializeField] private float startDelay = 1.5f;
    [SerializeField] private float dialoguePauseDuration = 2.5f;
    [SerializeField] private float monologueDuration = 2.5f;
    [SerializeField] private float hallwayPanDuration = 2f;
    
    [Header("Hallway Camera Pan")]
    [SerializeField] private Transform hallwayTarget; // Assign the hallway entrance transform
    [SerializeField] private float panHoldDuration = 1f; // How long to hold looking at hallway
    
    [Header("Trigger Settings")]
    [SerializeField] private Transform hallwayTriggerArea; // Optional: assign a trigger zone
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
        
        // Show the archive arrival sequence
        if (!hasShownArrival)
        {
            StartCoroutine(ArchiveArrivalSequence());
        }
    }
    
    void Update()
    {
        // Check if player is near hallway (if trigger area is set)
        if (!hasShownHallwayPrompt && hallwayTriggerArea != null && playerTransform != null)
        {
            float distance = Vector3.Distance(playerTransform.position, hallwayTriggerArea.position);
            if (distance <= hallwayTriggerDistance)
            {
                TriggerHallwayDiscovery();
            }
        }
    }
    
    /// <summary>
    /// Initial sequence when arriving at the archive
    /// </summary>
    private IEnumerator ArchiveArrivalSequence()
    {
        // Lock movement during intro
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.LockMovement("Archive arrival sequence");
        
        yield return new WaitForSeconds(startDelay);
        
        hasShownArrival = true;
        
        // Show Fragment explaining where they are
        foreach (string line in archiveArrivalDialogue)
        {
            if (CaptionManager.Instance != null)
            {
                // Determine if it's Fragment or Player speaking
                if (line.StartsWith("[Fragment]"))
                {
                    CaptionManager.Instance.ShowMonologue(line, monologueDuration);
                }
                else
                {
                    CaptionManager.Instance.ShowMonologue(line, monologueDuration);
                }
            }
            
            yield return new WaitForSeconds(dialoguePauseDuration);
        }
        
        // Brief pause before exploration hint
        yield return new WaitForSeconds(1f);
        
        // Show exploration dialogue
        foreach (string line in explorationDialogue)
        {
            if (CaptionManager.Instance != null)
            {
                CaptionManager.Instance.ShowMonologue(line, monologueDuration);
            }
            
            yield return new WaitForSeconds(dialoguePauseDuration);
        }
        
        // Unlock movement
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.UnlockMovement("Archive intro complete");
        
        Debug.Log("[LevelTwoCaptionController] Archive arrival sequence complete");
    }
    
    /// <summary>
    /// Called when player discovers/approaches the hallway
    /// </summary>
    public void TriggerHallwayDiscovery()
    {
        if (hasShownHallwayPrompt) return;
        
        hasShownHallwayPrompt = true;
        StartCoroutine(HallwayDiscoverySequence());
    }
    
    private IEnumerator HallwayDiscoverySequence()
    {
        // Lock movement briefly
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.LockMovement("Hallway discovery");
        
        // Fragment's prompt
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(hallwayPrompt, monologueDuration);
        }
        
        // Pan camera toward hallway
        if (hallwayTarget != null && mainCamera != null)
        {
            StartCoroutine(PanToHallway());
        }
        
        yield return new WaitForSeconds(hallwayPanDuration + panHoldDuration + hallwayPanDuration);
        
        // Player's response
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(hallwayApproach, monologueDuration);
        }
        
        yield return new WaitForSeconds(monologueDuration);
        
        // Unlock movement
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.UnlockMovement("Hallway discovery complete");
        
        Debug.Log("[LevelTwoCaptionController] Hallway discovery sequence complete");
    }
    
    /// <summary>
    /// Pan camera smoothly toward the hallway and back
    /// </summary>
    private IEnumerator PanToHallway()
    {
        if (isPanningToHallway) yield break;
        isPanningToHallway = true;
        
        // Store original rotation (we'll work with the camera holder/parent if available)
        Transform camTransform = mainCamera.transform;
        Transform camParent = camTransform.parent;
        
        // Use the parent (camera holder) if it exists for smoother control
        Transform targetTransform = camParent != null ? camParent : camTransform;
        Quaternion originalRotation = targetTransform.rotation;
        
        // Calculate direction to hallway
        Vector3 directionToHallway = (hallwayTarget.position - camTransform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToHallway);
        
        // Pan to hallway
        float elapsed = 0f;
        while (elapsed < hallwayPanDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hallwayPanDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            targetTransform.rotation = Quaternion.Slerp(originalRotation, targetRotation, smoothT);
            
            yield return null;
        }
        
        // Hold looking at hallway
        yield return new WaitForSeconds(panHoldDuration);
        
        // Pan back to original rotation
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
        
        // Ensure we're back to original
        targetTransform.rotation = originalRotation;
        
        isPanningToHallway = false;
    }
    
    /// <summary>
    /// Can be called from a trigger collider when player enters hallway area
    /// </summary>
    public void OnPlayerEnterHallwayArea()
    {
        TriggerHallwayDiscovery();
    }
}
