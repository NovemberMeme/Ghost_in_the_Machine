using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeBase : MonoBehaviour
{
    public TimeCycle TC;
    public bool spawnCond = false;

    void Update()
    {
        if (TC.foodCollected >= 3)
        {
            spawnCond = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (TC.foodCollected == 3 && other.gameObject.tag == "Player" && spawnCond)
        {
            TC.time = 180;
            TC.foodCollected = 0;
            spawnCond = false;
        }
    }
}
