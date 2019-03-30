using System.Collections;
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
        NeutralAttacking,
        Stunned,
        Damaged
    }

    public enum PlayerDirectionState
    {
        Forward,
        Upward,
        Downward
    }

    [Header("Controller mode stats: ")]
    [SerializeField] private bool controllerMode = false;

    [Header("Enum states: ")]
    public LeftWeaponState currentLeftWeaponState;
    public RightWeaponState currentRightWeaponState;
    public PlayerMobilityState currentMobilityState;
    public PlayerDirectionState currentPlayerDirectionState;
    public AttackDirectionState playerAttackDirectionState = AttackDirectionState.AttackingForward;

    [Header("Strafe stats: ")]
    public bool isStrafing = false;

    [Header("Currency stats: ")]
    public int coins;

    [Header("Sound Manager: ")]
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

    [Header("Power Attack stats: ")]
    [SerializeField] private float swordPowerAttackManaCost = 6;
    [SerializeField] private float shieldPowerAttackManaCost = 6;

    [Header("Mana stats: ")]
    public float maxMana = 12;
    public float currentMana = 12;

    private SpriteRenderer _swordArcSprite;

    private Vector2 origColliderSize;
    private float transformSizeModifier;
    private float dashColliderSizeMultiplier = 0.4f;

    public override void Start()
    {
        base.Start();
        transformSizeModifier = transform.localScale.x;
        origColliderSize = _collider.size;
    }

    public override void Update()
    {
        base.Update();

        DetermineMovement();

        ImplementState();
        ChangeState();

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

    public override void GetHit(Damage dmg)
    {
        if (isDead || !canBeDamaged || dmg.layer != "EnemyAttack")
            return;

        actualDamage = 0;

        if (currentLeftWeaponState == LeftWeaponState.Parrying || currentRightWeaponState == RightWeaponState.Parrying)
        {
            if (dmg.attackDirectionState == AttackDirectionState.AttackingDownward && currentPlayerDirectionState == PlayerDirectionState.Upward)
            {
                Parry(dmg.damageAmount);
            }
            else if (dmg.attackDirectionState == AttackDirectionState.AttackingForward && currentPlayerDirectionState == PlayerDirectionState.Forward)
            {
                Parry(dmg.damageAmount);
            }
            else if (dmg.attackDirectionState == AttackDirectionState.AttackingUpward && currentPlayerDirectionState == PlayerDirectionState.Downward)
            {
                Parry(dmg.damageAmount);
            }
            else if (currentLeftWeaponState == LeftWeaponState.Blocking || currentRightWeaponState == RightWeaponState.Blocking)
            {
                if (dmg.attackDirectionState == AttackDirectionState.AttackingDownward && currentPlayerDirectionState == PlayerDirectionState.Upward)
                {
                    Block(dmg.damageAmount);
                }
                else if (dmg.attackDirectionState == AttackDirectionState.AttackingForward && currentPlayerDirectionState == PlayerDirectionState.Forward)
                {
                    Block(dmg.damageAmount);
                }
                else if (dmg.attackDirectionState == AttackDirectionState.AttackingUpward && currentPlayerDirectionState == PlayerDirectionState.Downward)
                {
                    Block(dmg.damageAmount);
                }
                else
                {
                    TakeDamage(dmg);
                }
            }
        }
        else if (currentLeftWeaponState == LeftWeaponState.Blocking || currentRightWeaponState == RightWeaponState.Blocking)
        {
            if (dmg.attackDirectionState == AttackDirectionState.AttackingDownward && currentPlayerDirectionState == PlayerDirectionState.Upward)
            {
                Block(dmg.damageAmount);
            }
            else if (dmg.attackDirectionState == AttackDirectionState.AttackingForward && currentPlayerDirectionState == PlayerDirectionState.Forward)
            {
                Block(dmg.damageAmount);
            }
            else if (dmg.attackDirectionState == AttackDirectionState.AttackingUpward && currentPlayerDirectionState == PlayerDirectionState.Downward)
            {
                Block(dmg.damageAmount);
            }
            else
            {
                TakeDamage(dmg);
            }
        }
        else
        {
            TakeDamage(dmg);
        }
    }

    public override void TakeDamage(Damage dmg)
    {
        actualDamage = dmg.damageAmount;

        if (actualDamage > 0)
        {
            _anim.SetTrigger("Damaged");

            health -= actualDamage;
            UIManager.Instance.UpdateLives((int)health);

            int randomDamageSound = Random.Range(0, 3);

            switch (randomDamageSound)
            {
                case 0:
                    SoundManager.PlaySound("Damage1", gameObject.name);
                    break;
                case 1:
                    SoundManager.PlaySound("Damage2", gameObject.name);
                    break;
                case 2:
                    SoundManager.PlaySound("DamageBoneBreak", gameObject.name);
                    break;
            }

            if (health > 0)
            {
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

        if (dmg.stunningDuration > 0)
        {
            StartCoroutine(Stunned(dmg.stunningDuration));
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
                        //SoundManager.PlaySound("dash"); IMPLEMENT DASH SFX
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
                        //SoundManager.PlaySound("dash"); IMPLEMENT DASH SFX
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
            if (!isDashing)
            {
                _rigid.velocity = new Vector2(movementSpeed * Time.deltaTime, _rigid.velocity.y);
            }
            else
            {
                _rigid.velocity = new Vector2(movementSpeed * Time.deltaTime, 0);
            }
            

            // Mobility State Implementation

            //switch (currentMobilityState)
            //{
            //    case PlayerMobilityState.Standing:
            //        _rigid.velocity = new Vector3(0, _rigid.velocity.y);
            //        break;
            //    case PlayerMobilityState.Walking:
            //        _rigid.velocity = new Vector3(move * movementSpeed * slowMultiplier * Time.deltaTime, _rigid.velocity.y);
            //        break;
            //    case PlayerMobilityState.Dashing:
            //        _rigid.velocity = new Vector3(faceDirection * movementSpeed * dashMultiplier * Time.deltaTime, 0);
            //        break;
            //    case PlayerMobilityState.BackDashing:
            //        _rigid.velocity = new Vector3(-faceDirection * movementSpeed * dashMultiplier * Time.deltaTime, 0);
            //        break;
            //    case PlayerMobilityState.HealStarting:
            //        _rigid.velocity = new Vector3(0, _rigid.velocity.y);
            //        break;
            //    case PlayerMobilityState.Healing:
            //        _rigid.velocity = new Vector3(0, _rigid.velocity.y);
            //        break;
            //    case PlayerMobilityState.Stunned:
            //        _rigid.velocity = new Vector3(0, _rigid.velocity.y);
            //        break;
            //    default:
            //        _rigid.velocity = new Vector3(move * movementSpeed * Time.deltaTime, _rigid.velocity.y);
            //        break;
            //}
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

        if (!isStrafing && !isDashing)
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
        }
        else if (ground.collider != null)
        {
            isGrounded = true;
            _anim.SetBool("Grounded", true);
            canDoubleJump = true;
            canAirDash = true;
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
        isDashing = true;
        canDash = false;

        yield return new WaitForSeconds(dashDuration);

        //_collider.size = origColliderSize;
        //_collider.offset = Vector2.zero;

        _anim.SetBool("Dashing", false);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    protected override IEnumerator JumpDashing()
    {
        _anim.SetBool("Dashing", true);
        isDashing = true;
        canDash = false;

        yield return new WaitForSeconds(dashDuration);

        _anim.SetBool("Dashing", false);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    protected override IEnumerator Phasing()
    {
        SoundManager.PlaySound("PhaseShift", gameObject.name);

        _isPhasing = true;
        StartCoroutine(PhaseCooldown());

        yield return new WaitForSeconds(phaseDuration);

        _isPhasing = false;
    }

    public override IEnumerator TimeLapsing()
    {
        SoundManager.PlaySound("TimeLapse", gameObject.name);

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
        // to be replaced

        switch (currentMobilityState)
        {
            case PlayerMobilityState.Standing:
                movementSpeed = 0;
                if (currentLeftWeaponState != LeftWeaponState.Idling || currentRightWeaponState != RightWeaponState.Idling)
                {
                    _anim.SetBool("Still", true);
                }
                break;
            case PlayerMobilityState.Walking:
                movementSpeed = move * origMoveSpeed * slowMultiplier;
                _anim.SetBool("Still", false);
                break;
            case PlayerMobilityState.Running:
                movementSpeed = move * origMoveSpeed;
                break;
            case PlayerMobilityState.Dashing:
                movementSpeed = faceDirection * origMoveSpeed * dashMultiplier;
                break;
            case PlayerMobilityState.BackDashing:
                movementSpeed = -faceDirection * origMoveSpeed * dashMultiplier;
                break;
            case PlayerMobilityState.HealStarting:
                movementSpeed = 0;
                healTimer = 0;
                break;
            case PlayerMobilityState.Healing:
                movementSpeed = 0;
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
            case PlayerMobilityState.Stunned:
                movementSpeed = 0;
                currentLeftWeaponState = LeftWeaponState.Idling;
                currentRightWeaponState = RightWeaponState.Idling;
                currentPlayerDirectionState = PlayerDirectionState.Forward;
                break;
            default:
                movementSpeed = move * origMoveSpeed;
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
    }

    public void ChangeState()
    {
        if (_anim.GetBool("Stunned"))
            return;

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
                    SoundManager.PlaySound("SwordSwing", gameObject.name);
                }
                break;
            case LeftWeaponState.Blocking:
                _anim.ResetTrigger("Left_Attack");
                if (!Input.GetMouseButton(0) && !Input.GetButton("RB"))
                {
                    _anim.SetTrigger("Left_Attack");
                    SoundManager.PlaySound("SwordSwing", gameObject.name);
                }
                else if(Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Y"))
                {
                    if(currentMana >= swordPowerAttackManaCost)
                    {
                        _anim.SetTrigger("Left_PowerAttack");
                        SoundManager.PlaySound("SwordSwing", gameObject.name);

                        currentMana -= swordPowerAttackManaCost;
                        UIManager.Instance.UpdateMana(currentMana);
                    }
                }
                break;
            case LeftWeaponState.Attacking1:
                _anim.ResetTrigger("Left_Attack");
                if (Input.GetMouseButtonUp(0) || Input.GetButtonUp("RB"))
                {
                    _anim.SetTrigger("Left_Attack2");
                    SoundManager.PlaySound("SwordSwing", gameObject.name);
                }
                break;
            case LeftWeaponState.Attacking3:
                _anim.ResetTrigger("Left_Attack2");
                if(Input.GetMouseButtonUp(0) || Input.GetButtonUp("RB"))
                {
                    _anim.SetTrigger("Left_Attack3");
                    SoundManager.PlaySound("SwordSwing", gameObject.name);
                }
                break;
            case LeftWeaponState.Attacking4:
                _anim.ResetTrigger("Left_Attack3");
                break;
            case LeftWeaponState.PowerAttacking:
                stunDuration = swordStunDuration;
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
                    SoundManager.PlaySound("SwordSwing", gameObject.name);
                }
                break;
            case RightWeaponState.Blocking:
                if (!Input.GetMouseButton(1) && !Input.GetButton("LB"))
                {
                    _anim.SetTrigger("Right_Attack");
                    SoundManager.PlaySound("SwordSwing", gameObject.name);
                }
                else if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Y"))
                {
                    if(currentMana >= shieldPowerAttackManaCost)
                    {
                        _anim.SetTrigger("Right_PowerAttack");
                        SoundManager.PlaySound("SwordSwing", gameObject.name);

                        currentMana -= shieldPowerAttackManaCost;
                        UIManager.Instance.UpdateMana(currentMana);
                    }
                }
                break;
            case RightWeaponState.Attacking1:
                break;
            case RightWeaponState.PowerAttacking:
                stunDuration = shieldStunDuration;
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

        if(currentLeftWeaponState != LeftWeaponState.PowerAttacking && currentRightWeaponState != RightWeaponState.PowerAttacking)
        {
            stunDuration = 0;
        }
    }

    // ------------------------------------------------- Animation Event Functions ---------------------------------------------

    public void TurnDashOn()
    {
        _anim.SetBool("Dashing", true);
        isDashing = true;
        canDash = false;
    }

    public void TurnDashOff()
    {
        _anim.SetBool("Dashing", false);
        isDashing = false;
        //StartCoroutine(DashCooldown());
    }
}
