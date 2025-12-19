using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 物品介绍面板
/// 直接挂载到现有的 UI Panel 上使用
/// </summary>
public class ItemIntroductionPanel : MonoBehaviour
{
    [Header("UI 组件（可选，根据你的面板自定义）")]
    public Text itemNameText;           // 物品名称文本
    public Text introductionText;       // 物品介绍文本
    public Button closeButton;          // 关闭按钮

    private static ItemIntroductionPanel _instance;
    public static ItemIntroductionPanel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ItemIntroductionPanel>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        // 初始状态为隐藏
        gameObject.SetActive(false);

        // 绑定关闭按钮
        if (closeButton != null)
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    /// <summary>
    /// 显示物品介绍面板
    /// </summary>
    public void ShowItem(string itemName)
    {
        // 更新物品名称
        if (itemNameText != null)
            itemNameText.text = itemName;

        // 如果有对应的介绍文本，更新介绍
        // 这里可以根据 itemName 从数据库或其他地方获取介绍
        if (introductionText != null)
            introductionText.text = "这是 " + itemName + " 的介绍";

        // 显示面板
        gameObject.SetActive(true);
    }
}
