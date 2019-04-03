using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrushTrigger : MonoBehaviour
{
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = transform.parent.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.name == "Player")
        {
            anim.SetBool("Crush", true);
        }
    }
}
