using System;

[Serializable]
public class SavedInventorySlot
{
    public int itemId;   // -1 = empty
    public int amount;   // stack size
}

[Serializable]
public class SaveData
{
    public string sceneName;
    public float[] playerPos;                 // [x,y,z]
    public SavedInventorySlot[] primarySlots;
    public SavedInventorySlot[] secondarySlots;
    public long savedUnixMs;
}
