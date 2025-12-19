using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
/// 对话条件类型
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
