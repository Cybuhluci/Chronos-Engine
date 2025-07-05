using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tardisanimations : MonoBehaviour
{
    public Animator anim;
    public bool doorlocked, rightopen, leftopen, phoneopen;
    public string interaction;

    // Start is called before the first frame update
    void Start()
    {
        anim = anim.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!doorlocked) // if unlocked and door closed
        {
            if (interaction == "left-door" && !leftopen) // unlocked and left door closed
            {
                anim.SetBool("leftdooropen", true);
            }
            else if (interaction == "right-door" && !rightopen) // unlocked and right door closed
            {
                anim.SetBool("rightdooropen", true);
            }

            if (interaction == "phone" && !rightopen && !leftopen) // unlocked and both doors closed
            {
                anim.SetBool("phonedooropen", true);
            }
        }
    }
}
