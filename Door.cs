using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private bool isEntered = false;
    [SerializeField] private GameObject interactE;
    public bool isfubeng1;
    public static Door instance;
    private void Awake()
    {
        // 每个场景的门都是独立的，不需要跨场景保留
        instance = this;
    }

    private void OnDestroy()
    {
        // 当门被销毁时，清除静态引用
        if (instance == this)
        {
            instance = null;
        }
    }
    void Start()
    {
        interactE.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isEntered && interactE != null)
        {
            interactE.SetActive(true);
            if (Input.GetKeyDown(KeyCode.E))
            {
                isfubeng1 = true;
            }
        }
        else
        {
            interactE.SetActive(false);
        }
    }
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

}
