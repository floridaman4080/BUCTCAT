using UnityEngine;

/// <summary>
/// 物品数据类
/// 存储物品的基本信息：名称、描述、图标等
/// </summary>
[System.Serializable]
public class ItemData
{
    public string itemId;           // 物品ID（唯一标识）
    public string itemName;         // 物品显示名称
    public string description;      // 物品描述
    public Sprite icon;             // 物品图标
    [TextArea(3, 5)]
    public string introduction;     // 物品介绍（长文本）
}

/// <summary>
/// 物品数据库
/// 管理所有物品的信息
/// </summary>
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public ItemData[] items = new ItemData[0];

    private static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ItemDatabase>("ItemDatabase");
                if (_instance == null)
                {
                    Debug.LogError("ItemDatabase not found in Resources folder!");
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// 根据物品ID获取物品数据
    /// </summary>
    public ItemData GetItemData(string itemId)
    {
        foreach (ItemData item in items)
        {
            if (item.itemId == itemId)
                return item;
        }
        Debug.LogWarning($"物品 {itemId} 未找到在数据库中!");
        return null;
    }
}
