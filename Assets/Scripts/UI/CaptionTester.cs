using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Testing script for the Caption System - attach to a GameObject for easy testing
/// </summary>
public class CaptionTester : MonoBehaviour
{
    [Header("Testing Controls")]
    [SerializeField] private KeyCode testInstructionKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode testMonologueKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode testSystemKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode testKeyPickupKey = KeyCode.K;
    [SerializeField] private KeyCode testDoorOpenKey = KeyCode.D;
    [SerializeField] private KeyCode hideCaptiontKey = KeyCode.H;

    [Header("Test Messages")]
    [SerializeField] private string testInstruction = "Test instruction message";
    [SerializeField] private string testMonologue = "Test internal monologue...";
    [SerializeField] private string testSystem = "Test system notification";

    void Update()
    {
        // Test individual caption types
        if (Input.GetKeyDown(testInstructionKey))
        {
            TestInstruction();
        }
        
        if (Input.GetKeyDown(testMonologueKey))
        {
            TestMonologue();
        }
        
        if (Input.GetKeyDown(testSystemKey))
        {
            TestSystemMessage();
        }
        
        // Test key pickup sequence
        if (Input.GetKeyDown(testKeyPickupKey))
        {
            TestKeyPickup();
        }
        
        // Test door open celebration
        if (Input.GetKeyDown(testDoorOpenKey))
        {
            TestDoorOpen();
        }
        
        // Hide current caption
        if (Input.GetKeyDown(hideCaptiontKey))
        {
            HideCaption();
        }
    }
    
    void OnGUI()
    {
        // Display instructions on screen
        GUI.Box(new Rect(10, 10, 300, 150), "Caption System Tester");
        
        GUI.Label(new Rect(20, 40, 280, 20), $"Press {testInstructionKey} - Test Instruction");
        GUI.Label(new Rect(20, 60, 280, 20), $"Press {testMonologueKey} - Test Monologue");
        GUI.Label(new Rect(20, 80, 280, 20), $"Press {testSystemKey} - Test System Message");
        GUI.Label(new Rect(20, 100, 280, 20), $"Press {testKeyPickupKey} - Test Key Pickup");
        GUI.Label(new Rect(20, 120, 280, 20), $"Press {testDoorOpenKey} - Test Door Open");
        GUI.Label(new Rect(20, 140, 280, 20), $"Press {hideCaptiontKey} - Hide Caption");
        
        // Status info
        if (CaptionManager.Instance != null)
        {
            GUI.Label(new Rect(20, 160, 280, 20), "Caption Manager: ✓ Ready");
        }
        else
        {
            GUI.Label(new Rect(20, 160, 280, 20), "Caption Manager: ✗ Not Found");
        }
    }
    
    [ContextMenu("Test Instruction")]
    public void TestInstruction()
    {
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowInstruction(testInstruction);
            Debug.Log("CaptionTester: Showing test instruction");
        }
        else
        {
            Debug.LogWarning("CaptionTester: CaptionManager not found!");
        }
    }
    
    [ContextMenu("Test Monologue")]
    public void TestMonologue()
    {
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(testMonologue);
            Debug.Log("CaptionTester: Showing test monologue");
        }
        else
        {
            Debug.LogWarning("CaptionTester: CaptionManager not found!");
        }
    }
    
    [ContextMenu("Test System Message")]
    public void TestSystemMessage()
    {
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowSystemMessage(testSystem);
            Debug.Log("CaptionTester: Showing test system message");
        }
        else
        {
            Debug.LogWarning("CaptionTester: CaptionManager not found!");
        }
    }
    
    [ContextMenu("Test Key Pickup")]
    public void TestKeyPickup()
    {
        var levelController = FindFirstObjectByType<LevelOneCaptionController>();
        if (levelController != null)
        {
            levelController.OnKeyPickedUp();
            Debug.Log("CaptionTester: Triggering key pickup sequence");
        }
        else
        {
            Debug.LogWarning("CaptionTester: LevelOneCaptionController not found!");
        }
    }
    
    [ContextMenu("Test Door Open")]
    public void TestDoorOpen()
    {
        var levelController = FindFirstObjectByType<LevelOneCaptionController>();
        if (levelController != null)
        {
            levelController.OnDoorOpened();
            Debug.Log("CaptionTester: Triggering door open celebration");
        }
        else
        {
            Debug.LogWarning("CaptionTester: LevelOneCaptionController not found!");
        }
    }
    
    [ContextMenu("Hide Caption")]
    public void HideCaption()
    {
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.HideCaption();
            Debug.Log("CaptionTester: Hiding caption");
        }
        else
        {
            Debug.LogWarning("CaptionTester: CaptionManager not found!");
        }
    }
    
    [ContextMenu("Test All Captions")]
    public void TestAllCaptions()
    {
        StartCoroutine(TestAllCaptionsSequence());
    }
    
    private System.Collections.IEnumerator TestAllCaptionsSequence()
    {
        Debug.Log("CaptionTester: Starting full caption test sequence");
        
        TestInstruction();
        yield return new WaitForSeconds(4f);
        
        TestMonologue();
        yield return new WaitForSeconds(4f);
        
        TestSystemMessage();
        yield return new WaitForSeconds(4f);
        
        TestKeyPickup();
        
        Debug.Log("CaptionTester: Caption test sequence completed");
    }
}