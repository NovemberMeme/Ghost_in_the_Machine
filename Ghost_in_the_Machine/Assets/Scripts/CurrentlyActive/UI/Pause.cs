using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{

    [SerializeField] private bool Paused;
    [SerializeField] private GameObject PauseMenu;

    // Start is called before the first frame update
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (Paused == false)
            {
                Paused = true;
                Cursor.visible = true;
                PauseMenu.SetActive(true);
                Time.timeScale = 0;
            }

            else
            {
                PauseMenu.SetActive(false);
                Cursor.visible = false;
                Paused = false;
                Time.timeScale = 1;

            }
        }
    }
}
