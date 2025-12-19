using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerControl : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Animator anim;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (DialogueSystem.GetBool("workertalk"))
        {
            anim.SetBool("workertalk", true);
        }
        else if (!DialogueSystem.GetBool("workertalk"))
        {
            anim.SetBool("workertalk", false);
        }
    }
}
