using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;

    PartyMemberUI[] memberSlots;

    List<Yokai> yokais;
    YokaiParty party;
    int selection = 0;

    public Yokai SelectedMemeber => yokais[selection];

    // Party screen can be called from different staes like action sleection, runningturns and abouttotuse
    public BattleState? CalledFrom { get; set; }

    public void Init() {
        // True includes inactive
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true); 
        party = YokaiParty.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
    }

    public void SetPartyData() {
        yokais = party.YokaiList;

        for (int i = 0; i < memberSlots.Length; i++) {
            if (i < yokais.Count) {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(yokais[i]);
            } else {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        UpdateMemberSelection(selection);

        messageText.text = "Choose a Yokai.";
    }

    public void HandleUpdate(Action onSelected, Action onBack) {
        var prevSelection = selection;

        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            ++selection;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            --selection;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)){
            selection += 2;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)){
            selection -= 2;
        }

        selection = Mathf.Clamp(selection, 0, yokais.Count - 1);
        
        if (selection != prevSelection)
            UpdateMemberSelection(selection);

        if (Input.GetKeyDown(KeyCode.Z)) {
            onSelected?.Invoke();
        } else if (Input.GetKeyDown(KeyCode.X)) {
            onBack?.Invoke();
        }
    }

    public void UpdateMemberSelection(int slectedMember) {
        for (int i = 0; i < yokais.Count; i++) {
            if (i == slectedMember) {
                memberSlots[i].SetSelected(true);
            } else {
                memberSlots[i].SetSelected(false);
            }
        }
    } 

    public void ShowIfTmIsUsable(TmItem tmItem)
    {
        for (int i = 0; i < yokais.Count; i++)
        {
            string message = tmItem.CanBeTaught(yokais[i])? "ABLE!" : "NOT ABLE!";
            memberSlots[i].SetMessage(message);
        }
    }
    public void ClearTmUsableMessage()
    {
        for (int i = 0; i < yokais.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }

    public void SetMessageText(string message) {
        messageText.text = message;
    }
}
