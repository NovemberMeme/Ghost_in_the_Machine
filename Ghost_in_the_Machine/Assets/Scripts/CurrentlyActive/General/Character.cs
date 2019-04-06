using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public enum VerticalAttackDirection
{
    AttackingForward,
    AttackingUpward,
    AttackingDownward
}

public enum HorizontalAttackDirection
{
    AttackingLeftward,
    AttackingRightward
}

public enum Element
{
    Soul,
    Spirit,
    Psionic
}

public class Character : MonoBehaviour
{
    [Header("Health stats: ")]
    [SerializeField] protected int health = 4;

    public int Health
    {
        get
        {
            return health;
        }
        set
        {
            health = value;
        }
    }

    [Header("Combat stats: ")]
    [SerializeField] protected int leftParryValue = 0;
    [SerializeField] protected int leftBlockValue = 0;
    [SerializeField] protected int leftDamageValue = 0;

    [SerializeField] protected int rightParryValue = 0;
    [SerializeField] protected int rightBlockValue = 0;
    [SerializeField] protected int rightDamageValue = 0;

    [SerializeField] protected int parryValue = 0;
    [SerializeField] protected int blockValue = 0;
    [SerializeField] protected int damageValue = 0;

    [SerializeField] protected int parryStat = 1;
    [SerializeField] protected int blockStat = 1;
    [SerializeField] protected int damageStat = 1;

    public int ParryValue
    {
        get { return parryValue; }
    }

    public int DamageValue
    {
        get { return damageValue; }
    }

    [SerializeField] protected int actualDamage = 0;

    //public bool canDamage = true;
    //public bool canDamageSetter = true;
    //public bool canAttack = true;
    //public float attackCooldown = 0.5f;
    //public float attackHitBoxCooldown = 0.5f;

    [Header("Stun stats: ")]
    [SerializeField] protected float stunDuration = 0;

    public float StunDuration
    {
        get { return stunDuration; }
    }

    [SerializeField] protected float swordStunDuration = 2.0f;
    [SerializeField] protected float shieldStunDuration = 0.6f;

    [Header("Invulnerability stats: ")]
    [SerializeField] protected bool canBeDamaged = true;
    [SerializeField] protected float canBeHitCooldown = 0.17f;
    [SerializeField] protected float canBeDamagedCooldown = 1.0f;

    [Header("Death stats: ")]
    [SerializeField] protected bool isDead = false;

    public bool IsDead
    {
        get
        {
            return isDead;
        }
        set
        {
            isDead = value;
        }
    }

    [Header("Projectile stats: ")]
    [SerializeField] protected Transform projectilePos;

    public Transform ProjectilePos
    {
        get { return projectilePos; }
    }

    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] protected GameObject projectile_Right;
    [SerializeField] protected GameObject projectile_Left;
    [SerializeField] protected GameObject projectile_horizontal;
    [SerializeField] protected float projectileDelay = 0.66f;

    public float ProjectileDelay
    {
        get { return projectileDelay; }
    }

    [SerializeField] protected float projectileResetDelay = 1.5f;

    public float ProjectileResetDelay
    {
        get { return projectileResetDelay; }
    }

    [SerializeField] protected float projectileSpeed = 8.0f;

    [SerializeField] protected int projectileDamageValue = 1;

    public int ProjectileDamageValue
    {
        get { return projectileDamageValue; }
    }

    [Header("Move stats: ")]
    [SerializeField] protected float move;
    [SerializeField] protected float origMoveSpeed;
    [SerializeField] protected float movementSpeed;
    [SerializeField] protected float slowMultiplier = 0.5f;
    [SerializeField] protected bool isStopped = false;
    [SerializeField] protected float faceDirection = 1;
    [SerializeField] protected float origMoveDirection = 1;
    [SerializeField] protected float moveDirection = 1;
    [SerializeField] protected Vector3 origScale;

    [Header("Jump stats: ")]
    [SerializeField] protected float jumpVelocity;
    [SerializeField] protected float jumpCooldown = 1.5f;
    [SerializeField] protected bool isGrounded = true;
    [SerializeField] protected bool isJumping = false;
    [SerializeField] protected bool canJump = true;
    [SerializeField] protected bool canDoubleJump = false;
    [SerializeField] protected LayerMask layerMask;
    protected RaycastHit2D ground;
    
    [Header("Dash stats: ")]
    [SerializeField] protected float dashMultiplier = 3.5f;
    [SerializeField] protected float dashDuration = 0.4f;
    [SerializeField] protected float dashCooldown = 1.0f;
    [SerializeField] protected bool isDashing = false;
    [SerializeField] protected bool canDash = true;
    [SerializeField] protected bool canAirDash = true;

    [Header("Phase stats: ")]
    [SerializeField] protected float phaseDuration = 3.0f;
    [SerializeField] protected float phaseCooldown = 5.0f;
    [SerializeField] protected bool canPhase = true;
    [SerializeField] protected bool _isPhasing = false;

    [Header("Time Lapse stats: ")]
    [SerializeField] protected GameObject ghost;
    [SerializeField] protected float timeLapseDuration = 1;
    [SerializeField] protected bool isTimeLapsing = false;
    [SerializeField] protected float timeLapseManaCost = 12;

    [Header("Element stats: ")]
    [SerializeField] protected Element currentElement;
    [SerializeField] protected Element weaponElement;

    //[Header("Audio: ")]

    [Header("Components: ")]
    [SerializeField] protected Rigidbody2D _rigid;
    [SerializeField] protected CapsuleCollider2D _collider;
    [SerializeField] protected MeshRenderer _mesh;
    [SerializeField] protected Material _material;
    [SerializeField] protected Animator _anim;
    [SerializeField] protected SkeletonMecanim skeletonMecanim;

    [SerializeField] protected Vector2 colliderSize;

    [SerializeField] protected float flashTime = 0.5f;
    protected Color origColor;
    [SerializeField] protected float colliderSizeMultiplier;
    [SerializeField] protected float colliderOffsetY;

    public virtual void Start()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CapsuleCollider2D>();
        _mesh = GetComponent<MeshRenderer>();
        _material = GetComponent<Material>();
        _anim = GetComponent<Animator>();
        skeletonMecanim = GetComponent<SkeletonMecanim>();

        colliderSize = _collider.size * transform.localScale.x;

        origScale = transform.localScale;
        
        //origColor = _mesh.color;
        movementSpeed = origMoveSpeed;
    }

    public virtual void Update()
    {
        
    }

    public virtual void FixedUpdate()
    {

    }

    public virtual void GetHit(Damage dmg)
    {
        
    }

    public virtual void TakeDamage(Damage dmg)
    {
        _anim.SetTrigger("Damaged");
        PlayRandomDamagedSound();

        actualDamage = ElementCompute(currentElement, dmg.damageElement, dmg.damageAmount);
        health -= actualDamage;

        if (health > 0)
        {
            canBeDamaged = false;
            StartCoroutine(ResetCanBeDamaged());

            if (dmg.stunningDuration > 0)
            {
                StartCoroutine(Stunned(dmg.stunningDuration));
            }
        }
        else
        {
            Death();
        }
    }

    public virtual void Parry(Damage dmg)
    {
        actualDamage = ElementCompute(currentElement, dmg.damageElement, dmg.damageAmount) - parryValue;

        SoundManager.PlaySound("Parry", gameObject.name);
        StartCoroutine(ResetCanBeHit());
    }

    public virtual void Block(Damage dmg)
    {
        actualDamage = ElementCompute(currentElement, dmg.damageElement, dmg.damageAmount) - blockValue;

        SoundManager.PlaySound("BlockHit", gameObject.name);
        _anim.SetTrigger("Hit");

        //StartCoroutine(ResetCanBeHit());

        if (dmg.stunningDuration > 0)
        {
            StartCoroutine(Stunned(dmg.stunningDuration));
        }
    }

    public virtual void Move()
    {
        
    }

    public virtual int ElementCompute(Element currentElemetType, Element damageElement, int damage)
    {
        //Elements: Soul => Psionic => Spirit => Soul

        switch (currentElement)
        {
            case Element.Soul:
                if (damageElement == Element.Spirit)
                {
                    damage += 1;
                    return damage;
                }
                else if (damageElement == Element.Psionic)
                {
                    damage -= 1;
                    return damage;
                }
                else if(damageElement == Element.Soul)
                {
                    return damage;
                }
                return damage;
            case Element.Spirit:
                if (damageElement == Element.Psionic)
                {
                    damage += 1;
                    return damage;
                }
                else if (damageElement == Element.Soul)
                {
                    damage -= 1;
                    return damage;
                }
                else if (damageElement == Element.Spirit)
                {
                    return damage;
                }
                return damage;
            case Element.Psionic:
                if (damageElement == Element.Soul)
                {
                    damage += 1;
                    return damage;
                }
                else if (damageElement == Element.Spirit)
                {
                    damage -= 1;
                    return damage;
                }
                else if (damageElement == Element.Psionic)
                {
                    return damage;
                }
                return damage;
            default:
                return damage;
        }
    }

    public virtual void CheckGround()
    {
        ground = Physics2D.Raycast(transform.position, Vector2.down, colliderSize.y, layerMask);
        Debug.DrawRay(transform.position, Vector2.down * colliderSize.y, Color.cyan);

        if (ground.collider == null)
        {
            isGrounded = false;
            isStopped = false;
        }
        else if (ground.collider != null)
        {
            isGrounded = true;
            canDoubleJump = true;
            canDash = true;
        }
    }

    protected virtual void UpdateDirection()
    {
        if(faceDirection > 0)
        {
            transform.localScale = new Vector3(origScale.x, origScale.y, origScale.z);
        }
        else if(faceDirection < 0)
        {
            transform.localScale = new Vector3(-origScale.x, origScale.y, origScale.z);
        }
    }

    public void FlipX()
    {
        faceDirection = -faceDirection;
    }

    // Special Mobility Functions

    public virtual void Dash()
    {
        StartCoroutine(Dashing());
    }

    public virtual void JumpDash()
    {
        StartCoroutine(JumpDashing());
    }

    public virtual void BackDash()
    {
        StartCoroutine(BackDashing());
    }

    public virtual void BackJumpDash()
    {
        StartCoroutine(BackJumpDashing());
    }

    public virtual void Jump()
    {
        if (isGrounded && canJump)
        {
            _rigid.velocity = new Vector2(_rigid.velocity.x, jumpVelocity);
        }
    }

    public virtual void DoubleJump()
    {
        if (canDoubleJump)
        {
            _rigid.velocity = new Vector2(_rigid.velocity.x, jumpVelocity);
            canDoubleJump = false;
        }
    }

    // Ability Functions

    public virtual void ImplementPhase()
    {
        if (_isPhasing)
        {
            _mesh.enabled = false;
            _collider.enabled = false;
            _rigid.bodyType = RigidbodyType2D.Static;
        }
        else if (!_isPhasing)
        {
            _mesh.enabled = true;
            _collider.enabled = true;
            _rigid.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    protected virtual void TimeLapse()
    {
        StartCoroutine(TimeLapsing());
    }

    // Projectile Functions

    public virtual void Projectile_Straight()
    {
        Instantiate(projectilePrefab, projectilePos.position, Quaternion.identity);
    }

    public virtual void Projectile_Horizontal()
    {
        //GameObject go = Instantiate(projectile_horizontal, projectilePos.position, Quaternion.identity);
        //go.GetComponent<Projectile_Horizontal>().shootDirection = faceDirection;

        projectile_horizontal.transform.position = projectilePos.position;
        projectile_horizontal.GetComponent<Projectile_Horizontal>().ShootDirection = faceDirection;
        projectile_horizontal.GetComponent<Projectile_Horizontal>().ProjectileCurrentSpeed = projectileSpeed;
        projectile_horizontal.SetActive(true);
    }

    public virtual void Projectile_Left()
    {
        Instantiate(projectile_Left, projectilePos.position, Quaternion.identity);
    }

    public virtual void Projectile_Right()
    {
        Instantiate(projectile_Right, projectilePos.position, Quaternion.identity);
    }

    public virtual void Projectile_Left_Right()
    {
        Instantiate(projectile_Left, projectilePos.position, Quaternion.identity);
        Instantiate(projectile_Right, projectilePos.position, Quaternion.identity);
    }

    public virtual void Heal()
    {

    }

    public virtual void Death()
    {
        isDead = true;
    }

    public virtual void SwordPowerAttack()
    {

    }

    public virtual void ShieldPowerAttack()
    {

    }

    // Set Combat Values

    public virtual void SetRightCombatValues(int parry, int block, int damage, float stun)
    {
        rightParryValue = parry;
        rightBlockValue = block;
        rightDamageValue = damage;

        stunDuration = stun;

        //if (stunDuration == 0)
        //{
        //    stunDuration = stun;
        //}
    }

    public virtual void SetLeftCombatValues(int parry, int block, int damage, float stun)
    {
        leftParryValue = parry;
        leftBlockValue = block;
        leftDamageValue = damage;

        if (stunDuration == 0)
        {
            stunDuration = stun;
        }
    }

    // Coroutines

    //protected virtual IEnumerator Attacking()
    //{
    //    canAttack = false;

    //    yield return new WaitForSeconds(attackCooldown);

    //    canAttack = true;
    //}

    public virtual IEnumerator ResetCanBeHit()
    {
        yield return new WaitForSeconds(canBeHitCooldown);
        canBeDamaged = true;
    }

    public virtual IEnumerator ResetCanBeDamaged()
    {
        yield return new WaitForSeconds(canBeDamagedCooldown);
        canBeDamaged = true;
    }

    public virtual IEnumerator Stunned(float dmgStunDuration)
    {
        _anim.SetBool("Stunned", true);
        isStopped = true;

        yield return new WaitForSeconds(dmgStunDuration);

        _anim.SetBool("Stunned", false);
        isStopped = false;
    }

    public virtual IEnumerator Jumping()
    {
        canJump = false;

        yield return new WaitForSeconds(jumpCooldown);

        canJump = true;
    }

    protected virtual IEnumerator Dashing()
    {
        _anim.SetBool("Dashing", true);
        movementSpeed = origMoveSpeed;
        isDashing = true;
        canDash = false;

        yield return new WaitForSeconds(dashDuration);

        _anim.SetBool("Dashing", false);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    protected virtual IEnumerator BackDashing()
    {
        _anim.SetBool("BackDashing", true);
        movementSpeed = origMoveSpeed;
        isDashing = true;
        canDash = false;

        yield return new WaitForSeconds(dashDuration);

        _anim.SetBool("BackDashing", false);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    protected virtual IEnumerator JumpDashing()
    {
        _anim.SetBool("Dashing", true);
        isDashing = true;
        canDash = false;
        canAirDash = false;

        yield return new WaitForSeconds(dashDuration);

        _anim.SetBool("Dashing", false);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    protected virtual IEnumerator BackJumpDashing()
    {
        _anim.SetBool("BackDashing", true);
        isDashing = true;
        canDash = false;
        canAirDash = false;

        yield return new WaitForSeconds(dashDuration);

        _anim.SetBool("BackDashing", false);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    protected virtual IEnumerator Phasing()
    {
        _isPhasing = true;
        StartCoroutine(PhaseCooldown());
        yield return new WaitForSeconds(phaseDuration);
        _isPhasing = false;
    }

    protected virtual IEnumerator PhaseCooldown()
    {
        canPhase = false;
        yield return new WaitForSeconds(phaseCooldown);
        canPhase = true;
    }

    public virtual IEnumerator TimeLapsing()
    {
        transform.position = ghost.GetComponent<TimeLapse_Position>().TimeLapsePosition;
        health = ghost.GetComponent<TimeLapse_Position>().TimeLapsePlayerHealth;
        UIManager.Instance.UpdateLives((int)health);

        _mesh.enabled = false;
        _collider.enabled = false;
        _rigid.bodyType = RigidbodyType2D.Static;
        isTimeLapsing = true;

        yield return new WaitForSeconds(timeLapseDuration);

        _mesh.enabled = true;
        _collider.enabled = true;
        _rigid.bodyType = RigidbodyType2D.Dynamic;
        isTimeLapsing = false;
    }

    public virtual void ShootProjectile(int value)
    {
        //if(!isGrounded && value == 1 || isGrounded && value == 0)
        //{
        //    if(faceDirection == 1)
        //    {
        //        GameObject tmp = (GameObject)Instantiate(projectilePrefab, projectilePos.position, Quaternion.identity);

        //    }
        //    else
        //    {
        //        GameObject tmp = (GameObject)Instantiate(projectilePrefab, projectilePos.position, Quaternion.identity);

        //    }
        //}
    }

    // -------------------------------------------------------- Audio ------------------------------------------------------

    public void PlayRandomDamagedSound()
    {
        int randomDamageSound = Random.Range(0, 3);

        switch (randomDamageSound)
        {
            case 0:
                SoundManager.PlaySound("Damaged1", gameObject.name);
                break;
            case 1:
                SoundManager.PlaySound("Damaged2", gameObject.name);
                break;
            case 2:
                SoundManager.PlaySound("DamagedBoneBreak", gameObject.name);
                break;
        }
    }
}
