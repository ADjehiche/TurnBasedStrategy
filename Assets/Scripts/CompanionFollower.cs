using UnityEngine;

/// <summary>
/// Makes the companion smoothly follow the player with floating animation
/// Attach to companion GameObject
/// </summary>
public class CompanionFollower : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("The player transform to follow")]
    [SerializeField] private Transform player;
    
    [Tooltip("Offset from player position (x=side, y=height, z=forward/back)")]
    [SerializeField] private Vector3 followOffset = new Vector3(1.2f, 0.5f, 1.5f); // Right side, low, in front
    
    [Tooltip("How quickly the companion follows the player")]
    [SerializeField] private float followSpeed = 5f;
    
    [Tooltip("Minimum distance before companion starts moving")]
    [SerializeField] private float minFollowDistance = 0.3f;
    
    [Header("Floating Animation")]
    [Tooltip("Speed of the bobbing/floating motion")]
    [SerializeField] private float bobSpeed = 2f;
    
    [Tooltip("Height range of the bobbing motion")]
    [SerializeField] private float bobHeight = 0.3f;
    
    [Header("Rotation")]
    [Tooltip("How quickly companion rotates toward movement direction")]
    [SerializeField] private float rotationSpeed = 5f;
    
    [Header("State")]
    [Tooltip("Is the companion currently following?")]
    [SerializeField] private bool isFollowing = false;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    private float bobTimeOffset;
    
    void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                if (showDebugLogs)
                    Debug.Log("[CompanionFollower] Auto-found player");
            }
            else
            {
                Debug.LogError("[CompanionFollower] No player found! Assign player or add 'Player' tag.");
            }
        }
        
        // Random offset for bob animation so multiple companions don't sync
        bobTimeOffset = Random.Range(0f, Mathf.PI * 2f);
    }
    
    void Update()
    {
        if (!isFollowing || player == null)
        {
            return;
        }
        
        FollowPlayer();
    }
    
    private void FollowPlayer()
    {
        // Calculate target position with offset
        Vector3 targetPosition = player.position + player.TransformDirection(followOffset);
        
        // Add bobbing motion
        float bobOffset = Mathf.Sin((Time.time + bobTimeOffset) * bobSpeed) * bobHeight;
        targetPosition.y += bobOffset;
        
        // Check if far enough to move
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        if (showDebugLogs && Time.frameCount % 60 == 0) // Log once per second (at 60fps)
        {
            Debug.Log($"[CompanionFollower] Following={isFollowing}, Distance={distance:F2}, Target={targetPosition}, Current={transform.position}");
        }
        
        if (distance > minFollowDistance)
        {
            // Smooth follow using lerp
            transform.position = Vector3.Lerp(
                transform.position, 
                targetPosition, 
                followSpeed * Time.deltaTime
            );
            
            // Rotate toward movement direction
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0; // Keep rotation horizontal
            
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }
    
    /// <summary>
    /// Start following the player
    /// </summary>
    public void StartFollowing()
    {
        isFollowing = true;
        GameSession.CompanionActive = true;
        
        if (showDebugLogs)
            Debug.Log($"[CompanionFollower] ✅ Started following player! isFollowing={isFollowing}, player={player?.name}");
    }
    
    /// <summary>
    /// Stop following the player
    /// </summary>
    public void StopFollowing()
    {
        isFollowing = false;
        
        if (showDebugLogs)
            Debug.Log("[CompanionFollower] Stopped following player");
    }
    
    /// <summary>
    /// Check if companion is currently following
    /// </summary>
    public bool IsFollowing()
    {
        return isFollowing;
    }
    
    /// <summary>
    /// Set whether companion should follow (useful for restoring state)
    /// </summary>
    public void SetFollowing(bool follow)
    {
        isFollowing = follow;
    }
}
