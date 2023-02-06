using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HpBar hpBar; 

    [SerializeField] Color highlightedColor;

    Yokai _yokai;

    public void SetData(Yokai yokai) {
        _yokai = yokai;
        
        nameText.text = yokai.Base.Name;
        levelText.text = $"Lvl {yokai.Level}";
        hpBar.SetHP((float)yokai.HP / yokai.MaxHp);
    }

    public void SetSelected(bool selected) {
        if (selected) {
            nameText.color = highlightedColor;
        } else {
            nameText.color = Color.black;
        }
    }
}
