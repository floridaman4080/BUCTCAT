using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("对话设置")]
    public DialogueData dialogue;
    public int startNodeId = 0;

    [Header("交互设置")]
    public KeyCode interactKey = KeyCode.E;
    public GameObject interactHint; // "按E对话" 提示UI

    private bool playerInRange = false;

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
            if (DialogueSystem.Instance != null && !DialogueSystem.Instance.IsDialogueActive())
            {
                DialogueSystem.Instance.StartDialogue(dialogue, startNodeId);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactHint != null)
                interactHint.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactHint != null)
                interactHint.SetActive(false);
        }
    }

    // 2D版本
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactHint != null)
                interactHint.SetActive(true);
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

