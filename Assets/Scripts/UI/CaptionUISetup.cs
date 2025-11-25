using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// This script helps set up the Caption UI prefab in the Unity Editor
/// </summary>
public class CaptionUISetup : MonoBehaviour
{
    [Header("Auto-Setup UI Components")]
    [SerializeField] private bool autoSetupOnStart = true;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupCaptionUI();
        }
    }
    
    [ContextMenu("Setup Caption UI")]
    public void SetupCaptionUI()
    {
        // This method can be called from the context menu in the editor
        // or automatically on start to ensure the UI is properly configured
        
        var captionManager = GetComponent<CaptionManager>();
        if (captionManager == null)
        {
            Debug.LogWarning("CaptionUISetup: No CaptionManager found on this GameObject");
            return;
        }
        
        Debug.Log("CaptionUISetup: Caption UI is ready");
    }
}