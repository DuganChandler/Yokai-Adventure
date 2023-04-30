using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    Color original;

    public bool IsPlayerUnit 
    {
        get { return isPlayerUnit; }
    }

    public BattleHud Hud 
    {
        get { return hud; }
    }

    [SerializeField] Transform playerBattleSpot;
    [SerializeField] Transform enemyBattleSpot;

    public Yokai Yokai { get; set; }

    GameObject unitModel;

    public GameObject CurrentPlayerModel { get; set; }
    public GameObject CurrentEnemyModel { get; set; }

    private void Awake()
    {
        original = hud.GetComponent<Image>().color;
    }

    public void Setup(Yokai yokai) 
    {
        Yokai = yokai;
        unitModel = Yokai.Base.LinkedPrefab;
        if (isPlayerUnit) 
        {
            CurrentPlayerModel = Instantiate(unitModel, playerBattleSpot);
            CurrentPlayerModel.transform.Rotate(0f, 30f, 0f);
        } 
        else 
        {
            CurrentEnemyModel = Instantiate(unitModel, enemyBattleSpot);
            CurrentEnemyModel.transform.localScale = new Vector3(0.5f, 5f, 0.5f);
            CurrentEnemyModel.transform.Rotate(0f, 210f, 0f);
        }
        hud.gameObject.SetActive(true);
        hud.SetData(yokai);

        
    }

    public void SetSelected(bool selected)
    {

        hud.gameObject.GetComponent<Image>().color = (selected) ? GlobalSetting.i.HighlightedColor : original;
    }

    public void Clear() 
    {
        hud.gameObject.SetActive(false);
    }
}
