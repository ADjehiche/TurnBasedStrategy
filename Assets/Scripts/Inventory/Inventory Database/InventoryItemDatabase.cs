using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System/Item Database")]
public class InventoryItemDatabase : ScriptableObject
{
    public List<InventoryItemData> items = new();

    Dictionary<int, InventoryItemData> _byId;

    void OnEnable() => Rebuild();

    public void Rebuild()
    {
        _byId = new Dictionary<int, InventoryItemData>();
        foreach (var it in items)
        {
            if (it == null) continue;

            if (_byId.ContainsKey(it.itemID))
            {
                Debug.LogWarning($"Duplicate itemID {it.itemID} in database. Item '{it.name}' conflicts with '{_byId[it.itemID].name}'.");
                continue;
            }
            _byId.Add(it.itemID, it);
        }
    }

    public bool TryGetItem(int id, out InventoryItemData item)
    {
        if (_byId == null) Rebuild();
        return _byId.TryGetValue(id, out item);
    }
}
