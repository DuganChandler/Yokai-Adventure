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
}