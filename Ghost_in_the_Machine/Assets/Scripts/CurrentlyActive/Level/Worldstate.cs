using UnityEngine;
using UnityEngine.SceneManagement;

public class Worldstate : MonoBehaviour
{
    private GameObject continueBut;
    private GameObject resetBut;
    private GameObject panel;

    private bool checker;

    void Awake ()
    {
        if (SceneManager.GetActiveScene().name == "Title")
        {
            panel = GameObject.Find("Panel");
            continueBut = GameObject.Find("Continue");
            resetBut = GameObject.Find("Reset");
        }
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Title")
        {
            panel.SetActive(false);

            if (continueBut != null)
            {
                checker = true;
            }
            else
            {
                checker = false;
            }
        }
    }

    public void ConfirmNewGame ()
    {
        if (PlayerPrefs.HasKey("posX"))
        {
            panel.SetActive(true);
        } else
        {
            SceneManager.LoadScene("Prot");
        }
    }
    
    public void hidePan()
    {
        if (checker == true)
        {
            panel.SetActive(false);
        }
    }

    public void NewGame ()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Prot");
    }

    public void Continue ()
    {
        SceneManager.LoadScene("Prot");
    }

    public void loadTite()
    {
        SceneManager.LoadScene("Title");
    }

    void Update ()
    {
        if (checker == true)
        {
            if (PlayerPrefs.HasKey("posX"))
            {
                continueBut.SetActive(true);
            }
            else
            {
                continueBut.SetActive(false);
            }
        }
    }
}
