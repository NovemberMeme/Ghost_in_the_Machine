using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float min = 2f;
    public float max = 3f;

    void Start()
    {
        min = transform.position.x;
        max = transform.position.x + 3;
    }

    void Update()
    {
        transform.position = new Vector2(Mathf.PingPong(Time.time * 2, max - min) + min, transform.position.y);
    }
}
