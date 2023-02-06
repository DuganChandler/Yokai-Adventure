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
    RunningTurn,
    Busy,
    PartyScreen,
    AboutToUse,
    AbilityToForget,
    BattleOver
}

public enum BattleAction {
    Ability,
    SwitchYokai,
    UseItem,
    Run
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud; 
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] GameObject captureBall;
    [SerializeField] Transform ballSpawnPoint;
    [SerializeField] AbilitySelectionUI abilitySelectionUI;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentAbility;
    int currentMember;
    bool aboutToUseChoice = true;

    //bool canOneMoreTime; 

    YokaiParty playerParty;
    YokaiParty trainerParty;
    Yokai wildYokai;

    bool isTrainerBattle = false;
    int escapeAttempts;
    ThirdPersonController player;
    TrainerControler trainer;

    AbilitiesBase abilityToLearn;


    public void StartBattle(YokaiParty playerParty, Yokai wildYokai) {
        this.playerParty = playerParty;
        this.wildYokai = wildYokai;
        player = playerParty.GetComponent<ThirdPersonController>();
        isTrainerBattle = false;

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(YokaiParty playerParty, YokaiParty trainerParty) {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;

        player = playerParty.GetComponent<ThirdPersonController>();
        trainer = trainerParty.GetComponent<TrainerControler>();

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle() {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle) {
            // Wild Battle
            playerUnit.Setup(playerParty.GetHealthyYokai());
            enemyUnit.Setup(wildYokai);

            dialogBox.SetAbilityNames(playerUnit.Yokai.Abilities);
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Yokai.Base.Name} appeared!");
        } else {
            // Trainer Battle
            //first set models

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to Battle!");

            //send out trainer yokai
            enemyUnit.gameObject.SetActive(true);
            var enemyYokai = trainerParty.GetHealthyYokai();
            enemyUnit.Setup(enemyYokai);
            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {enemyYokai.Base.Name}!");

            //send out player yokai
            playerUnit.gameObject.SetActive(true);
            var playerYokai = playerParty.GetHealthyYokai();
            playerUnit.Setup(playerYokai);
            yield return dialogBox.TypeDialog($"Go {playerYokai.Base.Name}!");
            dialogBox.SetAbilityNames(playerUnit.Yokai.Abilities);
        }

        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection(); 
    }


    void BattleOver(bool won) {
        state = BattleState.BattleOver;

        //short version of a foreach using Linq :)
        playerParty.YokaiList.ForEach(y => y.OnBattleOver());

        OnBattleOver(won); 
        Destroy(playerUnit.CurrentPlayerModel);
        Destroy(enemyUnit.CurrentEnemyModel);
    }

    void ActionSelection() {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action.");
        dialogBox.EnableActionSelector(true);
    }

    void OpenPartyScreen() {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.YokaiList);
        partyScreen.gameObject.SetActive(true);
    }

    void AbilitySelection() {
        state = BattleState.AbilitySelection;
        dialogBox.EnableDialogText(false);
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableAbilitySelector(true);

    }

    IEnumerator AboutToUse(Yokai newYokai) {
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

    IEnumerator RunTurns(BattleAction playerAction) {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Ability) {
            playerUnit.Yokai.CurrentAbility = playerUnit.Yokai.Abilities[currentAbility];
            enemyUnit.Yokai.CurrentAbility = enemyUnit.Yokai.GetRandomAbility();

            int playerAbilityPriority = playerUnit.Yokai.CurrentAbility.Base.Priority;
            int enemyAbilityPriority = enemyUnit.Yokai.CurrentAbility.Base.Priority;

            // Check who goes first
            bool playerGoesFirst = true;
            if (enemyAbilityPriority > playerAbilityPriority) {
                playerGoesFirst = false;
            } else if (enemyAbilityPriority == playerAbilityPriority){
                playerGoesFirst = playerUnit.Yokai.Speed >= enemyUnit.Yokai.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondYokai = secondUnit.Yokai;

            //First turn
            yield return RunAbility(firstUnit, secondUnit, firstUnit.Yokai.CurrentAbility);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondYokai.HP > 0) {
                //Second turn
                yield return RunAbility(secondUnit, firstUnit, secondUnit.Yokai.CurrentAbility);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        } else {
            if (playerAction == BattleAction.SwitchYokai) {
                var selectedYokai = playerParty.YokaiList[currentMember];
                state = BattleState.Busy;
                yield return SwitchYokai(selectedYokai);
            } else if (playerAction == BattleAction.UseItem) {
                dialogBox.EnableActionSelector(false);
                yield return ThrowBall();
            } else if (playerAction == BattleAction.Run) {
                yield return TryToEscape();
            }

            //Enemy Turn
            var enemyAbilty = enemyUnit.Yokai.GetRandomAbility();
            yield return RunAbility(enemyUnit, playerUnit, enemyAbilty);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }
        if (state != BattleState.BattleOver) {
            ActionSelection();
        }
    }

    IEnumerator RunAbility(BattleUnit sourceUnit, BattleUnit targetUnit, Ability ability) {
        bool canRunAbility = sourceUnit.Yokai.OnBeforeAbility();

        // if yokai cannot run the ability, stop the coroutine
        if (!canRunAbility) {
            yield return ShowStatusChanges(sourceUnit.Yokai);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Yokai);

        ability.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Yokai.Base.Name} used {ability.Base.Name}");

        if (CheckIfAbilityHits(ability, sourceUnit.Yokai, targetUnit.Yokai)) {
            if (ability.Base.Category == AbilityCategory.Status) {
                yield return RunAbilityEffects(ability.Base.Effects, sourceUnit.Yokai, targetUnit.Yokai, ability.Base.Target);
            } else {
                var damageDetails = targetUnit.Yokai.TakeDamage(ability, sourceUnit.Yokai);
                yield return targetUnit.Hud.UpdateHP();
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
        yield return sourceUnit.Hud.UpdateHP();
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

    void CheckForBattleOver(BattleUnit defeatedUnit) {
        if (defeatedUnit.IsPlayerUnit) {
            Destroy(defeatedUnit.CurrentPlayerModel);
            var nextYokai = playerParty.GetHealthyYokai();
            if (nextYokai != null) {
                OpenPartyScreen();
            } else {
                BattleOver(false);
            }
        } else {
            if (!isTrainerBattle){
                BattleOver(true);
            } else {
                var nextYokai = trainerParty.GetHealthyYokai();
                if (nextYokai != null) {
                    //Send next yokai for trainer
                    StartCoroutine(AboutToUse(nextYokai));
                } else {
                    BattleOver(true);
                }
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
            case BattleState.PartyScreen:
                HandlePartySelection();
                break;
            case BattleState.AboutToUse:
                HandleAboutToUse();
                break;
            case BattleState.AbilityToForget:
                Action<int> onAbilitySelection = (abilityIndex) => {
                    abilitySelectionUI.gameObject.SetActive(false);
                    if (abilityIndex == YokaiBase.MaxNumAbilities) {
                        // Don't learn new Ability
                        StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Yokai.Base.Name} did no learn {abilityToLearn.Name}."));
                    } else {
                        // Forget the selected Ability
                        var selectedAbiliy = playerUnit.Yokai.Abilities[abilityIndex].Base;
                        StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Yokai.Base.Name} forgot {selectedAbiliy.Name} and lerned {abilityToLearn.Name}."));

                        playerUnit.Yokai.Abilities[abilityIndex] = new Ability(abilityToLearn);
                    }
                    abilityToLearn = null;
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
                    StartCoroutine(RunTurns(BattleAction.UseItem));
                    break;
                case 2:
                    prevState = state;
                    OpenPartyScreen();
                    // yokai
                    break;
                case 3:
                    // Run
                    StartCoroutine(RunTurns(BattleAction.Run));
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

        currentAbility = Mathf.Clamp(currentAbility, 0, playerUnit.Yokai.Abilities.Count - 1);

        dialogBox.UpdateAbilitySelection(currentAbility, playerUnit.Yokai.Abilities[currentAbility]);

        if (Input.GetKeyDown(KeyCode.Z)) {
            var ability = playerUnit.Yokai.Abilities[currentAbility];
            if (ability.PP == 0) return;

            dialogBox.EnableAbilitySelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Ability));
        } else if (Input.GetKeyDown(KeyCode.X)) {
            dialogBox.EnableAbilitySelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            ++currentMember;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            --currentMember;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)){
            currentMember += 2;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)){
            currentMember -= 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.YokaiList.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z)) {
            var selectedMember = playerParty.YokaiList[currentMember];
            if (selectedMember.HP <= 0) {
                partyScreen.SetMessageText("You cannot sent out a defeated Yokai.");
                return;
            }
            if (selectedMember == playerUnit.Yokai) {
                partyScreen.SetMessageText("You cannot switch the same Yokai.");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (prevState == BattleState.ActionSelection) {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchYokai));
            } else {
                state = BattleState.Busy;
                StartCoroutine(SwitchYokai(selectedMember));
            }
        } else if (Input.GetKeyDown(KeyCode.X)) {
            if (playerUnit.Yokai.HP <= 0) {
                partyScreen.SetMessageText("You have to choose a Yokai to continue.");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (prevState == BattleState.AboutToUse) {
                prevState = null;
                StartCoroutine(SendNextTrainerYokai());
            } else {
                ActionSelection();
            }
            
        }
    }

    void HandleAboutToUse() {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) {
            aboutToUseChoice = !aboutToUseChoice;
        }

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z)){
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true) {
                // Yes
                prevState = BattleState.AboutToUse; 
                OpenPartyScreen();
            } else {
                // No
                StartCoroutine(SendNextTrainerYokai());
            }
        } else if (Input.GetKeyDown(KeyCode.X)) {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerYokai());
        }
    }

    IEnumerator HandleYokaiDefeated (BattleUnit defeatedUnit) {
        yield return dialogBox.TypeDialog($"{defeatedUnit.Yokai.Base.Name} has been Defeated");
        yield return new WaitForSeconds(2f);

        if (!defeatedUnit.IsPlayerUnit) {
            // Exp Gain
            int expYield = defeatedUnit.Yokai.Base.ExpYield;
            int enemyLevel = defeatedUnit.Yokai.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Yokai.EXP += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Yokai.Base.Name} gained {expGain} experience!");
            yield return playerUnit.Hud.SetExpSmooth();

            // Check Level Up
            while (playerUnit.Yokai.CheckForLevelUp()) {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Yokai.Base.Name} grew to {playerUnit.Yokai.Level}!");

                //Try tp Learn New Move
                var newAbility = playerUnit.Yokai.GetLearnableAbilityAtCurrentLevel();
                if (newAbility != null) {
                    if (playerUnit.Yokai.Abilities.Count < YokaiBase.MaxNumAbilities) {
                        playerUnit.Yokai.LearnAbility(newAbility);
                        yield return dialogBox.TypeDialog($"{playerUnit.Yokai.Base.Name} learned {newAbility.Base.Name}!");
                        dialogBox.SetAbilityNames(playerUnit.Yokai.Abilities);
                    } else {
                        yield return dialogBox.TypeDialog($"{playerUnit.Yokai.Base.Name} is trying to learn {newAbility.Base.Name}!");
                        yield return dialogBox.TypeDialog($"But it cannot learn more than {YokaiBase.MaxNumAbilities}.");
                        yield return ChooseAbilityToForget(playerUnit.Yokai, newAbility.Base);
                        yield return new WaitUntil(() => state != BattleState.AbilityToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1f);
        }
                
        CheckForBattleOver(defeatedUnit);
    }

    IEnumerator SwitchYokai(Yokai newYokai) {
        if (playerUnit.Yokai.HP > 0) {
            yield return dialogBox.TypeDialog($"Get back here {playerUnit.Yokai.Base.Name}");
            playerUnit.CurrentPlayerModel.SetActive(false);
            yield return new WaitForSeconds(2f);
        }

        //playerUnit.CurrentPlayerModel.SetActive(false);
        
        playerUnit.Setup(newYokai);
        
        dialogBox.SetAbilityNames(newYokai.Abilities);
        yield return dialogBox.TypeDialog($"Go {newYokai.Base.Name}!");

        if (prevState == null){
            state = BattleState.RunningTurn;
        } else if (prevState == BattleState.AboutToUse) {
            prevState = null;
            StartCoroutine(SendNextTrainerYokai());
        }
            
    }

    IEnumerator SendNextTrainerYokai() {
        state = BattleState.Busy;
        enemyUnit.CurrentEnemyModel.SetActive(false);

        var nextYokai = trainerParty.GetHealthyYokai();
        enemyUnit.Setup(nextYokai);
        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {nextYokai.Base.Name}!");

        state = BattleState.RunningTurn;
    }

    IEnumerator ThrowBall() {
        state = BattleState.Busy;

        if (isTrainerBattle) {
            yield return dialogBox.TypeDialog($"You cannot steal the enemy's yokai!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"You are trying to capture the Yokai!");

        var captureBallObject = Instantiate(captureBall, ballSpawnPoint);
        enemyUnit.CurrentEnemyModel.SetActive(false);
        var ballAnimator = captureBallObject.GetComponentInChildren<Animator>();

        int shakeCount = TryToCatchYokai(enemyUnit.Yokai);

        //Animations
        for (int i = 0; i < Mathf.Min(shakeCount, 3); ++i) {
            ballAnimator.SetBool("Shake", true);
            yield return new WaitForSeconds(1f);
            ballAnimator.SetBool("Shake", false);
        }

        if (shakeCount == 4) {
            // Yokai Caught
            yield return dialogBox.TypeDialog($"{enemyUnit.Yokai.Base.Name} was caught!");

            playerParty.AddYokai(enemyUnit.Yokai);
            yield return dialogBox.TypeDialog($"{enemyUnit.Yokai.Base.Name} has been added to your party!");

            Destroy(captureBallObject);
            BattleOver(true);
        } else {
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
    int TryToCatchYokai(Yokai yokai) {
        float a = (3* yokai.MaxHp - 2 * yokai.HP) * yokai.Base.CatchRate * ConditionsDB.GetStatusBonus(yokai.Status) / (3 * yokai.MaxHp);

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
        state = BattleState.Busy;

        if (isTrainerBattle) {
            yield return dialogBox.TypeDialog($"You cannot run from trainer battles");
            state = BattleState.RunningTurn;
            yield break;
        }

        escapeAttempts++;

        int playerSpeed = playerUnit.Yokai.Speed;
        int enemySpeed = enemyUnit.Yokai.Speed;
        if (enemySpeed < playerSpeed) {
            yield return dialogBox.TypeDialog($"Ran away Safely");
            BattleOver(true);
        } else {
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