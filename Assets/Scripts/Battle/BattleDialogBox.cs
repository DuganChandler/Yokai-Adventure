using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI dialogText;
    [SerializeField] int letterPerSecond;

    [SerializeField] Color highlightedColor;

    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject abilitySelector;
    [SerializeField] GameObject abilityDetails;
    [SerializeField] GameObject choiceBox;

    [SerializeField] List<TextMeshProUGUI> actionTexts; 
    [SerializeField] List<TextMeshProUGUI> abilityTexts; 

    [SerializeField] TextMeshProUGUI ppText; 
    [SerializeField] TextMeshProUGUI typeText; 

    [SerializeField] TextMeshProUGUI yesText; 
    [SerializeField] TextMeshProUGUI noText; 

    public void SetDialog(string dialog) {
        dialogText.text = dialog; 
    }

    public IEnumerator TypeDialog(string dialog) {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray()) {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f/letterPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogText(bool enabled) {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled) {
        actionSelector.SetActive(enabled);
    }

    public void EnableAbilitySelector(bool enabled) {
        abilitySelector.SetActive(enabled);
        abilityDetails.SetActive(enabled);
    }

    public void EnableChoiceBox(bool enabled) {
        choiceBox.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction) {
        for (int i=0; i < actionTexts.Count; ++i) {
            if (i == selectedAction) {
                actionTexts[i].color = highlightedColor;
            } else {
                actionTexts[i].color = Color.black;
            }
        }
    }

    public void UpdateAbilitySelection(int selectedAbility, Ability ability) {
        for (int i = 0; i < abilityTexts.Count; ++i) {
            if (i == selectedAbility) {
                abilityTexts[i].color = highlightedColor;
            } else {
                abilityTexts[i].color = Color.black;
            }
        }
        //string costType = ability.Base.IsMana ? "MP" : "HP";
        ppText.text = $"PP {ability.PP}/{ability.Base.PP}";
        typeText.text = ability.Base.Element.ToString();
        
        if (ability.PP == 0) {
            ppText.color = Color.red;
        } else {
            ppText.color = Color.black;
        }
    }

    public void UpdateChoiceBox(bool yesSelected) {
        if (yesSelected) {
            yesText.color = highlightedColor;
            noText.color = Color.black;
        } else {
            noText.color = highlightedColor;
            yesText.color = Color.black; 
        }
    }

    public void SetAbilityNames(List<Ability> abilities) {
        for (int i = 0; i < abilityTexts.Count; ++i) {
            if (i < abilities.Count) {
                abilityTexts[i].text = abilities[i].Base.Name;
            } else {
                abilityTexts[i].text = "---";
            }
        }
    }
}