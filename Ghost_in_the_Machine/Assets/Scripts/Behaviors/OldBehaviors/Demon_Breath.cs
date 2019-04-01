using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demon_Breath : MonoBehaviour
{
    Enemy enemy;
    Animator anim;

    public void Start()
    {
        enemy = transform.parent.GetComponent<Enemy>();
        anim = transform.GetChild(1).GetComponent<Animator>();
    }

    public void DemonBreath()
    {
        anim.SetTrigger("BreathFire");
    }

    //--------------------------- Dashing -----------------------------------------

    public void Dash()
    {
        enemy.Dash();
    }

    //--------------------------- Projectiles -------------------------------------

    public void Fireball()
    {
        enemy.Projectile_Straight();
    }

    public void Fireball_Horizontal()
    {
        enemy.Projectile_Horizontal();
    }

    public void Fireball_Right()
    {
        enemy.Projectile_Right();
    }

    public void Fireball_Left()
    {
        enemy.Projectile_Left();
    }

    public void Fireball_Left_Right()
    {
        enemy.Projectile_Left_Right();
    }
}
