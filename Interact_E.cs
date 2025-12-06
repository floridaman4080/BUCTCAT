using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Interact_E : MonoBehaviour
{
    public GameObject interactE;
    private bool isPressE = false;
    // Start is called before the first frame update
    void Start()
    {
        interactE.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (对话系统.instance.isStart && !isPressE)
        {
            interactE.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            isPressE = true;
            interactE.SetActive(false);
        }
    }
}
