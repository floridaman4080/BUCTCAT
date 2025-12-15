using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 引用 TextMeshPro 命名空间
// 定义选项的数据结构
[System.Serializable]
public class DialogueOption
{
    public string text;
    public string nextId;
    public string reqItem;  // 需要的物品，为空则不需要
    public string giveItem; // 给予的物品，为空则不给
    public string action;   // 特殊动作，比如 "reset"
}

// 定义节点的数据结构
[System.Serializable]
public class SimpleDialogueNode
{
    public string id;
    public string speaker;
    [TextArea(3, 10)] public string text;
    public string mood; // angry, stern, neutral, etc.
    public List<DialogueOption> options = new List<DialogueOption>();
}
public class DialogueManager : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("UI 组件引用")]
    public TextMeshProUGUI speakerText;    // 显示说话人名字
    public TextMeshProUGUI dialogueText;   // 显示对话内容
    public Image backgroundImage;          // 背景图（用于改变颜色氛围）
    public Transform optionsContainer;     // 选项按钮的父物体
    public Button optionButtonPrefab;      // 选项按钮的预制体 (Prefab)

    [Header("UI - 物品栏")]
    public TextMeshProUGUI inventoryText;  // 简单显示拥有的物品

    [Header("设置")]
    public float typeSpeed = 0.05f;        // 打字机速度

    // 内部状态
    private Dictionary<string, SimpleDialogueNode> dialogueMap = new Dictionary<string, SimpleDialogueNode>();
    private List<string> inventory = new List<string>();
    private string currentTextFull = "";
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    void Start()
    {
        BuildDialogueData();

        // 2. 开始对话
        ShowNode("start");
    }

    // Update is called once per frame
    void BuildDialogueData()
    {
        // 节点: start
        var start = CreateNode("start", "守门人", "站住！前方是禁地。没有长官的许可，任何人不得入内。", "stern");
        start.options.Add(new DialogueOption { text = "我想进去看看。", nextId = "reject_1" });
        start.options.Add(new DialogueOption { text = "我是新来的守卫。", nextId = "lie_check" });
        start.options.Add(new DialogueOption { text = "（观察四周）", nextId = "search_area" });
        start.options.Add(new DialogueOption { text = "给你看这个令牌。", nextId = "pass_gate", reqItem = "古老的令牌" });
        dialogueMap.Add(start.id, start);

        // 节点: reject_1
        var reject = CreateNode("reject_1", "守门人", "好奇心会害死猫。快滚，别逼我动手。", "angry");
        reject.options.Add(new DialogueOption { text = "好的，我这就走。", nextId = "start" });
        reject.options.Add(new DialogueOption { text = "你敢威胁我？（拔剑）", nextId = "fight_ending" });
        dialogueMap.Add(reject.id, reject);

        // 节点: lie_check
        var lie = CreateNode("lie_check", "守门人", "新来的？哼，我怎么没听说过。你的徽章呢？", "suspicious");
        lie.options.Add(new DialogueOption { text = "我...我忘带了。", nextId = "reject_1" });
        lie.options.Add(new DialogueOption { text = "其实我在撒谎。", nextId = "start" });
        dialogueMap.Add(lie.id, lie);

        // 节点: search_area (获得物品)
        var search = CreateNode("search_area", "旁白", "你在旁边的草丛里翻找了一会儿... 发现了一个闪闪发光的东西！", "neutral");
        search.options.Add(new DialogueOption { text = "捡起来看看。", nextId = "get_item", giveItem = "古老的令牌" });
        dialogueMap.Add(search.id, search);

        // 节点: get_item
        var getItem = CreateNode("get_item", "系统", "你获得了物品：【古老的令牌】。", "system");
        getItem.options.Add(new DialogueOption { text = "回到守门人面前。", nextId = "start" });
        dialogueMap.Add(getItem.id, getItem);

        // 节点: pass_gate
        var pass = CreateNode("pass_gate", "守门人", "这...这是皇家亲卫队的令牌！失敬了，大人！请进！", "shocked");
        pass.options.Add(new DialogueOption { text = "（进入大门）", nextId = "good_ending" });
        dialogueMap.Add(pass.id, pass);

        // 结局
        var badEnd = CreateNode("fight_ending", "结局", "【坏结局】你被击败并扔出了城外。", "bad");
        badEnd.options.Add(new DialogueOption { text = "重新开始", nextId = "start", action = "reset" });
        dialogueMap.Add(badEnd.id, badEnd);

        var goodEnd = CreateNode("good_ending", "结局", "【真结局】你成功进入了神秘的禁地。", "good");
        goodEnd.options.Add(new DialogueOption { text = "再次游玩", nextId = "start", action = "reset" });
        dialogueMap.Add(goodEnd.id, goodEnd);
    }

    // 辅助构建函数
    SimpleDialogueNode CreateNode(string id, string speaker, string text, string mood)
    {
        return new SimpleDialogueNode { id = id, speaker = speaker, text = text, mood = mood };
    }

    // --- 核心逻辑 ---

    public void ShowNode(string nodeId)
    {
        if (!dialogueMap.ContainsKey(nodeId)) return;

        SimpleDialogueNode node = dialogueMap[nodeId];

        // 1. 更新文本 UI
        speakerText.text = node.speaker;
        currentTextFull = node.text;

        // 2. 启动打字机效果
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypewriterEffect(node.text));

        // 3. 更新背景氛围颜色
        UpdateMoodColor(node.mood);

        // 4. 生成选项按钮
        GenerateOptions(node.options);
    }

    IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in text.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
    }

    // 点击对话框可以跳过打字
    public void SkipTyping()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentTextFull;
            isTyping = false;
        }
    }

    void GenerateOptions(List<DialogueOption> options)
    {
        // 清除旧按钮
        foreach (Transform child in optionsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var opt in options)
        {
            // 检查是否有物品锁
            bool isLocked = !string.IsNullOrEmpty(opt.reqItem) && !inventory.Contains(opt.reqItem);

            // 实例化按钮
            Button btn = Instantiate(optionButtonPrefab, optionsContainer);
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();

            // 设置按钮文本
            if (isLocked)
            {
                btnText.text = $"<color=red>[缺少: {opt.reqItem}]</color> {opt.text}";
                btn.interactable = false; // 禁用点击
                // 也可以选择让它可以点击但播放“无法操作”的音效
            }
            else if (!string.IsNullOrEmpty(opt.reqItem))
            {
                btnText.text = $"<color=green>[持有: {opt.reqItem}]</color> {opt.text}";
                btn.interactable = true;
            }
            else
            {
                btnText.text = opt.text;
                btn.interactable = true;
            }

            // 绑定点击事件
            if (!isLocked)
            {
                btn.onClick.AddListener(() => OnOptionClicked(opt));
            }
        }
    }

    void OnOptionClicked(DialogueOption opt)
    {
        if (isTyping) return; // 防止在打字时误触跳转，或者可以在这里执行SkipTyping

        // 1. 处理给予物品
        if (!string.IsNullOrEmpty(opt.giveItem))
        {
            if (!inventory.Contains(opt.giveItem))
            {
                inventory.Add(opt.giveItem);
                UpdateInventoryUI();
            }
        }

        // 2. 处理特殊动作 (Reset)
        if (opt.action == "reset")
        {
            inventory.Clear();
            UpdateInventoryUI();
        }

        // 3. 跳转到下一节点
        if (!string.IsNullOrEmpty(opt.nextId))
        {
            ShowNode(opt.nextId);
        }
    }

    void UpdateInventoryUI()
    {
        if (inventoryText != null)
        {
            inventoryText.text = "物品: " + string.Join(", ", inventory);
        }
    }

    void UpdateMoodColor(string mood)
    {
        if (backgroundImage == null) return;

        Color targetColor = Color.black;
        switch (mood)
        {
            case "angry": targetColor = new Color(0.3f, 0, 0); break; //暗红
            case "good": targetColor = new Color(0, 0.2f, 0.4f); break; //暗蓝
            case "shocked": targetColor = new Color(0.3f, 0.3f, 0); break; //暗黄
            case "system": targetColor = new Color(0, 0.3f, 0.2f); break; //暗绿
            default: targetColor = new Color(0.1f, 0.1f, 0.1f); break; //深灰
        }
        backgroundImage.color = targetColor;
    }

    // 如果你有全屏点击遮罩，可以绑定这个
    public void OnScreenClick()
    {
        SkipTyping();
    }
}


