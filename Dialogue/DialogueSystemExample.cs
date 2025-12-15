using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话系统使用示例
/// 展示如何创建和触发不同类型的对话
/// </summary>
public class DialogueSystemExample : MonoBehaviour
{
    [Header("对话数据")]
    public DialogueData exampleDialogue;

    [Header("玩家数据 (模拟)")]
    public List<string> itemNames = new List<string>();    // 在Inspector中设置道具名
    public List<int> itemCounts = new List<int>();         // 对应的道具数量
    public List<string> flagNames = new List<string>();    // 在Inspector中设置标记名

    // 内部数据结构
    private Dictionary<string, int> playerItems = new Dictionary<string, int>();
    private HashSet<string> playerFlags = new HashSet<string>();
    private Dictionary<string, int> playerStats = new Dictionary<string, int>();

    private void Start()
    {
        // 初始化模拟数据
        InitializePlayerData();

        // 绑定条件检查方法到对话系统
        SetupDialogueSystem();
    }

    private void InitializePlayerData()
    {
        // 从Inspector设置的列表初始化道具
        for (int i = 0; i < itemNames.Count && i < itemCounts.Count; i++)
        {
            playerItems[itemNames[i]] = itemCounts[i];
        }

        // 从Inspector设置的列表初始化标记
        foreach (string flag in flagNames)
        {
            playerFlags.Add(flag);
        }

        // 模拟玩家属性
        playerStats["魅力"] = 15;
        playerStats["力量"] = 20;
        playerStats["智力"] = 18;
    }

    private void SetupDialogueSystem()
    {
        if (DialogueSystem.Instance != null)
        {
            // 绑定道具检查
            DialogueSystem.Instance.GetItemCount = (itemId) =>
            {
                return playerItems.ContainsKey(itemId) ? playerItems[itemId] : 0;
            };

            // 绑定标记检查
            DialogueSystem.Instance.HasFlag = (flagName) =>
            {
                return playerFlags.Contains(flagName);
            };

            // 绑定属性检查
            DialogueSystem.Instance.GetStatValue = (statName) =>
            {
                return playerStats.ContainsKey(statName) ? playerStats[statName] : 0;
            };

            // 订阅事件
            DialogueSystem.Instance.OnDialogueStart += OnDialogueStarted;
            DialogueSystem.Instance.OnDialogueEnd += OnDialogueEnded;
            DialogueSystem.Instance.OnChoiceMade += OnPlayerMadeChoice;
        }
    }

    private void OnDialogueStarted()
    {
        Debug.Log("对话开始！");
        // 可以在这里暂停游戏、禁用玩家移动等
        // Time.timeScale = 0f; // 暂停游戏（可选，注意会影响协程）
    }

    private void OnDialogueEnded()
    {
        Debug.Log("对话结束！");
        // 恢复游戏
        // Time.timeScale = 1f;
    }

    private void OnPlayerMadeChoice(DialogueChoice choice)
    {
        Debug.Log($"玩家选择了: {choice.choiceText}");
    }

    /// <summary>
    /// 开始示例对话（可以绑定到按钮或触发器）
    /// </summary>
    public void StartExampleDialogue()
    {
        if (exampleDialogue != null && DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.StartDialogue(exampleDialogue);
        }
        else
        {
            Debug.LogWarning("对话数据或对话系统未设置！");
        }
    }

    /// <summary>
    /// 添加道具
    /// </summary>
    public void AddItem(string itemId, int count = 1)
    {
        if (playerItems.ContainsKey(itemId))
            playerItems[itemId] += count;
        else
            playerItems[itemId] = count;

        Debug.Log($"获得道具: {itemId} x{count}，当前数量: {playerItems[itemId]}");
    }

    /// <summary>
    /// 移除道具
    /// </summary>
    public void RemoveItem(string itemId, int count = 1)
    {
        if (playerItems.ContainsKey(itemId))
        {
            playerItems[itemId] -= count;
            if (playerItems[itemId] <= 0)
                playerItems.Remove(itemId);

            Debug.Log($"失去道具: {itemId} x{count}");
        }
    }

    /// <summary>
    /// 设置标记
    /// </summary>
    public void SetFlag(string flagName)
    {
        playerFlags.Add(flagName);
        Debug.Log($"设置标记: {flagName}");
    }

    /// <summary>
    /// 移除标记
    /// </summary>
    public void RemoveFlag(string flagName)
    {
        playerFlags.Remove(flagName);
        Debug.Log($"移除标记: {flagName}");
    }

    /// <summary>
    /// 检查是否拥有道具
    /// </summary>
    public bool HasItem(string itemId, int count = 1)
    {
        return playerItems.ContainsKey(itemId) && playerItems[itemId] >= count;
    }

    private void Update()
    {
        // 按E键开始对话（测试用）
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (DialogueSystem.Instance != null && !DialogueSystem.Instance.IsDialogueActive())
            {
                StartExampleDialogue();
            }
        }

        // 测试按键 - 按1添加钥匙
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddItem("钥匙", 1);
        }

        // 测试按键 - 按2设置"击败BOSS"标记
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetFlag("击败BOSS");
        }

        // 测试按键 - 按3添加金币
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AddItem("金币", 10);
        }
    }

    private void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        if (DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.OnDialogueStart -= OnDialogueStarted;
            DialogueSystem.Instance.OnDialogueEnd -= OnDialogueEnded;
            DialogueSystem.Instance.OnChoiceMade -= OnPlayerMadeChoice;
        }
    }
}

/// <summary>
/// NPC对话触发器
/// 挂载到NPC上，玩家进入触发区域后可以开始对话
/// </summary>
public class NPCDialogueTrigger : MonoBehaviour
{
    [Header("对话设置")]
    [Tooltip("NPC的对话数据")]
    public DialogueData dialogueData;

    [Tooltip("起始节点索引（0表示第一个节点）")]
    public int startNodeIndex = 0;

    [Header("交互设置")]
    [Tooltip("交互按键")]
    public KeyCode interactKey = KeyCode.E;

    [Tooltip("交互提示UI（可选）")]
    public GameObject interactHint;

    [Header("触发设置")]
    [Tooltip("是否使用2D触发器")]
    public bool use2DTrigger = true;

    private bool playerInRange = false;

    private void Start()
    {
        // 初始隐藏交互提示
        if (interactHint != null)
        {
            interactHint.SetActive(false);
        }
    }

    private void Update()
    {
        // 玩家在范围内且按下交互键
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            TryStartDialogue();
        }
    }

    private void TryStartDialogue()
    {
        if (dialogueData == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 没有设置对话数据！");
            return;
        }

        if (DialogueSystem.Instance == null)
        {
            Debug.LogWarning("DialogueSystem实例不存在！");
            return;
        }

        // 如果当前没有对话进行中，开始新对话
        if (!DialogueSystem.Instance.IsDialogueActive())
        {
            DialogueSystem.Instance.StartDialogue(dialogueData, startNodeIndex);

            // 开始对话后隐藏交互提示
            if (interactHint != null)
            {
                interactHint.SetActive(false);
            }
        }
    }

    // ========== 3D触发器 ==========
    private void OnTriggerEnter(Collider other)
    {
        if (!use2DTrigger && other.CompareTag("Player"))
        {
            PlayerEnterRange();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!use2DTrigger && other.CompareTag("Player"))
        {
            PlayerExitRange();
        }
    }

    // ========== 2D触发器 ==========
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (use2DTrigger && other.CompareTag("Player"))
        {
            PlayerEnterRange();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (use2DTrigger && other.CompareTag("Player"))
        {
            PlayerExitRange();
        }
    }

    // ========== 进入/离开范围处理 ==========
    private void PlayerEnterRange()
    {
        playerInRange = true;

        // 显示交互提示
        if (interactHint != null)
        {
            interactHint.SetActive(true);
        }

        Debug.Log($"可以与 {gameObject.name} 对话，按 {interactKey} 键开始");
    }

    private void PlayerExitRange()
    {
        playerInRange = false;

        // 隐藏交互提示
        if (interactHint != null)
        {
            interactHint.SetActive(false);
        }
    }

    /// <summary>
    /// 检查这个NPC的对话是否已完成
    /// </summary>
    public bool IsDialogueCompleted()
    {
        if (dialogueData != null && DialogueSystem.Instance != null)
        {
            return DialogueSystem.Instance.IsDialogueCompleted(dialogueData);
        }
        return false;
    }
}
