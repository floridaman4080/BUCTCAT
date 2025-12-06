using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSwitch : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject present, past;
    [SerializeField] bool isPresentVisible = true;
    void Start()
    {
        present.SetActive(isPresentVisible);
        past.SetActive(!isPresentVisible);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            present.SetActive(!present.activeSelf);
            past.SetActive(!past.activeSelf);
        }
    }
}
