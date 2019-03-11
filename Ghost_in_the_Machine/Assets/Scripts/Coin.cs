using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinAmount;

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.tag == "Player")
        {
            Player player = coll.GetComponent<Player>();

            if (player != null)
            {
                player.AddCoins(coinAmount);
                Destroy(gameObject);
            }
        }
    }
}
