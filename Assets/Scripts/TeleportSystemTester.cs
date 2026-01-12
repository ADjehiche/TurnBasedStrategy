using UnityEngine;

/// <summary>
/// Simple tester for the teleportation system
/// </summary>
public class TeleportSystemTester : MonoBehaviour
{
    [Header("Test Controls")]
    [SerializeField] private KeyCode testTeleportKey = KeyCode.T;
    [SerializeField] private KeyCode checkTargetsKey = KeyCode.E;
    
    void Update()
    {
        if (Input.GetKeyDown(testTeleportKey))
        {
            TestTeleportCollection();
        }
        
        if (Input.GetKeyDown(checkTargetsKey))
        {
            CheckTeleportTargets();
        }
    }
    
    [ContextMenu("Test Teleport Collection")]
    public void TestTeleportCollection()
    {
        Debug.Log("=== TESTING TELEPORT COLLECTION ===");
        
        TeleportBlueFragmentCollectable teleporter = FindFirstObjectByType<TeleportBlueFragmentCollectable>();
        if (teleporter == null)
        {
            Debug.LogError("❌ No TeleportBlueFragmentCollectable found in scene!");
            Debug.LogError("Add SwapToTeleportSystem component to blue fragment to enable teleportation");
            return;
        }
        
        Debug.Log("✅ Found TeleportBlueFragmentCollectable, simulating interaction...");
        
        // Simulate interaction
        bool success;
        teleporter.Interact(null, out success);
        
        if (success)
        {
            Debug.Log("🚀 Teleport sequence started! Watch for dialogue and teleportation!");
        }
        else
        {
            Debug.LogError("❌ Teleport interaction failed");
        }
    }
    
    [ContextMenu("Check Teleport Targets")]
    public void CheckTeleportTargets()
    {
        Debug.Log("=== CHECKING TELEPORT TARGETS ===");
        
        // Check for various teleport targets
        GameObject mazeEntrance = GameObject.Find("MazeEntrance");
        Debug.Log($"MazeEntrance marker: {(mazeEntrance != null ? "✅ Found at " + mazeEntrance.transform.position : "❌ Not found")}");
        
        GameObject triggerObj = GameObject.Find("MazeDetectionTrigger");
        Debug.Log($"MazeDetectionTrigger: {(triggerObj != null ? "✅ Found at " + triggerObj.transform.position : "❌ Not found")}");
        
        MazeGenerator mazeGen = FindFirstObjectByType<MazeGenerator>();
        Debug.Log($"MazeGenerator: {(mazeGen != null ? "✅ Found at " + mazeGen.transform.position : "❌ Not found")}");
        
        // Check if player exists
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Debug.Log($"Player: {(player != null ? "✅ Found at " + player.transform.position : "❌ Not found (needs 'Player' tag)")}");
        
        // Check teleport components
        TeleportBlueFragmentCollectable[] teleporters = FindObjectsByType<TeleportBlueFragmentCollectable>(FindObjectsSortMode.None);
        Debug.Log($"Teleport components: {teleporters.Length} found");
        
        if (teleporters.Length > 0)
        {
            foreach (var teleporter in teleporters)
            {
                Debug.Log($"  - {teleporter.name} ready for teleportation");
            }
        }
        
        Debug.Log("=== TELEPORT CHECK COMPLETE ===");
    }
    
    [ContextMenu("Manual Teleport to Exit")]
    public void ManualTeleportToExit()
    {
        Debug.Log("=== MANUAL TELEPORT TO EXIT ===");
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ Player not found!");
            return;
        }
        
        // Find best teleport target
        Transform target = null;
        
        GameObject entrance = GameObject.Find("MazeEntrance");
        if (entrance != null)
        {
            target = entrance.transform;
        }
        else
        {
            GameObject trigger = GameObject.Find("MazeDetectionTrigger");
            if (trigger != null)
            {
                target = trigger.transform;
            }
            else
            {
                MazeGenerator mazeGen = FindFirstObjectByType<MazeGenerator>();
                if (mazeGen != null)
                {
                    target = mazeGen.transform;
                }
            }
        }
        
        if (target == null)
        {
            Debug.LogError("❌ No teleport target found!");
            return;
        }
        
        Vector3 teleportPos = target.position;
        
        // If using trigger, offset outside maze
        if (target.name.Contains("Trigger"))
        {
            teleportPos += Vector3.back * 5f;
        }
        
        teleportPos.y = player.transform.position.y;
        
        Debug.Log($"🚀 Teleporting player from {player.transform.position} to {teleportPos}");
        player.transform.position = teleportPos;
        Debug.Log("✅ Manual teleport complete!");
    }
}