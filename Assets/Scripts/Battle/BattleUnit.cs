using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    public bool IsPlayerUnit {
        get { return isPlayerUnit; }
    }

    public BattleHud Hud {
        get { return hud; }
    }

    [SerializeField] Transform playerBattleSpot;
    [SerializeField] Transform enemyBattleSpot;

    public Yokai Yokai { get; set; }

    GameObject unitModel;

    public GameObject CurrentPlayerModel { get; set; }
    public GameObject CurrentEnemyModel { get; set; }

    public void Setup(Yokai yokai) {
        Yokai = yokai;
        unitModel = Yokai.Base.LinkedPrefab;
        if (isPlayerUnit) {
            CurrentPlayerModel = Instantiate(unitModel, playerBattleSpot);
            CurrentPlayerModel.transform.Rotate(0f, 30f, 0f);
        } else {
            CurrentEnemyModel = Instantiate(unitModel, enemyBattleSpot);
            CurrentEnemyModel.transform.localScale = new Vector3(0.5f, 5f, 0.5f);
            CurrentEnemyModel.transform.Rotate(0f, 210f, 0f);
        }
        hud.gameObject.SetActive(true);
        hud.SetData(yokai);
    }

    public void Clear() {
        hud.gameObject.SetActive(false);
    }
}
