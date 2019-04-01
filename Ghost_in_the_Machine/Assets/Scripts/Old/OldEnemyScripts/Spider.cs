using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : Enemy
{
    //public GameObject acidEffectPrefab;
    //public float Health { get; set; }

    //public override void Init()
    //{
    //    base.Init();
    //    Health = base.health;
    //}

    //public override void Move()
    //{
    //    float distance = Vector3.Distance(transform.localPosition, player.transform.localPosition);
    //    if (distance > 6.0f)
    //    {
    //        isHit = false;
    //        _anim.SetBool("InCombat", false);
    //    }
    //    else
    //    {
    //        isHit = true;
    //        _anim.SetBool("InCombat", true);
    //    }

    //    FacePlayer();

    //    if (!isHit)
    //        _rigid.velocity = new Vector2(direction * movementSpeed * Time.deltaTime, _rigid.velocity.y);
    //}

    //public void Damage()
    //{
    //    if (isDead)
    //        return;

    //    Health -= 1;
    //    _anim.SetTrigger("Hit");
    //    isHit = true;
    //    _anim.SetBool("InCombat", true);

    //    if (Health <= 0)
    //    {
    //        isDead = true;
    //        _anim.SetTrigger("Death");

    //        GameObject droppedCoin = Instantiate(coin, transform.position, Quaternion.identity);
    //        droppedCoin.GetComponent<Coin>().coinAmount = coins;

    //        Destroy(gameObject, _anim.GetCurrentAnimatorStateInfo(0).length);
    //    }
    //}

    //public override void Attack()
    //{
    //    Instantiate(acidEffectPrefab, transform.position, Quaternion.identity);
    //}
}
