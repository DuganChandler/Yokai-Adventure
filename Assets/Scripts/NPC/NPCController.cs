using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class NPCController : MonoBehaviour, IInteractable
{
    // Fields
    [SerializeField] Dialog dialog;   
    [SerializeField] Animator animator;

    // State
    NPCState state;

    // NPC idle time
    float idleTimer = 0f;

    // NPC Types
    ItemGiver itemGiver;
    Healer healer;
    

    private void Awake() {
        healer = GetComponent<Healer>();
        itemGiver = GetComponent<ItemGiver>();
    }
    public IEnumerator Interact(Transform initiator)
    {
        // creates the singleton instance of DialogManager
        // be careful since it is really easy to call, can craeted unwanted dependencies
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            if (itemGiver != null && itemGiver.CanBeGiven())
            {
                yield return itemGiver.GiveItem(initiator.GetComponent<ThirdPersonController>());
            } 
            else if (healer != null)
            {
                yield return healer.Heal(initiator, dialog);
            }
            else
            {
                yield return DialogManager.Instance.ShowDialog(dialog);
                idleTimer = 0;
                state = NPCState.Idle;
            }
        }
    }



        
    private void Update() {
        if (state == NPCState.Idle) {
            if (idleTimer > 2f) {
                //move
                idleTimer = 0;
                return;
            }
        }
    }

    IEnumerator Walk() {
        state = NPCState.Walking;
        //move;
        //playmoveanim;
        yield return 0;
        state = NPCState.Idle;
    }
}

public enum NPCState {
    Idle,
    Walking,
    Dialog
}