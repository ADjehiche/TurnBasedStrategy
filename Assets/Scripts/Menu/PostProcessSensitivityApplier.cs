using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public static class PostProcessSensitivityApplier
{
    public static void Apply(float sensitivity01)
    {
        sensitivity01 = Mathf.Clamp01(sensitivity01);

        // Find a global post process volume in the active scene
        var volumes = Object.FindObjectsByType<PostProcessVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (volumes == null || volumes.Length == 0) return;

        PostProcessVolume global = null;
        foreach (var v in volumes)
        {
            if (v != null && v.isGlobal && v.profile != null)
            {
                global = v;
                break;
            }
        }
        if (global == null) return;

        var profile = global.profile;

        // Vignette: reduce intensity as sensitivity increases (less harsh)
        if (profile.TryGetSettings(out Vignette vignette) && vignette != null)
        {
            vignette.enabled.value = true;

            // Tune these two numbers if you want: 0.35 at 0 sensitivity down to 0.05 at 1 sensitivity
            float start = 0.35f;
            float end = 0.05f;
            vignette.intensity.value = Mathf.Lerp(start, end, sensitivity01);
        }

        // Bloom: reduce intensity as sensitivity increases (less glare)
        if (profile.TryGetSettings(out Bloom bloom) && bloom != null)
        {
            bloom.enabled.value = true;

            // Tune: 8 at 0 sensitivity down to 1 at 1 sensitivity
            float start = 8f;
            float end = 1f;
            bloom.intensity.value = Mathf.Lerp(start, end, sensitivity01);
        }
    }
}
