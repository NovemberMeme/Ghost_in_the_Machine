﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    private Enemy enemyScript;

    void Start()
    {
        enemyScript = transform.parent.GetComponent<Enemy>();
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (enemyScript.canDamage && coll.name == "Player")
        {
            Damage dmg = new Damage
            {
                damageAmount = enemyScript.damageValue,
                attackDirectionState = enemyScript.currentAttackDirectionState,
                layer = LayerMask.LayerToName(gameObject.layer),
                stunningDuration = 0
            };

            coll.gameObject.GetComponent<Player>().Damage(dmg);
            enemyScript.canDamage = false;
        }
    }
}
