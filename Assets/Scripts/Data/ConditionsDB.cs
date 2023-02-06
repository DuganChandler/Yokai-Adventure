using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB 
{

    public static void Init() {
        foreach (var kvp in Conditions) {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>() 
    {
        {
            ConditionID.psn,
            new Condition() 
            {
                Name = "Poison",
                StartMessage = "has been Poisoned",
                // lambda function
                OnAfterTurn = (Yokai yokai) => 
                {
                    yokai.UpdateHP(yokai.MaxHp / 8);
                    yokai.StatusChanges.Enqueue($"{yokai.Base.Name} was damaged by Poison.");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition() 
            {
                Name = "Burn",
                StartMessage = "has been Burned",
                // lambda function
                OnAfterTurn = (Yokai yokai) => 
                {
                    yokai.UpdateHP(yokai.MaxHp / 16);
                    yokai.StatusChanges.Enqueue($"{yokai.Base.Name} was damaged by Burn.");
                }
            }
        },
        {
            ConditionID.par,
            new Condition() 
            {
                Name = "Paralyzed",
                StartMessage = "has been Paralyzed",
                // lambda function
                OnBeforeAbility = (Yokai yokai) => 
                {
                    //Checking if yokai can get out of paralysis
                    if (Random.Range(1, 5) == 1) {
                        yokai.StatusChanges.Enqueue($"{yokai.Base.Name} is Paralyzed and cannot move.");
                        return false; 
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition() 
            {
                Name = "Freeze",
                StartMessage = "has been Frozen",
                // lambda function
                OnBeforeAbility = (Yokai yokai) => 
                {
                    //Checking if yokai can get out of paralysis
                    if (Random.Range(1, 5) == 1) {
                        yokai.CureStatus();
                        yokai.StatusChanges.Enqueue($"{yokai.Base.Name} broke out of being Frozen.");
                        return true; 
                    }
                    yokai.StatusChanges.Enqueue($"{yokai.Base.Name} is Frozen and cannot move.");
                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition() 
            {
                Name = "Sleep",
                StartMessage = "has been put to Sleep",
                // lambda function
                OnStart = (Yokai yokai) =>
                { 
                    // Sleep for 1-3 turns
                    yokai.StatusTime = Random.Range(1, 4);
                    Debug.Log($"Will be asleep for {yokai.StatusTime} turns.");
                },
                OnBeforeAbility = (Yokai yokai) => 
                {
                    if (yokai.StatusTime <= 0) {
                        yokai.CureStatus();
                        yokai.StatusChanges.Enqueue($"{yokai.Base.Name} hase woken up!");
                        return true;
                    }
                    yokai.StatusTime--;
                    yokai.StatusChanges.Enqueue($"{yokai.Base.Name} is Asleep.");
                    return false;
                }
            }
        },
        // Volatile Conditions
        {
            ConditionID.confusion,
            new Condition() 
            {
                Name = "Confusion",
                StartMessage = "has been Confused",
                // lambda function
                OnStart = (Yokai yokai) =>
                { 
                    // Confused for 1-4 turns
                    yokai.VolitileStatusTime = Random.Range(1, 5);
                    Debug.Log($"Will be asleep for {yokai.VolitileStatusTime} turns.");
                },
                OnBeforeAbility = (Yokai yokai) => 
                {
                    if (yokai.VolitileStatusTime <= 0) {
                        yokai.CureVolitileStatus();
                        yokai.StatusChanges.Enqueue($"{yokai.Base.Name} kicked out of its Confusion!");
                        return true;
                    }
                    yokai.VolitileStatusTime--;
                    // 50% chance to do an ability
                    if (Random.Range(1, 3) == 1){
                        return true;
                    }

                    // Hurt by confusion
                    yokai.StatusChanges.Enqueue($"{yokai.Base.Name} is Confused.");
                    yokai.UpdateHP(yokai.MaxHp / 8);
                    yokai.StatusChanges.Enqueue($"{yokai.Base.Name} hurt itslef in its Confusion.");
                    return false;
                }
            }   
        },
    };

    public static float GetStatusBonus(Condition condition) {
        if (condition == null) {
            return 1f;
        } else if (condition.Id == ConditionID.slp || condition.Id == ConditionID.frz) {
            return 2f;
        } else if (condition.Id == ConditionID.par || condition.Id == ConditionID.psn || condition.Id == ConditionID.brn) {
            return 1.5f;
        } else {
            return 1f;
        }
    }
}   

public enum ConditionID {
    none,
    psn, 
    brn, 
    slp, 
    par, 
    frz,
    confusion
}
