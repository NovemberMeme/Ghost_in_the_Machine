using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public GameObject shopPanel;

    private Player _player;

    private int currentItemSelected;

    private int currentItemCost;
    public List<int> itemCosts = new List<int>();

    public List<int> selectionPositions = new List<int>();

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.tag == "Player")
        {
            _player = coll.GetComponent<Player>();

            if(_player != null)
            {
                UIManager.Instance.UpdateShop(_player.Coins);
                //_player.canAttack = false;
                SelectItem(0);
            }

            shopPanel.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.tag == "Player")
        {
            _player = coll.GetComponent<Player>();
            //_player.canAttack = true;
            shopPanel.SetActive(false);
        }
    }

    private void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();

        shopPanel.SetActive(false);
    }

    private void Update()
    {
        UIManager.Instance.UpdateShop(_player.Coins);
    }

    public void SelectItem(int item)
    {
        UIManager.Instance.UpdateShopSelection(selectionPositions[item]);
        currentItemSelected = item;
        currentItemCost = itemCosts[item];
    }

    public void BuyItem()
    {
        if(_player.Coins >= currentItemCost)
        {
            _player.Coins -= currentItemCost;
            if(currentItemSelected == 2)
            {
                GameManager.Instance.HasKeyToCastle = true;
            }
        }
        else
        {
            shopPanel.SetActive(false);
        }
    }
}
