using TMPro;
using UnityEngine;

public static class TextScaleUtility
{
    public static void ApplyGlobalTextScale(float scale)
    {
        var texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var t in texts)
        {
            if (t == null) continue;

            if (!t.TryGetComponent(out TextScaleBase baseData))
            {
                baseData = t.gameObject.AddComponent<TextScaleBase>();
                baseData.baseAutoSize = t.enableAutoSizing;
                baseData.baseFontSize = t.fontSize;
                baseData.baseMin = t.fontSizeMin;
                baseData.baseMax = t.fontSizeMax;
            }

            if (baseData.baseAutoSize)
            {
                t.enableAutoSizing = true;
                t.fontSizeMin = baseData.baseMin * scale;
                t.fontSizeMax = baseData.baseMax * scale;
            }
            else
            {
                t.enableAutoSizing = false;
                t.fontSize = baseData.baseFontSize * scale;
            }
        }
    }
}

public class TextScaleBase : MonoBehaviour
{
    public bool baseAutoSize;
    public float baseFontSize;
    public float baseMin;
    public float baseMax;
}
