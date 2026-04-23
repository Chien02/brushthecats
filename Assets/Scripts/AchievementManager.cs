using System.Collections.Generic;
using UnityEngine;

// Struct này dùng để hiển thị trên Inspector
[System.Serializable]
public struct ItemDatabaseEntry
{
    public string itemID; // Ví dụ gõ vào: "bronze_brush"
    public GameObject itemPrefab; // Kéo thả Prefab tương ứng vào đây
}

public class AchievementManager : MonoBehaviour
{
    [Header("Item Database")]
    // Danh sách để bạn kéo thả trên Unity Editor
    public List<ItemDatabaseEntry> itemEntries;

    private Dictionary<string, GameObject> itemDictionary;

    void Awake()
    {
        // Chuyển từ List sang Dictionary lúc game bắt đầu để tra cứu không cần vòng lặp
        itemDictionary = new Dictionary<string, GameObject>();
        foreach (var entry in itemEntries)
        {
            itemDictionary.Add(entry.itemID, entry.itemPrefab);
        }
    }

    public void UnlockItem(string idToUnlock)
    {
        if (idToUnlock == "none") return;

        // Tìm trong Dictionary, nếu có ID này thì lấy Prefab ra
        if (itemDictionary.TryGetValue(idToUnlock, out GameObject prefabToSpawn))
        {
            Instantiate(prefabToSpawn);
            Debug.Log("Đã mở khóa: " + prefabToSpawn.name);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy item có ID: " + idToUnlock);
        }
    }
}