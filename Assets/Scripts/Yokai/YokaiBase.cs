using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Yokai", menuName = "Yokai/Create new Yokai")]
public class YokaiBase : ScriptableObject {
    
    [SerializeField] new string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] GameObject linkedPrefab;

    //Affinities
    [SerializeField] YokaiElement element1;
    [SerializeField] YokaiElement element2;

    public static int MaxNumAbilities {get; set; } = 4;

    // Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int mAttack;
    [SerializeField] int defense;
    [SerializeField] int mDefense;
    [SerializeField] int speed;
    [SerializeField] int expYield;
    [SerializeField] int catchRate = 255;

    [SerializeField] GrowthRate growthRate;

    [SerializeField] List<LearnableAbilities> learnableAbilities; 

    public int GetExpForLevel(int level) {
        switch(growthRate) {
            case GrowthRate.Fast:
                return 4 * (level * level * level) / 5;
            case GrowthRate.MediumFast:
                return level * level * level;
            default:
                return -1; 
        }
    }

    public string Name {
        get { return name; }
    }

    public string Description{
        get { return description; }
    }

    public GameObject LinkedPrefab {
        get { return linkedPrefab; }
    }

    public YokaiElement Element1 {
        get { return element1; }
    }

    public YokaiElement Element2 {
        get { return element2; }
    }

    public int MaxHp {
        get { return maxHp; }
    }

    public int Attack {
        get { return attack; }
    }

    public int MAttack {
        get { return mAttack; }
    }

    public int Defense {
        get { return defense; }
    }

    public int MDefense {
        get {return mDefense; }
    }

    public int Speed {
        get { return speed; }
    }

    public List<LearnableAbilities> LearnableAbilities {
        get { return learnableAbilities; }
    }

    public int CatchRate {
        get { return catchRate; }
    }

    public int ExpYield {
        get { return expYield; }
    }

    public GrowthRate GrowthRate {
        get { return growthRate; }
    }
}

[System.Serializable]
public class LearnableAbilities 
{
    [SerializeField] AbilitiesBase abilityBase;
    [SerializeField] int level; 

    public AbilitiesBase Base {
        get { return abilityBase; }
    }

    public int Level {
        get { return level; }
    }
}

public enum YokaiElement {
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

public enum Stat {
    Attack,
    Defense,
    MAttack,
    MDefense,
    Speed,
    // used to boost accuracy, not actual stats
    Accuracy,
    Evasion
}

public enum GrowthRate {
    Fast,
    MediumFast
}

public class ElementChart {
    static float[][] chart =
    {
        //
        /*NOR*/ new float[] {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 0f, 1f, 1f, 0.5f, 1f},
        /*FIR*/ new float[] {1f, 0.5f, 0.5f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 0.5f, 1f, 2f, 1f},
        /*WAT*/ new float[] {1f, 2f, 0.5f, 1f, 0.5f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f, 1f, 1f},
        /*ELE*/ new float[] {1f, 1f, 2f, 0.5f, 0.5f, 1f, 1f, 1f, 0f, 2f, 1f, 1f, 1f, 1f, 0.5f, 1f, 1f, 1f},
        /*GRS*/ new float[] {1f, 0.5f, 2f, 1f, 0.5f, 1f, 1f, 0.5f, 2f, 0.5f, 1f, 0.5f, 2f, 1f, 0.5f, 1f, 0.5f, 1f},
        /*ICE*/ new float[] {1f, 0.5f, 0.5f, 1f, 2f, 0.5f, 1f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f},
        /*FIG*/ new float[] {2f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f, 0.5f, 0.5f, 0.5f, 2f, 0f, 1f, 2f, 2f, 0.5f},
        /*POI*/ new float[] {1f, 1f, 1f, 1f, 2f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 2f, 2f, 0.5f},
        /*GRO*/ new float[] {1f, 2f, 1f, 2f, 0.5f, 1f, 1f, 2f, 1f, 0f, 1f, 0.5f, 2f, 1f, 1f, 1f, 2f, 1f},
        /*FLY*/ new float[] {1f, 1f, 1f, 0.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 1f, 1f, 0.5f, 1f},
        /*PSY*/ new float[] {1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 1f, 1f, 0.5f, 1f, 1f, 1f, 1f, 0f, 0.5f, 1f},
        /*BUG*/ new float[] {1f, 0.5f, 1f, 1f, 2f, 1f, 0.5f, 0.5f, 1f, 0.5f, 2f, 1f, 1f, 0.5f, 1f, 2f, 0.5f, 0.5f},
        /*ROC*/ new float[] {1f, 3f, 1f, 1f, 1f, 2f, 0.5f, 1f, 0.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 0.5f, 1f},
        /*GHO*/ new float[] {0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f, 0.5f, 1f, 1f},
        /*DRA*/ new float[] {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 0f},
        /*DRK*/ new float[] {},
        /*STL*/ new float[] {},
        /*FAI*/ new float[] {}
    };

    public static float GetEffectiveness(Element attackElement, YokaiElement defenseElement) { 
        if (attackElement == Element.None || defenseElement == YokaiElement.None)
            return 1;

        int row = (int)attackElement - 1;
        int col = (int)defenseElement - 1;

        return chart[row][col];
    }
}