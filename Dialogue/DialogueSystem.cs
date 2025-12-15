using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// 对话节点类型
/// </summary>
public enum DialogueNodeType
{
    Normal,         // 普通对话
    Conditional,    // 条件对话（需要道具等）
    Choice          // 分支选择对话
}

/// <summary>
/// 对话条件类型IsDialogueActive
/// </summary>
public enum ConditionType
{
    HasItem,        // 拥有道具
    HasFlag,        // 拥有标记（完成某任务等）
    StatCheck       // 属性检查
}

/// <summary>
/// 对话条件
/// </summary>
[Serializable]
public class DialogueCondition
{
    public ConditionType conditionType;
    public string conditionKey;     // 道具ID、标记名或属性名
    public int requiredValue = 1;   // 所需数量或数值
    public string failMessage = "条件不满足，无法继续对话。"; // 条件不满足时显示的消息
}

/// <summary>
/// 对话选项
/// </summary>
[Serializable]
public class DialogueChoice
{
    public string choiceText;           // 选项文本
    public int nextNodeId;              // 选择后跳转的节点ID
    public DialogueCondition condition; // 选项条件（可选）
    public bool hideIfConditionNotMet;  // 条件不满足时是否隐藏选项
    public UnityEvent onChoiceSelected; // 选择此选项时触发的事件
}

/// <summary>
/// 节点触发的Bool设置
/// </summary>
[Serializable]
public class DialogueBoolSetter
{
    public string boolName;         // Bool的名称/键
    public bool setValue = true;    // 设置为什么值
}

/// <summary>
/// 对话节点
/// </summary>
[Serializable]
public class DialogueNode
{
    public int nodeId;                      // 节点ID
    public string speakerName;              // 说话者名称
    [TextArea(3, 5)]
    public string dialogueText;             // 对话内容
    public Sprite speakerPortrait;          // 说话者头像
    public DialogueNodeType nodeType;       // 节点类型

    [Header("进度控制")]
    public bool isCheckpoint = false;       // 是否是检查点（下次对话从这里开始）
    public int resumeNodeId = -1;           // 如果设置了，下次对话会从这个节点开始（-1表示使用当前节点）

    [Header("Bool 触发器")]
    public List<DialogueBoolSetter> onEnterSetBools = new List<DialogueBoolSetter>();  // 进入节点时设置的Bool
    public List<DialogueBoolSetter> onExitSetBools = new List<DialogueBoolSetter>();   // 离开节点时设置的Bool

    // 普通对话 - 下一个节点ID（-1表示对话结束）
    public int nextNodeId = -1;

    // 条件对话
    public DialogueCondition condition;
    public int conditionMetNodeId;          // 条件满足时跳转的节点
    public int conditionNotMetNodeId;       // 条件不满足时跳转的节点

    // 分支选择
    public List<DialogueChoice> choices = new List<DialogueChoice>();

    // 事件
    public UnityEvent onNodeEnter;          // 进入此节点时触发
    public UnityEvent onNodeExit;           // 离开此节点时触发
}

/// <summary>
/// 对话数据
/// </summary>
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string dialogueId;
    public List<DialogueNode> nodes = new List<DialogueNode>();

    public DialogueNode GetNode(int nodeId)
    {
        return nodes.Find(n => n.nodeId == nodeId);
    }
}

/// <summary>
/// 对话系统主类
/// </summary>
public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Image speakerPortraitImage;
    public GameObject choicesContainer;
    public GameObject choiceButtonPrefab;
    public Button continueButton;
    public GameObject conditionHintPanel;
    public TextMeshProUGUI conditionHintText;

    [Header("Settings")]
    public float textSpeed = 0.05f;         // 文字显示速度
    public bool useTypewriterEffect = true;  // 是否使用打字机效果

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip typingSound;
    public AudioClip choiceSound;

    // 当前对话状态
    private DialogueData currentDialogue;
    private DialogueNode currentNode;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    // 对话进度记忆 - 记录每个对话应该从哪个节点开始
    private Dictionary<string, int> dialogueProgress = new Dictionary<string, int>();

    // 对话检查点 - 记录特定的"存档点"节点
    private Dictionary<string, HashSet<int>> dialogueCheckpoints = new Dictionary<string, HashSet<int>>();

    // ★ 对话完成状态 - 记录每个对话是否已完成
    private static Dictionary<string, bool> dialogueCompleted = new Dictionary<string, bool>();

    // ★ 全局 Bool 存储 - 可以被任何脚本访问
    private static Dictionary<string, bool> globalBools = new Dictionary<string, bool>();

    // 条件检查委托
    public Func<string, int> GetItemCount;      // 获取道具数量
    public Func<string, bool> HasFlag;          // 检查标记
    public Func<string, int> GetStatValue;      // 获取属性值

    // 事件
    public event Action OnDialogueStart;
    public event Action OnDialogueEnd;
    public event Action<DialogueNode> OnNodeChanged;
    public event Action<DialogueChoice> OnChoiceMade;
    public event Action<string> OnDialogueCompleted;  // ★ 对话完成事件，参数是对话ID
    public static event Action<string, bool> OnBoolChanged;  // ★ Bool变化事件

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        // 按空格或回车继续对话
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (currentNode != null && currentNode.nodeType != DialogueNodeType.Choice)
            {
                OnContinueClicked();
            }
        }

        // 按鼠标左键或触摸屏幕跳过打字效果
        if (isTyping)
        {
            // 鼠标点击
            if (Input.GetMouseButtonDown(0))
            {
                SkipTyping();
            }
            // 触摸屏幕（手机端）
            else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                SkipTyping();
            }
        }
    }

    /// <summary>
    /// 开始对话（自动从记忆的进度开始）
    /// </summary>
    public void StartDialogue(DialogueData dialogue, int startNodeId = 0)
    {
        if (dialogue == null || dialogue.nodes.Count == 0)
        {
            Debug.LogWarning("对话数据为空！");
            return;
        }

        currentDialogue = dialogue;
        isDialogueActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        OnDialogueStart?.Invoke();

        // 获取对话的唯一标识（优先使用dialogueId，否则使用资源名称）
        string dialogueKey = GetDialogueKey(dialogue);

        // 检查是否有保存的进度，如果有则从保存的位置开始
        int actualStartNode = startNodeId;
        if (!string.IsNullOrEmpty(dialogueKey) && dialogueProgress.ContainsKey(dialogueKey))
        {
            actualStartNode = dialogueProgress[dialogueKey];
            Debug.Log($"[对话系统] 从保存的进度开始: {dialogueKey} -> 节点 {actualStartNode}");
        }
        else
        {
            Debug.Log($"[对话系统] 从头开始对话: {dialogueKey} -> 节点 {actualStartNode}");
        }

        DisplayNode(actualStartNode);
    }

    /// <summary>
    /// 获取对话的唯一标识键
    /// </summary>
    private string GetDialogueKey(DialogueData dialogue)
    {
        if (dialogue == null) return null;

        // 优先使用设置的dialogueId，否则使用ScriptableObject的名称
        if (!string.IsNullOrEmpty(dialogue.dialogueId))
            return dialogue.dialogueId;

        return dialogue.name; // ScriptableObject的资源名称
    }

    /// <summary>
    /// 开始对话（强制从指定节点开始，忽略记忆）
    /// </summary>
    public void StartDialogueFromBeginning(DialogueData dialogue, int startNodeId = 0)
    {
        if (dialogue == null || dialogue.nodes.Count == 0)
        {
            Debug.LogWarning("对话数据为空！");
            return;
        }

        // 清除该对话的进度记录
        if (dialogueProgress.ContainsKey(dialogue.dialogueId))
        {
            dialogueProgress.Remove(dialogue.dialogueId);
        }

        currentDialogue = dialogue;
        isDialogueActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        OnDialogueStart?.Invoke();
        DisplayNode(startNodeId);
    }

    /// <summary>
    /// 保存当前对话进度（设置下次对话从哪个节点开始）
    /// </summary>
    public void SaveDialogueProgress(int nodeId)
    {
        if (currentDialogue != null)
        {
            string dialogueKey = GetDialogueKey(currentDialogue);
            if (!string.IsNullOrEmpty(dialogueKey))
            {
                dialogueProgress[dialogueKey] = nodeId;
                Debug.Log($"[对话系统] 手动保存进度: {dialogueKey} -> 节点 {nodeId}");
            }
        }
    }

    /// <summary>
    /// 设置指定对话的起始节点（用于外部设置）
    /// </summary>
    public void SetDialogueStartNode(string dialogueId, int nodeId)
    {
        dialogueProgress[dialogueId] = nodeId;
        Debug.Log($"设置对话起始节点: {dialogueId} -> 节点 {nodeId}");
    }

    /// <summary>
    /// 清除指定对话的进度（重置为从头开始）
    /// </summary>
    public void ClearDialogueProgress(string dialogueId)
    {
        if (dialogueProgress.ContainsKey(dialogueId))
        {
            dialogueProgress.Remove(dialogueId);
            Debug.Log($"清除对话进度: {dialogueId}");
        }
    }

    /// <summary>
    /// 清除所有对话进度
    /// </summary>
    public void ClearAllDialogueProgress()
    {
        dialogueProgress.Clear();
        Debug.Log("清除所有对话进度");
    }

    /// <summary>
    /// 获取指定对话的当前进度节点
    /// </summary>
    public int GetDialogueProgress(string dialogueId)
    {
        return dialogueProgress.ContainsKey(dialogueId) ? dialogueProgress[dialogueId] : 0;
    }

    /// <summary>
    /// 显示对话节点
    /// </summary>
    private void DisplayNode(int nodeId)
    {
        currentNode = currentDialogue.GetNode(nodeId);

        if (currentNode == null)
        {
            Debug.LogWarning($"[对话系统] 找不到节点 {nodeId}，对话结束");
            EndDialogue();
            return;
        }

        // 获取对话的唯一标识
        string dialogueKey = GetDialogueKey(currentDialogue);

        // 如果这个节点是检查点，自动保存进度
        if (currentNode.isCheckpoint && !string.IsNullOrEmpty(dialogueKey))
        {
            int saveNodeId = currentNode.resumeNodeId >= 0 ? currentNode.resumeNodeId : currentNode.nodeId;
            dialogueProgress[dialogueKey] = saveNodeId;
            Debug.Log($"[对话系统] ★ 检查点保存！对话 '{dialogueKey}' 下次将从节点 {saveNodeId} 开始");
        }

        // ★ 进入节点时设置 Bool
        if (currentNode.onEnterSetBools != null)
        {
            foreach (var boolSetter in currentNode.onEnterSetBools)
            {
                if (!string.IsNullOrEmpty(boolSetter.boolName))
                {
                    SetBool(boolSetter.boolName, boolSetter.setValue);
                }
            }
        }

        // 触发进入节点事件
        currentNode.onNodeEnter?.Invoke();
        OnNodeChanged?.Invoke(currentNode);

        // 显示说话者信息
        if (speakerNameText != null)
            speakerNameText.text = currentNode.speakerName;

        if (speakerPortraitImage != null)
        {
            if (currentNode.speakerPortrait != null)
            {
                speakerPortraitImage.sprite = currentNode.speakerPortrait;
                speakerPortraitImage.gameObject.SetActive(true);
            }
            else
            {
                speakerPortraitImage.gameObject.SetActive(false);
            }
        }

        // 显示对话文本
        if (useTypewriterEffect)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(currentNode.dialogueText));
        }
        else
        {
            if (dialogueText != null)
                dialogueText.text = currentNode.dialogueText;
        }

        // 根据节点类型处理UI
        HandleNodeType();
    }

    /// <summary>
    /// 处理不同类型的节点
    /// </summary>
    private void HandleNodeType()
    {
        // 清除之前的选项
        ClearChoices();

        switch (currentNode.nodeType)
        {
            case DialogueNodeType.Normal:
                if (continueButton != null)
                    continueButton.gameObject.SetActive(true);
                break;

            case DialogueNodeType.Conditional:
                if (continueButton != null)
                    continueButton.gameObject.SetActive(true);
                break;

            case DialogueNodeType.Choice:
                if (continueButton != null)
                    continueButton.gameObject.SetActive(false);
                DisplayChoices();
                break;
        }
    }

    /// <summary>
    /// 显示选项
    /// </summary>
    private void DisplayChoices()
    {
        if (choicesContainer == null)
        {
            Debug.LogError("choicesContainer 为空！请在 Inspector 中设置");
            return;
        }
        if (choiceButtonPrefab == null)
        {
            Debug.LogError("choiceButtonPrefab 为空！请在 Inspector 中设置选项按钮预制体");
            return;
        }

        Debug.Log($"显示 {currentNode.choices.Count} 个选项");

        foreach (var choice in currentNode.choices)
        {
            // 检查选项条件
            if (choice.condition != null && !CheckCondition(choice.condition))
            {
                if (choice.hideIfConditionNotMet)
                    continue;
            }

            GameObject choiceObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            Button button = choiceObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = choiceObj.GetComponentInChildren<TextMeshProUGUI>();

            if (button == null)
            {
                Debug.LogError("选项按钮预制体上没有 Button 组件！");
                continue;
            }

            if (buttonText == null)
            {
                Debug.LogWarning("选项按钮预制体上没有找到 TextMeshProUGUI 组件");
            }
            else
            {
                buttonText.text = choice.choiceText;
            }

            // 检查条件是否满足
            bool conditionMet = choice.condition == null || CheckCondition(choice.condition);

            button.interactable = conditionMet;

            DialogueChoice capturedChoice = choice;
            button.onClick.AddListener(() =>
            {
                Debug.Log($"按钮被点击: {capturedChoice.choiceText}");
                OnChoiceSelected(capturedChoice);
            });

            Debug.Log($"创建选项按钮: {choice.choiceText}, 可交互: {conditionMet}");

            // 如果条件不满足，可以改变按钮外观
            if (!conditionMet)
            {
                // 可以在这里添加视觉反馈，如灰色文字
                if (buttonText != null)
                    buttonText.color = Color.gray;
            }
        }

        choicesContainer.SetActive(true);
    }

    /// <summary>
    /// 清除选项
    /// </summary>
    private void ClearChoices()
    {
        if (choicesContainer == null) return;

        foreach (Transform child in choicesContainer.transform)
        {
            Destroy(child.gameObject);
        }

        choicesContainer.SetActive(false);
    }

    /// <summary>
    /// 选项被选中
    /// </summary>
    private void OnChoiceSelected(DialogueChoice choice)
    {
        if (audioSource != null && choiceSound != null)
            audioSource.PlayOneShot(choiceSound);

        choice.onChoiceSelected?.Invoke();
        OnChoiceMade?.Invoke(choice);

        // ★ 离开节点时设置 Bool
        ApplyExitBools();

        currentNode.onNodeExit?.Invoke();
        DisplayNode(choice.nextNodeId);
    }

    /// <summary>
    /// 点击继续按钮
    /// </summary>
    private void OnContinueClicked()
    {
        if (isTyping)
        {
            SkipTyping();
            return;
        }

        if (currentNode == null) return;

        int nextId = -1;

        switch (currentNode.nodeType)
        {
            case DialogueNodeType.Normal:
                nextId = currentNode.nextNodeId;
                break;

            case DialogueNodeType.Conditional:
                if (CheckCondition(currentNode.condition))
                {
                    nextId = currentNode.conditionMetNodeId;
                }
                else
                {
                    // 显示条件不满足提示
                    ShowConditionHint(currentNode.condition.failMessage);
                    nextId = currentNode.conditionNotMetNodeId;
                }
                break;

            case DialogueNodeType.Choice:
                // 选择型对话需要选择选项才能继续
                return;
        }

        // ★ 离开节点时设置 Bool
        ApplyExitBools();

        currentNode.onNodeExit?.Invoke();

        if (nextId < 0)
        {
            EndDialogue();
        }
        else
        {
            DisplayNode(nextId);
        }
    }

    /// <summary>
    /// 应用离开节点时的 Bool 设置
    /// </summary>
    private void ApplyExitBools()
    {
        if (currentNode != null && currentNode.onExitSetBools != null)
        {
            foreach (var boolSetter in currentNode.onExitSetBools)
            {
                if (!string.IsNullOrEmpty(boolSetter.boolName))
                {
                    SetBool(boolSetter.boolName, boolSetter.setValue);
                }
            }
        }
    }

    /// <summary>
    /// 检查条件是否满足
    /// </summary>
    private bool CheckCondition(DialogueCondition condition)
    {
        // 如果条件为空，或者条件Key为空，视为条件满足
        if (condition == null) return true;
        if (string.IsNullOrEmpty(condition.conditionKey)) return true;

        switch (condition.conditionType)
        {
            case ConditionType.HasItem:
                if (GetItemCount != null)
                    return GetItemCount(condition.conditionKey) >= condition.requiredValue;
                else
                    Debug.LogWarning("[对话系统] GetItemCount 委托未设置！请确保 DialogueSystemExample 已初始化");
                break;

            case ConditionType.HasFlag:
                if (HasFlag != null)
                    return HasFlag(condition.conditionKey);
                else
                    Debug.LogWarning("[对话系统] HasFlag 委托未设置！");
                break;

            case ConditionType.StatCheck:
                if (GetStatValue != null)
                    return GetStatValue(condition.conditionKey) >= condition.requiredValue;
                else
                    Debug.LogWarning("[对话系统] GetStatValue 委托未设置！");
                break;
        }

        return false;
    }

    /// <summary>
    /// 显示条件提示
    /// </summary>
    private void ShowConditionHint(string message)
    {
        if (conditionHintPanel != null && conditionHintText != null)
        {
            conditionHintText.text = message;
            conditionHintPanel.SetActive(true);
            StartCoroutine(HideConditionHint(3f));
        }
    }

    private IEnumerator HideConditionHint(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (conditionHintPanel != null)
            conditionHintPanel.SetActive(false);
    }

    /// <summary>
    /// 打字机效果
    /// </summary>
    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;

            if (audioSource != null && typingSound != null)
                audioSource.PlayOneShot(typingSound);

            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    /// <summary>
    /// 跳过打字效果，立即显示完整文本
    /// </summary>
    private void SkipTyping()
    {
        if (!isTyping) return;  // 如果没有在打字，直接返回

        // 先设置标志，防止重复调用
        isTyping = false;

        // 停止打字协程
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // 立即显示完整文本
        if (dialogueText != null && currentNode != null)
        {
            dialogueText.text = currentNode.dialogueText;
        }

        Debug.Log("[对话系统] 跳过打字效果，显示完整文本");
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    public void EndDialogue()
    {
        // ★ 记录对话已完成
        if (currentDialogue != null)
        {
            string dialogueKey = GetDialogueKey(currentDialogue);
            if (!string.IsNullOrEmpty(dialogueKey))
            {
                dialogueCompleted[dialogueKey] = true;
                Debug.Log($"[对话系统] 对话完成: {dialogueKey}");
                OnDialogueCompleted?.Invoke(dialogueKey);
            }
        }

        isDialogueActive = false;
        currentDialogue = null;
        currentNode = null;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        ClearChoices();

        OnDialogueEnd?.Invoke();
    }

    /// <summary>
    /// 检查是否正在对话中
    /// </summary>
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    /// <summary>
    /// 跳转到指定节点
    /// </summary>
    public void JumpToNode(int nodeId)
    {
        if (!isDialogueActive) return;
        DisplayNode(nodeId);
    }

    // ================= 对话完成状态相关方法 =================

    /// <summary>
    /// 检查指定对话是否已完成
    /// </summary>
    /// <param name="dialogueId">对话ID（DialogueData 的 dialogueId 或资源名）</param>
    /// <returns>是否已完成</returns>
    public static bool IsDialogueCompleted(string dialogueId)
    {
        return dialogueCompleted.ContainsKey(dialogueId) && dialogueCompleted[dialogueId];
    }

    /// <summary>
    /// 检查指定对话数据是否已完成
    /// </summary>
    public bool IsDialogueCompleted(DialogueData dialogue)
    {
        if (dialogue == null) return false;
        string key = GetDialogueKey(dialogue);
        return IsDialogueCompleted(key);
    }

    /// <summary>
    /// 手动设置对话完成状态
    /// </summary>
    public static void SetDialogueCompleted(string dialogueId, bool completed = true)
    {
        dialogueCompleted[dialogueId] = completed;
        Debug.Log($"[对话系统] 设置对话状态: {dialogueId} = {completed}");
    }

    /// <summary>
    /// 重置指定对话的完成状态
    /// </summary>
    public static void ResetDialogueCompleted(string dialogueId)
    {
        if (dialogueCompleted.ContainsKey(dialogueId))
        {
            dialogueCompleted.Remove(dialogueId);
            Debug.Log($"[对话系统] 重置对话状态: {dialogueId}");
        }
    }

    /// <summary>
    /// 重置所有对话的完成状态
    /// </summary>
    public static void ResetAllDialogueCompleted()
    {
        dialogueCompleted.Clear();
        Debug.Log("[对话系统] 重置所有对话状态");
    }

    /// <summary>
    /// 获取所有已完成的对话ID列表
    /// </summary>
    public static List<string> GetCompletedDialogues()
    {
        List<string> completed = new List<string>();
        foreach (var pair in dialogueCompleted)
        {
            if (pair.Value)
                completed.Add(pair.Key);
        }
        return completed;
    }

    // ================= 全局 Bool 相关方法 =================

    /// <summary>
    /// 设置一个 Bool 值（静态方法，任何地方都能调用）
    /// </summary>
    public static void SetBool(string boolName, bool value)
    {
        globalBools[boolName] = value;
        Debug.Log($"[对话系统] 设置 Bool: {boolName} = {value}");
        OnBoolChanged?.Invoke(boolName, value);
    }

    /// <summary>
    /// 获取一个 Bool 值（静态方法，任何地方都能调用）
    /// </summary>
    public static bool GetBool(string boolName, bool defaultValue = false)
    {
        if (globalBools.ContainsKey(boolName))
            return globalBools[boolName];
        return defaultValue;
    }

    /// <summary>
    /// 检查某个 Bool 是否存在
    /// </summary>
    public static bool HasBool(string boolName)
    {
        return globalBools.ContainsKey(boolName);
    }

    /// <summary>
    /// 删除一个 Bool
    /// </summary>
    public static void RemoveBool(string boolName)
    {
        if (globalBools.ContainsKey(boolName))
        {
            globalBools.Remove(boolName);
            Debug.Log($"[对话系统] 删除 Bool: {boolName}");
        }
    }

    /// <summary>
    /// 清除所有 Bool
    /// </summary>
    public static void ClearAllBools()
    {
        globalBools.Clear();
        Debug.Log("[对话系统] 清除所有 Bool");
    }

    /// <summary>
    /// 获取所有 Bool（用于存档等）
    /// </summary>
    public static Dictionary<string, bool> GetAllBools()
    {
        return new Dictionary<string, bool>(globalBools);
    }
}
