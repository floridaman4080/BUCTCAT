using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class WindOn : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("对话设置")]
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private int startNodeId = 0;
    public PlayableDirector playableDirector;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (DialogueSystem.GetBool("isWindOn"))
        {
            Debug.Log("WindOn");
            DialogueSystem.SetBool("isWindOn", false);
            playableDirector.Play();
        }
    }
    public void resumeDialogue()
    {
        DialogueSystem.Instance.StartDialogue(dialogue, startNodeId);
    }

}
