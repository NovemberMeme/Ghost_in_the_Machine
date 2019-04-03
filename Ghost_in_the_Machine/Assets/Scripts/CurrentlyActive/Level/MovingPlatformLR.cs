using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformLR : MonoBehaviour
{
    private float speed = 4f;
    public float threshold = 3f;
    public float maxLeft;
    public float maxRight;
    private int movDir;

    // Start is called before the first frame update
    void Start()
    {
        maxLeft = transform.position.x - threshold;
        maxRight = transform.position.x + threshold;

    }

    // Update is called once per frame
    void Update()
    {
        if (movDir == 1)
        {
            if (transform.position.x > maxLeft)
            {
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(maxLeft, transform.position.y), speed * Time.deltaTime);
            }
            else
            {
                movDir = 0;
            }

        }
        else if (movDir == 0)
        {
            if (transform.position.x < maxRight)
            {
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(maxRight, transform.position.y), speed * Time.deltaTime);
            }
            else
            {
                movDir = 1;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "player")
        {
            col.gameObject.transform.parent = this.transform;
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.tag == "player")
        {
            col.gameObject.transform.parent = null;
        }
    }
}
