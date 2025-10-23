using UnityEngine;
using UnityEngine.SceneManagement;

public class LightingManager : MonoBehaviour
{
    public static LightingManager instance;
    
    [SerializeField] private Light directionalLight;
    
    [SerializeField] private float lightIntensity = 1.0f;
    [SerializeField] private Color lightColor = Color.white;
    [SerializeField] private bool useSkybox = true;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyLightingSettings();
    }
    
    private void ApplyLightingSettings()
    {
        if (directionalLight == null)
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    break;
                }
            }
        }
        
        if (directionalLight != null)
        {
            directionalLight.intensity = lightIntensity;
            directionalLight.color = lightColor;
        }
        
        RenderSettings.skybox = useSkybox ? RenderSettings.skybox : null;
        
        DynamicGI.UpdateEnvironment();
    }
    
    public void UpdateLightingSettings(float intensity, Color color, bool useSkyboxSetting)
    {
        lightIntensity = intensity;
        lightColor = color;
        useSkybox = useSkyboxSetting;
        ApplyLightingSettings();
    }
}