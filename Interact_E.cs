using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Interact_E : MonoBehaviour
{
    public GameObject interactE;
    private bool isPressE = false;
    public bool isPress = false;
    public static Interact_E instance;

    private void Awake()
    {
        // 每个场景的触发器都是独立的，不需要跨场景保留
        instance = this;
    }

    private void OnDestroy()
    {
        // 当对象被销毁时，清除静态引用
        if (instance == this)
        {
            instance = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        interactE.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // if (对话系统.instance.isStart && !isPressE)
        // {
        //     interactE.SetActive(true);
        // }
        if (isPress || Input.GetKeyDown(KeyCode.E))
        {
            isPressE = true;
            interactE.SetActive(false);
            isPress = false;
            Debug.Log("按下按钮");
        }
    }
    public void Onpress()
    {
        isPress = true;

    }
}
