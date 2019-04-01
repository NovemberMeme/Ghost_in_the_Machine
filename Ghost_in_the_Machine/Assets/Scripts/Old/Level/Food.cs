using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    TimeCycle TC;

    public void Start()
    {
        TC = FindObjectOfType<TimeCycle>();
    }

    public void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.name == "Player")
        {
            TC.foodCollected += 1;
            Destroy(gameObject);
        }
    }
}
