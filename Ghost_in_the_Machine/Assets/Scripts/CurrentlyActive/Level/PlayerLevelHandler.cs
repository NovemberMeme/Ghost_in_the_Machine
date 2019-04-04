using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerLevelHandler : MonoBehaviour
{
    Player player;

    Rigidbody2D rb;

    TextMeshProUGUI scoreDisplay;

    [SerializeField] float movspeed = 10f;
    [SerializeField] private Vector2 res;
    [SerializeField] protected int score;

    public void SetHp(int hp)
    {
        player.Health += hp;
    }

    public Vector2 GetCheck()
    {
        return res;
    }

    void Awake()
    {
        if (PlayerPrefs.HasKey("posX"))
        {
            Vector2 posWake = new Vector2(PlayerPrefs.GetFloat("posX"), PlayerPrefs.GetFloat("posY"));
            score = PlayerPrefs.GetInt("score");
            transform.position = posWake;
        }

        player = GetComponent<Player>();
    }

    void Start()
    {
        //scoreDisplay = GameObject.Find("Score_dis").GetComponent<TextMeshProUGUI>();
        rb = GetComponent<Rigidbody2D>();
        res = transform.position;
    }

    void Update()
    {
        //DisplayScore();
        Death();
        //Movement();

        if (Input.GetKeyDown(KeyCode.T))
        {
            //res = new Vector2(52, 36);
            res = new Vector2(234, 42);
            player.transform.position = res;
        }
    }

    void DisplayScore()
    {
        //scoreDisplay.text = "Score: " + score;
    }

    void Death()
    {
        if (player.IsDead)
        {
            transform.position = res;
            player.Health = 5;
        }
    }

    void Movement()
    {
        float h = Input.GetAxisRaw("Horizontal") * movspeed;
        Vector2 movement = new Vector2(h, 0);
        movement.y = rb.velocity.y;

        rb.velocity = movement;

        if (Input.GetKeyDown(KeyCode.W))
        {
            rb.AddForce(Vector2.up * 22, ForceMode2D.Impulse);
        }
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        //CHECKPOINT
        if (col.gameObject.tag == "check")
        {
            res = col.gameObject.transform.position;
            //SAVES VALUES if player were to quit
            //values can be loaded
            PlayerPrefs.SetFloat("posX", res.x);
            PlayerPrefs.SetFloat("posY", res.y);
            PlayerPrefs.SetInt("score", score);

            PlayerPrefs.Save();

        }
        //TESTING collectibles
        else if (col.gameObject.tag == "collectible")
        {
            score++;
        }
    }
}
