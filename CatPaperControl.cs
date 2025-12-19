using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatPaperControl : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Animator anim;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (DialogueSystem.GetBool("haspaperfalse"))
        {
            anim.SetBool("haspaper", false);
            Debug.Log("haspaperfalse");

        }
        // if (anim.GetBool("haspaper") && anim.GetBool("Isjump"))
        // {
        //     anim.SetFloat("isjumppaper", 0);
        // }

    }
}
