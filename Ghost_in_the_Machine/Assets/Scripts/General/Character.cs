using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackDirectionState
{
    AttackingForward,
    AttackingUpward,
    AttackingDownward
}

public enum Element
{
    Soul,
    Spirit,
    Psionic
}

public class Character : MonoBehaviour
{
    public int health = 4;
    public string elementType;

    public int leftParryValue = 0;
    public int leftBlockValue = 0;
    public int leftDamageValue = 0;

    public int rightParryValue = 0;
    public int rightBlockValue = 0;
    public int rightDamageValue = 0;

    public int parryValue = 0;
    public int blockValue = 0;
    public int damageValue = 0;

    public bool canDamage = true;
    public bool canDamageSetter = true;
    public bool canAttack = true;
    public float attackCooldown = 0.5f;
    public float attackHitBoxCooldown = 0.5f;

    [Header("Stun stats: ")]
    public float stunDuration = 0;
    public float swordStunDuration = 2.0f;
    public float shieldStunDuration = 0.6f;

    [Header("Invulnerability stats: ")]
    public bool canBeDamaged = true;
    public float canBeHitCooldown = 0.17f;
    public float canBeDamagedCooldown = 1.0f;

    [Header("Death stats: ")]
    public bool isDead = false;

    [Header("Projectile stats: ")]
    public Transform projectilePos;
    public GameObject projectilePrefab;
    public GameObject projectile_Right;
    public GameObject projectile_Left;
    public GameObject projectile_horizontal;

    [Header("Move stats: ")]
    [SerializeField] protected float move;
    public float origMoveSpeed;
    public float movementSpeed;
    public float slowMultiplier = 0.5f;
    public bool isStopped = false;
    public float faceDirection = 1;
    public float origMoveDirection = 1;
    public float moveDirection = 1;
    public Vector3 origScale;

    [Header("Jump stats: ")]
    public float jumpVelocity;
    public float jumpCooldown = 1.5f;
    public bool isGrounded = true;
    public bool isJumping = false;
    public bool canJump = true;
    public bool canDoubleJump = false;
    [SerializeField] protected LayerMask layerMask;
    protected RaycastHit2D ground;
    
    [Header("Dash stats: ")]
    public float dashMultiplier = 3.5f;
    public float dashDuration = 0.4f;
    public float dashCooldown = 1.0f;
    public bool _isDashing = false;
    public bool canDash = true;
    public bool canAirDash = true;

    [Header("Phase stats: ")]
    public float phaseDuration = 3.0f;
    public float phaseCooldown = 5.0f;
    public bool canPhase = true;
    public bool _isPhasing = false;

    [Header("Time Lapse stats: ")]
    [SerializeField] protected GameObject ghost;
    [SerializeField] protected float timeLapseDuration = 1;
    [SerializeField] protected bool isTimeLapsing = false;
    [SerializeField] protected float timeLapseManaCost = 12;

    [Header("Element stats: ")]
    public Element currentElement;
    public Element weaponElement;

    [Header("Components: ")]
    public Rigidbody2D _rigid;
    public CapsuleCollider2D _collider;
    public MeshRenderer _mesh;
    public Material _material;
    public Animator _anim;

    public Vector2 colliderSize;

    public float flashTime = 0.5f;
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

        colliderSize = _collider.size * transform.localScale.x;

        origScale = transform.localScale;
        
        //origColor = _mesh.color;
        movementSpeed = origMoveSpeed;
    }

    public virtual void Update()
    {
        // Resets canDamage via animations

        //if (canDamageSetter)
        //{
        //    canDamage = true;
        //    canDamageSetter = false;
        //}
            
        UpdateDirection();

        CheckGround();

        //if (!isGrounded)
        //    canDash = false;
    }

    public virtual void FixedUpdate()
    {

    }

    public virtual void Damage(Damage damage)
    {
        FlashRed();
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

    public virtual void Hit()
    {

    }

    public virtual void Death()
    {
        FlashBlack();
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
        if(isGrounded)
            _rigid.velocity = new Vector2(_rigid.velocity.x, jumpVelocity);
    }

    public virtual void DoubleJump()
    {
        _rigid.velocity = new Vector2(_rigid.velocity.x, jumpVelocity);
        canDoubleJump = false;
    }

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

    protected virtual IEnumerator Attacking()
    {
        canAttack = false;

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

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
        yield return new WaitForSeconds(dmgStunDuration);
        _anim.SetBool("Stunned", false);
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
        _isDashing = true;
        canDash = false;

        yield return new WaitForSeconds(dashDuration);

        _anim.SetBool("Dashing", false);
        _isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    protected virtual IEnumerator BackDashing()
    {
        _anim.SetBool("BackDashing", true);
        movementSpeed = origMoveSpeed;
        _isDashing = true;
        canDash = false;

        yield return new WaitForSeconds(dashDuration);

        _anim.SetBool("BackDashing", false);
        _isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    protected virtual IEnumerator JumpDashing()
    {
        _anim.SetBool("Dashing", true);
        _isDashing = true;
        canDash = false;

        yield return new WaitForSeconds(dashDuration);

        _anim.SetBool("Dashing", false);
        _isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    protected virtual IEnumerator BackJumpDashing()
    {
        _anim.SetBool("BackDashing", true);
        _isDashing = true;
        canDash = false;

        yield return new WaitForSeconds(dashDuration);

        _anim.SetBool("BackDashing", false);
        _isDashing = false;

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
        transform.position = ghost.GetComponent<TimeLapse_Position>().timeLapsePosition;
        health = ghost.GetComponent<TimeLapse_Position>().timeLapsePlayerHealth;
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
        if(!isGrounded && value == 1 || isGrounded && value == 0)
        {
            if(faceDirection == 1)
            {
                GameObject tmp = (GameObject)Instantiate(projectilePrefab, projectilePos.position, Quaternion.identity);

            }
            else
            {
                GameObject tmp = (GameObject)Instantiate(projectilePrefab, projectilePos.position, Quaternion.identity);

            }
        }
    }

    public void FlashRed()
    {
        //_mesh.color = Color.red;
        Invoke("ResetColor", flashTime);
    }

    public void FlashBlack()
    {
        //_mesh.color = Color.black;
        //Invoke("ResetColor", flashTime*2);
    }

    private void ResetColor()
    {
        //_mesh.color = origColor;
    }
}
