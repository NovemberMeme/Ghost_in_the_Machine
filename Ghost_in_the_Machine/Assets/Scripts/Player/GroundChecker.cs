using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    private PlayerAnimation _playerAnim;
    private Player _player;

    // Start is called before the first frame update
    void Start()
    {
        _playerAnim = transform.parent.GetComponent<PlayerAnimation>();
        _player = transform.parent.GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            transform.parent.GetComponent<Player>().isGrounded = true;
            _playerAnim.Jump(false);
            _player.canDoubleJump = false;
            _player.canDash = true;
            //Debug.Log("Ground Enter");
        }
    }

    private void OnTriggerStay2D(Collider2D coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            transform.parent.GetComponent<Player>().isGrounded = true;
            //if(_playerAnim.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Jumping"))
            _playerAnim.Jump(false);
            _player.canDoubleJump = false;
            _player.canDash = true;
            //Debug.Log("Ground Enter");
        }
    }

    private void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            transform.parent.GetComponent<Player>().isGrounded = false;
            _playerAnim.Jump(true);
            _player.canDoubleJump = true;
            //Debug.Log("Ground Exit");
        }
    }
}
