using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class AbilitySelectionUI : MonoBehaviour
{
    [SerializeField] List<TextMeshProUGUI> abilityTexts;
    [SerializeField] Color highlightedColor;
    int currentSelection = 0;

    public void SetAbiliyData(List<AbilitiesBase> currentAbilities, AbilitiesBase newAbility) {
        for (int i = 0; i < currentAbilities.Count; ++i) {
            abilityTexts[i].text = currentAbilities[i].Name;
        }

        abilityTexts[currentAbilities.Count].text = newAbility.Name;
    }

    public void HandleAbilitySelection(Action<int> onSelected) {
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            ++currentSelection;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)){
            --currentSelection;
        }

        currentSelection = Mathf.Clamp(currentSelection, 0, YokaiBase.MaxNumAbilities);

        UpdateAbilitySelection(currentSelection);
        
        if (Input.GetKeyDown(KeyCode.Z)) {
            onSelected?.Invoke(currentSelection);
        }
    }

    public void UpdateAbilitySelection(int selection) {
        for (int i = 0; i < YokaiBase.MaxNumAbilities + 1; i++) {
            if (i == selection) {
                abilityTexts[i].color = highlightedColor;
            } else {
                abilityTexts[i].color = Color.black;
            }
        }
    }
}
