using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcidEffect : MonoBehaviour
{
    private GameObject _player;
    private Vector3 _playerPos;

    private void Start()
    {
        Destroy(gameObject, 5.0f);
        _player = GameObject.Find("Player");
        _playerPos = _player.transform.position;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _playerPos, 3.0f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.tag == "Player")
        {
            Destroy(gameObject);
        }
    }
}
