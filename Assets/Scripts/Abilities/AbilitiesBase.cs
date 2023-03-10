using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Abilities", menuName = "Yokai/Create new Ability", order = 0)]
public class AbilitiesBase : ScriptableObject 
{
    [SerializeField] new string name;

    [TextArea]
    [SerializeField] string description; 

    [SerializeField] Element element;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHits;
    [SerializeField] int pp;
    [SerializeField] int priority; 
    [SerializeField] AbilityCategory category;
    [SerializeField] AbilityEffects effects;
    [SerializeField] AbilityTarget target; 
    [SerializeField] List<Secondaries> secondaries; 

    public string Name {
        get { return name; }
    }

    public Element Element {
        get { return element; }
    }

    public int Power {
        get { return power; }
    }

    public int PP {
        get { return pp; }
    }

    public int Accuracy {
        get { return accuracy; }
    }

    public bool AlwaysHits {
        get { return alwaysHits; }
    }

    public int Priority {
        get { return priority; }
    }

    public AbilityCategory Category {
        get { return category; }
    }

    public AbilityEffects Effects {
        get { return effects; }
    }

    public AbilityTarget Target {
        get { return target; }
    }

    public List<Secondaries> Secondaries {
        get { return secondaries; }
    }
}

[System.Serializable]
public class AbilityEffects 
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status; 
    [SerializeField] ConditionID volitileStatus; 

    public List<StatBoost> Boosts {
        get { return boosts; }
    }

    public ConditionID Status {
        get { return status; }
    }

    public ConditionID VolitileStatus {
        get { return volitileStatus; }
    }
}

[System.Serializable]
public class Secondaries : AbilityEffects
{
    [SerializeField] int chance;
    [SerializeField] AbilityTarget target;

    public int Chance {
        get { return chance; }
    }

    public AbilityTarget Target {
        get { return target; }
    }
}

[System.Serializable]
public class StatBoost {
    public Stat stat;
    public int boost;
}

public enum Element {
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Dark,
    Steel,
    Fairy
}

public enum AbilityCategory {
    Physical,
    Special,
    Status
}

public enum AbilityTarget {
    Foe,
    Self
}