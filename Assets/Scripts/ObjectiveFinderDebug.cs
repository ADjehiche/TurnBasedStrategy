using UnityEngine;

public class ObjectiveFinderDebug : MonoBehaviour
{
    void Start()
    {
        var found = FindObjectsOfType<SimpleLevelTwoObjectives>(true);
        Debug.Log($"[ObjectiveFinderDebug] Found {found.Length} SimpleLevelTwoObjectives in scene.");
        foreach (var obj in found)
        {
            Debug.Log($"[ObjectiveFinderDebug] - GameObject: {obj.gameObject.name}, Active: {obj.gameObject.activeInHierarchy}, Enabled: {obj.enabled}");
        }
    }
}
