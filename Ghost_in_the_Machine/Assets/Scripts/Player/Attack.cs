using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private Player playerScript;

    private void Start()
    {
        playerScript = transform.parent.GetComponent<Player>();
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.name == "Player")
            return;

        if(coll.gameObject.GetComponent<Enemy>() != null && coll.gameObject.GetComponent<Enemy>().canBeDamaged)
        {
            Damage dmg = new Damage
            {
                damageAmount = playerScript.damageValue,
                attackDirectionState = playerScript.playerAttackDirectionState,
                layer = LayerMask.LayerToName(gameObject.layer),
                stunningDuration = playerScript.stunDuration,
                damageElement = Element.Soul
            };

            coll.gameObject.GetComponent<Character>().TakeDamamge(dmg);

            if(playerScript.currentMana < playerScript.maxMana)
            {
                playerScript.currentMana++;
                UIManager.Instance.UpdateMana(playerScript.currentMana);
            }
        }
    }
}
