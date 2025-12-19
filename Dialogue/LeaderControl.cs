using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderControl : MonoBehaviour

{
    // Start is called before the first frame update
    [SerializeField] private Animator anim;
    // [SerializeField] private DialogueData dialogue;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (DialogueSystem.GetBool("leadertalkwithpaper"))
        {
            anim.SetBool("leadertalkwithpaper", true);
        }
        else if (!DialogueSystem.GetBool("leadertalkwithpaper"))
        {
            anim.SetBool("leadertalkwithpaper", false);
        }
        else if (DialogueSystem.GetBool("leadertalkwithoutpaper"))
        {
            anim.SetBool("leadertalkwithoutpaper", true);
        }
        else if (!DialogueSystem.GetBool("leadertalkwithoutpaper"))
        {
            anim.SetBool("leadertalkwithoutpaper", false);
        }
        else if (DialogueSystem.GetBool("leaderwithoutpaper"))
        {
            anim.SetBool("leaderwithoutpaper", true);
        }
        else if (!DialogueSystem.GetBool("leaderwithoutpaper"))
        {
            anim.SetBool("leaderwithoutpaper", false);
        }
        else if (!DialogueSystem.GetBool("leaderscale"))
        {
            anim.SetBool("leaderscale", false);
        }
    }
    public void leaderwithoutpapertrue()
    {
        anim.SetBool("leaderwithoutpaper", true);
    }
    public void leaderwithoutpaperfalse()
    {
        anim.SetBool("leaderwithoutpaper", false);
    }
    public void leaderscale()
    {
        anim.SetBool("leaderscale", true);
    }
}
