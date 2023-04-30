using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using System;
using TMPro;
using Unity.VisualScripting;

public enum GameState {
    FreeRoam,
    Battle,
    Dialog,
    Cutscene,
    Menu,
    PartyScreen,
    Bag,
    Evolution
}

public class GameControler : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] ThirdPersonController playerController;

    [Header("UI")]
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    [Header("Battle/Encounters")]
    [SerializeField] BattleSystem battleController;
    [SerializeField] EncounterChecker encounterChecker;

    [Header("World Camera")]
    [SerializeField] Camera worldCamera;

    [Header("Tutorial")]
    [SerializeField] TextMeshProUGUI tutorial;

    bool showTutorial = true;
    
    // State management
    GameState state;
    GameState prevState;
    GameState stateBeforeEvolution;

    // Scene managment
    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PreviousScene { get; private set; }

    // Controllers
    MenuControl menuControl;
    public static GameControler Instance { get; private set; }

    private void Awake() {
        Instance = this;

        menuControl = GetComponent<MenuControl>();

        // Initializes all data
        YokaiDB.Init();
        AbilityDB.Init();
        ConditionsDB.Init();
        ItemDB.Init();
    }

    private void ShowTutorial()
    {
        if (!showTutorial)
        {
            tutorial.gameObject.SetActive(true);
            showTutorial = true;
        }
        else
        {
            tutorial.gameObject.SetActive(false);
            showTutorial = false;
        }
    }

    private void Start() {
        encounterChecker.OnEncounter += StartBattle;
        battleController.OnBattleOver += EndBattle;

        partyScreen.Init();

        encounterChecker.OnEnterTrainerView += (Collider trainerCollider) => 
        {
            var trainer = trainerCollider.GetComponentInParent<TrainerControler>();
            if (trainer != null) {
                state = GameState.Cutscene;
                StartCoroutine(trainer.TriggerTrainerBattle(playerController));
            }
        };

        DialogManager.Instance.OnShowDialog += () => {
            prevState = state;
            state = GameState.Dialog; 
        };

        DialogManager.Instance.OnDialogFinished += () => {
            if (state == GameState.Dialog)
                state = prevState; 
        };

        menuControl.onBack += () => {
            state = GameState.FreeRoam;
        };

        menuControl.onMenuSelected += OnMenuSelected;

        // listen ot evolution manager to change game state using actions.
        EvolutionManager.i.OnStartEvolution += () =>
        {
            stateBeforeEvolution = state;
            state = GameState.Evolution;
        };
        EvolutionManager.i.OnCompleteEvolution += () =>
        {
            partyScreen.SetPartyData();
            state = stateBeforeEvolution;

            AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);
        };
    }

    void StartBattle() {
        state = GameState.Battle;
        battleController.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<YokaiParty>();
        var wildYokai = CurrentScene.GetComponent<MapArea>().GetComponent<MapArea>().GetRandomWildYokai();

        var wildYokaiCopy = new Yokai(wildYokai.Base, wildYokai.Level);

        battleController.StartBattle(playerParty, wildYokaiCopy);
    }

    TrainerControler trainer;
    public void StartTrainerBattle(TrainerControler trainer, int unitCount=1) 
    {
        state = GameState.Battle;
        battleController.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<YokaiParty>();
        var trainerParty = trainer.GetComponent<YokaiParty>();
        
        battleController.StartTrainerBattle(playerParty, trainerParty, unitCount);
    }

    
    void EndBattle(bool won) 
    {
        // Check if it was trainer battle
        if (trainer != null && won == true) 
        {
            trainer.BattleLost();
            trainer = null;
        }

        // Reset party data ui
        partyScreen.SetPartyData();

        // set state 
        state = GameState.FreeRoam;
        battleController.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        // Check for evolutions
        var playerParty = playerController.GetComponent<YokaiParty>();
        bool hasEvolutions = playerParty.CheckForEvolution();

        if (hasEvolutions) 
            StartCoroutine(playerParty.RunEvolutions());
        else 
            AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ShowTutorial();
        }

        switch (state) {
            case GameState.FreeRoam:
                playerController.HandleUpdate();
                if (Input.GetKeyDown(KeyCode.Return)) {
                    menuControl.OpenMenu();
                    state = GameState.Menu;
                }
                break;
            case GameState.Battle:
                battleController.HandleUpdate();
                break;
            case GameState.Dialog:
                DialogManager.Instance.HandleUpdate();
                break;
            case GameState.Menu:
                menuControl.HandleUpdate();
                break;
            case GameState.PartyScreen:
                Action onSelected = () => {
                    //TODO: GO TO SUMMARY SCREEN
                    Debug.Log("you selected a yokai");
                };

                Action onBack = () => { 
                    partyScreen.gameObject.SetActive(false);
                    state = GameState.FreeRoam;
                };

                partyScreen.HandleUpdate(onSelected, onBack);
                break;
            case GameState.Bag:
                onBack = () => { 
                        inventoryUI.gameObject.SetActive(false);
                        state = GameState.FreeRoam;
                    };

                inventoryUI.HandleUpdate(onBack);
                break;
        }
    }

    public void SetCurrentScene(SceneDetails currScene) {
        PreviousScene = CurrentScene;
        CurrentScene = currScene;
    }

    void OnMenuSelected(int selectedItem) {
        switch(selectedItem) {
            case 0:
                // Yokai
                partyScreen.gameObject.SetActive(true);
                state = GameState.PartyScreen;
                break;
            case 1:
                // Bag
                inventoryUI.gameObject.SetActive(true);
                state = GameState.Bag;
                break;
            case 2:
                // Save
                SavingSystem.i.Save("saveSlot1");
                state = GameState.FreeRoam;
                break;
            case 3:
                // Load
                SavingSystem.i.Load("saveSlot1");
                state = GameState.FreeRoam;
                break;
        }
    }

    public GameState State => state;
}