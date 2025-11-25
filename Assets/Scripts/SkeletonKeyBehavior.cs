using UnityEngine;

/// <summary>
/// Marker component for the skeleton key dropped after defeating the skeleton.
/// This key can only open Door_2, not the original door.
/// </summary>
public class SkeletonKeyBehavior : MonoBehaviour
{
    [Header("Key Identification")]
    [Tooltip("Type of key - used by DoorTrigger to determine which doors this key can open")]
    public KeyType keyType = KeyType.SkeletonKey;
}

/// <summary>
/// Enum to identify different key types in the game
/// </summary>
public enum KeyType
{
    OriginalKey,  // The starting key that opens the first door
    SkeletonKey,  // Key dropped by skeleton, opens Door_2
    AnyKey        // Wildcard - any key can open (for testing or special doors)
}
