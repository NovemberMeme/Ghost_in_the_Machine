using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Right : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 5.0f);

        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(5 * Time.deltaTime, 0, 0));
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
