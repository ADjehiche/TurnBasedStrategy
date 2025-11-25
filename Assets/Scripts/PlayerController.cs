using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject camHolder;
    public Animator animator; // Reference to the character's animator
    public Transform headBone; // Reference to the character's head bone
    public bool attachCameraToHead = true; // Toggle for head-following camera
    public Vector3 headCameraOffset = new Vector3(0, 0.15f, 1.1f); // Offset from head bone (forward and above)
    public float headInfluence = 0.5f; // How much the head movement affects camera (0-1)
    public float speed, sensitivity, maxForce, jumpForce;
    public float cameraSmoothing = 5f; // Camera smoothing factor
    private Vector2 move, look;
    private float lookRotation;
    public bool grounded;
    private GameManager gameManager;
    private const String GAMEMANAGER_NAME = "GameManager";
    private const String BATTLE_TRIGGER_TAG = "battleTrigger";
    
    // Animation states
    private bool isMoving = false;
    private Vector3 targetCameraPosition;
    private Vector3 originalCameraPosition;

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();
    }
    
    // Jump disabled - not used in this game
    /*
    public void OnJump(InputAction.CallbackContext context)
    {
        Jump();
    }
    */

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        gameManager = GameObject.Find(GAMEMANAGER_NAME).GetComponent<GameManager>();
        
        // Find animator if not assigned
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        // Find head bone automatically
        if (headBone == null && animator != null)
        {
            FindHeadBone();
        }
        
        // Store original camera position
        originalCameraPosition = camHolder.transform.localPosition;
        targetCameraPosition = originalCameraPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Don't process input when movement is locked
        if (PlayerMovementLock.Instance != null && PlayerMovementLock.Instance.IsLocked())
        {
            return;
        }
    }

    private void FixedUpdate()
    {
        // Check if movement is locked
        if (PlayerMovementLock.Instance != null && PlayerMovementLock.Instance.IsLocked())
        {
            // Don't process movement when locked
            rb.velocity = new Vector3(0, rb.velocity.y, 0); // Keep y velocity for gravity
            return;
        }
        
        // Check for ground contact
        RaycastHit hit;
        grounded = Physics.Raycast(transform.position, Vector3.down, out hit, 1.1f);
        
        // Get input direction relative to camera
        Vector3 moveDirection = Vector3.zero;
        
        if(camHolder != null)
        {
            // Calculate movement direction based on camera orientation
            moveDirection = camHolder.transform.forward * move.y + camHolder.transform.right * move.x;
            moveDirection.y = 0; // Keep movement horizontal
            moveDirection = moveDirection.normalized;
        }
        
        // Calculate target velocity
        Vector3 velocity = moveDirection * speed;
        velocity.y = rb.velocity.y; // Preserve vertical velocity
        
        // Apply force to reach target velocity
        Vector3 velocityChange = velocity - rb.velocity;
        velocityChange.y = 0; // Don't apply force vertically
        
        // Clamp the force
        velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);
        
        // Apply the force
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
        
        // Update animation based on movement
        bool isCurrentlyMoving = moveDirection.magnitude > 0.1f;
        if (isCurrentlyMoving != isMoving)
        {
            isMoving = isCurrentlyMoving;
            if (animator != null)
            {
                animator.SetBool("IsWalking", isMoving);
            }
        }
    }

    // Jump disabled - not used in this game
    /*
    void Jump() {
        Vector3 jumpForces = Vector3.zero;

        if (grounded)
        {
            jumpForces = Vector3.up * jumpForce;
        }
        rb.AddForce(jumpForces, ForceMode.VelocityChange);
        
    }
    */
    
    void UpdateMovementAnimation()
    {
        if (animator != null)
        {
            // Method 1: Parameter-based (good for complex state machines)
            animator.SetBool("isMoving", isMoving);
            animator.SetFloat("Speed", move.magnitude);
            
            // Method 2: Direct CrossFade (more immediate, uncomment if you prefer instant response)
            /*
            if (isMoving)
            {
                animator.CrossFade("Walk", 0.05f);
            }
            else
            {
                animator.CrossFade("Idle", 0.05f);
            }
            */
        }
    }
    
    void FindHeadBone()
    {
        // Try to find head bone by common names, prioritizing "head" over "neck"
        Transform[] bones = animator.GetComponentsInChildren<Transform>();
        Transform neckBone = null;
        
        // First pass: Look specifically for head bones
        foreach (Transform bone in bones)
        {
            string boneName = bone.name.ToLower();
            if (boneName.Contains("head") && !boneName.Contains("neck"))
            {
                headBone = bone;
                Debug.Log($"Found head bone: {bone.name}");
                return; // Found head, exit immediately
            }
            
            // Store neck as backup
            if (boneName.Contains("neck") && neckBone == null)
            {
                neckBone = bone;
            }
        }
        
        // Second pass: If we found a neck, look for head as its child
        if (neckBone != null)
        {
            foreach (Transform child in neckBone)
            {
                string childName = child.name.ToLower();
                if (childName.Contains("head"))
                {
                    headBone = child;
                    Debug.Log($"Found head bone as child of neck: {child.name}");
                    return;
                }
            }
            
            // If no head child found, use neck as fallback
            headBone = neckBone;
            Debug.Log($"Using neck bone as fallback: {neckBone.name}");
        }
        
        if (headBone == null)
        {
            Debug.LogWarning("Could not find head bone automatically. Please assign it manually.");
        }
    }

    void LateUpdate()
    {
        // Don't rotate camera when movement is locked
        if (PlayerMovementLock.Instance != null && PlayerMovementLock.Instance.IsLocked())
        {
            return;
        }
        
        // Handle camera rotation
        transform.Rotate(Vector3.up * look.x * sensitivity);
        lookRotation += -look.y * sensitivity;
        lookRotation = Mathf.Clamp(lookRotation, -90, 90);
        camHolder.transform.localEulerAngles = new Vector3(lookRotation, 0, 0);
    }

    public void SetGrounded(bool state)
    {
        grounded = state;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(BATTLE_TRIGGER_TAG))
        {
            // Save battle trigger center for proper respawn position
            Vector3 triggerCenter = other.bounds.center;
            triggerCenter.y = transform.position.y; // Keep player's Y position
            GameSession.SetBattleTriggerPosition(triggerCenter);
            
            // Also save for GameManager (backward compatibility)
            gameManager.SavePlayerPosition(triggerCenter);
            
            gameManager.StartBattle();
        }
    }
    
}
