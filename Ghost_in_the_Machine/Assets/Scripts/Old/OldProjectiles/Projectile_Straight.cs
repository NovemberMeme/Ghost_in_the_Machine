using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Straight : MonoBehaviour
{
    private GameObject _player;
    private Vector3 _playerPos;

    private void Start()
    {
        Destroy(gameObject, 5.0f);
        _player = GameObject.Find("Player");
        _playerPos = _player.transform.position;

        float shootDirection = _playerPos.x - transform.position.x;

        if(shootDirection > 0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _playerPos, 3.0f * Time.deltaTime);

        if (transform.position == _playerPos)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.tag == "Player")
        {
            //coll.gameObject.GetComponent<Player>().Damage();
            Destroy(gameObject);
        }
    }
}
