using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public enum GameState {
    FreeRoam,
    Battle,
    Dialog,
    Cutscene
}

public class GameControler : MonoBehaviour
{
    [SerializeField] ThirdPersonController playerController;
    [SerializeField] BattleSystem battleController;
    [SerializeField] EncounterChecker encounterChecker;
    [SerializeField] Camera worldCamera;

    GameState state;

    public static GameControler Instance { get; private set; }

    private void Awake() {
        Instance = this;
        ConditionsDB.Init();
    }

    private void Start() {
        encounterChecker.OnEncounter += StartBattle;
        battleController.OnBattleOver += EndBattle;

        encounterChecker.OnEnterTrainerView += (Collider trainerCollider) => 
        {
            var trainer = trainerCollider.GetComponentInParent<TrainerControler>();
            if (trainer != null) {
                state = GameState.Cutscene;
                StartCoroutine(trainer.TriggerTrainerBattle(playerController));
            }
        };

        DialogManager.Instance.OnShowDialog += () => {
            state = GameState.Dialog; 
        };

        DialogManager.Instance.OnCloseDialog += () => {
            if (state == GameState.Dialog)
                state = GameState.FreeRoam; 
        };
    }

    void StartBattle() {
        state = GameState.Battle;
        battleController.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<YokaiParty>();
        var wildYokai = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildYokai();

        var wildYokaiCopy = new Yokai(wildYokai.Base, wildYokai.Level);

        battleController.StartBattle(playerParty, wildYokaiCopy);
    }

    TrainerControler trainer;
    public void StartTrainerBattle(TrainerControler trainer) {
        state = GameState.Battle;
        battleController.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<YokaiParty>();
        var trainerParty = trainer.GetComponent<YokaiParty>();

        battleController.StartTrainerBattle(playerParty, trainerParty);
    }

    
    void EndBattle(bool won) {
        if (trainer != null && won == true) {
            trainer.BattleLost();
            trainer = null;
        }

        state = GameState.FreeRoam;
        battleController.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    private void Update() {
        switch(state) {
            case GameState.FreeRoam:
                playerController.HandleUpdate();
                break;
            case GameState.Battle:
                battleController.HandleUpdate();
                break;
            case GameState.Dialog:
                DialogManager.Instance.HandleUpdate();
                break;
        }
    }
}