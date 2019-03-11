using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private bool _canDamage = true;
    private Character charScript;

    private void Start()
    {
        charScript = transform.parent.parent.GetComponent<Character>();
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.name == "Player")
            return;

        if (_canDamage)
        {
            if(coll.gameObject.GetComponent<Character>() != null)
            {
                coll.gameObject.GetComponent<Character>().Damage();
                _canDamage = false;
                StartCoroutine(ResetDamage());
            }
        }
    }

    private IEnumerator ResetDamage()
    {
        yield return new WaitForSeconds(charScript.attackHitBoxCooldown);
        _canDamage = true;
    }
}
