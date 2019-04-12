using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    PortalState ps;
    Animator portalanim;

    void Start ()
    {
        portalanim = GetComponent<Animator>();
        ps = GameObject.Find("WorldState").GetComponent<PortalState>();
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name == "Player")
        {
            portalanim.SetInteger("portalstate", 1);
            ps.addToPortals(gameObject);
            ps.DisplayUI();
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.name == "Player")
        {
            ps.HideUI();
        }
    }
}
