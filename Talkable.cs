using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Talkable : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private bool isEntered = false;
    [TextArea(1, 3)]
    public string[] lines;
    private bool isStartDialogue = false;
    [SerializeField] private bool hasName = false;
    private void OnTriggerEnter2D(Collider2D other0)
    {
        if (other0.CompareTag("Player"))
        {
            isEntered = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other0)
    {
        if (other0.CompareTag("Player"))
        {
            isEntered = false;
        }
    }
    void Start()
    {

    }
    void Update()
    {
        if (对话系统.instance.isStart == true && Input.GetKeyDown(KeyCode.E) && !isStartDialogue)
        {
            Print(lines);
            Debug.Log("start");
            isStartDialogue = true;
        }
    }
    public void Print(string[] lines)
    {
        对话系统.instance.ShowDialogue(lines, hasName);
    }
}
