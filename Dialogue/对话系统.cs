using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class 对话系统 : MonoBehaviour
{
    public static 对话系统 instance;
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText, nameText;

    [TextArea(1, 3)]
    public string[] dialogueLines;
    public int currentLine;
    [SerializeField] private float textTime = 0.1f;
    public bool isStart;
    private bool isScrolling;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        // 一开始隐藏对话框
        dialogueBox.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (dialogueBox == null) return;

        if (dialogueBox.activeInHierarchy && isStart && Input.GetMouseButtonUp(0) && !isScrolling)
        {
            currentLine++;
            if (currentLine < dialogueLines.Length)
            {
                CheckName();
                StartCoroutine(ScrollingText());
                // dialogueText.text = dialogueLines[currentLine];
            }
            else
            {
                dialogueBox.SetActive(false);
                move.instance.继续运动();
            }

        }
    }
    public void ShowDialogue(string[] _newLiens, bool _hasname)
    {
        dialogueLines = _newLiens;
        currentLine = 0;
        CheckName();
        nameText.gameObject.SetActive(_hasname);
        StartCoroutine(ScrollingText());
        // dialogueText.text = dialogueLines[currentLine];
        dialogueBox.SetActive(true);
    }
    public void gameStart()
    {
        StartCoroutine(_gameStart());

    }
    public IEnumerator _gameStart()
    {
        yield return new WaitForSeconds(5);
        isStart = true;
        currentLine = 0;
        dialogueText.text = dialogueLines[currentLine];
        // dialogueBox.SetActive(true);
    }
    // private void gameContinue()
    // {
    //     StartCoroutine(_gameContinue());
    // }
    // public IEnumerator _gameContinue()
    // {
    //     yield return new WaitForSeconds(1);
    //     move.instance.继续运动();
    // }
    private void CheckName()
    {
        if (dialogueLines[currentLine].StartsWith("n-"))
        {
            nameText.text = dialogueLines[currentLine].Replace("n-", "");
            currentLine++;
        }
    }
    private IEnumerator ScrollingText()
    {
        isScrolling = true;
        dialogueText.text = "";
        foreach (char letter in dialogueLines[currentLine].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textTime);
        }
        isScrolling = false;

    }
}
