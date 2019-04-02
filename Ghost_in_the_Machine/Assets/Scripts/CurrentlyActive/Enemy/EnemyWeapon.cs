using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    [SerializeField] protected Enemy enemyScript;

    protected virtual void Start()
    {
        enemyScript = transform.parent.GetComponent<Enemy>();
    }

    protected virtual void Update()
    {

    }

    protected virtual void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.name == "Player")
        {
            Damage dmg = new Damage
            {
                damageAmount = enemyScript.DamageValue,
                verticalAttackDirection = enemyScript.CurrentVerticalAttackDirectionState,
                horizontalAttackDirection = enemyScript.CurrentHorizontalAttackDirectionState,
                layer = LayerMask.LayerToName(gameObject.layer),
                stunningDuration = enemyScript.StunDuration,
                damageElement = Element.Soul
            };

            coll.gameObject.GetComponent<Player>().GetHit(dmg);
        }
    }
}
