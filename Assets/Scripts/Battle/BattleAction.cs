using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType { Ability, SwitchYokai, UseItem, Run }

public class BattleAction
{
    public ActionType Type { get; set; }
    public BattleUnit User { get; set; }
    public BattleUnit Target { get; set; }

    public Ability Ability { get; set; } // For Performing Abilities
    public Yokai SelectedYokai { get; set; } // For Switching

    public bool IsInvalid { get; set; }

    public int Priority => (Type == ActionType.Ability) ? Ability.Base.Priority : 999;
}
