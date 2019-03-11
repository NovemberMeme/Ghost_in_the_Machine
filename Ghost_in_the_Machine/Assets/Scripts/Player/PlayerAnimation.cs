using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public SoundManager soundManager;

    Animator _anim;
    //Animator _swordAnimation;

    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
        //_swordAnimation = transform.GetChild(2).GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(float move)
    {
        _anim.SetFloat("Move", Mathf.Abs(move));
    }

    public void Jump(bool jumping)
    {
        _anim.SetBool("Jumping", jumping);
    }

    public void Attack()
    {
        _anim.SetTrigger("Attack");
        //_swordAnimation.SetTrigger("SwordAnimation");
    }

    public void Phase()
    {
        _anim.SetTrigger("Phase");
    }

    public void StartDash()
    {
        _anim.SetLayerWeight(1, 1);
    }

    public void StopDash()
    {
        _anim.SetLayerWeight(1, 0);
    }

    public void StartJumpDash()
    {
        _anim.SetLayerWeight(2, 1);
    }

    public void StopJumpDash()
    {
        _anim.SetLayerWeight(2, 0);
    }

    public void GetHit()
    {
        soundManager.PlaySound("hit");
        _anim.SetTrigger("Hit");
    }

    public void Death()
    {
        soundManager.PlaySound("attack");
        _anim.SetTrigger("Death");
    }
}
