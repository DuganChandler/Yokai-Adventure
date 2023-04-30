using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Items/Create new TM or HM")]
public class TmItem : ItemBase
{
    [SerializeField] AbilitiesBase ability;
    [SerializeField] bool isHM;

    public override string Name => base.Name + $": {ability.Name}";

    public override bool Use(Yokai yokai)
    {
        // Learning ability is handled from inventory ui, if it was leraned then this shouold return true
        return yokai.HasAbility(ability);
    }

    public bool CanBeTaught(Yokai yokai) {
        return yokai.Base.LearnableByItems.Contains(ability);
    }

    public override bool IsReusable => isHM;

    public override bool CanUseInBattle => false;
    public AbilitiesBase Ability => ability;
    public bool IsHM => isHM;
}
