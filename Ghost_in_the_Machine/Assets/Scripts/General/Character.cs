using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public int health = 4;

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


    public bool canBeDamaged = true;
    public float canBeHitCooldown = 0.17f;
    public float canBeDamagedCooldown = 1.0f;

    public bool isDead = false;

    public Transform projectilePos;
    public GameObject projectilePrefab;
    public GameObject projectile_Right;
    public GameObject projectile_Left;
    public GameObject projectile_horizontal;

    public float origMoveSpeed;
    public float movementSpeed;
    public float slowMultiplier = 0.5f;
    public bool isStopped = false;
    public float direction = 1;
    public Vector3 origScale;

    public float jumpVelocity;
    public float jumpCooldown = 1.5f;
    public bool isGrounded = true;
    public bool isJumping = false;
    public bool canJump = true;
    public bool canDoubleJump = false;
    [SerializeField] protected LayerMask layerMask;
    protected RaycastHit2D ground;

    public float dashMultiplier = 3.5f;
    public float dashDuration = 0.4f;
    public float dashCooldown = 1.0f;
    public bool _isDashing = false;
    public bool canDash = true;

    public float phaseDuration = 3.0f;
    public float phaseCooldown = 5.0f;
    public bool canPhase = true;
    public bool _isPhasing = false;

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
        if(direction > 0)
        {
            transform.localScale = new Vector3(origScale.x, origScale.y, origScale.z);
        }
        else if(direction < 0)
        {
            transform.localScale = new Vector3(-origScale.x, origScale.y, origScale.z);
        }
    }

    public void FlipX()
    {
        direction = -direction;
    }

    public virtual void Dash()
    {
        StartCoroutine(Dashing());
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

    public virtual void JumpDash()
    {
        StartCoroutine(JumpDashing());
    }

    public virtual void Phase()
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

    protected virtual IEnumerator Attacking()
    {
        canAttack = false;

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    public virtual IEnumerator Jumping()
    {
        canJump = false;

        yield return new WaitForSeconds(jumpCooldown);

        canJump = true;
    }

    protected virtual IEnumerator Dashing()
    {
        movementSpeed = origMoveSpeed;
        _isDashing = true;
        canDash = false;

        yield return new WaitForSeconds(dashDuration);

        _isDashing = false;

        StartCoroutine(DashCooldown());
    }

    protected virtual IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    protected virtual IEnumerator JumpDashing()
    {
        _isDashing = true;
        canDash = false;

        yield return new WaitForSeconds(dashDuration);

        _isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        if (isGrounded)
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

    public virtual void ShootProjectile(int value)
    {
        if(!isGrounded && value == 1 || isGrounded && value == 0)
        {
            if(direction == 1)
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
