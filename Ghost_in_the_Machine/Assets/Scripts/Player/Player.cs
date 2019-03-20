﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public enum LeftWeaponState
    {
        Idling,
        Parrying,
        Blocking,
        Attacking1,
        Attacking2,
        Attacking3,
        Attacking4,
        PowerAttacking,
        Casting
    }

    public enum RightWeaponState
    {
        Idling,
        Parrying,
        Blocking,
        Attacking1,
        PowerAttacking,
        Casting
    }

    public enum PlayerMobilityState
    {
        Standing,
        Walking,
        Running,
        JumpRising,
        JumpFalling,
        JumpLanding,
        Dashing,
        BackDashing,
        Phasing,
        HealStarting,
        Healing,
        NeutralChargeStarting,
        NeutralCharging,
        NeutralAttacking
    }

    public enum PlayerDirectionState
    {
        Forward,
        Upward,
        Downward
    }

    [SerializeField] private bool controllerMode = false;

    [Header("Enum states: ")]
    public LeftWeaponState currentLeftWeaponState;
    public RightWeaponState currentRightWeaponState;
    public PlayerMobilityState currentMobilityState;
    public PlayerDirectionState currentPlayerDirectionState;
    public AttackDirectionState playerAttackDirectionState = AttackDirectionState.AttackingForward;

    private AnimatorClipInfo[] currentAnimatorClipInfo_BaseLayer;
    private AnimatorClipInfo[] currentAnimatorClipInfo_LeftArm;
    private AnimatorClipInfo[] currentAnimatorClipInfo_RightArm;
    private AnimatorClipInfo[] currentAnimatorClipInfo_LeftArmDown;
    private AnimatorClipInfo[] currentAnimatorClipInfo_RightArmDown;
    private AnimatorClipInfo[] currentAnimatorClipInfo_LeftArmUp;
    private AnimatorClipInfo[] currentAnimatorClipInfo_RightArmUp;
    [SerializeField] private string currentAnimationName_BaseLayer;
    [SerializeField] private string currentAnimationName_LeftArm;
    [SerializeField] private string currentAnimationName_RightArm;
    [SerializeField] private string currentAnimationName_LeftArmDown;
    [SerializeField] private string currentAnimationName_RightArmDown;
    [SerializeField] private string currentAnimationName_LeftArmUp;
    [SerializeField] private string currentAnimationName_RightArmUp;

    public bool isStrafing = false;

    public int coins;

    public SoundManager soundManager;

    // For adjusting gravity value when falling down so that player falls down faster when they don't hold the jump button
    // but they still jump higher and longer when they hold down the jump button

    [Header("Player Jump Multiplier stats: ")]
    [SerializeField] private float fallMultiplier = 3.5f;
    [SerializeField] private float lowJumpMultiplier = 2.0f;
    
    // For unlocking new player abilities

    [Header("Unlock stats: ")]
    public bool unlockedDoubleJump = true;
    public bool unlockedDash = true;
    public bool unlockedPhase = true;

    [Header("Heal stats: ")]
    [SerializeField] private float healTimer = 0;
    [SerializeField] private float healCooldown = 1;
    [SerializeField] private bool healSet = false;
    [SerializeField] private float healManaCost = 4;

    [Header("Mana stats: ")]
    public float maxMana = 12;
    public float currentMana = 12;

    private PlayerAnimation _playerAnim;
    private SpriteRenderer _swordArcSprite;

    private Vector2 origColliderSize;
    private float transformSizeModifier;
    private float dashColliderSizeMultiplier = 0.4f;

    public override void Start()
    {
        base.Start();
        transformSizeModifier = transform.localScale.x;
        _playerAnim = GetComponent<PlayerAnimation>();
        origColliderSize = _collider.size;
    }

    public override void Update()
    {
        base.Update();

        DetermineMovement();

        ImplementState();
        ChangeState();
        ApplyState();

        ImplementPhase();

        // Controller mode

        if (Input.GetKeyDown(KeyCode.J) || Input.GetButtonDown("View Button"))
        {
            controllerMode = !controllerMode;
        }

        // Strafing

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetAxisRaw("LT") > 0)
        {
            isStrafing = true;
            _anim.SetBool("Slowed", true);
        }
        else if(currentLeftWeaponState == LeftWeaponState.Idling && currentRightWeaponState == RightWeaponState.Idling && !Input.GetKey(KeyCode.LeftControl) && Input.GetAxisRaw("LT") <= 0)
        {
            isStrafing = false;
            _anim.SetBool("Slowed", false);
        }
        else if(!Input.GetKey(KeyCode.LeftControl) && Input.GetAxisRaw("LT") <= 0)
        {
            isStrafing = false;
        }

        // Healing

        if(health < 4 && isGrounded && currentMana >= healManaCost)
        {
            if (Input.GetKey(KeyCode.E) || Input.GetButton("X"))
            {
                _anim.SetBool("HealStarting", true);
            }
            else
            {
                _anim.SetBool("HealStarting", false);
                _anim.SetBool("Healing", false);
            }
        }
        else
        {
            _anim.SetBool("HealStarting", false);
            _anim.SetBool("Healing", false);
        }

        // Phase

        if (!isTimeLapsing && ((Input.GetKey(KeyCode.Q) || Input.GetButton("L3")) && canPhase && unlockedPhase))
        {
            soundManager.PlaySound("phase");
            StartCoroutine(Phasing());
        }

        // Time Lapse

        if (!isTimeLapsing && !_isPhasing && currentMana >= timeLapseManaCost)
        {
            if (Input.GetKeyDown(KeyCode.F) && currentLeftWeaponState == LeftWeaponState.Idling && currentRightWeaponState == RightWeaponState.Idling)
            {
                TimeLapse();
            }
            else if (Input.GetButtonDown("Y") && currentLeftWeaponState == LeftWeaponState.Idling && currentRightWeaponState == RightWeaponState.Idling)
            {
                TimeLapse();
            }
        }

        // Unlock buttons

        if (Input.GetKeyDown(KeyCode.P))
        {
            unlockedDoubleJump = !unlockedDoubleJump;
        }

        if(Input.GetKeyDown(KeyCode.O))
        {
            unlockedDash = !unlockedDash;
        }

        if(Input.GetKeyDown(KeyCode.I))
        {
            unlockedPhase = !unlockedPhase;
        }

        if(health <= 0)
        {
            isDead = true;
        }
    }

    public override void FixedUpdate()
    {
        Move();
    }

    public override void Damage(Damage damage)
    {
        if (isDead || !canBeDamaged || damage.layer != "EnemyAttack")
            return;

        int actualDamage = 0;

        if (currentLeftWeaponState == LeftWeaponState.Parrying || currentRightWeaponState == RightWeaponState.Parrying)
        {
            if (damage.attackDirectionState == AttackDirectionState.AttackingDownward && currentPlayerDirectionState == PlayerDirectionState.Upward)
            {
                actualDamage = damage.damageAmount - parryValue;
            }
            else if (damage.attackDirectionState == AttackDirectionState.AttackingForward && currentPlayerDirectionState == PlayerDirectionState.Forward)
            {
                actualDamage = damage.damageAmount - parryValue;
            }
            else if (damage.attackDirectionState == AttackDirectionState.AttackingUpward && currentPlayerDirectionState == PlayerDirectionState.Downward)
            {
                actualDamage = damage.damageAmount - parryValue;
            }
            else if (currentLeftWeaponState == LeftWeaponState.Blocking || currentRightWeaponState == RightWeaponState.Blocking)
            {
                if (damage.attackDirectionState == AttackDirectionState.AttackingDownward && currentPlayerDirectionState == PlayerDirectionState.Upward)
                {
                    actualDamage = damage.damageAmount - blockValue;
                    _anim.SetTrigger("Hit");
                }
                else if (damage.attackDirectionState == AttackDirectionState.AttackingForward && currentPlayerDirectionState == PlayerDirectionState.Forward)
                {
                    actualDamage = damage.damageAmount - blockValue;
                    _anim.SetTrigger("Hit");
                }
                else if (damage.attackDirectionState == AttackDirectionState.AttackingUpward && currentPlayerDirectionState == PlayerDirectionState.Downward)
                {
                    actualDamage = damage.damageAmount - blockValue;
                    _anim.SetTrigger("Hit");
                }
                else
                {
                    actualDamage = damage.damageAmount;
                    _anim.SetTrigger("Damaged");
                }
            }
        }
        else if (currentLeftWeaponState == LeftWeaponState.Blocking || currentRightWeaponState == RightWeaponState.Blocking)
        {
            if (damage.attackDirectionState == AttackDirectionState.AttackingDownward && currentPlayerDirectionState == PlayerDirectionState.Upward)
            {
                actualDamage = damage.damageAmount - blockValue;
                _anim.SetTrigger("Hit");
            }
            else if (damage.attackDirectionState == AttackDirectionState.AttackingForward && currentPlayerDirectionState == PlayerDirectionState.Forward)
            {
                actualDamage = damage.damageAmount - blockValue;
                _anim.SetTrigger("Hit");
            }
            else if (damage.attackDirectionState == AttackDirectionState.AttackingUpward && currentPlayerDirectionState == PlayerDirectionState.Downward)
            {
                actualDamage = damage.damageAmount - blockValue;
                _anim.SetTrigger("Hit");
            }
            else
            {
                actualDamage = damage.damageAmount;
                _anim.SetTrigger("Damaged");
            }
        }
        else
        {
            actualDamage = damage.damageAmount;
            _anim.SetTrigger("Damaged");
        }

        if (actualDamage > 0)
        {
            health -= actualDamage;
            UIManager.Instance.UpdateLives((int)health);

            if (health > 0)
            {
                //_playerAnim.GetHit();
                
                canBeDamaged = false;
                StartCoroutine(ResetCanBeDamaged());
                //StartCoroutine(ResetCanBeHit());
            }
            else
            {
                Death();
            }
        }
        else
        {
            canBeDamaged = false;
            StartCoroutine(ResetCanBeHit());
        }
    }

    public virtual void DetermineMovement()
    {
        // Later used in calculation for movement

        if(!_anim.GetBool("HealStarting") && !_anim.GetBool("Healing"))
        {
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f)
            {
                move = Input.GetAxisRaw("Horizontal");
            }
            else
            {
                move = 0;
            }
        }
        else
        {
            move = 0;
        }

        // For jumping

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("A"))
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (canDoubleJump && unlockedDoubleJump)
            {
                DoubleJump();
            }
        }

        // For Dashing

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetAxisRaw("RT") > 0)
        {
            if (canDash && unlockedDash)
            {
                if(move == 0 || moveDirection - faceDirection != 0)
                {
                    if (isGrounded)
                    {
                        soundManager.PlaySound("dash");
                        BackDash();
                        canDash = false;
                    }
                    else if (!isGrounded)
                    {
                        if (canAirDash)
                        {
                            BackJumpDash();
                            canAirDash = false;
                        }
                    }
                }
                else
                {
                    if (isGrounded)
                    {
                        soundManager.PlaySound("dash");
                        Dash();
                        canDash = false;
                    }
                    else if (!isGrounded)
                    {
                        if (canAirDash)
                        {
                            JumpDash();
                            canAirDash = false;
                        }
                    }
                }
            }
        }

        _anim.SetFloat("VerticalSpeed", _rigid.velocity.y);
        _anim.SetFloat("Moving", Mathf.Abs(move));
    }

    public override void Move()
    {
        // The actual adjustments for gravity values which result in varying jump heights and durations based on button pressed duration
        // is here

        if (_rigid.velocity.y < 0)
        {
            _rigid.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (_rigid.velocity.y > 0 && (!Input.GetKey(KeyCode.Space) && !Input.GetButton("A")))
        {
            _rigid.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        // The actual Movement is here

        if(!isStopped && !isDead)
        {
            switch (currentMobilityState)
            {
                case PlayerMobilityState.Walking:
                    _rigid.velocity = new Vector3(move * movementSpeed * slowMultiplier * Time.deltaTime, _rigid.velocity.y);
                    break;
                case PlayerMobilityState.Dashing:
                    _rigid.velocity = new Vector3(faceDirection * movementSpeed * dashMultiplier * Time.deltaTime, 0);
                    break;
                case PlayerMobilityState.BackDashing:
                    _rigid.velocity = new Vector3(-faceDirection * movementSpeed * dashMultiplier * Time.deltaTime, 0);
                    break;
                case PlayerMobilityState.HealStarting:
                    _rigid.velocity = new Vector3(0, _rigid.velocity.y);
                    break;
                case PlayerMobilityState.Healing:
                    _rigid.velocity = new Vector3(0, _rigid.velocity.y);
                    break;
                default:
                    _rigid.velocity = new Vector3(move * movementSpeed * Time.deltaTime, _rigid.velocity.y);
                    break;
            }
        }
        else if(isStopped || isDead)
        {
            _rigid.velocity = Vector3.zero;
        }
    }

    protected override void UpdateDirection()
    {
        if (faceDirection == 1)
        {
            transform.localScale = new Vector3(origScale.x, origScale.y, origScale.z);
        }
        else if (faceDirection == -1)
        {
            transform.localScale = new Vector3(-origScale.x, origScale.y, origScale.z);
        }

        if (move > 0)
        {
            moveDirection = 1;
        }
        else if (move < 0)
        {
            moveDirection = -1;
        }

        if (!isStrafing && !_isDashing)
        {
            if (moveDirection == 1)
            {
                faceDirection = 1;
            }
            else if (moveDirection == -1)
            {
                faceDirection = -1;
            }
        }
    }

    public override void CheckGround()
    {
        ground = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Vector2.down, _collider.size.y*transformSizeModifier/5, layerMask);
        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y), Vector2.down * _collider.size.y*transformSizeModifier/5, Color.cyan);

        if (ground.collider == null)
        {
            isGrounded = false;
            _anim.SetBool("Grounded", false);
            //_playerAnim.Jump(true);
        }
        else if (ground.collider != null)
        {
            isGrounded = true;
            _anim.SetBool("Grounded", true);
            canDoubleJump = true;
            canAirDash = true;
            //canDash = true;
            //_playerAnim.Jump(false);
        }
    }

    public override void Jump()
    {
        if (isGrounded)
        {
            currentMobilityState = PlayerMobilityState.JumpRising;
            _rigid.velocity = new Vector2(_rigid.velocity.x, jumpVelocity);
        }
    }

    public override void DoubleJump()
    {
        currentMobilityState = PlayerMobilityState.JumpRising;
        _rigid.velocity = new Vector2(_rigid.velocity.x, jumpVelocity);
        canDoubleJump = false;
    }

    protected override IEnumerator Dashing()
    {
        //_collider.size = new Vector2(0.5f, 0.5f);
        //_collider.offset = new Vector2(0, colliderOffsetY);

        _anim.SetBool("Dashing", true);
        _isDashing = true;
        canDash = false;

        yield return new WaitForSeconds(dashDuration);

        //_collider.size = origColliderSize;
        //_collider.offset = Vector2.zero;

        _anim.SetBool("Dashing", false);
        _isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    protected override IEnumerator JumpDashing()
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

    public override IEnumerator TimeLapsing()
    {
        transform.position = ghost.GetComponent<TimeLapse_Position>().timeLapsePosition;
        health = ghost.GetComponent<TimeLapse_Position>().timeLapsePlayerHealth;
        UIManager.Instance.UpdateLives((int)health);
        currentMana -= 12;
        UIManager.Instance.UpdateMana(currentMana);

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

    public override void Death()
    {
        base.Death();
        //_playerAnim.Death();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UIManager.Instance.UpdateCoinCount(coins);
    }

    public override void ImplementPhase()
    {
        if (_isPhasing)
        {
            _mesh.enabled = false;
            _collider.enabled = false;
            _rigid.bodyType = RigidbodyType2D.Static;
        }
        else if (!_isPhasing && !isTimeLapsing)
        {
            _mesh.enabled = true;
            _collider.enabled = true;
            _rigid.bodyType = RigidbodyType2D.Dynamic;
        }

        if (_isPhasing && (Input.GetKeyUp(KeyCode.Q) || Input.GetButtonUp("L3")))
        {
            _isPhasing = false;
            _mesh.enabled = true;
            _collider.enabled = true;
            _rigid.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    //------------------------------------------------ Enum States --------------------------------------------------

    public void ImplementState()
    {
        switch (currentMobilityState)
        {
            case PlayerMobilityState.Standing:
                if(currentLeftWeaponState != LeftWeaponState.Idling || currentRightWeaponState != RightWeaponState.Idling)
                {
                    _anim.SetBool("Still", true);
                }
                break;
            case PlayerMobilityState.Walking:
                _anim.SetBool("Still", false);
                break;
            case PlayerMobilityState.Dashing:
                break;
            case PlayerMobilityState.HealStarting:
                healTimer = 0;
                break;
            case PlayerMobilityState.Healing:
                _anim.SetBool("HealStarting", false);
                healTimer += Time.deltaTime;

                if (healTimer >= healCooldown)
                {
                    if (healTimer < 4)
                    {
                        health++;
                        currentMana -= healManaCost;
                        UIManager.Instance.UpdateMana(currentMana);
                        UIManager.Instance.UpdateLives(health);
                        healTimer = 0;
                    }
                }
                break;
        }

        switch (currentLeftWeaponState)
        {
            case LeftWeaponState.Idling:
                leftParryValue = 0;
                leftBlockValue = 0;
                leftDamageValue = 0;
                break;
            case LeftWeaponState.Parrying:
                leftParryValue = 1;
                leftBlockValue = 0;
                leftDamageValue = 0;
                break;
            case LeftWeaponState.Blocking:
                leftParryValue = 0;
                leftBlockValue = 1;
                leftDamageValue = 0;
                break;
            case LeftWeaponState.Attacking1:
                leftParryValue = 0;
                leftBlockValue = 0;
                leftDamageValue = 1;
                break;
            case LeftWeaponState.Attacking3:
                leftParryValue = 0;
                leftBlockValue = 0;
                leftDamageValue = 1;
                break;
            case LeftWeaponState.Attacking4:
                leftParryValue = 0;
                leftBlockValue = 0;
                leftDamageValue = 1;
                break;
            case LeftWeaponState.PowerAttacking:
                leftParryValue = 0;
                leftBlockValue = 0;
                leftDamageValue = 1;
                movementSpeed = 0;
                break;
        }

        switch (currentRightWeaponState)
        {
            case RightWeaponState.Idling:
                rightParryValue = 0;
                rightBlockValue = 0;
                rightDamageValue = 0;
                break;
            case RightWeaponState.Parrying:
                rightParryValue = 1;
                rightBlockValue = 0;
                rightDamageValue = 0;
                break;
            case RightWeaponState.Blocking:
                rightParryValue = 0;
                rightBlockValue = 1;
                rightDamageValue = 0;
                break;
            case RightWeaponState.Attacking1:
                rightParryValue = 0;
                rightBlockValue = 0;
                rightDamageValue = 1;
                break;
            case RightWeaponState.PowerAttacking:
                rightParryValue = 0;
                rightBlockValue = 0;
                rightDamageValue = 1;
                movementSpeed = 0;
                break;
        }

        // parry/ block/ attack values are equal to the greater value among both weapons

        if(leftParryValue > rightParryValue)
        {
            parryValue = leftParryValue;
        }
        else
        {
            parryValue = rightParryValue;
        }

        if (leftBlockValue > rightBlockValue)
        {
            blockValue = leftBlockValue;
        }
        else
        {
            blockValue = rightBlockValue;
        }

        if(leftDamageValue > rightDamageValue)
        {
            damageValue = leftDamageValue;
        }
        else
        {
            damageValue = rightDamageValue;
        }

        if(currentLeftWeaponState != LeftWeaponState.PowerAttacking && currentRightWeaponState != RightWeaponState.PowerAttacking)
        {
            movementSpeed = origMoveSpeed;
        }
    }

    public void ChangeState()
    {
        switch (currentLeftWeaponState)
        {
            case LeftWeaponState.Idling:
                _anim.ResetTrigger("Left_Attack");
                _anim.ResetTrigger("Left_Attack2");
                _anim.ResetTrigger("Left_Attack_Rushed");
                if (Input.GetMouseButton(0) || Input.GetButton("RB"))
                {
                    _anim.SetTrigger("Left_Parry");
                }
                break;
            case LeftWeaponState.Parrying:
                _anim.ResetTrigger("Left_Parry");
                _anim.ResetTrigger("Left_Attack");
                if (!Input.GetMouseButton(0) && !Input.GetButton("RB"))
                {
                    _anim.SetTrigger("Left_Attack_Rushed");
                }
                break;
            case LeftWeaponState.Blocking:
                _anim.ResetTrigger("Left_Attack");
                if (!Input.GetMouseButton(0) && !Input.GetButton("RB"))
                {
                    _anim.SetTrigger("Left_Attack");
                }
                else if(Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Y"))
                {
                    _anim.SetTrigger("Left_PowerAttack");
                }
                break;
            case LeftWeaponState.Attacking1:
                _anim.ResetTrigger("Left_Attack");
                if (Input.GetMouseButtonUp(0) || Input.GetButtonUp("RB"))
                {
                    _anim.SetTrigger("Left_Attack2");
                }
                break;
            case LeftWeaponState.Attacking3:
                _anim.ResetTrigger("Left_Attack2");
                if(Input.GetMouseButtonUp(0) || Input.GetButtonUp("RB"))
                {
                    _anim.SetTrigger("Left_Attack3");
                }
                break;
            case LeftWeaponState.Attacking4:
                _anim.ResetTrigger("Left_Attack3");
                break;
        }

        switch (currentRightWeaponState)
        {
            case RightWeaponState.Idling:
                _anim.ResetTrigger("Right_Attack");
                _anim.ResetTrigger("Right_Attack_Rushed");
                if (Input.GetMouseButton(1) || Input.GetButton("LB"))
                {
                    _anim.SetTrigger("Right_Parry");
                }
                break;
            case RightWeaponState.Parrying:
                _anim.ResetTrigger("Right_Parry");
                if (!Input.GetMouseButton(1) && !Input.GetButton("LB"))
                {
                    _anim.SetTrigger("Right_Attack_Rushed");
                }
                break;
            case RightWeaponState.Blocking:
                if (!Input.GetMouseButton(1) && !Input.GetButton("LB"))
                {
                    _anim.SetTrigger("Right_Attack");
                }
                else if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Y"))
                {
                    _anim.SetTrigger("Right_PowerAttack");
                }
                break;
            case RightWeaponState.Attacking1:
                break;
        }

        if (controllerMode)
        {
            if (Input.GetAxis("Vertical") > 0.25f)
            {
                currentPlayerDirectionState = PlayerDirectionState.Downward;
                _anim.SetLayerWeight(1, 0);
                _anim.SetLayerWeight(2, 0);
                _anim.SetLayerWeight(3, 1);
                _anim.SetLayerWeight(4, 1);
                _anim.SetLayerWeight(5, 0);
                _anim.SetLayerWeight(6, 0);
            }
            else if (Input.GetAxis("Vertical") < -0.25f)
            {
                currentPlayerDirectionState = PlayerDirectionState.Upward;
                _anim.SetLayerWeight(1, 0);
                _anim.SetLayerWeight(2, 0);
                _anim.SetLayerWeight(3, 0);
                _anim.SetLayerWeight(4, 0);
                _anim.SetLayerWeight(5, 1);
                _anim.SetLayerWeight(6, 1);
            }
            else
            {
                currentPlayerDirectionState = PlayerDirectionState.Forward;
                _anim.SetLayerWeight(1, 1);
                _anim.SetLayerWeight(2, 1);
                _anim.SetLayerWeight(3, 0);
                _anim.SetLayerWeight(4, 0);
                _anim.SetLayerWeight(5, 0);
                _anim.SetLayerWeight(6, 0);
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.S))
            {
                currentPlayerDirectionState = PlayerDirectionState.Downward;
                _anim.SetLayerWeight(1, 0);
                _anim.SetLayerWeight(2, 0);
                _anim.SetLayerWeight(3, 1);
                _anim.SetLayerWeight(4, 1);
                _anim.SetLayerWeight(5, 0);
                _anim.SetLayerWeight(6, 0);
            }
            else if (Input.GetKey(KeyCode.W))
            {
                currentPlayerDirectionState = PlayerDirectionState.Upward;
                _anim.SetLayerWeight(1, 0);
                _anim.SetLayerWeight(2, 0);
                _anim.SetLayerWeight(3, 0);
                _anim.SetLayerWeight(4, 0);
                _anim.SetLayerWeight(5, 1);
                _anim.SetLayerWeight(6, 1);
            }
            else
            {
                currentPlayerDirectionState = PlayerDirectionState.Forward;
                _anim.SetLayerWeight(1, 1);
                _anim.SetLayerWeight(2, 1);
                _anim.SetLayerWeight(3, 0);
                _anim.SetLayerWeight(4, 0);
                _anim.SetLayerWeight(5, 0);
                _anim.SetLayerWeight(6, 0);
            }
        }
        

        if(currentLeftWeaponState != LeftWeaponState.Idling || currentRightWeaponState != RightWeaponState.Idling)
        {
            _anim.SetBool("Still", true);
            _anim.SetBool("Slowed", true);
        }
        else if(currentLeftWeaponState == LeftWeaponState.Idling && currentRightWeaponState == RightWeaponState.Idling)
        {
            _anim.SetBool("Still", false);
            _anim.SetBool("Slowed", false);
        }
    }

    public void ApplyState()
    {
        currentAnimatorClipInfo_BaseLayer = _anim.GetCurrentAnimatorClipInfo(0);
        currentAnimatorClipInfo_LeftArm = _anim.GetCurrentAnimatorClipInfo(1);
        currentAnimatorClipInfo_RightArm = _anim.GetCurrentAnimatorClipInfo(2);
        currentAnimatorClipInfo_LeftArmDown = _anim.GetCurrentAnimatorClipInfo(3);
        currentAnimatorClipInfo_RightArmDown = _anim.GetCurrentAnimatorClipInfo(4);
        currentAnimatorClipInfo_LeftArmUp = _anim.GetCurrentAnimatorClipInfo(5);
        currentAnimatorClipInfo_RightArmUp = _anim.GetCurrentAnimatorClipInfo(6);

        currentAnimationName_BaseLayer = currentAnimatorClipInfo_BaseLayer[0].clip.name;
        currentAnimationName_LeftArm = currentAnimatorClipInfo_LeftArm[0].clip.name;
        currentAnimationName_RightArm = currentAnimatorClipInfo_RightArm[0].clip.name;
        currentAnimationName_LeftArmDown = currentAnimatorClipInfo_LeftArmDown[0].clip.name;
        currentAnimationName_RightArmDown = currentAnimatorClipInfo_RightArmDown[0].clip.name;
        currentAnimationName_LeftArmUp = currentAnimatorClipInfo_LeftArmUp[0].clip.name;
        currentAnimationName_RightArmUp = currentAnimatorClipInfo_RightArmUp[0].clip.name;

        switch (currentAnimationName_LeftArm)
        {
            case "Nov_Idle_Left_Override":
                currentLeftWeaponState = LeftWeaponState.Idling;
                break;
            case "Nov_Left_Sword_Parry1_Override":
                currentLeftWeaponState = LeftWeaponState.Parrying;
                break;
            case "Nov_Left_Sword_Block1_Override":
                currentLeftWeaponState = LeftWeaponState.Blocking;
                break;
            case "Nov_Left_Sword_Attack1_Override":
                currentLeftWeaponState = LeftWeaponState.Attacking1;
                break;
            case "Nov_Left_Sword_Attack3_Override":
                if(currentPlayerDirectionState == PlayerDirectionState.Forward)
                {
                    currentLeftWeaponState = LeftWeaponState.Attacking3;
                }
                break;
            case "Nov_Left_Sword_Attack4_Override":
                if (currentPlayerDirectionState == PlayerDirectionState.Forward)
                {
                    currentLeftWeaponState = LeftWeaponState.Attacking4;
                }
                break;
            case "Nov_Left_Sword_PowerAttack":
                currentLeftWeaponState = LeftWeaponState.PowerAttacking;
                break;
        }

        switch (currentAnimationName_LeftArmDown)
        {
            case "Nov_Left_Sword_Attack_Down_Override":
                if(currentPlayerDirectionState == PlayerDirectionState.Downward)
                {
                    currentLeftWeaponState = LeftWeaponState.Attacking1;
                }
                break;
        }

        switch (currentAnimationName_LeftArmUp)
        {
            case "Nov_Left_Sword_Attack_Up_Override":
                if (currentPlayerDirectionState == PlayerDirectionState.Upward)
                {
                    currentLeftWeaponState = LeftWeaponState.Attacking1;
                }
                break;
        }

        switch (currentAnimationName_RightArm)
        {
            case "Nov_Idle_Right_Override":
                currentRightWeaponState = RightWeaponState.Idling;
                break;
            case "Nov_Right_Shield_Parry1_Override":
                currentRightWeaponState = RightWeaponState.Parrying;
                break;
            case "Nov_Right_Shield_Block1_Override":
                currentRightWeaponState = RightWeaponState.Blocking;
                break;
            case "Nov_Right_Shield_Attack1_Override":
                currentRightWeaponState = RightWeaponState.Attacking1;
                break;
            case "Nov_Right_Shield_PowerAttack":
                currentRightWeaponState = RightWeaponState.PowerAttacking;
                break;
        }

        switch (currentAnimationName_RightArmDown)
        {
            case "Nov_Right_Sword_Attack_Down_Override":
                if (currentPlayerDirectionState == PlayerDirectionState.Downward)
                {
                    currentRightWeaponState = RightWeaponState.Attacking1;
                }
                break;
        }

        switch (currentAnimationName_RightArmUp)
        {
            case "Nov_Right_Sword_Attack_Up_Override":
                if (currentPlayerDirectionState == PlayerDirectionState.Upward)
                {
                    currentRightWeaponState = RightWeaponState.Attacking1;
                }
                break;
        }

        switch (currentAnimationName_BaseLayer)
        {
            case "Nov_Idle":
                currentMobilityState = PlayerMobilityState.Standing;
                break;
            case "Nov_Walk":
                currentMobilityState = PlayerMobilityState.Walking;
                break;
            case "Nov_Run":
                currentMobilityState = PlayerMobilityState.Running;
                break;
            case "Nov_Dash":
                currentMobilityState = PlayerMobilityState.Dashing;
                break;
            case "Nov_Dash_Back":
                currentMobilityState = PlayerMobilityState.BackDashing;
                break;
            case "Nov_Jump_Fall":
                currentMobilityState = PlayerMobilityState.JumpFalling;
                break;
            case "Nov_Jump_Rise":
                currentMobilityState = PlayerMobilityState.JumpRising;
                break;
            case "Nov_Jump_Land":
                currentMobilityState = PlayerMobilityState.JumpLanding;
                break;
        }
    }

    // ------------------------------------------------- Animation Event Functions ---------------------------------------------

    public void TurnDashOn()
    {
        _anim.SetBool("Dashing", true);
        _isDashing = true;
        canDash = false;
    }

    public void TurnDashOff()
    {
        _anim.SetBool("Dashing", false);
        _isDashing = false;
        //StartCoroutine(DashCooldown());
    }
}
