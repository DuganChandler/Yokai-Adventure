using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleHud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] HpBar hpBar;
    [SerializeField] GameObject expBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;   

    Yokai _yokai;

    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Yokai yokai) {
        if (_yokai != null) {
            _yokai.OnStatusChanged -= SetStatusText;
            _yokai.OnHpChanged -= UpdateHP;
        }

        _yokai = yokai;
        
        nameText.text = yokai.Base.Name;
        SetLevel();
        hpBar.SetHP((float)yokai.HP / yokai.MaxHp);
        SetExp();

        statusColors = new Dictionary<ConditionID, Color>() 
        {
            {ConditionID.psn, psnColor},
            {ConditionID.brn, brnColor},
            {ConditionID.slp, slpColor},
            {ConditionID.par, parColor},
            {ConditionID.frz, frzColor}
        };

        SetStatusText();
        _yokai.OnStatusChanged += SetStatusText;
        _yokai.OnHpChanged += UpdateHP;
    }

    void SetStatusText() {
        if (_yokai.Status == null) {
            statusText.text = "";
        } else {
            statusText.text = _yokai.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_yokai.Status.Id];
        }
    }

    public void SetLevel() {
        levelText.text = $"Lvl {_yokai.Level}";
    }

    public void SetExp() {
        if (expBar == null) return;

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    
    public IEnumerator SetExpSmooth(bool reset = false) {
        if (expBar == null) yield break;

        if (reset) {
            expBar.transform.localScale = new Vector3(0, 1, 1);
        }

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }
    

    float GetNormalizedExp() {
        int currentLevelExp = _yokai.Base.GetExpForLevel(_yokai.Level);
        int nextLevelExp = _yokai.Base.GetExpForLevel(_yokai.Level + 1);

        float normalizedExp = (float)(_yokai.EXP - currentLevelExp) / (nextLevelExp - currentLevelExp);

        return Mathf.Clamp01(normalizedExp);
    }

    public void UpdateHP() {
        StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator UpdateHPAsync() {
        
        yield return hpBar.SetHPSmooth((float) _yokai.HP / _yokai.MaxHp); 
    }

    public IEnumerator WaitForHPUpdate() {
        yield return new WaitUntil(() => hpBar.IsUpdating == false);
    }

    public void ClearData() {
        if (_yokai != null) {
            _yokai.OnStatusChanged -= SetStatusText;
            _yokai.OnHpChanged -= UpdateHP;
        }
    }
}