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
        HealStarting,
        Healing,
        NeutralChargeStarting,
        NeutralCharging,
        NeutralAttacking,
        Phasing
    }

    //public enum PlayerAirState
    //{
    //    JumpRising,
    //    JumpFalling,
    //    JumpLanding
    //}

    public enum PlayerDirectionState
    {
        Forward,
        Upward,
        Downward
    }

    public LeftWeaponState currentLeftWeaponState;
    public RightWeaponState currentRightWeaponState;
    public PlayerMobilityState currentMobilityState;
    public PlayerDirectionState currentDirectionState;

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


    private int parryValue = 0;
    private int blockValue = 0;
    private int damageValue = 0;



    public bool isStrafing = false;

    public int coins;

    public SoundManager soundManager;

    // For adjusting gravity value when falling down so that player falls down faster when they don't hold the jump button
    // but they still jump higher and longer when they hold down the jump button

    [SerializeField] private float fallMultiplier = 3.5f;
    [SerializeField] private float lowJumpMultiplier = 2.0f;

    private float move;
    
    // For unlocking new player abilities

    public bool unlockedDoubleJump = true;
    public bool unlockedDash = true;
    public bool unlockedPhase = true;

    private PlayerAnimation _playerAnim;
    private SpriteRenderer _swordArcSprite;

    private Vector2 origColliderSize;
    private float transformSizeModifier = 0.1f;
    private float dashColliderSizeMultiplier = 0.4f;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        _playerAnim = GetComponent<PlayerAnimation>();
        origColliderSize = _collider.size;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        Movement();

        ImplementState();
        ChangeState();
        ApplyState();

        if(Input.GetMouseButtonDown(0) && isGrounded && canAttack)
        {
            soundManager.PlaySound("attack");
            //_playerAnim.Attack();
            StartCoroutine(Attacking());
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            isStrafing = true;
        }
        else
        {
            isStrafing = false;
        }

        if(Input.GetKeyDown(KeyCode.P))
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

        if(Input.GetKey(KeyCode.Q) && canPhase && unlockedPhase)
        {
            soundManager.PlaySound("phase");
            StartCoroutine(Phasing());
        }

        if(Input.GetKeyDown(KeyCode.U))
        {
            Damage();
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            //_playerAnim.Death();
        }

        if(Input.GetKeyDown(KeyCode.X))
        {
            //_playerAnim.GetHit();
        }

        Phase();

        if(health <= 0)
        {
            isDead = true;
        }
    }

    void Movement()
    {
        // Later used in calculation for movement

        move = Input.GetAxisRaw("Horizontal");

        // For jumping

        if (Input.GetButtonDown("Jump"))
        {
            if(isGrounded && currentMobilityState != PlayerMobilityState.Dashing)
            {
                Jump();
            }
            else if(canDoubleJump && unlockedDoubleJump && currentMobilityState != PlayerMobilityState.Dashing)
            {
                DoubleJump();
            }
        }

        // For Dashing

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(canDash && unlockedDash)
            {
                if(isGrounded)
                {
                    soundManager.PlaySound("dash");
                    Dash();
                }
                else if(!isGrounded)
                {
                    JumpDash();
                }
            }
        }

        // The actual adjustments for gravity values which result in varying jump heights and durations based on button pressed duration
        // is here

        if (_rigid.velocity.y < 0)
        {
            _rigid.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (_rigid.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            _rigid.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        _anim.SetFloat("VerticalSpeed", _rigid.velocity.y);

        // The actual Movement is here

        if(!isStopped && !isDead)
        {
            switch (currentMobilityState)
            {
                case PlayerMobilityState.Walking:
                    _rigid.velocity = new Vector3(move * movementSpeed * slowMultiplier * Time.deltaTime, _rigid.velocity.y);
                    break;
                case PlayerMobilityState.Dashing:
                    _rigid.velocity = new Vector3(direction * movementSpeed * dashMultiplier * Time.deltaTime, 0);
                    break;
                default:
                    _rigid.velocity = new Vector3(move * movementSpeed * Time.deltaTime, _rigid.velocity.y);
                    break;
            }

            //if (!_isDashing)
            //{
            //    _rigid.velocity = new Vector3(move * movementSpeed * Time.deltaTime, _rigid.velocity.y);
            //}
            //else if (_isDashing)
            //{
            //    _rigid.velocity = new Vector3(direction * movementSpeed * dashMultiplier * Time.deltaTime, _rigid.velocity.y);
            //}

            if (isGrounded)
            {
                _anim.SetFloat("Moving", Mathf.Abs(move));
                //_playerAnim.Move(move);
            }

        }
        else if(isStopped || isDead)
        {
            _rigid.velocity = Vector3.zero;
        }
    }

    protected override void UpdateDirection()
    {
        if (move > 0)
        {
            direction = 1;
        }
        else if (move < 0)
        {
            direction = -1;
        }

        if (!isStrafing)
        {
            if (direction == 1)
            {
                transform.localScale = new Vector3(origScale.x, origScale.y, origScale.z);
            }
            else if (direction == -1)
            {
                transform.localScale = new Vector3(-origScale.x, origScale.y, origScale.z);
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
            canDash = true;
            //_playerAnim.Jump(false);
        }
    }

    public override void Jump()
    {
        if (isGrounded)
        {
            _rigid.velocity = new Vector2(_rigid.velocity.x, jumpVelocity);
            //_playerAnim.Jump(true);
        }
    }

    public override void DoubleJump()
    {
        _rigid.velocity = new Vector2(_rigid.velocity.x, jumpVelocity);
        //_playerAnim.Jump(true);
        canDoubleJump = false;
    }

    protected override IEnumerator Dashing()
    {
        //_collider.size = new Vector2(0.5f, 0.5f);
        //_collider.offset = new Vector2(0, colliderOffsetY);

        _anim.SetBool("Dashing", true);
        _isDashing = true;
        canDash = false;
        //_playerAnim.StartDash();

        yield return new WaitForSeconds(dashDuration);

        //_collider.size = origColliderSize;
        //_collider.offset = Vector2.zero;

        _isDashing = false;
        //_playerAnim.StopDash();

        StartCoroutine(DashCooldown());
    }

    protected override IEnumerator JumpDashing()
    {
        _anim.SetBool("Dashing", true);
        _isDashing = true;
        canDash = false;
        //_playerAnim.StartJumpDash();

        yield return new WaitForSeconds(dashDuration);

        _isDashing = false;
        //_playerAnim.StopJumpDash();

        yield return new WaitForSeconds(dashCooldown);

        if (isGrounded)
            canDash = true;
    }

    public override void Damage()
    {
        base.Damage();

        if (!isDead)
        {
            health -= 1;
            UIManager.Instance.UpdateLives((int)health);

            if (health > 0)
            {
                //_playerAnim.GetHit();
            }
            else
            {
                Death();
            }
        }
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

    //------------------------------------------------ Enum States --------------------------------------------------

    public void ImplementState()
    {
        switch (currentLeftWeaponState)
        {
            case LeftWeaponState.Idling:
                parryValue = 0;
                blockValue = 0;
                damageValue = 0;
                break;
            case LeftWeaponState.Parrying:
                parryValue = 1;
                blockValue = 0;
                damageValue = 0;
                break;
            case LeftWeaponState.Blocking:
                parryValue = 0;
                blockValue = 1;
                damageValue = 0;
                break;
            case LeftWeaponState.Attacking1:
                parryValue = 0;
                blockValue = 0;
                damageValue = 1;
                break;
            case LeftWeaponState.PowerAttacking:
                movementSpeed = 0;
                break;
        }

        switch (currentRightWeaponState)
        {
            case RightWeaponState.Idling:
                parryValue = 0;
                blockValue = 0;
                damageValue = 0;
                break;
            case RightWeaponState.Parrying:
                parryValue = 1;
                blockValue = 0;
                damageValue = 0;
                break;
            case RightWeaponState.Blocking:
                parryValue = 0;
                blockValue = 1;
                damageValue = 0;
                break;
            case RightWeaponState.Attacking1:
                parryValue = 0;
                blockValue = 0;
                damageValue = 1;
                break;
            case RightWeaponState.PowerAttacking:
                movementSpeed = 0;
                break;
        }

        switch (currentMobilityState)
        {
            case PlayerMobilityState.Dashing:
                break;
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
                if (Input.GetMouseButton(0))
                {
                    _anim.SetTrigger("Left_Parry");
                }
                break;
            case LeftWeaponState.Parrying:
                _anim.ResetTrigger("Left_Parry");
                _anim.ResetTrigger("Left_Attack");
                if (!Input.GetMouseButton(0))
                {
                    _anim.SetTrigger("Left_Attack_Rushed");
                }
                break;
            case LeftWeaponState.Blocking:
                _anim.ResetTrigger("Left_Attack");
                if (!Input.GetMouseButton(0))
                {
                    _anim.SetTrigger("Left_Attack");
                }
                else if(Input.GetKeyDown(KeyCode.F))
                {
                    _anim.SetTrigger("Left_PowerAttack");
                }
                break;
            case LeftWeaponState.Attacking1:
                _anim.ResetTrigger("Left_Attack");
                if (Input.GetMouseButtonUp(0))
                {
                    _anim.SetTrigger("Left_Attack2");
                }
                break;
            case LeftWeaponState.Attacking3:
                _anim.ResetTrigger("Left_Attack2");
                if(Input.GetMouseButtonUp(0))
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
                if (Input.GetMouseButton(1))
                {
                    _anim.SetTrigger("Right_Parry");
                }
                break;
            case RightWeaponState.Parrying:
                _anim.ResetTrigger("Right_Parry");
                if (!Input.GetMouseButton(1))
                {
                    _anim.SetTrigger("Right_Attack_Rushed");
                }
                break;
            case RightWeaponState.Blocking:
                if (!Input.GetMouseButton(1))
                {
                    _anim.SetTrigger("Right_Attack");
                }
                else if (Input.GetKeyDown(KeyCode.F))
                {
                    _anim.SetTrigger("Right_PowerAttack");
                }
                break;
            case RightWeaponState.Attacking1:
                break;
        }

        switch (currentMobilityState)
        {
            case PlayerMobilityState.Standing:
                break;
            case PlayerMobilityState.Walking:
                _anim.SetBool("Still", false);
                break;
            case PlayerMobilityState.Dashing:
                break;

        }

        if (Input.GetKey(KeyCode.S))
        {
            currentDirectionState = PlayerDirectionState.Downward;
            _anim.SetLayerWeight(1, 0);
            _anim.SetLayerWeight(2, 0);
            _anim.SetLayerWeight(3, 1);
            _anim.SetLayerWeight(4, 1);
            _anim.SetLayerWeight(5, 0);
            _anim.SetLayerWeight(6, 0);
        }
        else if(Input.GetKey(KeyCode.W))
        {
            currentDirectionState = PlayerDirectionState.Upward;
            _anim.SetLayerWeight(1, 0);
            _anim.SetLayerWeight(2, 0);
            _anim.SetLayerWeight(3, 0);
            _anim.SetLayerWeight(4, 0);
            _anim.SetLayerWeight(5, 1);
            _anim.SetLayerWeight(6, 1);
        }
        else if(Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.W))
        {
            currentDirectionState = PlayerDirectionState.Forward;
            _anim.SetLayerWeight(1, 1);
            _anim.SetLayerWeight(2, 1);
            _anim.SetLayerWeight(3, 0);
            _anim.SetLayerWeight(4, 0);
            _anim.SetLayerWeight(5, 0);
            _anim.SetLayerWeight(6, 0);
        }
        else
        {
            currentDirectionState = PlayerDirectionState.Forward;
            _anim.SetLayerWeight(1, 1);
            _anim.SetLayerWeight(2, 1);
            _anim.SetLayerWeight(3, 0);
            _anim.SetLayerWeight(4, 0);
            _anim.SetLayerWeight(5, 0);
            _anim.SetLayerWeight(6, 0);
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
                if(currentDirectionState == PlayerDirectionState.Forward)
                {
                    currentLeftWeaponState = LeftWeaponState.Attacking3;
                }
                break;
            case "Nov_Left_Sword_Attack4_Override":
                if (currentDirectionState == PlayerDirectionState.Forward)
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
                if(currentDirectionState == PlayerDirectionState.Downward)
                {
                    currentLeftWeaponState = LeftWeaponState.Attacking1;
                }
                break;
        }

        switch (currentAnimationName_LeftArmUp)
        {
            case "Nov_Left_Sword_Attack_Up_Override":
                if (currentDirectionState == PlayerDirectionState.Upward)
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
                if (currentDirectionState == PlayerDirectionState.Downward)
                {
                    currentRightWeaponState = RightWeaponState.Attacking1;
                }
                break;
        }

        switch (currentAnimationName_RightArmUp)
        {
            case "Nov_Right_Sword_Attack_Up_Override":
                if (currentDirectionState == PlayerDirectionState.Upward)
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
            case "Nov_Jump_Fall":
                currentMobilityState = PlayerMobilityState.JumpFalling;
                break;
            case "Nov_Jump_Rise":
                //currentMobilityState = PlayerMobilityState.JumpRising;
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
        StartCoroutine(DashCooldown());
    }
}
