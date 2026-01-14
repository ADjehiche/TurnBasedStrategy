using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SIMPLE objective UI - connects to your existing panel with custom image
/// Just updates the text and slider on your existing UI elements
/// </summary>
public class SimpleObjectiveUI : MonoBehaviour
{
    [Header("Your Existing UI References")]
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private GameObject objectivePanel;
    
    [Header("Settings")]
    [SerializeField] private bool hideSliderWhenNotUsed = true;
    [SerializeField] private string defaultText = "No current objective";
    
    private bool isInitialized = false;
    
    private void Start()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        if (isInitialized) return;
        
        // Validate that UI references are assigned
        if (objectiveText == null)
        {
            Debug.LogError("[SimpleObjectiveUI] ObjectiveText not assigned! Please drag your text component to the ObjectiveText field.");
        }
        
        if (objectivePanel == null)
        {
            Debug.LogError("[SimpleObjectiveUI] ObjectivePanel not assigned! Please drag your panel GameObject to the ObjectivePanel field.");
        }
        
        // Setup slider
        if (progressSlider != null)
        {
            progressSlider.interactable = false; // Make it display-only
            progressSlider.value = 0f;
            
            if (hideSliderWhenNotUsed)
            {
                progressSlider.gameObject.SetActive(false);
            }
        }
        
        // Set default text directly (avoid calling SetObjectiveText to prevent recursion)
        if (objectiveText != null)
        {
            objectiveText.text = defaultText;
        }
        
        // Hide panel initially
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }
        
        isInitialized = true;
        
        Debug.Log("[SimpleObjectiveUI] Initialized with existing UI elements");
    }
    
    /// <summary>
    /// Update the objective text - this is the main method you'll use
    /// </summary>
    public void SetObjectiveText(string newText)
    {
        if (!isInitialized) Initialize();
        
        if (objectiveText != null)
        {
            objectiveText.text = newText;
            Debug.Log($"[SimpleObjectiveUI] Updated text: {newText}");
        }
        else
        {
            Debug.LogWarning("[SimpleObjectiveUI] ObjectiveText is null! Make sure to assign it in the inspector.");
        }
        
        // Show panel when setting text
        if (objectivePanel != null && !string.IsNullOrEmpty(newText) && newText != defaultText)
        {
            objectivePanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Update the progress slider (0.0 to 1.0)
    /// </summary>
    public void SetProgress(float progress)
    {
        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(true);
            progressSlider.value = Mathf.Clamp01(progress);
            Debug.Log($"[SimpleObjectiveUI] Updated progress: {progress:P0}");
        }
    }
    
    /// <summary>
    /// Hide the progress slider
    /// </summary>
    public void HideProgress()
    {
        if (progressSlider != null && hideSliderWhenNotUsed)
        {
            progressSlider.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Clear the objective and hide panel
    /// </summary>
    public void ClearObjective()
    {
        if (objectiveText != null)
        {
            objectiveText.text = "";
        }
        
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }
        
        Debug.Log("[SimpleObjectiveUI] Cleared objective");
    }
    
    /// <summary>
    /// Show the objective panel
    /// </summary>
    public void ShowPanel()
    {
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide the objective panel
    /// </summary>
    public void HidePanel()
    {
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Set a custom panel sprite (if your panel uses an Image component)
    /// </summary>
    public void SetPanelSprite(Sprite customSprite)
    {
        if (objectivePanel != null)
        {
            Image panelImage = objectivePanel.GetComponent<Image>();
            if (panelImage != null)
            {
                if (customSprite != null)
                {
                    panelImage.sprite = customSprite;
                    panelImage.type = Image.Type.Sliced;
                    panelImage.color = Color.white;
                    Debug.Log("[SimpleObjectiveUI] Applied custom sprite to panel");
                }
                else
                {
                    panelImage.sprite = null;
                    panelImage.type = Image.Type.Simple;
                    panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Default dark color
                    Debug.Log("[SimpleObjectiveUI] Removed custom sprite from panel");
                }
            }
        }
    }
}