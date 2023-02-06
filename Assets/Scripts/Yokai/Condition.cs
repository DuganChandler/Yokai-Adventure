using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Condition
{
    public ConditionID Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }

    public Action<Yokai> OnStart { get; set; }
 
    // use func to return a value (bool is return)
    public Func<Yokai, bool> OnBeforeAbility { get; set; }

    public Action<Yokai> OnAfterTurn { get; set; }
}
