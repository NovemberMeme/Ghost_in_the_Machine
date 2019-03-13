using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    public enum EnemyControlState
    {
        Idling,
        Patrolling,
        Chasing,
        Attacking
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
    public EnemyLeftWeaponState currentEnemyLeftWeaponState;
    public EnemyRightWeaponState currentEnemyRightWeaponState;
    public EnemyDirectionState currentEnemyDirectionState;

    private int parryValue = 0;
    private int blockValue = 0;
    private int damageValue = 0;

    

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
    private float moveCooldown;
    private float chaseTimer = 0;
    private bool nextMoveSet = false;

    // Idling

    public bool enemyIdleSet = false;
    public float minIdle;
    public float maxIdle;
    [SerializeField] private float idleDuration;
    [SerializeField] private float idleTimer = 0;
    [SerializeField] private bool idleCooldownSet = false;

    // Patrolling

    public float minPatrol;
    public float maxPatrol;
    [SerializeField] private float patrolDuration;
    [SerializeField] private float patrolTimer = 0;
    [SerializeField] private bool patrolCooldownSet = false;

    // The Move function checks on update whether the player is near enough on the x axis to warrant chasing
    // Potentially update this to include the y axis so that enemies vertically on the same region as the player don't randomly chase them

    public bool isChasing = false;
    [SerializeField] private float distance;
    [SerializeField] private float xDistance;
    public float chaseTriggerLength = 10.0f;
    public Vector3 playerDirection;

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

        ImplementState();
        ChangeState();

        //if(isJumpingEnemy)
        //    Debug.Log(_rigid.velocity.x);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        Move();
    }

    public override void Damage()
    {
        if (isDead)
            return;

        health -= 1;
        //_anim.SetTrigger("Hit");
        isHit = true;
        StartCoroutine(ResetInvulnerability());

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
            isHit = false;
            isChasing = false;
        }
        else
        {
            isChasing = true;
        }

        if (isChasing)
        {
            _anim.SetBool("EnemyChasing", true);

            if (!isAttacking)
                FacePlayer();
        }
        else
        {
            _anim.SetBool("EnemyChasing", false);
        }
    }

    public virtual void Move()
    {
        if(!isHit && !isDead)
        {
            if(!isStopped)
            {
                if(_isDashing)
                {
                    _rigid.velocity = new Vector3(movementSpeed * dashMultiplier * Time.deltaTime * direction, _rigid.velocity.y);
                }
                else if ((!isChasing || (isChasing && distance > 0.5f && xDistance > 1.2f)))
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
        //Debug.Log(currentMove);
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

    public void ImplementState()
    {
        switch (currentEnemyControlState)
        {
            case EnemyControlState.Idling:
                movementSpeed = 0;
                idleTimer += Time.deltaTime;
                if (!idleCooldownSet)
                {
                    idleDuration = Random.Range(minIdle, maxIdle);
                    idleCooldownSet = true;
                }
                if(idleTimer >= idleDuration)
                {
                    _anim.SetBool("EnemyPatrolling", true);
                    currentEnemyControlState = EnemyControlState.Patrolling;
                    idleCooldownSet = false;
                }
                break;
            case EnemyControlState.Patrolling:
                patrolTimer += Time.deltaTime;
                movementSpeed = origMoveSpeed;
                if (!patrolCooldownSet)
                {
                    patrolDuration = Random.Range(minPatrol, maxPatrol);
                    patrolCooldownSet = true;
                }
                if(patrolTimer >= patrolDuration)
                {
                    _anim.SetBool("EnemyPatrolling", false);
                    currentEnemyControlState = EnemyControlState.Idling;
                    patrolCooldownSet = false;
                }
                break;
            case EnemyControlState.Chasing:
                _anim.SetInteger("CurrentMove", 0);
                movementSpeed = origMoveSpeed;
                chaseTimer += Time.deltaTime;
                if(!nextMoveSet)
                {
                    moveCooldown = Random.Range(moveCooldownMin, moveCooldownMax);
                    nextMoveSet = true;
                }
                if(chaseTimer >= moveCooldown)
                {
                    NextRandomMove();
                    nextMoveSet = false;
                }
                break;
            case EnemyControlState.Attacking:
                break;
        }

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
    }

    public void ChangeState()
    {
        //if (_anim.GetBool("EnemyChasing"))
        //{
        //    currentEnemyControlState = EnemyControlState.Chasing;
        //    enemyIdleSet = false;
        //}

        //if (!_anim.GetBool("EnemyChasing") && !enemyIdleSet)
        //{
        //    currentEnemyControlState = EnemyControlState.Idling;
        //    enemyIdleSet = true;
        //}
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
    }
}
