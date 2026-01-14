using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public static class PostProcessSensitivityApplier
{
    public static void Apply(float sensitivity01)
    {
        sensitivity01 = Mathf.Clamp01(sensitivity01);

        var volumes = Object.FindObjectsByType<PostProcessVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (volumes == null || volumes.Length == 0) return;

        foreach (var v in volumes)
        {
            if (v == null || !v.isGlobal) continue;

            // Ensure we modify a runtime instance, not the asset
            if (v.sharedProfile != null && (v.profile == null || v.profile == v.sharedProfile))
                v.profile = Object.Instantiate(v.sharedProfile);

            var profile = v.profile;
            if (profile == null) continue;

            // Higher sensitivity => less harsh post effects
            if (profile.TryGetSettings(out Vignette vignette) && vignette != null)
            {
                vignette.enabled.Override(true);
                vignette.intensity.Override(Mathf.Lerp(0.35f, 0.05f, sensitivity01));
            }

            if (profile.TryGetSettings(out Bloom bloom) && bloom != null)
            {
                bloom.enabled.Override(true);
                bloom.intensity.Override(Mathf.Lerp(8f, 1f, sensitivity01));
            }
        }
    }
}
