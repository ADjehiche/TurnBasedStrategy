using UnityEngine;

/// <summary>
/// Controls player movement locking during cutscenes and dialogue
/// Singleton pattern for easy access from any script
/// </summary>
public class PlayerMovementLock : MonoBehaviour
{
    public static PlayerMovementLock Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Camera playerCamera;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool isLocked = false;
    private Vector2 savedMove;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Auto-find components if not assigned
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
        
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }
    
    /// <summary>
    /// Lock player movement and camera look
    /// </summary>
    public void LockMovement(string reason = "")
    {
        if (isLocked)
        {
            if (showDebugLogs)
                Debug.LogWarning($"[PlayerMovementLock] Already locked! (Reason: {reason})");
            return;
        }
        
        isLocked = true;
        
        if (playerController != null && playerController.rb != null)
        {
            // Stop all movement
            playerController.rb.velocity = Vector3.zero;
            playerController.rb.angularVelocity = Vector3.zero;
        }
        
        if (showDebugLogs)
            Debug.Log($"[PlayerMovementLock] ✅ Movement LOCKED - {reason}");
    }
    
    /// <summary>
    /// Unlock player movement and camera look
    /// </summary>
    public void UnlockMovement(string reason = "")
    {
        if (!isLocked)
        {
            if (showDebugLogs)
                Debug.LogWarning($"[PlayerMovementLock] Already unlocked! (Reason: {reason})");
            return;
        }
        
        isLocked = false;
        
        if (showDebugLogs)
            Debug.Log($"[PlayerMovementLock] ✅ Movement UNLOCKED - {reason}");
    }
    
    /// <summary>
    /// Check if movement is currently locked
    /// </summary>
    public bool IsLocked()
    {
        return isLocked;
    }
    
    void Update()
    {
        // Override input when locked
        if (isLocked && playerController != null && playerController.rb != null)
        {
            // Force zero velocity when locked
            playerController.rb.velocity = new Vector3(0, playerController.rb.velocity.y, 0);
            playerController.rb.angularVelocity = Vector3.zero;
        }
    }
}
