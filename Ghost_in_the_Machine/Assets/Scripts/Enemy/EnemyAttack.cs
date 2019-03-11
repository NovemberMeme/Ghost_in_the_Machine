using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private bool _canDamage = true;
    private Character charScript;

    private void Start()
    {
        charScript = transform.parent.parent.GetComponent<Character>();
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (_canDamage && coll.name == "Player")
        {
            coll.gameObject.GetComponent<Player>().Damage();
            _canDamage = false;
            StartCoroutine(ResetDamage());
        }
    }

    private IEnumerator ResetDamage()
    {
        yield return new WaitForSeconds(charScript.attackHitBoxCooldown);
        _canDamage = true;
    }
}
