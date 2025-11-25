using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    
    [Range(0f, 1f)]
    public float volume = 1f;
    
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    
    [Range(0f, 1f)]
    public float spatialBlend = 0f; // 0 = 2D, 1 = 3D
    
    public bool loop = false;
    
    [HideInInspector]
    public AudioSource source;
}
