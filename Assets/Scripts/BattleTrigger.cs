using UnityEngine;

/// <summary>
/// Triggers a specific battle scene when the player enters the collider.
/// Assign this to trigger colliders in Level One to specify which battle to load.
/// </summary>
public class BattleTrigger : MonoBehaviour
{
    [Header("Battle Scene Configuration")]
    [Tooltip("Name of the battle scene to load (e.g., 'Battle_1', 'Battle_2')")]
    [SerializeField] private string battleSceneName = "Battle_1";

    [Header("Trigger Settings")]
    [Tooltip("Only trigger once, then destroy this trigger")]
    [SerializeField] private bool oneTimeOnly = true;

    [Tooltip("Visual indicator in scene view")]
    [SerializeField] private Color gizmoColor = Color.red;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if player entered
        if (other.CompareTag("Player"))
        {
            // Prevent re-triggering
            if (oneTimeOnly && hasTriggered)
            {
                return;
            }

            hasTriggered = true;
            StartBattle(other.transform);
        }
    }

    private void StartBattle(Transform playerTransform)
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("[BattleTrigger] GameManager not found!");
            return;
        }

        // Save battle trigger center for proper respawn position
        Vector3 triggerCenter = GetComponent<Collider>().bounds.center;
        triggerCenter.y = playerTransform.position.y; // Keep player's Y position
        GameSession.SetBattleTriggerPosition(triggerCenter);

        // Save player position for GameManager (backward compatibility)
        gameManager.SavePlayerPosition(triggerCenter);

        // Store which battle scene to load
        GameSession.SetBattleSceneName(battleSceneName);

        Debug.Log($"[BattleTrigger] Starting battle: {battleSceneName}");

        // Start the battle with the specific scene
        gameManager.StartBattle();
    }

    // Draw trigger area in scene view
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        }
    }
}
