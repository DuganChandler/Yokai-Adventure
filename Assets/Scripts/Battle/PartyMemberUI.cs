using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] HpBar hpBar; 

    Yokai _yokai;

    public void Init(Yokai yokai) {
        _yokai = yokai;
        UpdateData();
        SetMessage("");

        _yokai.OnHpChanged += UpdateData;
    }

    void UpdateData() {
        nameText.text = _yokai.Base.Name;
        levelText.text = $"Lvl {_yokai.Level}";
        hpBar.SetHP((float)_yokai.HP / _yokai.MaxHp);
    }

    public void SetSelected(bool selected) {
        if (selected) {
            nameText.color = GlobalSetting.i.HighlightedColor;
        } else {
            nameText.color = Color.black;
        }
    }

    public void SetMessage(string message)
    {
        messageText.text= message;
    }
}
