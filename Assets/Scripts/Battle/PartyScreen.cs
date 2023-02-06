using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;

    PartyMemberUI[] memberSlots;

    List<Yokai> yokais;

    public void Init() {
        // True includes inactive
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true); 
    }

    public void SetPartyData(List<Yokai> yokais) {
        this.yokais = yokais;

        for (int i = 0; i < memberSlots.Length; i++) {
            if (i < yokais.Count) {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(yokais[i]);
            } else {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        messageText.text = "Choose a Yokai.";
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

    public void SetMessageText(string message) {
        messageText.text = message;
    }
}
