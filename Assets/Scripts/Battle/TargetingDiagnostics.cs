using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Diagnostic tool to help debug targeting issues.
/// Attach this to any GameObject in your battle scene and check the console for detailed info.
/// </summary>
public class TargetingDiagnostics : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool enableDiagnostics = true;
    [SerializeField] private KeyCode diagnosticKey = KeyCode.D; // Press D to run diagnostics
    
    [Header("Camera")]
    [SerializeField] private Camera worldCamera;

    private void Start()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;

        if (enableDiagnostics)
        {
            Debug.Log("=== TARGETING DIAGNOSTICS ENABLED ===");
            Debug.Log($"Press '{diagnosticKey}' to run full scene diagnostics");
            RunStartupDiagnostics();
        }
    }

    private void Update()
    {
        if (!enableDiagnostics) return;

        // Run diagnostics when key is pressed
        if (Input.GetKeyDown(diagnosticKey))
        {
            RunFullDiagnostics();
        }

        // Show what's under the mouse cursor
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShowMouseTargets();
        }
    }

    private void RunStartupDiagnostics()
    {
        Debug.Log("\n=== SCENE SETUP CHECK ===");

        // Check for enemies
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        Debug.Log($"✓ Found {enemies.Length} enemies in scene:");
        foreach (var enemy in enemies)
        {
            CheckEnemySetup(enemy);
        }

        // Check for player
        PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
        if (player != null)
        {
            Debug.Log($"✓ Found PlayerHealth: {player.gameObject.name}");
            CheckPlayerSetup(player);
        }
        else
        {
            Debug.LogError("✗ No PlayerHealth found in scene!");
        }

        // Check for EventSystem
        if (EventSystem.current != null)
        {
            Debug.Log($"✓ EventSystem found: {EventSystem.current.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("⚠ No EventSystem found! UI raycasting won't work.");
        }

        // Check for TargetingSystem
        if (TargetingSystem.Instance != null)
        {
            Debug.Log($"✓ TargetingSystem found: {TargetingSystem.Instance.gameObject.name}");
        }
        else
        {
            Debug.LogError("✗ No TargetingSystem.Instance found!");
        }

        Debug.Log("=== SCENE SETUP CHECK COMPLETE ===\n");
    }

    private void RunFullDiagnostics()
    {
        Debug.Log("\n=== FULL DIAGNOSTICS ===");
        RunStartupDiagnostics();
        
        // Check layers
        Debug.Log("\n--- LAYER INFORMATION ---");
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            Debug.Log($"Enemy '{enemy.gameObject.name}' is on layer: {LayerMask.LayerToName(enemy.gameObject.layer)} ({enemy.gameObject.layer})");
        }

        Debug.Log("=== FULL DIAGNOSTICS COMPLETE ===\n");
    }

    private void CheckEnemySetup(EnemyHealth enemy)
    {
        string name = enemy.gameObject.name;
        Debug.Log($"\n  Enemy: {name}");
        Debug.Log($"    - Position: {enemy.transform.position}");
        Debug.Log($"    - Tag: {enemy.gameObject.tag}");
        Debug.Log($"    - Layer: {LayerMask.LayerToName(enemy.gameObject.layer)}");
        Debug.Log($"    - Active: {enemy.gameObject.activeInHierarchy}");
        
        // Check for colliders
        Collider[] colliders3D = enemy.GetComponentsInChildren<Collider>();
        Collider2D[] colliders2D = enemy.GetComponentsInChildren<Collider2D>();
        
        if (colliders3D.Length > 0)
        {
            Debug.Log($"    - 3D Colliders: {colliders3D.Length}");
            foreach (var col in colliders3D)
            {
                Debug.Log($"      • {col.GetType().Name} on '{col.gameObject.name}' (enabled: {col.enabled})");
            }
        }
        
        if (colliders2D.Length > 0)
        {
            Debug.Log($"    - 2D Colliders: {colliders2D.Length}");
            foreach (var col in colliders2D)
            {
                Debug.Log($"      • {col.GetType().Name} on '{col.gameObject.name}' (enabled: {col.enabled})");
            }
        }
        
        if (colliders3D.Length == 0 && colliders2D.Length == 0)
        {
            Debug.LogWarning($"    ⚠ WARNING: No colliders found on {name} or its children! Enemy won't be clickable.");
        }

        // Check for Canvas elements (for UI-based targeting)
        UnityEngine.UI.Graphic[] graphics = enemy.GetComponentsInChildren<UnityEngine.UI.Graphic>();
        if (graphics.Length > 0)
        {
            Debug.Log($"    - UI Graphics: {graphics.Length} (for Canvas-based clicking)");
        }
    }

    private void CheckPlayerSetup(PlayerHealth player)
    {
        Debug.Log($"  Player: {player.gameObject.name}");
        Debug.Log($"    - Position: {player.transform.position}");
        Debug.Log($"    - Active: {player.gameObject.activeInHierarchy}");
        
        // Check for colliders
        Collider[] colliders3D = player.GetComponentsInChildren<Collider>();
        Collider2D[] colliders2D = player.GetComponentsInChildren<Collider2D>();
        
        if (colliders3D.Length > 0 || colliders2D.Length > 0)
        {
            Debug.Log($"    - Colliders: {colliders3D.Length} (3D), {colliders2D.Length} (2D)");
        }

        // Check for Canvas elements
        UnityEngine.UI.Graphic[] graphics = player.GetComponentsInChildren<UnityEngine.UI.Graphic>();
        if (graphics.Length > 0)
        {
            Debug.Log($"    - UI Graphics: {graphics.Length}");
        }
    }

    private void ShowMouseTargets()
    {
        if (Mouse.current == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Debug.Log($"\n=== MOUSE CLICK at {screenPos} ===");

        // Check UI
        if (EventSystem.current != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            Debug.Log($"UI Raycast hits: {results.Count}");
            foreach (var result in results)
            {
                if (result.gameObject != null)
                {
                    EnemyHealth enemy = result.gameObject.GetComponentInParent<EnemyHealth>();
                    string enemyInfo = enemy != null ? $" [HAS ENEMYHEALTH: {enemy.gameObject.name}]" : "";
                    Debug.Log($"  • UI Hit: {result.gameObject.name}{enemyInfo}");
                }
            }
        }

        // Check 3D world
        if (worldCamera != null)
        {
            Ray ray = worldCamera.ScreenPointToRay(screenPos);
            RaycastHit[] hits3D = Physics.RaycastAll(ray, 1000f);
            
            Debug.Log($"3D Physics hits: {hits3D.Length}");
            foreach (var hit in hits3D)
            {
                EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
                string enemyInfo = enemy != null ? $" [HAS ENEMYHEALTH: {enemy.gameObject.name}]" : "";
                Debug.Log($"  • 3D Hit: {hit.collider.gameObject.name} at distance {hit.distance}{enemyInfo}");
            }

            // Check 2D world
            RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
            if (hit2D.collider != null)
            {
                EnemyHealth enemy = hit2D.collider.GetComponentInParent<EnemyHealth>();
                string enemyInfo = enemy != null ? $" [HAS ENEMYHEALTH: {enemy.gameObject.name}]" : "";
                Debug.Log($"2D Physics hit: {hit2D.collider.gameObject.name}{enemyInfo}");
            }
            else
            {
                Debug.Log("2D Physics hit: None");
            }
        }

        Debug.Log("=========================\n");
    }
}
