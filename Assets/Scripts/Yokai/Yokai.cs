using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Yokai
{
    [Header("Yokai SO")]
    [SerializeField] YokaiBase _base;

    [Header("Yokai Level")]
    [SerializeField] int level;

    public Yokai(YokaiBase yBase, int yLevel) {
        _base = yBase;
        level = yLevel;

        Init();
    }


    public YokaiBase Base {
        get { 
            return _base;
        }
    }

    public int Level { 
        get {
            return level;
        }
    }

    public int EXP {get; set; }
    public int HP { get; set; }
    public List<Ability> Abilities { get; set; } 
    public Ability CurrentAbility { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Condition Status { get; private set; }
    public int StatusTime { get; set; }
    public Condition VolitileStatus { get; private set; }
    public int VolitileStatusTime { get; set; }

    public Queue<string> StatusChanges { get; private set; }
    public event System.Action OnStatusChanged;
    public event System.Action OnHpChanged;

    public void Init() {

        //Generate Moves
        Abilities = new List<Ability>();
        foreach (var ability in Base.LearnableAbilities) {
            if (ability.Level <= Level)
                Abilities.Add(new Ability(ability.Base));

            if (Abilities.Count >= YokaiBase.MaxNumAbilities)
                break;
        }

        EXP = Base.GetExpForLevel(Level);

        CalculateStats();
        HP = MaxHp;

        StatusChanges = new Queue<string>();
        ResetStatBoost();
        Status = null;
        VolitileStatus = null;
    }

    public Yokai(YokaiSaveData saveData) {
        _base = YokaiDB.GetObjectByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        EXP = saveData.exp;

        if (saveData.statusId != null) {
            Status = ConditionsDB.Conditions[saveData.statusId.Value];
        } else {
            Status = null;
        }

        Abilities = saveData.abilities.Select(s => new Ability(s)).ToList();

        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoost();
        VolitileStatus = null;
    }

    public YokaiSaveData GetSaveData() {
        var saveData = new YokaiSaveData() {
            name = Base.name,
            hp = HP,
            level = Level,
            exp = EXP,
            statusId = Status?.Id,
            abilities = Abilities.Select(a => a.GetSaveData()).ToList()
        };

        return saveData;
    }

    void CalculateStats() {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.MAttack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.MDefense, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);

        int oldMaxHP = MaxHp;
        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + Level;

        if (oldMaxHP != 0)
            HP += MaxHp - oldMaxHP;
    }

    void ResetStatBoost() {
        StatBoosts = new Dictionary<Stat, int>() {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.MAttack, 0},
            {Stat.MDefense, 0},
            {Stat.Speed, 0},
            {Stat.Accuracy, 0},
            {Stat.Evasion, 0}
        };
    }

    int GetStat(Stat stat) {
        int statVal = Stats[stat];

        // APPLY STAT BOOST
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f};

        if (boost >= 0) {
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        } else {
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);
        }

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts) {
        foreach (var statBoost in statBoosts) {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);     

            if (boost > 0) {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            } else {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }
        }
    }

    public bool CheckForLevelUp() {
        if (EXP >= Base.GetExpForLevel(level + 1)) {
            ++level;
            CalculateStats();
            return true;
        }
        return false;
    }

    public LearnableAbilities GetLearnableAbilityAtCurrentLevel() {
        return Base.LearnableAbilities.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnAbility(AbilitiesBase abilityToLearn) {
        if (Abilities.Count > YokaiBase.MaxNumAbilities) {
            return;
        }

        Abilities.Add(new Ability(abilityToLearn));
    }

    public bool HasAbility(AbilitiesBase abilityToCheck) {
        return Abilities.Count(a => a.Base == abilityToCheck) > 0;
    }

    public Evolution CheckForEvolution()
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredLevel <= level && e.RequiredLevel != 0);
    }

    public Evolution CheckForEvolution(ItemBase item)
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredItem == item);
    }

    public void Evolve(Evolution evolution)
    {
        _base = evolution.EvolvesInto;
        CalculateStats();
    }

    public int Attack {
        get { return GetStat(Stat.Attack); }
    }

    public int MAttack {
        get { return GetStat(Stat.MAttack); }
    }

    public int Defense {
        get { return GetStat(Stat.Defense); }
    }

    public int  MDefense{
        get { return GetStat(Stat.MDefense); }
    }

    public int Speed {
        get { return GetStat(Stat.Speed); }
    }

    public int MaxHp { get; private set; }

    public DamageDetails TakeDamage(Ability ability, Yokai attacker) {
        float crit = 1f;
        if (Random.value * 100f <= 6.25) {
            crit = 2f;
        }

        float element = ElementChart.GetEffectiveness(ability.Base.Element, this.Base.Element1) * ElementChart.GetEffectiveness(ability.Base.Element, this.Base.Element2);

        var damageDetails = new DamageDetails() 
        {
            ElementEffectiveness = element,
            Crit = crit,
            Defeated = false
        };

        float attack = (ability.Base.Category == AbilityCategory.Special) ? attacker.MAttack : attacker.Attack;
        float defense = (ability.Base.Category == AbilityCategory.Special) ? MDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * element * crit;
        float a = (2 * attacker.Level + 10) / 250f;
        float d= a * ability.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        DecreaseHP(damage);

        return damageDetails; 
    }

    public void DecreaseHP(int damage) {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        OnHpChanged?.Invoke();
    }

    public void IncreaseHP(int amount) {
        HP = Mathf.Clamp(HP + amount, 0, MaxHp);
        OnHpChanged?.Invoke();
    }

    public void SetStatus(ConditionID conditionId) {
        if (Status != null) {
            return;
        }

        Status = ConditionsDB.Conditions[conditionId];
        // null conditional operator so does not crash if null
        Status?.OnStart?.Invoke(this); 
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}.");
        OnStatusChanged?.Invoke();
    }

    public void SetVolitileStatus(ConditionID conditionId) {
        if (VolitileStatus != null) {
            return;
        }

        VolitileStatus = ConditionsDB.Conditions[conditionId];
        // null conditional operator so does not crash if null
        VolitileStatus?.OnStart?.Invoke(this); 
        StatusChanges.Enqueue($"{Base.Name} {VolitileStatus.StartMessage}.");
    }

    public void CureStatus() {
        Status  = null;
        OnStatusChanged?.Invoke();
    }

    public void CureVolitileStatus() {
        VolitileStatus  = null;
    }

    public Ability GetRandomAbility() {
        var abilitesWithPP = Abilities.Where(x => x.PP > 0).ToList();
        
        int r = Random.Range(0, abilitesWithPP.Count);
        return abilitesWithPP[r];
    }

    public void Heal() {
        Debug.Log("Healed");
        HP = MaxHp;
        OnHpChanged?.Invoke();
    }


    public void OnBattleOver() {
        VolitileStatus = null;
        ResetStatBoost(); 
    }

    public void OnAfterTurn() {
        // Only call this func if not null (null condition operator)
        Status?.OnAfterTurn?.Invoke(this);
        VolitileStatus?.OnAfterTurn?.Invoke(this);
    }

    public bool OnBeforeAbility() {
        bool canPerformMove = true;
        if (Status?.OnBeforeAbility != null) {
            if (!Status.OnBeforeAbility(this)) {
                canPerformMove = false;
            }
        }

        if (VolitileStatus?.OnBeforeAbility != null) {
            if (!VolitileStatus.OnBeforeAbility(this)) {
                canPerformMove = false;
            }
        }
        return canPerformMove;
    }
}

public class DamageDetails
{
    public bool Defeated { get; set; }
    public float Crit { get; set; }
    public float ElementEffectiveness { get; set; }
}

[System.Serializable]
public class YokaiSaveData {
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusId;
    public string name;
    public List<AbilitySaveData> abilities;
}