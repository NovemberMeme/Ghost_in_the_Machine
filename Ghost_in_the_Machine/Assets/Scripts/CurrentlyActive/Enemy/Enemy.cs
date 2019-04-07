using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    protected enum EnemySkin
    {
        SoddenSoldier,
        WistfulWarrior,
        MadMage,
        SorrowfulStrider,
        NullifiedKnight,
        BitterBrute,
        Bulwark,
        Regulus
    }

    protected enum EnemyControlState
    {
        Idling,
        Patrolling,
        Chasing,
        Attacking
    }

    protected enum EnemyMobilityState
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
        Stunned
    }

    protected enum EnemyChaseState
    {
        NotChasing,
        ChasingToward,
        ChasingForward,
        ChasingBackward,
        ChasingStill
    }

    protected enum EnemyLeftWeaponState
    {
        Idling,
        Parrying,
        Blocking,
        Attacking1,
        Attacking2,
        Attacking3,
        PowerAttacking1,
        ComboAttacking1,
        ComboAttacking2,
        ComboAttacking3,
        ProjectileAttacking1,
        Casting
    }

    protected enum EnemyRightWeaponState
    {
        Idling,
        Parrying,
        Blocking,
        Attacking1,
        Attacking2,
        Attacking3,
        PowerAttacking1,
        ComboAttacking1,
        ComboAttacking2,
        ComboAttacking3,
        Casting
    }

    protected enum EnemyDirectionState
    {
        Forward,
        Upward,
        Downward
    }

    [Header("Enum States: ")]
    [SerializeField] protected EnemySkin currentEnemySkin;
    [SerializeField] protected EnemyControlState currentEnemyControlState;
    [SerializeField] protected EnemyMobilityState currentEnemyMobilityState = EnemyMobilityState.Standing;
    [SerializeField] protected EnemyChaseState currentEnemyChaseState;
    [SerializeField] protected EnemyLeftWeaponState currentEnemyLeftWeaponState;
    [SerializeField] protected EnemyRightWeaponState currentEnemyRightWeaponState;
    [SerializeField] protected EnemyDirectionState currentEnemyDirectionState;
    [SerializeField] protected VerticalAttackDirection currentVerticalAttackDirectionState = VerticalAttackDirection.AttackingForward;
    [SerializeField] protected HorizontalAttackDirection currentHorizontalAttackDirectionState;

    public VerticalAttackDirection CurrentVerticalAttackDirectionState
    {
        get { return currentVerticalAttackDirectionState; }
    }

    public HorizontalAttackDirection CurrentHorizontalAttackDirectionState
    {
        get { return currentHorizontalAttackDirectionState; }
    }

    //public GameObject coin;
    //public int enemyCoins;

    [SerializeField] protected bool isJumpingEnemy = false;

    // The list of known moves dictates which moves in the animator this enemy is allowed to access
    // Minimum and maximum random cooldowns in between moves differs greatly per enemy

    [Header("Attack stats: ")]
    [SerializeField] protected bool isAttacking = false;
    [SerializeField] protected int currentAction;
    [SerializeField] protected List<int> knownActions = new List<int>();
    [SerializeField] protected List<int> moveSet = new List<int>();
    [SerializeField] protected List<List<int>> moveSets = new List<List<int>>();
    //[SerializeField] private List<int> allMoves = new List<int>();



    [SerializeField] protected float moveCooldownMin; //RENAME TO ACTION COOLDOWN?
    [SerializeField] protected float moveCooldownMax; //RENAME TO ACTION COOLDOWN?
    [SerializeField] protected float actionCooldown;
    [SerializeField] protected float chaseTimer = 0;
    [SerializeField] protected bool nextActionSet = false;

    [Header("Combo stats: ")]
    [SerializeField] protected bool comboSet = false;
    [SerializeField] protected int comboChance = 30;

    // Idling

    [Header("Idle stats: ")]
    [SerializeField] protected bool enemyIdleSet = false;
    [SerializeField] protected float minIdle;
    [SerializeField] protected float maxIdle;
    [SerializeField] protected float idleDuration;
    [SerializeField] protected float idleTimer = 0;
    [SerializeField] protected bool idleDurationSet = false;

    // Patrolling

    [Header("Patrol stats: ")]
    [SerializeField] protected float minPatrol;
    [SerializeField] protected float maxPatrol;
    [SerializeField] protected float patrolDuration;
    [SerializeField] protected float patrolTimer = 0;
    [SerializeField] protected bool patrolDurationSet = false;

    // ChasingStill

    [Header("Chase stats: ")]
    [SerializeField] protected float chasingStillMin = 1.0f;
    [SerializeField] protected float chasingStillMax = 2.0f;
    [SerializeField] protected bool movingTowardsPlayer = true;
    [SerializeField] protected int chaseForwardChance = 30;
    protected float chasingStillDuration = 0;
    protected float chasingStillTimer = 0;
    protected bool chasingStillDurationSet = false;

    // ChasingForward

    [Header("Chasing forward stats: ")]
    [SerializeField] protected float chasingForwardMin = 1.0f;
    [SerializeField] protected float chasingForwardMax = 2.0f;
    protected float chasingForwardDuration = 0;
    protected float chasingForwardTimer = 0;
    protected bool chasingForwardDurationSet = false;

    // ChasingBackward

    [Header("Chasing backward stats: ")]
    [SerializeField] protected float chasingBackwardMin = 1.0f;
    [SerializeField] protected float chasingBackwardMax = 2.0f;
    protected float chasingBackwardDuration = 0;
    protected float chasingBackwardTimer = 0;
    protected bool chasingBackwardDurationSet = false;

    // The Move function checks on update whether the player is near enough on the x axis to warrant chasing
    // Potentially update this to include the y axis so that enemies vertically on the same region as the player don't randomly chase them

    [Header("Distance stats: ")]
    [SerializeField] protected bool isChasing = false;
    [SerializeField] protected float distance;
    [SerializeField] protected float xDistance;
    [SerializeField] protected float chaseTriggerLength = 10.0f;
    [SerializeField] protected Vector3 playerDirection;

    [SerializeField] protected float chaseStillLength = 1.0f;
    [SerializeField] protected float chaseForwardLengthMax = 0.5f;
    [SerializeField] protected float chaseBackwardLengthMax = 2.0f;

    [Header("Subtypes: ")]

    [Header("Blocker stats: ")]
    [SerializeField] protected bool isBlocker;
    [SerializeField] protected float exposedDuration = 2;
    [SerializeField] protected float exposureTimer = 0;

    [Header("Dasher stats: ")]
    [SerializeField] protected bool isDasher;
    [SerializeField] protected float dashCooldownMin;
    [SerializeField] protected float dashCooldownMax;
    [SerializeField] protected float dashTimer;
    [SerializeField] protected bool dashCooldownSet;
    [SerializeField] protected int dashBackwardChance = 50;
    [SerializeField] protected int dashBackwardChanceCurrent;

    [Header("Jumper stats: ")]
    [SerializeField] protected bool isJumper;
    [SerializeField] protected float jumpCooldownMin;
    [SerializeField] protected float jumpCooldownMax;
    [SerializeField] protected float jumpTimer;
    [SerializeField] protected bool jumpCooldownSet;

    // Checks if it trigger-enters a wall on either side or trigger-exits a cliff on either side
    // This is to automatically flip the enemy and prevent them from running off cliffs or hitting walls
    // Can also automatically tell the enemy to jump over walls

    protected RaycastHit2D rightWall;
    protected RaycastHit2D leftWall;
    protected RaycastHit2D rightLedge;
    protected RaycastHit2D leftLedge;

    [SerializeField] protected Player player;

    public virtual void Attack()
    {

    }

    public override void Start()
    {
        base.Start();
        player = GameObject.Find("Player").GetComponent<Player>();
        faceDirection = 1;

        if (isBlocker)
        {
            _anim.SetBool("Blocker", true);
        }
    }

    public override void Update()
    {
        UpdateDirection();

        CheckGround();

        ChangeSkin();

        CheckSides();

        CheckPlayer();

        ImplementControlState();
        ImplementMobilityState();
        ImplementChaseState();
        ImplementNonStates();
        ImplementLeftWeaponState();
        ImplementRightWeaponState();
        ImplementCombatValues();

        ChangeState();

        //if(isJumpingEnemy)
        //    Debug.Log(_rigid.velocity.x);
    }

    public override void FixedUpdate()
    {
        Move();
    }

    public override void GetHit(Damage dmg)
    {
        if (isDead || !canBeDamaged || dmg.layer != "Sword")
            return;

        if(currentHorizontalAttackDirectionState == dmg.horizontalAttackDirection)
        {
            TakeDamage(dmg);
            return;
        }

        if(currentEnemyRightWeaponState == EnemyRightWeaponState.Blocking)
        {
            Block(dmg);
        }
        else
        {
            TakeDamage(dmg);
        }
    }

    public virtual void CheckPlayer()
    {
        distance = Vector3.Distance(transform.localPosition, player.transform.localPosition);
        xDistance = Mathf.Abs(player.transform.position.x - transform.position.x);

        if (distance > chaseTriggerLength)
        {
            isChasing = false;
            _anim.SetBool("EnemyChasing", false);
            currentEnemyControlState = EnemyControlState.Idling;
        }

        if (distance < chaseTriggerLength)
        {
            isChasing = true;
            _anim.SetBool("EnemyChasing", true);

            if (currentEnemyControlState != EnemyControlState.Attacking)
            {
                currentEnemyControlState = EnemyControlState.Chasing;
            }
        }
    }

    public override void Move()
    {
        _anim.SetFloat("VerticalSpeed", _rigid.velocity.y);
        _anim.SetFloat("Moving", Mathf.Abs(movementSpeed/origMoveSpeed));

        if(!isStopped && !isDead)
        {
            _rigid.velocity = new Vector2(moveDirection * movementSpeed * Time.deltaTime, _rigid.velocity.y);
        }
        else if(isStopped || isDead)
        {
            _rigid.velocity = Vector3.zero;
        }

        //_anim.SetFloat("Moving", Mathf.Abs(_rigid.velocity.x));
        //_anim.SetFloat("VerticalSpeed", _rigid.velocity.y);
    }

    protected override void UpdateDirection()
    {
        if (faceDirection > 0)
        {
            transform.localScale = new Vector3(origScale.x, origScale.y, origScale.z);
            currentHorizontalAttackDirectionState = HorizontalAttackDirection.AttackingRightward;
        }
        else if (faceDirection < 0)
        {
            transform.localScale = new Vector3(-origScale.x, origScale.y, origScale.z);
            currentHorizontalAttackDirectionState = HorizontalAttackDirection.AttackingLeftward;
        }

        if (!isDashing && !isChasing)
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

    public virtual void FacePlayer()
    {
        playerDirection = player.transform.position - transform.position;

        if (playerDirection.x > 0)
        {
            faceDirection = 1;
        }
        else
        {
            faceDirection = -1;
        }

        if (movingTowardsPlayer)
        {
            moveDirection = faceDirection;
        }
        else
        {
            moveDirection = -faceDirection;
        }
    }

    public void CheckSides()
    {
        //Check for walls

        rightWall = Physics2D.Raycast(new Vector2(transform.position.x + ((colliderSize.x/2) + 0.5f), transform.position.y + (colliderSize.y/2) + colliderSize.y), Vector2.down, 1.2f * colliderSize.y, layerMask);
        Debug.DrawRay(new Vector2(transform.position.x + ((colliderSize.x / 2) + 0.5f), transform.position.y + (colliderSize.y / 2) + colliderSize.y), Vector2.down * 1.2f * colliderSize.y, Color.yellow);

        if (rightWall.collider != null)
        {
            if (!isChasing)
            {
                moveDirection = -1;
            }
            else
            {
                //if(isJumpingEnemy && isGrounded && canJump)
                //{
                //    Jump();
                //}
            }
        }

        leftWall = Physics2D.Raycast(new Vector2(transform.position.x - ((colliderSize.x / 2) + 0.5f), transform.position.y + (colliderSize.y / 2) + colliderSize.y), Vector2.down, 1.2f * colliderSize.y, layerMask);
        Debug.DrawRay(new Vector2(transform.position.x - ((colliderSize.x / 2) + 0.5f), transform.position.y + (colliderSize.y / 2) + colliderSize.y), Vector2.down * 1.2f * colliderSize.y, Color.blue);

        if (leftWall.collider != null)
        {
            if (!isChasing)
            {
                moveDirection = 1;
            }
            else
            {
                //if (isJumpingEnemy && isGrounded && canJump)
                //{
                //    Jump();
                //}
            }
        }

        //Check for ledges

        rightLedge = Physics2D.Raycast(new Vector2(transform.position.x + 0.7f * ((colliderSize.x / 2) + 0.5f), transform.position.y + colliderSize.y), Vector2.down, 1.2f * colliderSize.y, layerMask);
        Debug.DrawRay(new Vector2(transform.position.x + 0.7f * ((colliderSize.x / 2) + 0.5f), transform.position.y + colliderSize.y), Vector2.down * 1.2f * colliderSize.y, Color.yellow);

        if (rightLedge.collider == null)
        {
            if (!isChasing)
            {
                moveDirection = -1;
            }
            else if(isChasing)
            {
                if (faceDirection == 1)
                {
                    isStopped = true;
                }
                else if (faceDirection == -1)
                {
                    isStopped = false;
                }

                //if (!isJumpingEnemy)
                //{
                //    if (faceDirection == 1)
                //    {
                //        isStopped = true;
                //    }
                //    else if (faceDirection == -1)
                //    {
                //        isStopped = false;
                //    }
                //}
                //else if (isJumpingEnemy)
                //{
                //    if(canJump)
                //    {
                //        isStopped = false;
                //        Jump();
                //    }
                //    else if(!canJump)
                //    {
                //        isStopped = true;
                //    }
                //}
            }
        }

        leftLedge = Physics2D.Raycast(new Vector2(transform.position.x - 0.7f * ((colliderSize.x / 2) + 0.5f), transform.position.y + colliderSize.y), Vector2.down, 1.2f * colliderSize.y, layerMask);
        Debug.DrawRay(new Vector2(transform.position.x - 0.7f * ((colliderSize.x / 2) + 0.5f), transform.position.y + colliderSize.y), Vector2.down * 1.2f * colliderSize.y, Color.blue);

        if (leftLedge.collider == null)
        {
            if (!isChasing)
            {
                moveDirection = 1;
            }
            else if(isChasing)
            {
                if (faceDirection == -1)
                {
                    isStopped = true;
                }
                else if (faceDirection == 1)
                {
                    isStopped = false;
                }

                //if (!isJumpingEnemy)
                //{
                //    if(faceDirection == -1)
                //    {
                //        isStopped = true;
                //    }
                //    else if(faceDirection == 1)
                //    {
                //        isStopped = false;
                //    }
                //}
                //else if (isJumpingEnemy)
                //{
                //    if (canJump)
                //    {
                //        isStopped = false;
                //        Jump();
                //    }
                //    else if (!canJump)
                //    {
                //        isStopped = true;
                //    }
                //}
            }
        }

        if (leftLedge.collider != null && rightLedge.collider != null)
        {
            isStopped = false;
        }
    }

    public override void CheckGround()
    {
        ground = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Vector2.down, colliderSize.y / 5, layerMask);
        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y), Vector2.down * colliderSize.y / 5, Color.cyan);

        if (ground.collider == null)
        {
            isGrounded = false;
            _anim.SetBool("Grounded", false);
            isStopped = false;
        }
        else if (ground.collider != null)
        {
            isGrounded = true;
            _anim.SetBool("Grounded", true);
            canDoubleJump = true;
        }
    }

    public override void Death()
    {
        Destroy(gameObject);
    }

    // Coroutines

    protected override IEnumerator Dashing()
    {
        _anim.SetBool("Dashing", true);
        currentEnemyMobilityState = EnemyMobilityState.Dashing;
        movementSpeed = faceDirection * origMoveSpeed * dashMultiplier;
        isDashing = true;
        canDash = false;

        yield return new WaitForSeconds(dashDuration);

        _anim.SetBool("Dashing", false);
        isDashing = false;
        canDash = true;
    }

    protected override IEnumerator BackDashing()
    {
        _anim.SetBool("BackDashing", true);
        currentEnemyMobilityState = EnemyMobilityState.BackDashing;
        movementSpeed = -faceDirection * origMoveSpeed * dashMultiplier;
        isDashing = true;
        canDash = false;

        yield return new WaitForSeconds(dashDuration);

        _anim.SetBool("BackDashing", false);
        isDashing = false;
        canDash = true;
    }

    protected override IEnumerator JumpDashing()
    {
        _anim.SetBool("Dashing", true);
        currentEnemyMobilityState = EnemyMobilityState.Dashing;
        movementSpeed = faceDirection * origMoveSpeed * dashMultiplier;
        isDashing = true;
        canDash = false;

        yield return new WaitForSeconds(dashDuration);

        _anim.SetBool("Dashing", false);
        isDashing = false;
        canDash = true;
    }

    protected override IEnumerator BackJumpDashing()
    {
        _anim.SetBool("BackDashing", true);
        currentEnemyMobilityState = EnemyMobilityState.BackDashing;
        movementSpeed = -faceDirection * origMoveSpeed * dashMultiplier;
        isDashing = true;
        canDash = false;

        yield return new WaitForSeconds(dashDuration);

        _anim.SetBool("BackDashing", false);
        isDashing = false;
        canDash = true;
    }

    public override IEnumerator Stunned(float dmgStunDuration)
    {
        _anim.SetBool("Stunned", true);
        isStopped = true;
        currentEnemyMobilityState = EnemyMobilityState.Stunned;

        yield return new WaitForSeconds(dmgStunDuration);

        _anim.SetBool("Stunned", false);
        isStopped = false;
        currentEnemyMobilityState = EnemyMobilityState.Standing;
    }

    //-------------------------------------------- Enum States ----------------------------------------------------------

    // Implement the behaviors and functions of each State

    // Enemy Control State

    public virtual void ImplementControlState()
    {
        if (_anim.GetBool("Stunned"))
            return;

        switch (currentEnemyControlState)
        {
            case EnemyControlState.Idling:
                idleTimer += Time.deltaTime;

                movementSpeed = 0;

                if (!idleDurationSet)
                {
                    SetEnemyIdleDuration();
                }

                if (idleTimer >= idleDuration)
                {
                    SetEnemyPatrol();
                }
                break;
            case EnemyControlState.Patrolling:
                patrolTimer += Time.deltaTime;

                if (!patrolDurationSet)
                {
                    SetEnemyPatrolDuration();
                }

                if (patrolTimer >= patrolDuration)
                {
                    SetEnemyIdle();
                }
                break;
            case EnemyControlState.Chasing:
                chaseTimer += Time.deltaTime;

                _anim.SetInteger("CurrentMove", 0);

                if (!isDashing)
                {
                    FacePlayer();
                }

                if (!comboSet)
                {
                    SetEnemyCombo();
                }

                if (!nextActionSet)
                {
                    SetNextEnemyActionCooldown();
                }

                if (chaseTimer >= actionCooldown)
                {
                    comboSet = false;
                    NextRandomAction();
                }

                if (isDasher)
                {
                    dashTimer += Time.deltaTime;

                    if (!dashCooldownSet)
                    {
                        SetRandomDashDirection();
                    }

                    if (dashTimer >= dashCooldown)
                    {
                        SetEnemyDash();
                    }
                }

                if (isJumper)
                {
                    jumpTimer += Time.deltaTime;

                    if (!jumpCooldownSet)
                    {
                        SetRandomEnemyJumpCooldown();
                    }

                    if (jumpTimer >= jumpCooldown)
                    {
                        SetEnemyJump();
                    }
                }
                break;
            case EnemyControlState.Attacking:
                chaseTimer = 0;
                break;
        }
    }

    // Enemy Control State Functions

    // Idle Functions

    public virtual void SetEnemyIdleDuration()
    {
        idleDuration = Random.Range(minIdle, maxIdle);
        idleDurationSet = true;
    }

    public virtual void SetEnemyIdle()
    {
        _anim.SetBool("EnemyPatrolling", false);
        currentEnemyControlState = EnemyControlState.Idling;
        patrolDurationSet = false;
    }

    // Patrol Functions

    public virtual void SetEnemyPatrolDuration()
    {
        patrolDuration = Random.Range(minPatrol, maxPatrol);
        patrolDurationSet = true;
    }

    public virtual void SetEnemyPatrol()
    {
        _anim.SetBool("EnemyPatrolling", true);
        currentEnemyControlState = EnemyControlState.Patrolling;
        idleDurationSet = false;
    }

    // Combo/ Action Functions

    public virtual void SetEnemyCombo()
    {
        int comboChanceCurrent = Random.Range(1, 101);

        if (comboChanceCurrent <= comboChance)
        {
            NextRandomAction();
        }

        comboSet = true;
    }

    public virtual void SetNextEnemyActionCooldown()
    {
        nextActionSet = true;
        actionCooldown = Random.Range(moveCooldownMin, moveCooldownMax);
    }

    public virtual void NextRandomAction()
    {
        nextActionSet = false;

        int actionIndex = Random.Range(0, knownActions.Count);
        currentAction = knownActions[actionIndex];
        _anim.SetInteger("CurrentMove", currentAction);
        chaseTimer = 0;
    }

    // Dash Functions

    public virtual void SetRandomDashDirection()
    {
        dashBackwardChanceCurrent = Random.Range(1, 101);
        dashCooldown = Random.Range(dashCooldownMin, dashCooldownMax);

        dashCooldownSet = true;
    }

    public virtual void SetEnemyDash()
    {
        if (dashBackwardChanceCurrent <= dashBackwardChance)
        {
            if (isGrounded)
            {
                StartCoroutine(Dashing());
            }
            else
            {
                StartCoroutine(JumpDashing());
            }
        }
        else
        {
            if (isGrounded)
            {
                StartCoroutine(BackDashing());
            }
            else
            {
                StartCoroutine(BackJumpDashing());
            }
        }

        dashCooldownSet = false;
        dashTimer = 0;
    }

    // Jump Functions

    public virtual void SetRandomEnemyJumpCooldown()
    {
        jumpCooldown = Random.Range(jumpCooldownMin, jumpCooldownMax);
        jumpCooldownSet = true;
    }

    public virtual void SetEnemyJump()
    {
        Jump();
        jumpCooldownSet = false;
        jumpTimer = 0;
    }

    // Enemy Mobiity State

    public virtual void ImplementMobilityState()
    {
        switch (currentEnemyMobilityState)
        {
            case EnemyMobilityState.Standing:
                movementSpeed = 0;
                break;
            case EnemyMobilityState.Walking:
                movementSpeed = origMoveSpeed * slowMultiplier;
                break;
            case EnemyMobilityState.Running:
                movementSpeed = origMoveSpeed;
                break;
            case EnemyMobilityState.Dashing:
                movementSpeed = origMoveSpeed * dashMultiplier * faceDirection;
                break;
            case EnemyMobilityState.BackDashing:
                movementSpeed = origMoveSpeed * dashMultiplier * -faceDirection;
                break;
            case EnemyMobilityState.Stunned:
                currentEnemyLeftWeaponState = EnemyLeftWeaponState.Idling;
                currentEnemyRightWeaponState = EnemyRightWeaponState.Idling;
                currentEnemyDirectionState = EnemyDirectionState.Forward;
                movementSpeed = 0;
                break;
            //default:
            //    movementSpeed = origMoveSpeed;
            //    break;
        }
    }

    // Enemy Chase State

    public virtual void ImplementChaseState()
    {
        if (_anim.GetBool("Stunned"))
            return;

        switch (currentEnemyChaseState)
        {
            case EnemyChaseState.NotChasing:

                //movementSpeed = origMoveSpeed;
                _anim.SetBool("Slowed", false);

                if (currentEnemyControlState == EnemyControlState.Chasing)
                {
                    currentEnemyChaseState = EnemyChaseState.ChasingToward;
                }
                break;
            case EnemyChaseState.ChasingToward:

                if (!isDashing && !isStopped)
                {
                    movementSpeed = origMoveSpeed;
                    _anim.SetBool("Slowed", false);
                }

                if (xDistance < chaseStillLength)
                {
                    currentEnemyChaseState = EnemyChaseState.ChasingStill;
                }
                break;
            case EnemyChaseState.ChasingStill:
                chasingStillTimer += Time.deltaTime;

                if (!isDashing)
                {
                    movementSpeed = 0;
                    _anim.SetBool("Slowed", false);
                }

                if (xDistance > chaseBackwardLengthMax)
                {
                    currentEnemyChaseState = EnemyChaseState.ChasingToward;
                }

                if (!chasingStillDurationSet)
                {
                    SetChasingStillDuration();
                }
                if (chasingStillTimer >= chasingStillDuration)
                {
                    SetRandomChaseDirection();
                }
                break;
            case EnemyChaseState.ChasingForward:
                chasingForwardTimer += Time.deltaTime;

                if (xDistance > chaseBackwardLengthMax)
                {
                    currentEnemyChaseState = EnemyChaseState.ChasingToward;
                }

                if (!isDashing)
                {
                    if (xDistance < chaseForwardLengthMax || isStopped)
                    {
                        movementSpeed = 0;
                    }
                    else
                    {
                        movementSpeed = origMoveSpeed * slowMultiplier;
                        _anim.SetBool("Slowed", true);
                    }
                }

                if (!chasingForwardDurationSet)
                {
                    SetChasingForwardDuration();
                }
                if (chasingForwardTimer >= chasingForwardDuration)
                {
                    SetChasingStill();
                }
                break;
            case EnemyChaseState.ChasingBackward:
                chasingBackwardTimer += Time.deltaTime;

                if (xDistance > chaseBackwardLengthMax)
                {
                    currentEnemyChaseState = EnemyChaseState.ChasingToward;
                }
                else
                {
                    if (!isDashing && !isStopped)
                    {
                        _anim.SetBool("Slowed", true);
                        movementSpeed = -faceDirection * origMoveSpeed * slowMultiplier;
                    }

                    movingTowardsPlayer = false;
                }

                if (!chasingBackwardDurationSet)
                {
                    SetChasingBackwardDuration();
                }
                if (chasingBackwardTimer >= chasingBackwardDuration)
                {
                    SetChasingStill();
                }
                break;
        }
    }

    // Chasing State Functions

    public virtual void SetChasingStillDuration()
    {
        chasingStillDuration = Random.Range(chasingStillMin, chasingStillMax);
        chasingStillDurationSet = true;
    }

    public virtual void SetRandomChaseDirection()
    {
        int forwardOrBackward = Random.Range(1, 101);
        if (forwardOrBackward <= chaseForwardChance)
        {
            currentEnemyChaseState = EnemyChaseState.ChasingForward;
        }
        else
        {
            currentEnemyChaseState = EnemyChaseState.ChasingBackward;
        }
        chasingStillDurationSet = false;
    }

    public virtual void SetChasingForwardDuration()
    {
        chasingForwardDuration = Random.Range(chasingForwardMin, chasingForwardMax);
        chasingForwardDurationSet = true;
    }

    public virtual void SetChasingBackwardDuration()
    {
        chasingBackwardDuration = Random.Range(chasingBackwardMin, chasingBackwardMax);
        chasingBackwardDurationSet = true;
    }

    public virtual void SetChasingStill()
    {
        currentEnemyChaseState = EnemyChaseState.ChasingStill;
        chasingForwardDurationSet = false;
    }

    // If current state is not the following

    public virtual void ImplementNonStates()
    {
        if (currentEnemyMobilityState != EnemyMobilityState.Dashing && currentEnemyMobilityState != EnemyMobilityState.BackDashing)
        {
            _anim.SetBool("Dashing", false);
            _anim.SetBool("BackDashing", false);
        }

        if (currentEnemyControlState != EnemyControlState.Idling)
        {
            idleTimer = 0;
        }

        if (currentEnemyControlState != EnemyControlState.Patrolling)
        {
            patrolTimer = 0;
        }

        if (currentEnemyControlState != EnemyControlState.Chasing)
        {
            chaseTimer = 0;
            nextActionSet = false;
        }

        if (currentEnemyChaseState != EnemyChaseState.ChasingStill)
        {
            chasingStillTimer = 0;
        }

        if (currentEnemyChaseState != EnemyChaseState.ChasingForward)
        {
            chasingForwardTimer = 0;
        }

        if (currentEnemyChaseState != EnemyChaseState.ChasingBackward)
        {
            chasingBackwardTimer = 0;
            movingTowardsPlayer = true;
        }

        //if(currentEnemyLeftWeaponState != EnemyLeftWeaponState.PowerAttacking1 && currentEnemyRightWeaponState != EnemyRightWeaponState.PowerAttacking1)
        //{
        //    stunDuration = 0;
        //}

        if(currentEnemyRightWeaponState != EnemyRightWeaponState.Idling)
        {
            exposureTimer = 0;
        }
    }

    // Enemy Left Weapon State

    public virtual void ImplementLeftWeaponState()
    {
        switch (currentEnemyLeftWeaponState)
        {
            case EnemyLeftWeaponState.Idling:
                SetLeftCombatValues(0, 0, 0, 0);
                break;
            case EnemyLeftWeaponState.Parrying:
                SetLeftCombatValues(parryStat, 0, 0, 0);
                break;
            case EnemyLeftWeaponState.Blocking:
                SetLeftCombatValues(0, blockStat, 0, 0);
                break;
            case EnemyLeftWeaponState.Attacking1:
                SetLeftCombatValues(0, 0, damageStat, 0);
                break;
            case EnemyLeftWeaponState.Attacking2:
                SetLeftCombatValues(0, 0, damageStat, 0);
                break;
            case EnemyLeftWeaponState.Attacking3:
                SetLeftCombatValues(0, 0, damageStat, 0);
                break;
            case EnemyLeftWeaponState.ComboAttacking1:
                SetLeftCombatValues(0, 0, damageStat, 0);
                break;
            case EnemyLeftWeaponState.ComboAttacking2:
                SetLeftCombatValues(0, 0, damageStat, 0);
                break;
            case EnemyLeftWeaponState.ComboAttacking3:
                SetLeftCombatValues(0, 0, damageStat, 0);
                break;
            case EnemyLeftWeaponState.ProjectileAttacking1:
                SetLeftCombatValues(0, 0, damageStat, 0);
                break;
            case EnemyLeftWeaponState.PowerAttacking1:
                SetLeftCombatValues(0, 0, damageStat, swordStunDuration);
                break;
        }
    }

    // Enemy Right Weapon State

    public virtual void ImplementRightWeaponState()
    {
        switch (currentEnemyRightWeaponState)
        {
            case EnemyRightWeaponState.Idling:
                SetRightCombatValues(0, 0, 0, 0);

                exposureTimer += Time.deltaTime;

                if(exposureTimer >= exposedDuration)
                {
                    _anim.SetTrigger("Right_Parry");
                }

                break;
            case EnemyRightWeaponState.Parrying:
                SetRightCombatValues(parryStat, 0, 0, 0);
                break;
            case EnemyRightWeaponState.Blocking:
                SetRightCombatValues(0, blockStat, 0, 0);
                break;
            case EnemyRightWeaponState.Attacking1:
                SetRightCombatValues(0, 0, damageStat, shieldStunDuration);
                break;
            case EnemyRightWeaponState.Attacking2:
                SetRightCombatValues(0, 0, damageStat, shieldStunDuration);
                break;
            case EnemyRightWeaponState.Attacking3:
                SetRightCombatValues(0, 0, damageStat, shieldStunDuration);
                break;
            case EnemyRightWeaponState.PowerAttacking1:
                SetRightCombatValues(0, 0, damageStat, shieldStunDuration);
                break;
        }

        if(currentEnemyRightWeaponState != EnemyRightWeaponState.Idling)
        {
            exposureTimer = 0;
        }
    }

    // Parry/ block/ attack values are equal to the greater value among both weapons

    public virtual void ImplementCombatValues()
    {
        if (leftParryValue > rightParryValue)
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

        if (leftDamageValue > rightDamageValue)
        {
            damageValue = leftDamageValue;
        }
        else
        {
            damageValue = rightDamageValue;
        }
    }

    public virtual void ChangeState()
    {
        if (_anim.GetBool("EnemyChasing") && currentEnemyControlState != EnemyControlState.Attacking)
        {
            //currentEnemyControlState = EnemyControlState.Chasing;
            enemyIdleSet = false;
        }

        if (!_anim.GetBool("EnemyChasing") && !enemyIdleSet)
        {
            //currentEnemyControlState = EnemyControlState.Idling;
            enemyIdleSet = true;
        }
    }

    // ------------------------------------------------- Animation Event Functions ---------------------------------------------

    //public void SetEnemyIdlingState()
    //{
    //    currentEnemyControlState = EnemyControlState.Idling;
    //}

    //public void SetEnemyPatrollingState()
    //{
    //    currentEnemyControlState = EnemyControlState.Patrolling;
    //}

    //public void SetEnemyChasingState()
    //{
    //    currentEnemyControlState = EnemyControlState.Chasing;
    //    notComboingSet = false;
    //    _anim.SetTrigger("SetChasing");
    //    _anim.ResetTrigger("SetChasing");
    //}

    public void SetEnemyChasingStateIfChasing()
    {
        if(isChasing == true)
        {
            chaseTimer = 0;
            currentEnemyControlState = EnemyControlState.Chasing;
            //notComboingSet = false;
            //_anim.SetTrigger("SetChasing");
            //_anim.ResetTrigger("SetChasing");
            //_anim.SetTrigger("SetChasingDone");
            //_anim.ResetTrigger("SetChasingDone");
            //_anim.SetBool("Attacking", false);
            //Debug.Log("SetChasing Done!");
        }
    }

    // ---------------------------------------------- Skins ---------------------------------------------------

    public void ChangeSkin()
    {
        skeletonMecanim.skeleton.SetSkin(currentEnemySkin.ToString());
    }
}
