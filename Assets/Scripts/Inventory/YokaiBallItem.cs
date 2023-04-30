using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new YokaiBall")]
public class YokaiBallItem : ItemBase
{
    [SerializeField] float catchRateModifier = 1;
    
    public override bool Use(Yokai yokai)
    {
        return true;
    }

    public override bool CanUseOutsideBattle => false;
    public float CatchRateModifier => catchRateModifier;
}
