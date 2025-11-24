using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject door;
    [SerializeField] private string keyTag = "key";

    [Header("Key Type Restriction")]
    [Tooltip("Which type of key can open this door")]
    [SerializeField] private KeyType requiredKeyType = KeyType.AnyKey;
    [Tooltip("If true, any key can open this door (ignores requiredKeyType)")]
    [SerializeField] private bool acceptAnyKey = false;

    [Header("Hinge")]
    [SerializeField] private Transform hingePoint;
    [SerializeField] private Vector3 hingeLocalOffset = Vector3.zero;

    [Header("Open settings")]
    [SerializeField] private float openSpeed = 90f;
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private bool useYAxis = true;

    private bool isOpening = false;
    private GameObject key;

    private const String DOOR_NAME = "Door";
    private const String KEY_NAME = "Key";

    void Start()
    {
        if (door == null)
            door = GameObject.Find(DOOR_NAME);

        key = GameObject.Find(KEY_NAME);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isOpening) return;

        if (other.CompareTag(keyTag))
        {
            // Check if this key is allowed to open this door
            if (!IsKeyAllowed(other.gameObject))
            {
                Debug.Log($"[DoorTrigger] Wrong key type! This door requires: {requiredKeyType}");
                return;
            }

            // Destroy the colliding object (child collider)
            if (other.gameObject != null)
                Destroy(other.gameObject);
            
            // Also destroy the parent key GameObject (the actual key prefab root)
            if (other.transform.parent != null)
                Destroy(other.transform.parent.gameObject);
            
            // Destroy the original key reference if it exists
            if (key != null)
                Destroy(key);

            if (door != null)
            {
                StartCoroutine(OpenDoorCoroutine());
            }
        }
    }

    /// <summary>
    /// Check if the given key object is allowed to open this door
    /// </summary>
    private bool IsKeyAllowed(GameObject keyObject)
    {
        // If door accepts any key, allow it
        if (acceptAnyKey || requiredKeyType == KeyType.AnyKey)
        {
            return true;
        }

        // Check if key has SkeletonKeyBehavior component
        // Check both the object itself and its parent (since key uses child collider)
        var skeletonKey = keyObject.GetComponent<SkeletonKeyBehavior>();
        if (skeletonKey == null && keyObject.transform.parent != null)
        {
            skeletonKey = keyObject.transform.parent.GetComponent<SkeletonKeyBehavior>();
        }

        if (skeletonKey != null)
        {
            // Key has a type marker, check if it matches
            Debug.Log($"[DoorTrigger] Found {skeletonKey.keyType} key, door requires: {requiredKeyType}");
            return skeletonKey.keyType == requiredKeyType;
        }
        else
        {
            // No SkeletonKeyBehavior component means it's the original key
            // Original key can only open doors that require OriginalKey
            Debug.Log($"[DoorTrigger] Found original key, door requires: {requiredKeyType}");
            return requiredKeyType == KeyType.OriginalKey;
        }
    }

    private IEnumerator OpenDoorCoroutine()
    {
        isOpening = true;
        
        // Trigger celebration monologue when door starts opening
        TriggerDoorOpenCaption();
        
        Transform doorT = door.transform;

        Vector3 hingeWorldPos = (hingePoint != null) ? hingePoint.position : doorT.TransformPoint(hingeLocalOffset);
        Vector3 axis = useYAxis ? Vector3.up : Vector3.right;

        float totalRotated = 0f;
        float targetRotation = Mathf.Abs(openAngle);

        while (Mathf.Abs(totalRotated) < targetRotation)
        {
            float step = openSpeed * Time.deltaTime;
            if (Mathf.Abs(totalRotated + step) > targetRotation)
            {
                step = targetRotation - Mathf.Abs(totalRotated);
            }

            float rotationStep = Mathf.Sign(openAngle) * step;
            doorT.RotateAround(hingeWorldPos, axis, rotationStep);
            
            totalRotated += step;
            yield return null;
        }

        isOpening = false;

        Destroy(this.gameObject);
    }
    
    private void TriggerDoorOpenCaption()
    {
        // Find and trigger the caption controller when door opens
        var levelController = FindFirstObjectByType<LevelOneCaptionController>();
        if (levelController != null)
        {
            levelController.OnDoorOpened();
            Debug.Log("DoorTrigger: Door open celebration caption triggered!");
        }
        else
        {
            Debug.LogWarning("DoorTrigger: LevelOneCaptionController not found in scene");
        }
    }
}
