using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability
{
    public AbilitiesBase Base { get; set; }
    public int PP { get; set; }

    public Ability(AbilitiesBase aBase) {
        Base = aBase;
        PP = aBase.PP;
    }

    public Ability(AbilitySaveData saveData) {
        Base =  AbilityDB.GetObjectByName(saveData.name);
        PP = saveData.pp;
    }

    public AbilitySaveData GetSaveData() {
        var saveData = new AbilitySaveData() {
            name = Base.name,
            pp = PP
        };

        return saveData;
    }

    public void IncreasePP(int amount) {
        PP = Mathf.Clamp(PP + amount, 0, Base.PP);
    }
}

[System.Serializable]
public class AbilitySaveData {
    public string name;
    public int pp;
}