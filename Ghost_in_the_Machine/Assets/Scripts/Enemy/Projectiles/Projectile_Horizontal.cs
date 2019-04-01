using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Horizontal : EnemyWeapon
{
    public float shootDirection;

    protected float projectileResetTimer;

    [SerializeField] private float projectileCurrentSpeed = 0;

    public float ProjectileCurrentSpeed
    {
        set { projectileCurrentSpeed = value; }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        enemyScript = transform.parent.GetComponent<Enemy>();

        projectileResetTimer = 0;
        projectileCurrentSpeed = 0;

        gameObject.SetActive(false);

        transform.parent = null;
    }

    // Update is called once per frame
    protected override void Update()
    {
        transform.Translate(new Vector3(projectileCurrentSpeed * Time.deltaTime * shootDirection, 0, 0));

        if (shootDirection < 0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }

        if (gameObject.activeSelf)
        {
            projectileResetTimer += Time.deltaTime;   
        }
        
        if (projectileResetTimer >= enemyScript.ProjectileResetDelay)
        {
            ResetProjectile();
        }
    }

    protected override void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.name == "Player")
        {
            Damage dmg = new Damage
            {
                damageAmount = enemyScript.ProjectileDamageValue,
                attackDirectionState = enemyScript.CurrentAttackDirectionState,
                layer = LayerMask.LayerToName(gameObject.layer),
                stunningDuration = enemyScript.StunDuration,
                damageElement = Element.Soul
            };

            Debug.Log(dmg.damageAmount);

            coll.gameObject.GetComponent<Player>().GetHit(dmg);

            ResetProjectile();
        }
    }

    protected void ResetProjectile()
    {
        projectileResetTimer = 0;
        projectileCurrentSpeed = 0;
        gameObject.SetActive(false);
        transform.position = enemyScript.ProjectilePos.position;
    }
}
