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

        if(coll.gameObject.GetComponent<Enemy>() != null)
        {
            Damage dmg = new Damage
            {
                damageAmount = playerScript.DamageValue,
                attackDirectionState = playerScript.PlayerAttackDirectionState,
                layer = LayerMask.LayerToName(gameObject.layer),
                stunningDuration = playerScript.StunDuration,
                damageElement = Element.Soul
            };

            coll.gameObject.GetComponent<Character>().GetHit(dmg);

            if(playerScript.CurrentMana < playerScript.MaxMana)
            {
                playerScript.CurrentMana++;
                UIManager.Instance.UpdateMana(playerScript.CurrentMana);
            }
        }
    }
}
