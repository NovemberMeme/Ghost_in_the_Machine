using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformUD : MonoBehaviour
{
    private float speed = 4f;
    public float threshold = 3f;
    public float maxDown;
    public float maxUp;
    private int movDir;

    // Start is called before the first frame update
    void Start()
    {
        maxDown = transform.position.y - threshold;
        maxUp = transform.position.y + threshold;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (movDir == 1)
        {
            if (transform.position.y > maxDown)
            {
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, maxDown), speed * Time.deltaTime);
            } else
            {
                movDir = 0;
            }
            
        }
        else if (movDir == 0)
        {
            if (transform.position.y < maxUp)
            {
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, maxUp), speed * Time.deltaTime);
            }
            else
            {
                movDir = 1;
            }
        }
    }

    void OnCollisionEnter2D (Collision2D col)
    {
        if (col.gameObject.tag == "player")
        {
            col.gameObject.transform.parent = this.transform;
        } 
    }

    void OnCollisionExit2D (Collision2D col)
    {
        if (col.gameObject.tag == "player")
        {
            col.gameObject.transform.parent = null;
        }
    }
}
