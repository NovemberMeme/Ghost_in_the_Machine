using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Damage
{
    public int damageAmount;
    public VerticalAttackDirection verticalAttackDirection;
    public HorizontalAttackDirection horizontalAttackDirection;
    public string layer;
    public float stunningDuration;
    public Element damageElement;
}
