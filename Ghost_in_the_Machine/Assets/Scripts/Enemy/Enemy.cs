using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
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

    // Minimum and Maximum random durations for Idling and Patrolling

    public float minIdle;
    public float maxIdle;
    public float minPatrol;
    public float maxPatrol;

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

    public virtual void Move()
    {
        distance = Vector3.Distance(transform.localPosition, player.transform.localPosition);
        xDistance = Mathf.Abs(player.transform.position.x - transform.position.x);

        if(distance > chaseTriggerLength)
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
            _anim.SetBool("Chasing", true);

            if(!isAttacking)
                FacePlayer();
        }
        else
        {
            _anim.SetBool("Chasing", false);
        }

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

        rightWall = Physics2D.Raycast(new Vector2(transform.position.x + ((_collider.size.x/2) + 0.5f), transform.position.y + (_collider.size.y/2)), Vector2.down, _collider.size.y, layerMask);
        Debug.DrawRay(new Vector2(transform.position.x + ((_collider.size.x / 2) + 0.5f), transform.position.y + (_collider.size.y / 2)), Vector2.down * _collider.size.y, Color.yellow);

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

        leftWall = Physics2D.Raycast(new Vector2(transform.position.x - ((_collider.size.x / 2) + 0.5f), transform.position.y + (_collider.size.y / 2)), Vector2.down, _collider.size.y, layerMask);
        Debug.DrawRay(new Vector2(transform.position.x - ((_collider.size.x / 2) + 0.5f), transform.position.y + (_collider.size.y / 2)), Vector2.down * _collider.size.y, Color.blue);

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

        rightLedge = Physics2D.Raycast(new Vector2(transform.position.x + 0.7f * ((_collider.size.x / 2) + 0.5f), transform.position.y), Vector2.down, _collider.size.y, layerMask);
        Debug.DrawRay(new Vector2(transform.position.x + 0.7f * ((_collider.size.x / 2) + 0.5f), transform.position.y), Vector2.down * _collider.size.y, Color.yellow);

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

        leftLedge = Physics2D.Raycast(new Vector2(transform.position.x - 0.7f * ((_collider.size.x / 2) + 0.5f), transform.position.y), Vector2.down, _collider.size.y, layerMask);
        Debug.DrawRay(new Vector2(transform.position.x - 0.7f * ((_collider.size.x / 2) + 0.5f), transform.position.y), Vector2.down * _collider.size.y, Color.blue);

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
        float length = (_collider.size.y / 2) - colliderOffsetY + 0.3f;
        ground = Physics2D.Raycast(transform.position, Vector2.down, length, layerMask);
        Debug.DrawRay(transform.position, Vector2.down * length, Color.cyan);

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
}
