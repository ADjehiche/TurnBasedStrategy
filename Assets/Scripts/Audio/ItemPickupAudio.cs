using UnityEngine;

/// <summary>
/// Plays audio when items are picked up
/// Call PlayPickupSound() from your item pickup logic
/// </summary>
public class ItemPickupAudio : MonoBehaviour
{
    [Header("Sound Names")]
    [SerializeField] private string pickupSoundName = "ItemPickup";
    
    /// <summary>
    /// Play the item pickup sound
    /// Call this method when an item is picked up
    /// </summary>
    public static void PlayPickupSound(string soundName = "ItemPickup")
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(soundName);
            Debug.Log($"[ItemPickupAudio] Playing pickup sound: {soundName}");
        }
        else
        {
            Debug.LogWarning("[ItemPickupAudio] AudioManager instance not found!");
        }
    }
    
    /// <summary>
    /// Play pickup sound at a specific position (3D audio)
    /// </summary>
    public static void PlayPickupSoundAtPosition(Vector3 position, string soundName = "ItemPickup")
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAtPosition(soundName, position);
            Debug.Log($"[ItemPickupAudio] Playing pickup sound at position: {position}");
        }
        else
        {
            Debug.LogWarning("[ItemPickupAudio] AudioManager instance not found!");
        }
    }
}
