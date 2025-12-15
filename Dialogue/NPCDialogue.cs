using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class NPCDialogue : MonoBehaviour
{
    //仅单次对话，对话结束则不可再次交互
    // Start is called before the first frame update
    [Header("对话设置")]
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private int startNodeId = 0;

    [Header("交互设置")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactHint; // "按E对话" 提示UI
    private bool playerInRange = false;

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
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (DialogueSystem.Instance != null && !DialogueSystem.Instance.IsDialogueActive() && !DialogueSystem.Instance.IsDialogueCompleted(dialogue))
            {
                DialogueSystem.Instance.StartDialogue(dialogue, startNodeId);
            }
            interactHint.SetActive(false);
        }
        if (playableDirector != null && playableDirector.state == PlayState.Playing)
        {

        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactHint != null && !DialogueSystem.Instance.IsDialogueCompleted(dialogue))
                interactHint.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactHint != null && DialogueSystem.Instance.IsDialogueCompleted(dialogue))
                interactHint.SetActive(false);
        }
    }

    // 2D版本
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactHint != null && !DialogueSystem.Instance.IsDialogueCompleted(dialogue))
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
}

