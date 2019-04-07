using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bounceScript : MonoBehaviour
{
    [SerializeField] private float bounceTime = 5f;
    [SerializeField] private bool isActive = true;
    private Rigidbody2D player;
    private BoxCollider2D collide;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        collide = GetComponent<BoxCollider2D>();
        player = GameObject.Find("player").GetComponent<Rigidbody2D>();
    }

    void Update ()
    {
        if(isActive == false)
        {
            bounceTime -= Time.deltaTime;
            if (bounceTime <= 0)
            {
                anim.SetBool("isOpen", true);
                isActive = true;
                collide.enabled = true;
                bounceTime = 5f;

            }
        }
    }

    void OnTriggerEnter2D (Collider2D col)
    {
        if (isActive == true && col.gameObject.tag == "player")
        {
            anim.SetBool("isOpen", false);
            player.velocity = (Vector3.up * 20);
            isActive = false;
            collide.enabled = false;
        }
    }
}
