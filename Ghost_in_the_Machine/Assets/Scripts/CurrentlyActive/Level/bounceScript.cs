using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bounceScript : MonoBehaviour
{
    public float bounceForce = 2f;
    public float bounceTime = 10f;
    private bool isActive = true;
    private Rigidbody2D player;
    private BoxCollider2D[] collide;

    // Start is called before the first frame update
    void Start()
    {
        collide = GetComponentsInParent<BoxCollider2D>();
        player = GameObject.Find("player").GetComponent<Rigidbody2D>();
    }

    void Update ()
    {
        if(isActive == false)
        {
            bounceTime -= Time.deltaTime;
            if (bounceTime <= 0)
            {
                isActive = true;
                collide[0].enabled = true;
                collide[1].enabled = true;
                bounceTime = 10f;

            }
        }
    }

    void OnCollisionEnter2D (Collision2D col)
    {
        if (isActive == true && col.gameObject.tag == "player")
        {
            player.AddForce(Vector2.up * 20, ForceMode2D.Impulse);
            isActive = false;
            collide[0].enabled = false;
            collide[1].enabled = false;
        }
    }
}
