using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEffects : MonoBehaviour
{
    //private float movspeed = 8f;
    //private int turnState = 0;
    //Rigidbody2D rb;
    Vector2 origBloodfxScale;
    Vector2 origShieldfxScale;
    
    //EFFECTS
    private GameObject bloodfx;
    private GameObject shieldfx;

    void Start()
    {
        //GET GAMEOBJECTS FROM RESOURCES FOLDER
        shieldfx = Resources.Load<GameObject>("efx_shield");
        bloodfx = Resources.Load<GameObject>("efx_blood");
        origShieldfxScale = shieldfx.transform.localScale;
        origBloodfxScale = bloodfx.transform.localScale;

        //rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //float xmov = Input.GetAxis("Horizontal");

        ////PLAYER TURNING TURNS EFX SPRITE
        //if (xmov < 0)
        //{
        //    if (turnState == 0)
        //    {
        //        //efx turn left
        //        bloodfx.GetComponent<SpriteRenderer>().flipX = true;
        //        shieldfx.GetComponent<SpriteRenderer>().flipX = true;

        //        transform.Rotate(new Vector3(0, 180, 0));
        //        turnState = 1;
        //    }
        //} else if (xmov > 0)
        //{
        //    if (turnState == 1)
        //    {
        //        //efx turn right
        //        bloodfx.GetComponent<SpriteRenderer>().flipX = false;
        //        shieldfx.GetComponent<SpriteRenderer>().flipX = false;

        //        transform.Rotate(new Vector3(0, -180, 0));
        //        turnState = 0;
        //    }
        //}

        //Vector2 movement = new Vector2(xmov * movspeed, 0);
        //rb.velocity = movement;
        
    }

    //CHECK WHICH COLLIDER HIT
    void OnTriggerEnter2D (Collider2D col)
    {
        ////FOR SPARK
        //if (col.gameObject.tag == "shield")
        //{
        //    GameObject spawnFx = Instantiate(shieldfx, col.gameObject.transform.position, Quaternion.identity);
        //    Destroy(spawnFx, 0.35f);
        //}
        ////BLOOD SPLATTER
        //else if (col.gameObject.tag == "body")
        //{
        //    GameObject spawnFx = Instantiate(bloodfx, col.gameObject.transform.position, Quaternion.identity);
        //    Destroy(spawnFx, 0.35f);
        //}
    }

    public void Spark()
    {
        if(transform.parent.localScale.x > 0)
        {
            GameObject spawnFx = Instantiate(shieldfx, new Vector2(transform.position.x + 1.5f, transform.position.y), Quaternion.identity);
            spawnFx.transform.localScale = new Vector2(-origShieldfxScale.x, origShieldfxScale.y);
            Destroy(spawnFx, 0.35f);
            Debug.Log("Right");
        }
        else if (transform.parent.localScale.x < 0)
        {
            GameObject spawnFx = Instantiate(shieldfx, new Vector2(transform.position.x - 1.5f, transform.position.y), Quaternion.identity);
            spawnFx.transform.localScale = new Vector2(origShieldfxScale.x, origShieldfxScale.y);
            Destroy(spawnFx, 0.35f);
            Debug.Log("Left");
        }
    }

    public void Splatter()
    {
        if (transform.parent.localScale.x > 0)
        {
            GameObject spawnFx = Instantiate(bloodfx, new Vector2(transform.position.x - 1, transform.position.y), Quaternion.identity);
            spawnFx.transform.localScale = new Vector2(-origBloodfxScale.x, origBloodfxScale.y);
            Destroy(spawnFx, 0.35f);
            Debug.Log("Right");
        }
        else if(transform.parent.localScale.x < 0)
        {
            GameObject spawnFx = Instantiate(bloodfx, new Vector2(transform.position.x + 1, transform.position.y), Quaternion.identity);
            spawnFx.transform.localScale = new Vector2(origBloodfxScale.x, origBloodfxScale.y);
            Destroy(spawnFx, 0.35f);
            Debug.Log("Left");
        }
        
    }
}
