using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spikeScript : MonoBehaviour
{
    private GameObject player;
    private PlayerLevelHandler playerLevelHandler;
    private Player playerScript;

    private bool canDamage = true;

    void Start ()
    {
        player = GameObject.Find("Player");
        playerLevelHandler = player.GetComponent<PlayerLevelHandler>();
        playerScript = player.GetComponent<Player>();
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        //if (coll.gameObject.name == "Player")
        //{
        //    playercs.SetHp(-1);
        //    player.transform.position = playercs.GetCheck();
        //    Debug.Log("Spiked");
        //}
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.name == "Player" && canDamage)
        {
            //playercs.SetHp(-1);
            Damage dmg = new Damage
            {
                damageAmount = 1,
                verticalAttackDirection = VerticalAttackDirection.AttackingForward,
                horizontalAttackDirection = HorizontalAttackDirection.AttackingRightward,
                layer = LayerMask.LayerToName(gameObject.layer),
                stunningDuration = 0,
                damageElement = Element.Soul
            };

            playerScript.TakeDamage(dmg);

            player.transform.position = playerLevelHandler.GetCheck();
            Debug.Log("Spiked");
            StartCoroutine(SpikeDamageCooldown());
        }
    }

    public IEnumerator SpikeDamageCooldown()
    {
        canDamage = false;

        yield return new WaitForSeconds(1);

        canDamage = true;
    }
}
