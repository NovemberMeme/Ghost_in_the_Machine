using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleTest : MonoBehaviour
{
    private float movSpeed = 1.25f;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 movement = new Vector2(-1 * movSpeed, 0);
        movement.y = rb.velocity.y;

        rb.velocity = movement;
        
    }
}
