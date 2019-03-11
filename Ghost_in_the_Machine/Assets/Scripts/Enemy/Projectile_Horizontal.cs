using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Horizontal : MonoBehaviour
{
    public float shootDirection;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 5.0f);

        if (shootDirection < 0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(-5 * Time.deltaTime * shootDirection, 0, 0));
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.tag == "Player")
        {
            coll.gameObject.GetComponent<Player>().Damage();
            Destroy(gameObject);
        }
    }
}
