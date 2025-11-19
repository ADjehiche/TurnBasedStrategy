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
    public void OnJump(InputAction.CallbackContext context)
    {
        Jump();
    }

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

    }

    private void FixedUpdate()
    {
        Move();
    }

    void Jump() {
        Vector3 jumpForces = Vector3.zero;

        if (grounded)
        {
            jumpForces = Vector3.up * jumpForce;
        }
        rb.AddForce(jumpForces, ForceMode.VelocityChange);
        
    }

    void Move()
    {
        Vector3 currentVelocity = rb.velocity;
        Vector3 targetVelocity = new Vector3(move.x, 0, move.y);
        targetVelocity *= speed;

        targetVelocity = transform.TransformDirection(targetVelocity);

        Vector3 velocityChange = (targetVelocity - currentVelocity);
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);

        Vector3.ClampMagnitude(velocityChange, maxForce);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        // Handle movement animation with lower threshold for more responsive animation
        bool currentlyMoving = move.magnitude > 0.01f; // Lower threshold for quicker response
        if (currentlyMoving != isMoving)
        {
            isMoving = currentlyMoving;
            UpdateMovementAnimation();
        }
    }
    
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

    void Look() { 
        transform.Rotate(Vector3.up * look.x * sensitivity);
        lookRotation += -look.y * sensitivity;
        lookRotation = Math.Clamp(lookRotation, -90, 90);
        
        if (attachCameraToHead && headBone != null)
        {
            // Calculate head-influenced position with direct offset application
            Vector3 worldOffset = transform.TransformDirection(headCameraOffset);
            Vector3 headPosition = headBone.position + worldOffset;
            
            // Calculate base position (original camera position)
            Vector3 basePosition = transform.position + transform.TransformDirection(originalCameraPosition);
            
            // Blend between base position and head position based on influence
            Vector3 targetPosition = Vector3.Lerp(basePosition, headPosition, headInfluence);
            
            camHolder.transform.position = Vector3.Lerp(
                camHolder.transform.position,
                targetPosition,
                cameraSmoothing * Time.deltaTime
            );
            
            // Keep camera rotation independent of head bone rotation
            camHolder.transform.eulerAngles = new Vector3(lookRotation, transform.eulerAngles.y, 0);
        }
        else
        {
            // Original camera behavior with smoothing
            camHolder.transform.localPosition = Vector3.Lerp(
                camHolder.transform.localPosition, 
                targetCameraPosition, 
                cameraSmoothing * Time.deltaTime
            );
            
            camHolder.transform.eulerAngles = new Vector3(lookRotation, camHolder.transform.eulerAngles.y, camHolder.transform.eulerAngles.z);
        }
    }

    void LateUpdate()
    {
        Look();
    }

    public void SetGrounded(bool state)
    {
        grounded = state;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(BATTLE_TRIGGER_TAG))
        {
            gameManager.StartBattle();
        }
    }
    
}
