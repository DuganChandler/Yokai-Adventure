using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;

    [Header("Status Conditions")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Yokai yokai)
    {
        // Revive
        if (revive || maxRevive) {
            if (yokai.HP > 0) {
                return false;
            }

            if (revive) {
                yokai.IncreaseHP(yokai.MaxHp / 2);
            } else if (maxRevive) {
                yokai.IncreaseHP(yokai.MaxHp);
            }

            yokai.CureStatus();

            return true;
        }

        if (yokai.HP == 0) {
            return false;
        }

        // Restore HP
        if (restoreMaxHP || hpAmount > 0) {
            if (yokai.HP == yokai.MaxHp) {
                return false;
            }

            if (restoreMaxHP) {
                yokai.IncreaseHP(yokai.MaxHp);
            } else {
                yokai.IncreaseHP(hpAmount);
            }
        }

        // Recover Status
        if (recoverAllStatus || status != ConditionID.none) {
            if (yokai.Status == null && yokai.VolitileStatus == null) {
                return false;
            }

            if (recoverAllStatus) {
                yokai.CureStatus();
                yokai.CureVolitileStatus();
            } else {
                if (yokai.Status.Id == status) {
                    yokai.CureStatus();
                } else if (yokai.VolitileStatus.Id != ConditionID.none && yokai.VolitileStatus.Id == status) {
                    yokai.CureVolitileStatus();
                } else {
                    return false;
                }
            }
        }

        // Restore PP
        if (restoreMaxPP) {
            yokai.Abilities.ForEach(a => a.IncreasePP(a.Base.PP));
        } else if (ppAmount > 0) {
            yokai.Abilities.ForEach(a => a.IncreasePP(ppAmount));
        }

        return true;
    }
}
