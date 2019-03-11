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

    private void Awake()
    {
        _instance = this;
    }

    public TMP_Text playerCoinCountText;
    public Image selectionImg;
    public TMP_Text coinCountText;

    public List<Image> lifeUnits = new List<Image>();

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
        }
    }
}
