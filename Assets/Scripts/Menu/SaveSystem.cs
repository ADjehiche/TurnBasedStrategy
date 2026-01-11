using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    static string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    public static bool HasSave() => File.Exists(SavePath);

    public static void SaveGame(Transform playerTransform, PlayerInventoryHolder holder)
    {
        if (playerTransform == null) throw new ArgumentNullException(nameof(playerTransform));
        if (holder == null) throw new ArgumentNullException(nameof(holder));

        Vector3 pos = playerTransform.position;

        var data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            playerPos = new[] { pos.x, pos.y, pos.z },
            primarySlots = SerializeInventory(holder.InventorySystem),
            secondarySlots = SerializeInventory(holder.SecondaryInventorySystem),
            savedUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
    }

    public static SaveData LoadGame()
    {
        if (!File.Exists(SavePath)) return null;

        var json = File.ReadAllText(SavePath);
        if (string.IsNullOrWhiteSpace(json)) return null;

        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void ApplyLoadedData(SaveData data, Transform playerTransform, PlayerInventoryHolder holder, InventoryItemDatabase db)
    {
        if (data == null || playerTransform == null || holder == null || db == null) return;

        if (data.playerPos != null && data.playerPos.Length == 3)
        {
            playerTransform.position = new Vector3(data.playerPos[0], data.playerPos[1], data.playerPos[2]);
        }

        ApplyInventory(data.primarySlots, holder.InventorySystem, db);
        ApplyInventory(data.secondarySlots, holder.SecondaryInventorySystem, db);
    }

    static SavedInventorySlot[] SerializeInventory(InventorySystem inv)
    {
        if (inv == null || inv.InventorySlots == null) return System.Array.Empty<SavedInventorySlot>();

        int n = inv.InventorySlots.Count;
        var arr = new SavedInventorySlot[n];

        for (int i = 0; i < n; i++)
        {
            var slot = inv.InventorySlots[i];

            bool empty = (slot == null) || (slot.ItemData == null) || (slot.StackSize < 0);
            if (empty)
            {
                arr[i] = new SavedInventorySlot { itemId = -1, amount = 0 };
            }
            else
            {
                arr[i] = new SavedInventorySlot { itemId = slot.ItemData.itemID, amount = slot.StackSize };
            }
        }

        return arr;
    }


    static void ApplyInventory(SavedInventorySlot[] saved, InventorySystem inv, InventoryItemDatabase db)
    {
        if (inv == null || inv.InventorySlots == null) return;

        // Clear
        for (int i = 0; i < inv.InventorySlots.Count; i++)
        {
            inv.InventorySlots[i].Clear();
            inv.OnInventorySlotChanged?.Invoke(inv.InventorySlots[i]);
        }

        if (saved == null) return;

        int count = Mathf.Min(saved.Length, inv.InventorySlots.Count);
        for (int i = 0; i < count; i++)
        {
            var s = saved[i];
            if (s == null || s.itemId < 0 || s.amount <= 0) continue;

            if (db.TryGetItem(s.itemId, out var item) && item != null)
            {
                inv.InventorySlots[i].UpdateInventorySlot(item, s.amount);
                inv.OnInventorySlotChanged?.Invoke(inv.InventorySlots[i]);
            }
        }
    }
}
