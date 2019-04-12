using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("UI Manager is Null!");
            }

            return _instance;
        }
    }

    [SerializeField] private TextMeshProUGUI unlockText;
    [SerializeField] private GameObject buttonToPress;
    [SerializeField] private List<GameObject> buttonsToPress = new List<GameObject>();

    private void Awake()
    {
        _instance = this;
        manaBarOrigScale = manaBar.transform.localScale;
    }

    private void Start()
    {
        buttonToPress = buttonsToPress[0];
        unlockText.text = "T to Restart\n R to load last checkpoint\n Esc to Pause";
        DisplayButtonToPress(0);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.E))
        {
            unlockText.text = "";
            HideAllButtonsToPress();
        }
    }

    [SerializeField] private TMP_Text playerCoinCountText;
    [SerializeField] private Image selectionImg;
    [SerializeField] private TMP_Text coinCountText;

    [SerializeField] private List<Image> lifeUnits = new List<Image>();

    [SerializeField] private Image manaBar;
    [SerializeField] private Vector2 manaBarOrigScale;
    [SerializeField] private float maxMana = 12;

    public void UpdateShop(int coinCount)
    {
        playerCoinCountText.text = "" + coinCount.ToString() + "G";
    }

    public void UpdateShopSelection(int yPos)
    {
        selectionImg.rectTransform.anchoredPosition = new Vector2(selectionImg.rectTransform.anchoredPosition.x, yPos);
    }

    public void UpdateCoinCount(int count)
    {
        coinCountText.text = "" + count.ToString() + "G";
    }

    public void UpdateLives(int livesLeft)
    {
        for(int i = 0; i < 4; i++)
        {
            if(i >= livesLeft)
            {
                lifeUnits[i].enabled = false;
            }
            else
            {
                lifeUnits[i].enabled = true;
            }
        }
    }

    public void UpdateMana(float mana)
    {
        manaBar.transform.localScale = new Vector2((mana / maxMana) * manaBarOrigScale.x, manaBarOrigScale.y);
    }

    public virtual void DisplayUnlockText(string unlockedAbility)
    {
        switch (unlockedAbility)
        {
            case "Heal":
                unlockText.text = "Holding E Spends Mana to Heal";
                DisplayButtonToPress(0);
                break;
            case "Mana":
                unlockText.text = "Attack enemies to regain Mana";
                DisplayButtonToPress(2);
                break;
            case "RightBlock":
                unlockText.text = "Block or Parry Enemy Slashes with your Shield";
                DisplayButtonToPress(1);
                break;
            case "LeftBlock":
                unlockText.text = "Block or Parry Enemy Stabs with your Sword";
                DisplayButtonToPress(2);
                break;
            case "Dash":
                unlockText.text = "Unlocked " + unlockedAbility + "!\n Dash in your moving direction or\n Stand Still to Dash Backwards";
                DisplayButtonToPress(3);
                break;
            case "DoubleJump":
                unlockText.text = "Unlocked " + unlockedAbility + "!";
                break;
            case "PhaseShift":
                unlockText.text = "Unlocked " + unlockedAbility + "!";
                break;
            case "TimeLapse":
                unlockText.text = "Unlocked " + unlockedAbility + "!\n Press F without blocking with any weapons to Time Lapse";
                break;

        }


        StartCoroutine(RemoveUnlockText());
    }

    public virtual IEnumerator RemoveUnlockText()
    {
        yield return new WaitForSeconds(5);

        unlockText.text = "";
    }

    // --------------------------------------------------- Button to Press Functions -----------------------------------------------------

    public void HideAllButtonsToPress()
    {
        foreach (var item in buttonsToPress)
        {
            item.SetActive(false);
        }
    }

    public void DisplayButtonToPress(int index)
    {
        HideAllButtonsToPress();
        buttonsToPress[index].SetActive(true);
    }
}
