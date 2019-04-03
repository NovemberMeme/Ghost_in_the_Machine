using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    PortalState ps;

    void Start ()
    {
        ps = GameObject.Find("WorldState").GetComponent<PortalState>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "player")
        {
            ps.addToPortals(gameObject);
            ps.DisplayUI();
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.tag == "player")
        {
            ps.HideUI();
        }
    }
}
