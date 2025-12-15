using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class NPCDialogue01 : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("对话设置")]
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private int startNodeId = 0;

    [Header("交互设置")]
    // [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactHint; // "偷听" 提示UI
    [SerializeField] private GameObject target;
    private bool playerInRange = false;
    private bool isListen;
    private bool isResume = false;
    [Header("Timeline 设置")]
    [Tooltip("拖入场景中的 PlayableDirector（Timeline播放器）")]
    [SerializeField] private PlayableDirector playableDirector;

    void Start()
    {
        if (interactHint != null)
            interactHint.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //如果玩家在范围内 按下了按钮 并且是在timeline之后
        if (playerInRange && isListen && isResume)
        {
            if (DialogueSystem.Instance != null && !DialogueSystem.Instance.IsDialogueActive() && !DialogueSystem.GetBool("找回纸"))
            {
                isListen = false;
                DialogueSystem.Instance.StartDialogue(dialogue, startNodeId);
            }
        }
        //对话中或者过场动画中不显示交互ui
        if ((playableDirector != null && playableDirector.state == PlayState.Playing) || DialogueSystem.Instance.IsDialogueActive())
        {
            interactHint.SetActive(false);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            //只有碰到并且没有完成对话并且对话没有进行才会显示ui
            if (interactHint != null && !DialogueSystem.GetBool("找回纸") && isResume && !DialogueSystem.Instance.IsDialogueActive())
            {
                interactHint.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactHint != null)
                interactHint.SetActive(false);
        }
    }

    //判断是否完成timeline
    public void resumeDialogue01()
    {
        isResume = true;
    }

    /// <summary>
    /// 关闭指定物体的 BoxCollider2D
    /// </summary>
    /// <param name="target">要关闭碰撞体的物体</param>
    public void DisableBoxCollider2D()
    {
        if (target != null)
        {
            BoxCollider2D collider = target.GetComponent<BoxCollider2D>();
            collider.enabled = false;
            collider.enabled = true;
            // Debug.Log($"已关闭 {target.name} 的 BoxCollider2D");
        }
    }

    //交互偷听
    public void eavesdrop01()
    {
        isListen = true;
    }
}
