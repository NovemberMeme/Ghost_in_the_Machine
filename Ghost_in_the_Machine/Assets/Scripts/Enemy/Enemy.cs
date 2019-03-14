using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackDirectionState
{
    AttackingForward,
    AttackingUpward,
    AttackingDownward
}

public class Enemy : Character
{
    public enum EnemyControlState
    {
        Idling,
        Patrolling,
        Chasing,
        Attacking
    }

    public enum EnemyChaseState
    {
        NotChasing,
        ChasingToward,
        ChasingForward,
        ChasingBackward,
        ChasingStill
    }

    public enum EnemyLeftWeaponState
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

    public enum EnemyRightWeaponState
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

    public enum EnemyDirectionState
    {
        Forward,
        Upward,
        Downward
    }

    public EnemyControlState currentEnemyControlState;
    public EnemyChaseState currentEnemyChaseState;
    public EnemyLeftWeaponState currentEnemyLeftWeaponState;
    public EnemyRightWeaponState currentEnemyRightWeaponState;
    public EnemyDirectionState currentEnemyDirectionState;
    public AttackDirectionState currentAttackDirectionState = AttackDirectionState.AttackingForward;

    public GameObject coin;
    public int enemyCoins;

    public bool isJumpingEnemy = false;

    // The list of known moves dictates which moves in the animator this enemy is allowed to access
    // Minimum and maximum random cooldowns in between moves differs greatly per enemy

    public bool isAttacking = false;
    public int currentMove;
    public List<int> knownMoves = new List<int>();
    //[SerializeField] private List<int> allMoves = new List<int>();
    public float moveCooldownMin;
    public float moveCooldownMax;
    [SerializeField] private float moveCooldown;
    [SerializeField] private float chaseTimer = 0;
    private bool nextMoveSet = false;

    private bool notComboingSet = false;
    [SerializeField] private bool isComboing = false;
    [SerializeField] private int comboLengthMax = 2;
    [SerializeField] private int comboLengthCurrent = 0;
    [SerializeField] private int comboChance = 30;

    // Idling

    public bool enemyIdleSet = false;
    public float minIdle;
    public float maxIdle;
    [SerializeField] private float idleDuration;
    [SerializeField] private float idleTimer = 0;
    [SerializeField] private bool idleDurationSet = false;

    // Patrolling

    public float minPatrol;
    public float maxPatrol;
    [SerializeField] private float patrolDuration;
    [SerializeField] private float patrolTimer = 0;
    [SerializeField] private bool patrolDurationSet = false;

    // ChasingStill

    [SerializeField] private float chasingStillMin = 1.0f;
    [SerializeField] private float chasingStillMax = 2.0f;
    private float chasingStillDuration = 0;
    private float chasingStillTimer = 0;
    private bool chasingStillDurationSet = false;

    // Animation Event Replacements

    [SerializeField] private bool callChasingFunctionSet = false;
    [SerializeField] private bool callAttackingFunctionSet = false;

    // ChasingForward

    [SerializeField] private float chasingForwardMin = 1.0f;
    [SerializeField] private float chasingForwardMax = 2.0f;
    private float chasingForwardDuration = 0;
    private float chasingForwardTimer = 0;
    private bool chasingForwardDurationSet = false;

    // ChasingBackward

    [SerializeField] private float chasingBackwardMin = 1.0f;
    [SerializeField] private float chasingBackwardMax = 2.0f;
    private float chasingBackwardDuration = 0;
    private float chasingBackwardTimer = 0;
    private bool chasingBackwardDurationSet = false;

    // The Move function checks on update whether the player is near enough on the x axis to warrant chasing
    // Potentially update this to include the y axis so that enemies vertically on the same region as the player don't randomly chase them

    public bool isChasing = false;
    [SerializeField] private float distance;
    [SerializeField] private float xDistance;
    public float chaseTriggerLength = 10.0f;
    public Vector3 playerDirection;

    [SerializeField] private float chaseStillLength = 1.0f;
    [SerializeField] private float chaseForwardLengthMax = 0.5f;
    [SerializeField] private float chaseBackwardLengthMax = 2.0f;

    // Checks if it trigger-enters a wall on either side or trigger-exits a cliff on either side
    // This is to automatically flip the enemy and prevent them from running off cliffs or hitting walls
    // Can also automatically tell the enemy to jump over walls

    private RaycastHit2D rightWall;
    private RaycastHit2D leftWall;
    private RaycastHit2D rightLedge;
    private RaycastHit2D leftLedge;

    public Player player;

    public virtual void Attack()
    {

    }

    public override void Start()
    {
        base.Start();
        player = GameObject.Find("Player").GetComponent<Player>();
        direction = 1;
        //_collider.size = colliderSizeMultiplier * _mesh.size;
        //_collider.offset = new Vector2(0, 0 + colliderOffsetY);
    }

    public override void Update()
    {
        base.Update();

        CheckSides();

        CheckPlayer();

        ImplementAnimationEventReplacements();

        ImplementState();
        ChangeState();

        //if(isJumpingEnemy)
        //    Debug.Log(_rigid.velocity.x);
    }

    public override void FixedUpdate()
    {
        Move();
    }

    public override void Damage(Damage damage)
    {
        if (isDead || !canBeDamaged || damage.layer != "Sword")
            return;

        health -= 1;
        //_anim.SetTrigger("Hit");
        canBeDamaged = false;
        StartCoroutine(ResetCanBeDamaged());

        if (health <= 0)
        {
            isDead = true;

            Death();

            //GameObject droppedCoin = Instantiate(coin, transform.position, Quaternion.identity);
            //droppedCoin.GetComponent<Coin>().coinAmount = enemyCoins;

            Destroy(gameObject, flashTime*2);
        }
        else
        {
            FlashRed();
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
        }
        else if(distance < chaseTriggerLength && !_anim.GetBool("EnemyChasing"))
        {
            isChasing = true;
            _anim.SetBool("EnemyChasing", true);

            if (!isAttacking)
                FacePlayer();
        }
    }

    public virtual void Move()
    {
        if(!isDead)
        {
            if(!isStopped)
            {
                if(_isDashing)
                {
                    _rigid.velocity = new Vector3(movementSpeed * dashMultiplier * Time.deltaTime * direction, _rigid.velocity.y);
                }
                else if ((!isChasing || (isChasing && xDistance > chaseStillLength)))
                {
                    _rigid.velocity = new Vector2(direction * movementSpeed * Time.deltaTime, _rigid.velocity.y);
                }
                else
                {
                    _rigid.velocity = new Vector2(0, _rigid.velocity.y);
                }
            }
            else if(isStopped)
            {
                _rigid.velocity = new Vector2(0, _rigid.velocity.y);
            }
        }

        _anim.SetFloat("Moving", Mathf.Abs(_rigid.velocity.x));
        _anim.SetFloat("VerticalSpeed", _rigid.velocity.y);
    }

    public virtual void FacePlayer()
    {
        playerDirection = player.transform.position - transform.position;

        if (playerDirection.x > 0)
        {
            direction = 1;
        }
        else
        {
            direction = -1;
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
                direction = -1;
            }
            else
            {
                if(isJumpingEnemy && isGrounded && canJump)
                {
                    Jump();
                }
            }
        }

        leftWall = Physics2D.Raycast(new Vector2(transform.position.x - ((colliderSize.x / 2) + 0.5f), transform.position.y + (colliderSize.y / 2) + colliderSize.y), Vector2.down, 1.2f * colliderSize.y, layerMask);
        Debug.DrawRay(new Vector2(transform.position.x - ((colliderSize.x / 2) + 0.5f), transform.position.y + (colliderSize.y / 2) + colliderSize.y), Vector2.down * 1.2f * colliderSize.y, Color.blue);

        if (leftWall.collider != null)
        {
            if (!isChasing)
            {
                direction = 1;
            }
            else
            {
                if (isJumpingEnemy && isGrounded && canJump)
                {
                    Jump();
                }
            }
        }

        //Check for ledges

        rightLedge = Physics2D.Raycast(new Vector2(transform.position.x + 0.7f * ((colliderSize.x / 2) + 0.5f), transform.position.y + colliderSize.y), Vector2.down, 1.2f * colliderSize.y, layerMask);
        Debug.DrawRay(new Vector2(transform.position.x + 0.7f * ((colliderSize.x / 2) + 0.5f), transform.position.y + colliderSize.y), Vector2.down * 1.2f * colliderSize.y, Color.yellow);

        if (rightLedge.collider == null)
        {
            if (!isChasing)
            {
                direction = -1;
            }
            else if(isChasing)
            {
                if (!isJumpingEnemy)
                {
                    if (direction == 1)
                    {
                        isStopped = true;
                    }
                    else if (direction == -1)
                    {
                        isStopped = false;
                    }
                }
                else if (isJumpingEnemy)
                {
                    if(canJump)
                    {
                        isStopped = false;
                        Jump();
                    }
                    else if(!canJump)
                    {
                        isStopped = true;
                    }
                }
            }
        }

        leftLedge = Physics2D.Raycast(new Vector2(transform.position.x - 0.7f * ((colliderSize.x / 2) + 0.5f), transform.position.y + colliderSize.y), Vector2.down, 1.2f * colliderSize.y, layerMask);
        Debug.DrawRay(new Vector2(transform.position.x - 0.7f * ((colliderSize.x / 2) + 0.5f), transform.position.y + colliderSize.y), Vector2.down * 1.2f * colliderSize.y, Color.blue);

        if (leftLedge.collider == null)
        {
            if (!isChasing)
            {
                direction = 1;
            }
            else if(isChasing)
            {
                if(!isJumpingEnemy)
                {
                    if(direction == -1)
                    {
                        isStopped = true;
                    }else if(direction == 1)
                    {
                        isStopped = false;
                    }
                }
                else if (isJumpingEnemy)
                {
                    if (canJump)
                    {
                        isStopped = false;
                        Jump();
                    }
                    else if (!canJump)
                    {
                        isStopped = true;
                    }
                }
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
            canDash = true;
        }
    }

    public override void Jump()
    {
        if (isGrounded && canJump)
        {
            _rigid.velocity = new Vector2(_rigid.velocity.x, jumpVelocity);
            _anim.SetTrigger("Jump");
            StartCoroutine(Jumping());
        }
    }

    public override void DoubleJump()
    {
        if(canDoubleJump)
        {
            _rigid.velocity = new Vector2(_rigid.velocity.x, jumpVelocity);
            canDoubleJump = false;
            _anim.SetTrigger("Jump");
        }
    }

    public virtual void NextRandomMove()
    {
        int moveIndex = Random.Range(0, knownMoves.Count);
        currentMove = knownMoves[moveIndex];
        _anim.SetInteger("CurrentMove", currentMove);
        _anim.SetBool("Attacking", true);
        currentEnemyControlState = EnemyControlState.Attacking;
        Debug.Log("Attacking!");
    }

    public virtual void Projectile_Straight()
    {
        Instantiate(projectilePrefab, projectilePos.position, Quaternion.identity);
    }

    public virtual void Projectile_Horizontal()
    {
        GameObject go = Instantiate(projectile_horizontal, projectilePos.position, Quaternion.identity);
        go.GetComponent<Projectile_Horizontal>().shootDirection = -direction;
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

    //-------------------------------------------- Enum States ----------------------------------------------------------

    // Animation Event Replacement Implementation

    public void ImplementAnimationEventReplacements()
    {
        if (callChasingFunctionSet)
        {
            SetEnemyChasingStateIfChasing();
            callChasingFunctionSet = false;
        }

        if (callAttackingFunctionSet)
        {
            SetEnemyAttackingState();
            callAttackingFunctionSet = false;
        }
    }

    // Implement the behaviors and functions of each State

    public void ImplementState()
    {
        // Enemy Control State

        switch (currentEnemyControlState)
        {
            case EnemyControlState.Idling:
                movementSpeed = 0;
                idleTimer += Time.deltaTime;
                if (!idleDurationSet)
                {
                    idleDuration = Random.Range(minIdle, maxIdle);
                    idleDurationSet = true;
                }
                if(idleTimer >= idleDuration)
                {
                    _anim.SetBool("EnemyPatrolling", true);
                    currentEnemyControlState = EnemyControlState.Patrolling;
                    idleDurationSet = false;
                }
                break;
            case EnemyControlState.Patrolling:
                movementSpeed = origMoveSpeed;
                patrolTimer += Time.deltaTime;
                if (!patrolDurationSet)
                {
                    patrolDuration = Random.Range(minPatrol, maxPatrol);
                    patrolDurationSet = true;
                }
                if(patrolTimer >= patrolDuration)
                {
                    _anim.SetBool("EnemyPatrolling", false);
                    currentEnemyControlState = EnemyControlState.Idling;
                    patrolDurationSet = false;
                }
                break;
            case EnemyControlState.Chasing:
                _anim.SetInteger("CurrentMove", 0);
                movementSpeed = origMoveSpeed;
                chaseTimer += Time.deltaTime;

                if (!notComboingSet)
                {
                    int comboChanceCurrent = Random.Range(1, 101);

                    if (comboChanceCurrent <= comboChance)
                    {
                        comboLengthCurrent = Random.Range(1, comboLengthMax);
                    }

                    notComboingSet = true;
                }

                if (comboLengthCurrent > 0)
                {
                    comboLengthCurrent--;
                    NextRandomMove();
                }
                else if (comboLengthCurrent <= 0)
                {
                    notComboingSet = true;

                    if (!nextMoveSet)
                    {
                        moveCooldown = Random.Range(moveCooldownMin, moveCooldownMax);
                        nextMoveSet = true;
                    }
                }

                if (chaseTimer >= moveCooldown)
                {
                    nextMoveSet = false;
                    notComboingSet = false;
                    NextRandomMove();
                }
                break;
            case EnemyControlState.Attacking:
                break;
        }

        // Enemy Chase State

        switch (currentEnemyChaseState)
        {
            case EnemyChaseState.NotChasing:
                if(currentEnemyControlState == EnemyControlState.Chasing)
                {
                    currentEnemyChaseState = EnemyChaseState.ChasingToward;
                }
                break;
            case EnemyChaseState.ChasingToward:
                if(xDistance < chaseStillLength)
                {
                    currentEnemyChaseState = EnemyChaseState.ChasingStill;
                }
                break;
            case EnemyChaseState.ChasingStill:
                chasingStillTimer += Time.deltaTime;
                if(xDistance > chaseBackwardLengthMax)
                {
                    currentEnemyChaseState = EnemyChaseState.ChasingToward;
                }
                if(!chasingStillDurationSet)
                {
                    chasingStillDuration = Random.Range(chasingStillMin, chasingStillMax);
                    chasingStillDurationSet = true;
                }
                if(chasingStillTimer >= chasingStillDuration)
                {
                    int forwardOrBackward = Random.Range(0, 2);
                    if(forwardOrBackward == 1)
                    {
                        currentEnemyChaseState = EnemyChaseState.ChasingForward;
                    }
                    else
                    {
                        currentEnemyChaseState = EnemyChaseState.ChasingBackward;
                    }
                    chasingStillDurationSet = false;
                }
                break;
            case EnemyChaseState.ChasingForward:
                chasingForwardTimer += Time.deltaTime;
                if (xDistance > chaseBackwardLengthMax)
                {
                    currentEnemyChaseState = EnemyChaseState.ChasingToward;
                }
                if(xDistance < chaseForwardLengthMax)
                {
                    movementSpeed = 0;
                }
                else
                {
                    movementSpeed = origMoveSpeed;
                }
                if (!chasingForwardDurationSet)
                {
                    chasingForwardDuration = Random.Range(chasingForwardMin, chasingForwardMax);
                    chasingForwardDurationSet = true;
                }
                if(chasingForwardTimer >= chasingForwardDuration)
                {
                    currentEnemyChaseState = EnemyChaseState.ChasingStill;
                    chasingForwardDurationSet = false;
                }
                break;
            case EnemyChaseState.ChasingBackward:
                chasingBackwardTimer += Time.deltaTime;
                if (xDistance > chaseBackwardLengthMax)
                {
                    currentEnemyChaseState = EnemyChaseState.ChasingToward;
                }
                if (!chasingBackwardDurationSet)
                {
                    chasingBackwardDuration = Random.Range(chasingBackwardMin, chasingBackwardMax);
                    chasingBackwardDurationSet = true;
                }
                if (chasingBackwardTimer >= chasingBackwardDuration)
                {
                    currentEnemyChaseState = EnemyChaseState.ChasingStill;
                    chasingBackwardDurationSet = false;
                }
                break;
        }

        // If current state is not the following

        if(currentEnemyControlState != EnemyControlState.Idling)
        {
            idleTimer = 0;
            movementSpeed = origMoveSpeed;
        }

        if(currentEnemyControlState != EnemyControlState.Patrolling)
        {
            patrolTimer = 0;
        }

        if(currentEnemyControlState != EnemyControlState.Chasing)
        {
            chaseTimer = 0;
            nextMoveSet = false;
        }

        if(currentEnemyChaseState != EnemyChaseState.ChasingStill)
        {
            chasingStillTimer = 0;
        }

        if(currentEnemyChaseState != EnemyChaseState.ChasingForward)
        {
            chasingForwardTimer = 0;
        }

        if(currentEnemyChaseState != EnemyChaseState.ChasingBackward)
        {
            chasingBackwardTimer = 0;
        }

        // Enemy Left Weapon State

        switch (currentEnemyLeftWeaponState)
        {
            case EnemyLeftWeaponState.Idling:
                leftParryValue = 0;
                leftBlockValue = 0;
                leftDamageValue = 0;
                break;
            case EnemyLeftWeaponState.Parrying:
                leftParryValue = 1;
                leftBlockValue = 0;
                leftDamageValue = 0;
                break;
            case EnemyLeftWeaponState.Blocking:
                leftParryValue = 0;
                leftBlockValue = 1;
                leftDamageValue = 0;
                break;
            case EnemyLeftWeaponState.Attacking1:
                leftParryValue = 0;
                leftBlockValue = 0;
                leftDamageValue = 1;
                break;
            case EnemyLeftWeaponState.Attacking2:
                leftParryValue = 0;
                leftBlockValue = 0;
                leftDamageValue = 1;
                break;
            case EnemyLeftWeaponState.Attacking3:
                leftParryValue = 0;
                leftBlockValue = 0;
                leftDamageValue = 1;
                break;
            case EnemyLeftWeaponState.ComboAttacking1:
                leftParryValue = 0;
                leftBlockValue = 0;
                leftDamageValue = 1;
                break;
            case EnemyLeftWeaponState.ComboAttacking2:
                leftParryValue = 0;
                leftBlockValue = 0;
                leftDamageValue = 1;
                break;
            case EnemyLeftWeaponState.ComboAttacking3:
                leftParryValue = 0;
                leftBlockValue = 0;
                leftDamageValue = 1;
                break;
            case EnemyLeftWeaponState.PowerAttacking1:
                leftParryValue = 0;
                leftBlockValue = 0;
                leftDamageValue = 1;
                break;
        }

        // Enemy Right Weapon State

        switch (currentEnemyRightWeaponState)
        {
            case EnemyRightWeaponState.Idling:
                rightParryValue = 0;
                rightBlockValue = 0;
                rightDamageValue = 0;
                break;
            case EnemyRightWeaponState.Parrying:
                rightParryValue = 1;
                rightBlockValue = 0;
                rightDamageValue = 0;
                break;
            case EnemyRightWeaponState.Blocking:
                rightParryValue = 0;
                rightBlockValue = 1;
                rightDamageValue = 0;
                break;
            case EnemyRightWeaponState.Attacking1:
                rightParryValue = 0;
                rightBlockValue = 0;
                rightDamageValue = 1;
                break;
            case EnemyRightWeaponState.Attacking2:
                rightParryValue = 0;
                rightBlockValue = 0;
                rightDamageValue = 1;
                break;
            case EnemyRightWeaponState.Attacking3:
                rightParryValue = 0;
                rightBlockValue = 0;
                rightDamageValue = 1;
                break;
            case EnemyRightWeaponState.PowerAttacking1:
                rightParryValue = 0;
                rightBlockValue = 0;
                rightDamageValue = 1;
                break;
        }

        // parry/ block/ attack values are equal to the greater value among both weapons

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

    public void ChangeState()
    {
        if (_anim.GetBool("EnemyChasing") && !_anim.GetBool("Attacking"))
        {
            currentEnemyControlState = EnemyControlState.Chasing;
            enemyIdleSet = false;
        }

        if (!_anim.GetBool("EnemyChasing") && !enemyIdleSet)
        {
            currentEnemyControlState = EnemyControlState.Idling;
            enemyIdleSet = true;
        }
    }

    // ------------------------------------------------- Animation Event Functions ---------------------------------------------

    public void SetEnemyIdlingState()
    {
        currentEnemyControlState = EnemyControlState.Idling;
    }

    public void SetEnemyPatrollingState()
    {
        currentEnemyControlState = EnemyControlState.Patrolling;
    }

    public void SetEnemyChasingState()
    {
        currentEnemyControlState = EnemyControlState.Chasing;
        notComboingSet = false;
        _anim.SetTrigger("SetChasing");
        _anim.ResetTrigger("SetChasing");
    }

    public void SetEnemyChasingStateIfChasing()
    {
        if(isChasing == true)
        {
            chaseTimer = 0;
            currentEnemyControlState = EnemyControlState.Chasing;
            notComboingSet = false;
            _anim.SetTrigger("SetChasing");
            //_anim.ResetTrigger("SetChasing");
            _anim.SetTrigger("SetChasingDone");
            //_anim.ResetTrigger("SetChasingDone");
            _anim.SetBool("Attacking", false);
            //Debug.Log("SetChasing Done!");
        }
    }

    public void SetEnemyAttackingState()
    {
        currentEnemyControlState = EnemyControlState.Attacking;
    }
}
