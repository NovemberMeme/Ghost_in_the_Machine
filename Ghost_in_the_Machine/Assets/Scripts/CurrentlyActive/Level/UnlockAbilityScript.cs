﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockAbilityScript : MonoBehaviour
{
    public enum UnlockedAbility
    {
        RightBlock,
        LeftBlock,
        Dash,
        DoubleJump,
        PogoJump,
        PhaseShift,
        TimeLapse,
        Heal,
        Mana
    }

    [SerializeField] private UnlockedAbility unlockedAbility;

    Player player;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.name == "Player")
        {
            player = coll.GetComponent<Player>();

            UnlockAbility();
        }
    }

    public void UnlockAbility()
    {
        switch (unlockedAbility)
        {
            case UnlockedAbility.RightBlock:
                player.UnlockedBlock = true;
                break;
            case UnlockedAbility.LeftBlock:
                break;
            case UnlockedAbility.Dash:
                player.UnlockedDash = true;
                break;
            case UnlockedAbility.DoubleJump:
                player.UnlockedDoubleJump = true;
                break;
            case UnlockedAbility.PogoJump:
                player.UnlockedPogoJump = true;
                break;
            case UnlockedAbility.PhaseShift:
                player.UnlockedPhase = true;
                break;
            case UnlockedAbility.TimeLapse:
                player.UnlockedTimeLapse = true;
                break;
            case UnlockedAbility.Heal:
                break;
        }

        UIManager.Instance.DisplayUnlockText(unlockedAbility.ToString());
        Destroy(gameObject);
    }
}
