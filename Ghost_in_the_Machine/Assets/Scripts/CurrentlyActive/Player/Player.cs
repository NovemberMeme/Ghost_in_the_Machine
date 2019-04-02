using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    private enum LeftWeaponState
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

    private enum RightWeaponState
    {
        Idling,
        Parrying,
        Blocking,
        Attacking1,
        PowerAttacking,
        Casting
    }

    private enum PlayerMobilityState
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

    private enum PlayerDirectionState
    {
        Forward,
        Upward,
        Downward
    }

    [Header("Controller mode stats: ")]
    [SerializeField] private bool controllerMode = false;

    [Header("Enum states: ")]
    [SerializeField] private LeftWeaponState currentLeftWeaponState;
    [SerializeField] private RightWeaponState currentRightWeaponState;
    [SerializeField] private PlayerMobilityState currentMobilityState;
    [SerializeField] private PlayerDirectionState currentPlayerDirectionState;
    [SerializeField] private VerticalAttackDirection playerVerticalAttackDirectionState = VerticalAttackDirection.AttackingForward;
    [SerializeField] private HorizontalAttackDirection playerHorizontalAttackDirectionState;

    public VerticalAttackDirection PlayerVerticalAttackDirectionState
    {
        get { return playerVerticalAttackDirectionState; }
    }

    public HorizontalAttackDirection PlayerHorizontalAttackDirectionState
    {
        get { return playerHorizontalAttackDirectionState; }
    }

    [Header("Strafe stats: ")]
    [SerializeField] private bool isStrafing = false;

    [Header("Currency stats: ")]
    [SerializeField] private int coins;

    public int Coins
    {
        get
        {
            return coins;
        }
        set
        {
            coins = value;
        }
    }

    // For adjusting gravity value when falling down so that player falls down faster when they don't hold the jump button
    // but they still jump higher and longer when they hold down the jump button

    [Header("Player Jump Multiplier stats: ")]
    [SerializeField] private float fallMultiplier = 3.5f;
    [SerializeField] private float lowJumpMultiplier = 2.0f;
    
    // For unlocking new player abilities

    [Header("Unlock stats: ")]
    [SerializeField] private bool unlockedDoubleJump = true;
    [SerializeField] private bool unlockedDash = true;
    [SerializeField] private bool unlockedPhase = true;
    [SerializeField] private bool unlockedTimeLapse = true;

    [Header("Heal stats: ")]
    [SerializeField] private float healTimer = 0;
    [SerializeField] private float healCooldown = 1;
    [SerializeField] private bool healSet = false;
    [SerializeField] private float healManaCost = 4;

    [Header("Power Attack stats: ")]
    [SerializeField] private float swordPowerAttackManaCost = 6;
    [SerializeField] private float shieldPowerAttackManaCost = 6;

    [Header("Mana stats: ")]
    [SerializeField] private float maxMana = 12;
    [SerializeField] private float currentMana = 12;
    
    public float MaxMana
    {
        get { return maxMana; }
    }

    public float CurrentMana
    {
        get
        {
            return currentMana;
        }
        set
        {
            currentMana = value;
        }
    }


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
        UpdateDirection();

        CheckGround();

        DetermineMovement();

        ImplementState();
        ChangeState();

        ImplementPhase();

        PlayerInput();

        if(health <= 0)
        {
            isDead = true;
        }
    }

    public override void FixedUpdate()
    {
        Move();
    }

    public virtual void PlayerInput()
    {
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
        else if (currentLeftWeaponState == LeftWeaponState.Idling && currentRightWeaponState == RightWeaponState.Idling && !Input.GetKey(KeyCode.LeftControl) && Input.GetAxisRaw("LT") <= 0)
        {
            isStrafing = false;
            _anim.SetBool("Slowed", false);
        }
        else if (!Input.GetKey(KeyCode.LeftControl) && Input.GetAxisRaw("LT") <= 0)
        {
            isStrafing = false;
        }

        // Healing

        if (health < 4 && isGrounded && currentMana >= healManaCost)
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

        if (!isTimeLapsing && !_isPhasing && currentMana >= timeLapseManaCost && unlockedTimeLapse &&
            currentLeftWeaponState == LeftWeaponState.Idling && currentRightWeaponState == RightWeaponState.Idling)
        {
            if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Y"))
            {
                TimeLapse();
            }
        }

        // Unlock buttons

        if (Input.GetKeyDown(KeyCode.P))
        {
            unlockedDoubleJump = !unlockedDoubleJump;
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            unlockedTimeLapse = !unlockedTimeLapse;
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            unlockedPhase = !unlockedPhase;
        }
    }

    public override void GetHit(Damage dmg)
    {
        if (isDead || !canBeDamaged || dmg.layer != "EnemyAttack")
            return;

        if(playerHorizontalAttackDirectionState == dmg.horizontalAttackDirection)
        {
            TakeDamage(dmg);
            return;
        }

        if (dmg.verticalAttackDirection != VerticalAttackDirection.AttackingForward)
        {
            if (currentRightWeaponState == RightWeaponState.Parrying)
            {
                Parry(dmg);
                Debug.Log("Right Parry");
            }
            else if(currentRightWeaponState == RightWeaponState.Blocking)
            {
                Block(dmg);
                Debug.Log("Right Block");
            }
            else
            {
                TakeDamage(dmg);
            }
        }
        else if(dmg.verticalAttackDirection == VerticalAttackDirection.AttackingForward)
        {
            if(currentLeftWeaponState == LeftWeaponState.Parrying)
            {
                Parry(dmg);
                Debug.Log("Left Parry");
            }
            else if(currentLeftWeaponState == LeftWeaponState.Blocking)
            {
                Block(dmg);
                Debug.Log("Left Block");
            }
            else
            {
                TakeDamage(dmg);
            }
        }

        //if (currentLeftWeaponState == LeftWeaponState.Parrying || currentRightWeaponState == RightWeaponState.Parrying)
        //{
        //    if ((dmg.verticalAttackDirection == VerticalAttackDirection.AttackingDownward && currentPlayerDirectionState == PlayerDirectionState.Upward) ||
        //        (dmg.verticalAttackDirection == VerticalAttackDirection.AttackingForward && currentPlayerDirectionState == PlayerDirectionState.Forward) ||
        //        (dmg.verticalAttackDirection == VerticalAttackDirection.AttackingUpward && currentPlayerDirectionState == PlayerDirectionState.Downward))
        //    {
        //        Parry(dmg);
        //    }
        //    else if (currentLeftWeaponState == LeftWeaponState.Blocking || currentRightWeaponState == RightWeaponState.Blocking)
        //    {
        //        if ((dmg.verticalAttackDirection == VerticalAttackDirection.AttackingDownward && currentPlayerDirectionState == PlayerDirectionState.Upward) ||
        //            (dmg.verticalAttackDirection == VerticalAttackDirection.AttackingForward && currentPlayerDirectionState == PlayerDirectionState.Forward) ||
        //            (dmg.verticalAttackDirection == VerticalAttackDirection.AttackingUpward && currentPlayerDirectionState == PlayerDirectionState.Downward))
        //        {
        //            Block(dmg);
        //        }
        //        else
        //        {
        //            TakeDamage(dmg);
        //        }
        //    }
        //}
        //else if (currentLeftWeaponState == LeftWeaponState.Blocking || currentRightWeaponState == RightWeaponState.Blocking)
        //{
        //    if ((dmg.verticalAttackDirection == VerticalAttackDirection.AttackingDownward && currentPlayerDirectionState == PlayerDirectionState.Upward) ||
        //            (dmg.verticalAttackDirection == VerticalAttackDirection.AttackingForward && currentPlayerDirectionState == PlayerDirectionState.Forward) ||
        //            (dmg.verticalAttackDirection == VerticalAttackDirection.AttackingUpward && currentPlayerDirectionState == PlayerDirectionState.Downward))
        //    {
        //        Block(dmg);
        //    }
        //    else
        //    {
        //        TakeDamage(dmg);
        //    }
        //}
        //else
        //{
        //    TakeDamage(dmg);
        //}
    }

    public override void TakeDamage(Damage dmg)
    {
        _anim.SetTrigger("Damaged");
        PlayRandomDamagedSound();

        actualDamage = ElementCompute(currentElement, dmg.damageElement, dmg.damageAmount);
        health -= actualDamage;
        UIManager.Instance.UpdateLives((int)health);

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

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetAxisRaw("RT") > 0) && canDash && unlockedDash)
        {
            if(move == 0 || moveDirection - faceDirection != 0)
            {
                if (isGrounded)
                {
                    //SoundManager.PlaySound("dash"); IMPLEMENT DASH SFX
                    BackDash();
                }
                else if (!isGrounded && canAirDash)
                {
                    BackJumpDash();
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
                else if (!isGrounded && canAirDash)
                {
                    JumpDash();
                    canAirDash = false;
                }
            }
        }

        _anim.SetFloat("VerticalSpeed", _rigid.velocity.y);
        _anim.SetFloat("Moving", Mathf.Abs(move));
    }

    public override void Move()
    {
        // The actual adjustments for gravity values which result in varying jump heights and durations based on button pressed duration is here

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
            playerHorizontalAttackDirectionState = HorizontalAttackDirection.AttackingRightward;
        }
        else if (faceDirection == -1)
        {
            transform.localScale = new Vector3(-origScale.x, origScale.y, origScale.z);
            playerHorizontalAttackDirectionState = HorizontalAttackDirection.AttackingLeftward;
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

    public override void Heal()
    {
        health++;
        currentMana -= healManaCost;
        UIManager.Instance.UpdateMana(currentMana);
        UIManager.Instance.UpdateLives(health);
        healTimer = 0;
    }

    public override void Death()
    {
        // IMPLEMENT PLAYER DEATH

        isDead = true;
    }

    public override void SwordPowerAttack()
    {
        _anim.SetTrigger("Left_PowerAttack");

        currentMana -= swordPowerAttackManaCost;
        UIManager.Instance.UpdateMana(currentMana);
    }

    public override void ShieldPowerAttack()
    {
        _anim.SetTrigger("Right_PowerAttack");

        currentMana -= shieldPowerAttackManaCost;
        UIManager.Instance.UpdateMana(currentMana);
    }

    public void AddCoins(int amount)
    {
        //coins += amount;
        //UIManager.Instance.UpdateCoinCount(coins);
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

    // Coroutines

    protected override IEnumerator Dashing()
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

        transform.position = ghost.GetComponent<TimeLapse_Position>().TimeLapsePosition;
        health = ghost.GetComponent<TimeLapse_Position>().TimeLapsePlayerHealth;
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

    //------------------------------------------------ Enum States --------------------------------------------------

    public void ImplementState()
    {
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
                    Heal();
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
                SetLeftCombatValues(0, 0, 0, 0);
                break;
            case LeftWeaponState.Parrying:
                SetLeftCombatValues(parryStat, 0, 0, 0);
                break;
            case LeftWeaponState.Blocking:
                SetLeftCombatValues(0, blockStat, 0, 0);
                break;
            case LeftWeaponState.Attacking1:
                SetLeftCombatValues(0, 0, damageStat, 0);
                break;
            case LeftWeaponState.Attacking3:
                SetLeftCombatValues(0, 0, damageStat, 0);
                break;
            case LeftWeaponState.Attacking4:
                SetLeftCombatValues(0, 0, damageStat, 0);
                break;
            case LeftWeaponState.PowerAttacking:
                SetLeftCombatValues(0, 0, damageStat, swordStunDuration);
                movementSpeed = 0;
                break;
        }

        switch (currentRightWeaponState)
        {
            case RightWeaponState.Idling:
                SetRightCombatValues(0, 0, 0, 0);
                break;
            case RightWeaponState.Parrying:
                SetRightCombatValues(parryStat, 0, 0, 0);
                break;
            case RightWeaponState.Blocking:
                SetRightCombatValues(0, blockStat, 0, 0);
                break;
            case RightWeaponState.Attacking1:
                SetRightCombatValues(0, 0, damageStat, 0);
                break;
            case RightWeaponState.PowerAttacking:
                SetRightCombatValues(0, 0, damageStat, shieldStunDuration);
                movementSpeed = 0;
                break;
        }

        if(currentLeftWeaponState == LeftWeaponState.Blocking && currentRightWeaponState == RightWeaponState.Blocking && !isDashing)
        {
            movementSpeed = 0;
            _anim.SetBool("Idling", true);
        }
        else
        {
            _anim.SetBool("Idling", false);
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
                    if(currentMana >= swordPowerAttackManaCost)
                    {
                        SwordPowerAttack();
                    }
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
            case LeftWeaponState.PowerAttacking:
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
                    if(currentMana >= shieldPowerAttackManaCost)
                    {
                        ShieldPowerAttack();
                    }
                }
                break;
            case RightWeaponState.Attacking1:
                break;
            case RightWeaponState.PowerAttacking:
                break;
        }

        if (controllerMode)
        {
            if (Input.GetAxis("Vertical") > 0.25f)
            {
                //currentPlayerDirectionState = PlayerDirectionState.Downward;
                //SetAllLayerWeights(0, 1, 0);
            }
            else if (Input.GetAxis("Vertical") < -0.25f)
            {
                //currentPlayerDirectionState = PlayerDirectionState.Upward;
                //SetAllLayerWeights(0, 0, 1);
            }
            else
            {
                currentPlayerDirectionState = PlayerDirectionState.Forward;
                SetAllLayerWeights(1, 0, 0);
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.S))
            {
                //currentPlayerDirectionState = PlayerDirectionState.Downward;
                //SetAllLayerWeights(0, 1, 0);
            }
            else if (Input.GetKey(KeyCode.W))
            {
                //currentPlayerDirectionState = PlayerDirectionState.Upward;
                //SetAllLayerWeights(0, 0, 1);
            }
            else
            {
                currentPlayerDirectionState = PlayerDirectionState.Forward;
                SetAllLayerWeights(1, 0, 0);
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

        //if(currentLeftWeaponState != LeftWeaponState.PowerAttacking && currentRightWeaponState != RightWeaponState.PowerAttacking)
        //{
        //    stunDuration = 0;
        //}
    }

    public void SetAllLayerWeights(float layerWeightA, float layerWeightB, float layerWeightC)
    {
        _anim.SetLayerWeight(1, layerWeightA);
        _anim.SetLayerWeight(2, layerWeightA);
        _anim.SetLayerWeight(3, layerWeightB);
        _anim.SetLayerWeight(4, layerWeightB);
        _anim.SetLayerWeight(5, layerWeightC);
        _anim.SetLayerWeight(6, layerWeightC);
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
