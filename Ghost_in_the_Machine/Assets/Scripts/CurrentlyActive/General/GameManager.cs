using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    PlayerLevelHandler playerLevelHandler;
    Player player;
    public static GameManager Instance
    {
        get
        {
            if(_instance == null)
            {
                Debug.LogError("GameManager is null!");
            }

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
        playerLevelHandler = GameObject.Find("Player").GetComponent<PlayerLevelHandler>();
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //SceneManager.LoadScene(0);
            player.Health = 4;
            UIManager.Instance.UpdateLives(player.Health);
            player.IsDead = false;
            player.transform.position = playerLevelHandler.res;
        }
    }

    public bool HasKeyToCastle { get; set; }
}
