using UnityEngine;
using System.Collections;

/// <summary>
/// Manages Combat Wing area in Level Two
/// Handles Red Fragment spawning after battle victory
/// </summary>
public class CombatWingManager : MonoBehaviour
{
    [Header("Red Fragment")]
    [Tooltip("Red Fragment prefab to spawn after Combat Wing victory")]
    [SerializeField] private GameObject redFragmentPrefab;
    [Tooltip("Fallback spawn position if no saved position")]
    [SerializeField] private Transform fallbackSpawnPoint;
    [SerializeField] private float spawnDelay = 1f;
    
    [Header("Red Fragment Follower")]
    [Tooltip("Follower prefab to spawn if fragment was already collected (returning from flashback)")]
    [SerializeField] private GameObject redFragmentFollowerPrefab;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private bool hasSpawnedFragment = false;
    private bool hasSpawnedFollower = false;
    
    void Start()
    {
        // Case 1: Returning from flashback - spawn follower directly
        if (GameSession.HasPlayedRageFlashback && !hasSpawnedFollower)
        {
            if (debugMode) Debug.Log("[CombatWingManager] HasPlayedRageFlashback is true - spawning follower");
            SpawnRedFragmentFollower();
        }
        // Case 2: Combat Wing victory but no flashback yet - spawn collectible
        else if (GameSession.CombatWingVictory && !GameSession.HasCollectedRedFragment && !hasSpawnedFragment)
        {
            if (debugMode) Debug.Log("[CombatWingManager] Combat Wing victory detected - spawning Red Fragment");
            StartCoroutine(SpawnRedFragmentDelayed());
        }
        else if (debugMode)
        {
            Debug.Log($"[CombatWingManager] No spawn needed - Victory: {GameSession.CombatWingVictory}, Collected: {GameSession.HasCollectedRedFragment}, FlashbackPlayed: {GameSession.HasPlayedRageFlashback}");
        }
    }
    
    /// <summary>
    /// Spawn Red Fragment after short delay for dramatic effect
    /// </summary>
    private IEnumerator SpawnRedFragmentDelayed()
    {
        yield return new WaitForSeconds(spawnDelay);
        
        SpawnRedFragment();
    }
    
    /// <summary>
    /// Spawn the Red Fragment at the saved position or fallback
    /// </summary>
    private void SpawnRedFragment()
    {
        if (redFragmentPrefab == null)
        {
            Debug.LogError("[CombatWingManager] Red Fragment prefab not assigned!");
            return;
        }
        
        if (hasSpawnedFragment)
        {
            if (debugMode) Debug.Log("[CombatWingManager] Fragment already spawned this session");
            return;
        }
        
        // Use saved position or fallback
        Vector3 spawnPos = GameSession.RedFragmentSpawnPosition;
        
        if (spawnPos == Vector3.zero && fallbackSpawnPoint != null)
        {
            spawnPos = fallbackSpawnPoint.position;
        }
        
        // Spawn slightly above ground
        spawnPos.y += 0.5f;
        
        // Spawn the fragment
        GameObject fragment = Instantiate(redFragmentPrefab, spawnPos, Quaternion.identity);
        hasSpawnedFragment = true;
        
        // Play spawn effect sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("ItemSpawn");
        }
        
        if (debugMode) Debug.Log($"[CombatWingManager] 🔴 Red Fragment spawned at {spawnPos}");
        
        // Clear the victory flag so it doesn't spawn again on scene reload
        // (But keep HasCollectedRedFragment false until player actually picks it up)
    }
    
    /// <summary>
    /// Spawn the follower directly (used when returning from flashback with fragment already collected)
    /// </summary>
    private void SpawnRedFragmentFollower()
    {
        if (redFragmentFollowerPrefab == null)
        {
            Debug.LogError("[CombatWingManager] Red Fragment Follower prefab not assigned!");
            return;
        }
        
        if (hasSpawnedFollower)
        {
            if (debugMode) Debug.Log("[CombatWingManager] Follower already spawned this session");
            return;
        }
        
        // Find player for spawn position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[CombatWingManager] Player not found!");
            return;
        }
        
        Vector3 spawnPos = player.transform.position + Vector3.left * 2f + Vector3.up * 0.5f;
        
        // Spawn the follower
        GameObject follower = Instantiate(redFragmentFollowerPrefab, spawnPos, Quaternion.identity);
        follower.name = "RedCompanion"; // Name it for debugging
        hasSpawnedFollower = true;
        
        if (debugMode) Debug.Log($"[CombatWingManager] Spawned follower at {spawnPos}");
        
        // Start following - try multiple methods
        CompanionFollower followerScript = follower.GetComponent<CompanionFollower>();
        if (followerScript != null)
        {
            followerScript.StartFollowing();
            followerScript.SetFollowing(true); // Backup - set directly
            if (debugMode) Debug.Log($"[CombatWingManager] Called StartFollowing and SetFollowing(true)");
        }
        else
        {
            Debug.LogError($"[CombatWingManager] NO CompanionFollower component on prefab {redFragmentFollowerPrefab.name}!");
        }
        
        if (debugMode) Debug.Log("[CombatWingManager] 🔴 Red Fragment Follower spawned and following!");
        
        // Show message
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowSystemMessage("[Rage Fragment following]", 2f);
        }
    }
    
    // ===== TESTING =====
    
    [ContextMenu("Test: Force Spawn Fragment")]
    public void TestForceSpawn()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("[CombatWingManager] Must be in Play mode!");
            return;
        }
        
        GameSession.CombatWingVictory = true;
        SpawnRedFragment();
    }
}
