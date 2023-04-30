using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StarterAssets;
using System.Linq;


public enum BattleState {
    Start,
    ActionSelection,
    AbilitySelection,
    TargetSelection,
    RunningTurn,
    Busy,
    PartyScreen,
    AboutToUse,
    AbilityToForget,
    BattleOver,
    Bag
}

public class BattleSystem : MonoBehaviour
{
    [Header("Yokai Units")]
    [SerializeField] BattleUnit playerUnitSingle;
    [SerializeField] BattleUnit enemyUnitSingle;
    [SerializeField] List<BattleUnit> playerUnitsMulti;
    [SerializeField] List<BattleUnit> enemyUnitsMulti;
    

    [Header("UI")]
    [SerializeField] BattleHud playerHud; 
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] AbilitySelectionUI abilitySelectionUI;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] GameObject singleBattleElements;
    [SerializeField] GameObject doubleBattleElements;

    [Header("Yokai Ball Position")]
    [SerializeField] GameObject captureBall;
    [SerializeField] Transform ballSpawnPoint;

    [Header("Audio")]
    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip trainerBattleMusic;
    [SerializeField] AudioClip battleVictoryMusic;

    // battle over evnet
    public event Action<bool> OnBattleOver;

    // Unit lists
    List<BattleUnit> playerUnits;
    List<BattleUnit> enemyUnits;

    // Battle Actions
    List<BattleAction> actions;

    // state control
    BattleState state;

    // action selection
    int currentAction;
    int currentAbility;
    int currentTarget;
    bool aboutToUseChoice = true;

    // Yokai setup
    YokaiParty playerParty;
    YokaiParty trainerParty;
    Yokai wildYokai;

    // For multi Battles
    int unitCount = 1;
    int actionIndex = 0;
    BattleUnit currentUnit;
    BattleUnit unitTryingToLearn;

    // Trainer Battles
    TrainerControler trainer;
    bool isTrainerBattle = false;
    
    // escape attempts
    int escapeAttempts;

    // player
    ThirdPersonController player;
    
    // learning abilities
    AbilitiesBase abilityToLearn;

    // Switching unit
    BattleUnit unitToSwitch;


    public void StartBattle(YokaiParty playerParty, Yokai wildYokai) {
        this.playerParty = playerParty;
        this.wildYokai = wildYokai;
        player = playerParty.GetComponent<ThirdPersonController>();
        isTrainerBattle = false;

        // Wild is always 1 atm
        unitCount = 1;

        AudioManager.i.PlayMusic(wildBattleMusic);

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(YokaiParty playerParty, YokaiParty trainerParty, int unitCount=1) {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<ThirdPersonController>();
        trainer = trainerParty.GetComponent<TrainerControler>();

        this.unitCount = unitCount;

        AudioManager.i.PlayMusic(trainerBattleMusic);

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle() 
    {
        singleBattleElements.SetActive(unitCount == 1);
        doubleBattleElements.SetActive(unitCount > 1);

        if (unitCount == 1)
        {
            playerUnits = new List<BattleUnit>() { playerUnitSingle };
            enemyUnits = new List<BattleUnit>() { enemyUnitSingle };

        }
        else
        {
            // grab coppies of unit lists
            playerUnits = playerUnitsMulti.GetRange(0, playerUnitsMulti.Count);
            enemyUnits = enemyUnitsMulti.GetRange(0, enemyUnitsMulti.Count);
        }

        // assuming that the player  and enemy units are same
        for (int i = 0; i < playerUnits.Count; i++)
        {
            playerUnits[i].Clear();
            enemyUnits[i].Clear();
        } 

        if (!isTrainerBattle) {
            // Wild Battle (they always have 1)
            playerUnits[0].Setup(playerParty.GetHealthyYokai());
            enemyUnits[0].Setup(wildYokai);

            dialogBox.SetAbilityNames(playerUnits[0].Yokai.Abilities);
            yield return dialogBox.TypeDialog($"A wild {enemyUnits[0].Yokai.Base.Name} appeared!");
        } 
        else 
        {
            // Trainer Battle
            //first set models

            /*
            for (int i = 0; i < playerUnits.Count; i++)
            {
                playerUnits[i].gameObject.SetActive(false);
                enemyUnits[i].gameObject.SetActive(false);
            }
            */

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to Battle!");

            // send out trainer yokai
            var enemyYokais = trainerParty.GetHealthyYokais(unitCount);

            for (int i = 0; i < unitCount; i++)
            {
                enemyUnits[i].gameObject.SetActive(true);
                enemyUnits[i].Setup(enemyYokais[i]);
            }

            string names = String.Join(" and ", enemyYokais.Select(y => y.Base.Name));
            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {names}!");

            // send out player yokai
            var playerYokais = playerParty.GetHealthyYokais(unitCount);

            for (int i = 0; i < unitCount; i++)
            {
                playerUnits[i].gameObject.SetActive(true);
                playerUnits[i].Setup(playerYokais[i]);
            }

            names = String.Join(" and ", playerYokais.Select(y => y.Base.Name));
            yield return dialogBox.TypeDialog($"Go {names}!");
        }

        escapeAttempts = 0;
        partyScreen.Init();

        actions = new List<BattleAction>();

        // since it is the start pass 0
        ActionSelection(0); 
    }


    void BattleOver(bool won) 
    {
        // Set state to battle over
        state = BattleState.BattleOver;

        //short version of a foreach using Linq :)
        playerParty.YokaiList.ForEach(y => y.OnBattleOver());

        // Clear data
        playerUnits.ForEach(u => u.Hud.ClearData());
        enemyUnits.ForEach(u => u.Hud.ClearData());

        // Destory units

        for (int i = 0; i < unitCount; i++)
        {
            if (playerUnits[i].CurrentPlayerModel != null)
            {
                Destroy(playerUnits[i].CurrentPlayerModel);
            }            
        }
  
        OnBattleOver(won); 
    }

    void ActionSelection(int actionIndex) {
        state = BattleState.ActionSelection;

        this.actionIndex = actionIndex;
        currentUnit = playerUnits[actionIndex];

        dialogBox.SetAbilityNames(currentUnit.Yokai.Abilities);

        dialogBox.SetDialog($"Choose an action for {currentUnit.Yokai.Base.Name}.");
        dialogBox.EnableActionSelector(true);
    }

    void OpenBag() {
        state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);

    }

    void OpenPartyScreen() {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true);
    }

    void AbilitySelection() {
        state = BattleState.AbilitySelection;
        dialogBox.EnableDialogText(false);
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableAbilitySelector(true);
    }

    void TargetSelection()
    {
        state = BattleState.TargetSelection;
        currentTarget = 0;
    }

    IEnumerator AboutToUse(Yokai newYokai) 
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newYokai.Base.Name}. Would you like to switch?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseAbilityToForget(Yokai yokai, AbilitiesBase newAbility) {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose an ability you want to forget.");
        abilitySelectionUI.gameObject.SetActive(true);
        // Linq
        abilitySelectionUI.SetAbiliyData(yokai.Abilities.Select(x => x.Base).ToList(), newAbility);
        abilityToLearn = newAbility;
        state = BattleState.AbilityToForget;
    }

    void AddBattleAction(BattleAction action)
    {
        // Add action of particular yokai
        actions.Add(action);

        // Check if all player actions are selected
        if (actions.Count == unitCount)
        {
            // Choose enemy actions
            foreach (var enemyUnit in enemyUnits)
            {
                var randAction = new BattleAction()
                {
                    Type = ActionType.Ability,
                    User = enemyUnit,
                    Target = playerUnits[UnityEngine.Random.Range(0, playerUnits.Count)],
                    Ability = enemyUnit.Yokai.GetRandomAbility(),
                };
                actions.Add(randAction);
            }

            // Sort Actions
            actions = actions.OrderByDescending(a => a.Priority).ThenByDescending(a => a.User.Yokai.Speed).ToList();

            // run actions one by one
            StartCoroutine(RunTurns());
        } 
        else
        {
            // go to action of next yokai if there is one
            ActionSelection(actionIndex + 1);
        }
    }

    IEnumerator RunTurns() 
    {
        state = BattleState.RunningTurn;

        foreach (var action in actions)
        {
            // skip rest of code if the action is invalid
            if (action.IsInvalid) continue;

            if (action.Type == ActionType.Ability)
            {
                yield return RunAbility(action.User, action.Target, action.Ability);
                yield return RunAfterTurn(action.User);
                if (state == BattleState.BattleOver) yield break;

            }
            else if (action.Type == ActionType.SwitchYokai)
            {
                state = BattleState.Busy;
                yield return SwitchYokai(action.User, action.SelectedYokai);
            } 
            else if (action.Type == ActionType.UseItem)
            {
                // this is handled from item screen. jsut go to enemy turn
                dialogBox.EnableActionSelector(false);
            } 
            else if (action.Type == ActionType.Run)
            {
                yield return TryToEscape();
            }

            if (state == BattleState.BattleOver) break;
        }

        if (state != BattleState.BattleOver)
        {
            actions.Clear();
            ActionSelection(0);
        }
    }

    IEnumerator RunAbility(BattleUnit sourceUnit, BattleUnit targetUnit, Ability ability) {
        bool canRunAbility = sourceUnit.Yokai.OnBeforeAbility();

        // if yokai cannot run the ability, stop the coroutine
        if (!canRunAbility) {
            yield return ShowStatusChanges(sourceUnit.Yokai);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Yokai);

        ability.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Yokai.Base.Name} used {ability.Base.Name}");

        if (CheckIfAbilityHits(ability, sourceUnit.Yokai, targetUnit.Yokai)) 
        {
            // Sound fx
            AudioManager.i.PlaySfx(ability.Base.Sound);

            yield return new WaitForSeconds(1f);

            AudioManager.i.PlaySfx(AudioID.Hit);

            if (ability.Base.Category == AbilityCategory.Status) 
            {
                yield return RunAbilityEffects(ability.Base.Effects, sourceUnit.Yokai, targetUnit.Yokai, ability.Base.Target);
            } 
            else 
            {
                var damageDetails = targetUnit.Yokai.TakeDamage(ability, sourceUnit.Yokai);
                yield return targetUnit.Hud.WaitForHPUpdate();
                yield return ShowDamageDetails(damageDetails);
            }

            if (ability.Base.Secondaries != null && ability.Base.Secondaries.Count > 0 && targetUnit.Yokai.HP > 0) {
                foreach (var secondary in ability.Base.Secondaries) {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance) {
                        yield return RunAbilityEffects(secondary, sourceUnit.Yokai, targetUnit.Yokai, secondary.Target);
                    }
                }
            }

            if (targetUnit.Yokai.HP <= 0) {
                yield return HandleYokaiDefeated(targetUnit);
            }
        } else {
            yield return dialogBox.TypeDialog($"{sourceUnit.Yokai.Base.Name}'s attack missed!");
        }
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit) {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        // Status Like burn or poison may defeat yokai after their turn 
        sourceUnit.Yokai.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Yokai);
        yield return sourceUnit.Hud.WaitForHPUpdate();
        if (sourceUnit.Yokai.HP <= 0) {
            yield return HandleYokaiDefeated(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        } 
    }

    IEnumerator RunAbilityEffects(AbilityEffects effects, Yokai source, Yokai target, AbilityTarget abilityTarget) {
            // Stat Boosting
            if (effects.Boosts != null) {
                if (abilityTarget == AbilityTarget.Self) {
                    source.ApplyBoosts(effects.Boosts);
                } else {
                    target.ApplyBoosts(effects.Boosts);
                }
            }

            // Status Conition
            if (effects.Status != ConditionID.none) {
                target.SetStatus(effects.Status);
            }

            // VolitileStatus Conition
            if (effects.VolitileStatus != ConditionID.none) {
                target.SetVolitileStatus(effects.VolitileStatus);
            }

            yield return ShowStatusChanges(source);
            yield return ShowStatusChanges(target);
    }

    bool CheckIfAbilityHits(Ability ability, Yokai source, Yokai target) {

        if (ability.Base.AlwaysHits) {
            return true;
        }

        float abilityAccuracy = ability.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = source.StatBoosts[Stat.Evasion];

        var boostValues = new float[] {1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f};

        if (accuracy > 0){
            abilityAccuracy *= boostValues[accuracy];
        } else {
            abilityAccuracy /= boostValues[-accuracy];
        }

        if (evasion > 0){
            abilityAccuracy /= boostValues[evasion];
        } else {
            abilityAccuracy *= boostValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= abilityAccuracy;
    }
    
    IEnumerator ShowStatusChanges(Yokai yokai) {
        while (yokai.StatusChanges.Count > 0) {
            var message = yokai.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void NextStepsAfterBeingDefeated(BattleUnit defeatedUnit) 
    {
        // Remove action of defeated yokai
        var actionToRemove = actions.FirstOrDefault(a => a.User == defeatedUnit);
        if (actionToRemove != null) actionToRemove.IsInvalid = true;

        if (defeatedUnit.IsPlayerUnit) 
        {
            Destroy(defeatedUnit.CurrentPlayerModel);
            var activeYokai = playerUnits.Select(u => u.Yokai).Where(y => y.HP > 0).ToList();
            var nextYokai = playerParty.GetHealthyYokai(activeYokai);

            if (activeYokai.Count() == 0 && nextYokai == null)
            {
                BattleOver(false);
            }
            else if (nextYokai != null)
            {
                // send out Next yokai
                unitToSwitch = defeatedUnit;
                OpenPartyScreen();
            }
            else if (nextYokai == null && activeYokai.Count > 0)
            {
                // No yokai left to send but can still battle
                playerUnits.Remove(defeatedUnit);
                defeatedUnit.Hud.gameObject.SetActive(false);

                // redirect attacks/actions from defeated yokai to healthy one
                var actionsToChange = actions.Where(a => a.Target == defeatedUnit).ToList();
                actionsToChange.ForEach(a => a.Target = playerUnits.First());
            }
        } 
        else 
        {
            Destroy(defeatedUnit.CurrentEnemyModel);
            if (!isTrainerBattle)
            {
                BattleOver(true);
                return;
            }

            var activeYokai = enemyUnits.Select(u => u.Yokai).Where(y => y.HP > 0).ToList();
            var nextYokai = trainerParty.GetHealthyYokai(activeYokai);

            if (activeYokai.Count() == 0 && nextYokai == null)
            {
                BattleOver(true);
            }
            else if (nextYokai != null)
            {
                

                if (unitCount == 1)
                {
                    // send out Next yokai
                    unitToSwitch = playerUnits[0];
                    StartCoroutine(AboutToUse(nextYokai));
                }  
                else
                {
                    StartCoroutine(SendNextTrainerYokai());
                }
                    
            }
            else if (nextYokai == null && activeYokai.Count > 0)
            {
                // No yokai left to send but can still battle
                enemyUnits.Remove(defeatedUnit);
                defeatedUnit.Hud.gameObject.SetActive(false);

                // redirect attacks/actions from defeated yokai to healthy one
                var actionsToChange = actions.Where(a => a.Target == defeatedUnit).ToList();
                actionsToChange.ForEach(a => a.Target = enemyUnits.First());
            }
            
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails) {
        if (damageDetails.Crit > 1) {
            yield return dialogBox.TypeDialog("A critcal hit !");
        }
        if (damageDetails.ElementEffectiveness > 1f) {
            yield return dialogBox.TypeDialog("It was super Effective!");
        } else if (damageDetails.ElementEffectiveness < 1f) {
            yield return dialogBox.TypeDialog("It was not very Effective!");
        }
    }

    public void HandleUpdate() {
        switch (state) {
            case BattleState.ActionSelection:
                HandleActionSelection();
                break;
            case BattleState.AbilitySelection:
                HandleAbilitySelection();
                break;
            case BattleState.TargetSelection:
                HandleTargetSelection();
                break;
            case BattleState.PartyScreen:
                HandlePartySelection();
                break;
            case BattleState.Bag:
                // in future use state machine using events. Not about to refactor that tbh
                Action onBack = () => 
                {
                    inventoryUI.gameObject.SetActive(false);
                    state = BattleState.ActionSelection;
                };

                Action<ItemBase> onItemUsed = (ItemBase usedItem) => 
                {
                    StartCoroutine(OnItemUsed(usedItem));
                };

                inventoryUI.HandleUpdate(onBack, onItemUsed);
                break;
            case BattleState.AboutToUse:
                HandleAboutToUse();
                break;
            case BattleState.AbilityToForget:
                Action<int> onAbilitySelection = (abilityIndex) => 
                {
                    abilitySelectionUI.gameObject.SetActive(false);
                    if (abilityIndex == YokaiBase.MaxNumAbilities) 
                    {
                        // Don't learn new Ability
                        StartCoroutine(dialogBox.TypeDialog($"{unitTryingToLearn.Yokai.Base.Name} did no learn {abilityToLearn.Name}."));
                    } 
                    else 
                    {
                        // Forget the selected Ability
                        var selectedAbiliy = unitTryingToLearn.Yokai.Abilities[abilityIndex].Base;
                        StartCoroutine(dialogBox.TypeDialog($"{unitTryingToLearn.Yokai.Base.Name} forgot {selectedAbiliy.Name} and lerned {abilityToLearn.Name}."));

                        unitTryingToLearn.Yokai.Abilities[abilityIndex] = new Ability(abilityToLearn);
                    }
                    
                    abilityToLearn = null;
                    unitTryingToLearn = null;
                    state = BattleState.RunningTurn;
                };

                abilitySelectionUI.HandleAbilitySelection(onAbilitySelection);
                break;
        }
    }

    //attack = 0, run = 1, abilityu = 1, items = 3
    void HandleActionSelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            ++currentAction;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            --currentAction;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)){
            currentAction += 2;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)){
            currentAction -= 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z)) {
            switch(currentAction) {
                case 0:
                    // Fight
                    AbilitySelection();
                    break;
                case 1:
                    // Bag
                    //StartCoroutine(RunTurns(BattleAction.UseItem));
                    OpenBag();
                    break;
                case 2:
                    OpenPartyScreen();
                    // yokai
                    break;
                case 3:
                    // Run
                    var action = new BattleAction()
                    {
                        Type = ActionType.Run,
                        User = currentUnit,
                    };
                    AddBattleAction(action);
                    break;
            }
        }
    }

    void HandleAbilitySelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            ++currentAbility;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            --currentAbility;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)){
            currentAbility += 2;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)){
            currentAbility -= 2;
        }

        currentAbility = Mathf.Clamp(currentAbility, 0, currentUnit.Yokai.Abilities.Count - 1);

        dialogBox.UpdateAbilitySelection(currentAbility, currentUnit.Yokai.Abilities[currentAbility]);

        if (Input.GetKeyDown(KeyCode.Z)) 
        {
            var ability = currentUnit.Yokai.Abilities[currentAbility];
            if (ability.PP == 0) return;

            dialogBox.EnableAbilitySelector(false);
            dialogBox.EnableDialogText(true);

            if (enemyUnits.Count > 1) 
            {
                // Target Selection
                TargetSelection();
            }
            else
            {
                var action = new BattleAction()
                {
                    Type = ActionType.Ability,
                    User = currentUnit,
                    Target = enemyUnits[0],
                    Ability = ability
                };
                AddBattleAction(action);
            }

            
        } 
        else if (Input.GetKeyDown(KeyCode.X)) 
        {
            dialogBox.EnableAbilitySelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection(actionIndex);
        }
    }

    void HandleTargetSelection()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ++currentTarget;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            --currentTarget;
        }

        currentTarget = Mathf.Clamp(currentTarget, 0, enemyUnits.Count - 1);

        for (int i = 0; i < enemyUnits.Count; i++)
        {
            enemyUnits[i].SetSelected(i == currentTarget);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            enemyUnits[currentTarget].SetSelected(false);

            var action = new BattleAction()
            {
                Type = ActionType.Ability,
                User = currentUnit,
                Target = enemyUnits[currentTarget],
                Ability = currentUnit.Yokai.Abilities[currentAbility],
            };
            AddBattleAction(action);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            enemyUnits[currentTarget].SetSelected(false);
            AbilitySelection();
        }

    }

    void HandlePartySelection() 
    {
        // action of when yokai is selected
        Action onSelected = () => 
        {
            // set selected member and check hp
            var selectedMember = partyScreen.SelectedMemeber;
            if (selectedMember.HP <= 0) 
            {
                partyScreen.SetMessageText("You cannot sent out a defeated Yokai.");
                return;
            }
            if (playerUnits.Any(p => p.Yokai == selectedMember)) 
            {
                partyScreen.SetMessageText("You cannot switch with an active Yokai.");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.ActionSelection) 
            {
                var action = new BattleAction()
                {
                    Type = ActionType.SwitchYokai,
                    User = currentUnit,
                    SelectedYokai = selectedMember,
                };
                AddBattleAction(action);
            } 
            else 
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchYokai(unitToSwitch, selectedMember, isTrainerAboutToUse));
                unitToSwitch = null;
            }

            partyScreen.CalledFrom = null;
        };

        Action onBack = () => 
        {
            if (playerUnits.Any(u => u.Yokai.HP <= 0)) 
            {
                partyScreen.SetMessageText("You have to choose a Yokai to continue.");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.AboutToUse) 
            {
                StartCoroutine(SendNextTrainerYokai());
            } 
            else 
            {
                ActionSelection(actionIndex);
            }
            partyScreen.CalledFrom = null;
        };

        partyScreen.HandleUpdate(onSelected, onBack);
    }

    void HandleAboutToUse() 
    {
        // decide if you want to switch
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) 
        {
            aboutToUseChoice = !aboutToUseChoice;
        }

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        // if yes select yokai, if now send out trainer yokai
        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true) 
            {
                // Yes 
                OpenPartyScreen();
            } 
            else 
            {
                // No
                StartCoroutine(SendNextTrainerYokai());
            }
        } 
        else if (Input.GetKeyDown(KeyCode.X)) // no
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerYokai());
        }
    }

    // called when an enemy yokai has been defeated
    IEnumerator HandleYokaiDefeated (BattleUnit defeatedUnit) 
    {
        yield return dialogBox.TypeDialog($"{defeatedUnit.Yokai.Base.Name} has been Defeated");
        yield return new WaitForSeconds(2f);

        yield return HandleExpGain(defeatedUnit);
                
        NextStepsAfterBeingDefeated(defeatedUnit);
    }

    IEnumerator HandleExpGain(BattleUnit defeatedUnit)
    {
        if (!defeatedUnit.IsPlayerUnit)
        {
            // playing musics
            bool battleWon = true;
            if (isTrainerBattle) battleWon = trainerParty.GetHealthyYokai() == null;
            if (battleWon) AudioManager.i.PlayMusic(battleVictoryMusic);

            // Exp Gain
            int expYield = defeatedUnit.Yokai.Base.ExpYield;
            int enemyLevel = defeatedUnit.Yokai.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            // calcuate exp gain
            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / (7 * unitCount));

            // give exp to each yokai that is in the current battle
            for (int i = 0; i < unitCount; i++)
            {
                var playerUnit = playerUnits[i];

                playerUnit.Yokai.EXP += expGain;
                yield return dialogBox.TypeDialog($"{playerUnit.Yokai.Base.Name} gained {expGain} experience!");
                yield return playerUnit.Hud.SetExpSmooth();

                // Check Level Up
                while (playerUnit.Yokai.CheckForLevelUp())
                {
                    playerUnit.Hud.SetLevel();
                    yield return dialogBox.TypeDialog($"{playerUnit.Yokai.Base.Name} grew to {playerUnit.Yokai.Level}!");

                    //Try tp Learn New Move
                    var newAbility = playerUnit.Yokai.GetLearnableAbilityAtCurrentLevel();
                    if (newAbility != null)
                    {
                        if (playerUnit.Yokai.Abilities.Count < YokaiBase.MaxNumAbilities)
                        {
                            playerUnit.Yokai.LearnAbility(newAbility.Base);
                            yield return dialogBox.TypeDialog($"{playerUnit.Yokai.Base.Name} learned {newAbility.Base.Name}!");
                            dialogBox.SetAbilityNames(playerUnit.Yokai.Abilities);
                        }
                        else
                        {
                            unitTryingToLearn = playerUnit;
                            yield return dialogBox.TypeDialog($"{playerUnit.Yokai.Base.Name} is trying to learn {newAbility.Base.Name}!");
                            yield return dialogBox.TypeDialog($"But it cannot learn more than {YokaiBase.MaxNumAbilities}.");
                            yield return ChooseAbilityToForget(playerUnit.Yokai, newAbility.Base);
                            yield return new WaitUntil(() => state != BattleState.AbilityToForget);
                            yield return new WaitForSeconds(2f);
                        }
                    }
                    yield return playerUnit.Hud.SetExpSmooth(true);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator SwitchYokai(BattleUnit unitToSwitch, Yokai newYokai, bool isTrainerAboutToUse = false) 
    {
        // make sure the yokai has enough hp
        if (unitToSwitch.Yokai.HP > 0) 
        {
            yield return dialogBox.TypeDialog($"Get back here {unitToSwitch.Yokai.Base.Name}");
            unitToSwitch.CurrentPlayerModel.SetActive(false);
            yield return new WaitForSeconds(2f);
        }

        //playerUnit.CurrentPlayerModel.SetActive(false);
        
        // setup the new yokai
        unitToSwitch.Setup(newYokai);
        
        dialogBox.SetAbilityNames(newYokai.Abilities);
        yield return dialogBox.TypeDialog($"Go {newYokai.Base.Name}!");

        // if enemy trainer needs to send out
        if (isTrainerAboutToUse) {
            StartCoroutine(SendNextTrainerYokai());
        } else {
            state = BattleState.RunningTurn;
        }   
    }

    IEnumerator SendNextTrainerYokai() 
    {
        // Change state
        state = BattleState.Busy;

        // Get active yokai and defeated yokai
        var defeatedUnit = enemyUnits.First(unit => unit.Yokai.HP == 0);
        var activeYokai = enemyUnits.Select(u => u.Yokai).Where(y => y.HP > 0).ToList();

        // Disable the enemy model
        defeatedUnit.CurrentEnemyModel.SetActive(false);

        // get the next yokai the trainer can throw out
        var nextYokai = trainerParty.GetHealthyYokai(activeYokai);
        // Set up the next yokai
        defeatedUnit.Setup(nextYokai);

        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {nextYokai.Base.Name}!");

        // set battle state to running turn
        state = BattleState.RunningTurn;
    }

    IEnumerator OnItemUsed(ItemBase usedItem) {
        // set state and inventory ui
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        // if a capture ball, throw the ball
        if (usedItem is YokaiBallItem) {
            yield return ThrowBall((YokaiBallItem)usedItem);
        }

        // add battle action
        var action = new BattleAction()
        {
            Type = ActionType.UseItem,
            User = currentUnit,
        };
        AddBattleAction(action);
    }

    IEnumerator ThrowBall(YokaiBallItem yokaiBallItem) {
        // set state
        state = BattleState.Busy;

        // if trainer battle do not allow catching
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You cannot steal the enemy's yokai!");
            state = BattleState.RunningTurn;
            yield break;
        }

        // get units
        var playerUnit = playerUnits[0];
        var enemyUnit = enemyUnits[0];

        yield return dialogBox.TypeDialog($"You are trying to capture the Yokai with a {yokaiBallItem.Name.ToUpper()}!");

        var captureBallObject = Instantiate(captureBall, ballSpawnPoint);

        enemyUnit.CurrentEnemyModel.SetActive(false);
        var ballAnimator = captureBallObject.GetComponentInChildren<Animator>();

        int shakeCount = TryToCatchYokai(enemyUnit.Yokai, yokaiBallItem);

        // ball shake animations
        for (int i = 0; i < Mathf.Min(shakeCount, 3); ++i) {
            ballAnimator.SetBool("Shake", true);
            yield return new WaitForSeconds(1f);
            ballAnimator.SetBool("Shake", false);
        }

        // if shakecount is 4 yokai is caught
        if (shakeCount == 4) 
        {
            // Yokai Caught
            yield return dialogBox.TypeDialog($"{enemyUnit.Yokai.Base.Name} was caught!");

            playerParty.AddYokai(enemyUnit.Yokai);
            yield return dialogBox.TypeDialog($"{enemyUnit.Yokai.Base.Name} has been added to your party!");

            Destroy(captureBallObject);
            BattleOver(true);
        } 
        else 
        {
            // Broke out
            yield return new WaitForSeconds(1f);
            Destroy(captureBallObject);
            enemyUnit.CurrentEnemyModel.SetActive(true);
            
            if (shakeCount < 2){
                yield return dialogBox.TypeDialog($"{enemyUnit.Yokai.Base.Name} broke free!");
            } else {
                yield return dialogBox.TypeDialog($"{enemyUnit.Yokai.Base.Name} was almost Caught!");
            }
            Destroy(captureBallObject);
            state = BattleState.RunningTurn;
        }
    }

    // ShakeCount
    int TryToCatchYokai(Yokai yokai, YokaiBallItem yokaiBallItem) {

        // caculate catch rate of yokai
        float a = (3* yokai.MaxHp - 2 * yokai.HP) * yokai.Base.CatchRate * yokaiBallItem.CatchRateModifier * ConditionsDB.GetStatusBonus(yokai.Status) / (3 * yokai.MaxHp);

        if (a >= 255) {
            return 4;
        }

        float b = 1048560 / MathF.Sqrt(MathF.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4) {
            if (UnityEngine.Random.Range(0, 65535) >= b){
                break;
            }
            ++shakeCount;
        }

        return shakeCount;
    }
    IEnumerator TryToEscape() {
        // Set state to busy
        state = BattleState.Busy;

        // If trainer do not let escape
        if (isTrainerBattle) {
            yield return dialogBox.TypeDialog($"You cannot run from trainer battles");
            state = BattleState.RunningTurn;
            yield break;
        }

        // Increment escape attmeps
        escapeAttempts++;

        // Calculate if player can escape
        int playerSpeed = playerUnits[0].Yokai.Speed;
        int enemySpeed = enemyUnits[0].Yokai.Speed;

        if (enemySpeed < playerSpeed) 
        {
            yield return dialogBox.TypeDialog($"Ran away Safely");
            BattleOver(true);
        } 
        else 
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f) {
                yield return dialogBox.TypeDialog($"Ran away Safely!");
                BattleOver(true);
            } else {
                yield return dialogBox.TypeDialog($"Could not Escape!");
                state = BattleState.RunningTurn;
            }
        }
        
    }
}