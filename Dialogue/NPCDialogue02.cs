using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class NPCDialogue02 : MonoBehaviour
{

    //仅单次对话，对话结束则不可再次交互
    // Start is called before the first frame update
    [Header("对话设置")]
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private int startNodeId = 0;

    [Header("交互设置")]
    // [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactHint; // "按E对话" 提示UI
    // private bool playerInRange = false;
    private bool isListen;

    [Header("Timeline 设置")]
    [Tooltip("拖入场景中的 PlayableDirector（Timeline播放器）")]
    [SerializeField] private PlayableDirector playableDirector;
    public bool isStart;
    void Start()
    {
        if (interactHint != null)
            interactHint.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (isListen)
        {
            if (DialogueSystem.Instance != null && !DialogueSystem.Instance.IsDialogueActive() && !DialogueSystem.Instance.IsDialogueCompleted(dialogue))
            {
                DialogueSystem.Instance.StartDialogue(dialogue, startNodeId);
            }
            // interactHint.SetActive(false);
        }
        if (DialogueSystem.Instance.IsDialogueCompleted(dialogue))
        {
            move.instance.继续运动();
        }
        // if ((playableDirector != null && playableDirector.state == PlayState.Playing) || DialogueSystem.Instance.IsDialogueActive())
        // {
        //     interactHint.SetActive(false);
        // }

    }

    public void eavesdrop02()
    {
        isListen = true;
        interactHint.SetActive(false);

        // isListen = false;
    }
    public void gameStart()
    {
        StartCoroutine(_gameStart());

    }
    public IEnumerator _gameStart()
    {
        yield return new WaitForSeconds(9.5f);
        isStart = true;
        interactHint.SetActive(true);
        // dialogueBox.SetActive(true);
    }
}
