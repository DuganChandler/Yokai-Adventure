using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class NPCController : MonoBehaviour, IInteractable
{
    [SerializeField] Dialog dialog;   
    [SerializeField] Animator animator;
    NPCState state;
    float idleTimer = 0f;
    [SerializeField] bool canHeal;
    [SerializeField] ThirdPersonController playerController;
    YokaiParty playerParty;
    Healer healer;
    Transform player;

    private void Awake() {
        var playerParty = playerController.GetComponent<YokaiParty>();
        healer = GetComponent<Healer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    public void Interact()
    {
        // creates the singleton instance of DialogManager
        // be careful since it is really easy to call, can craeted unwanted dependencies
        if (state == NPCState.Idle){
            state = NPCState.Dialog;
            if (healer != null){
                StartCoroutine(healer.Heal(transform, dialog));
            } else {
                StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => {
                    idleTimer = 0;
                    state = NPCState.Idle;
                }));
            }
        }
        //idleTimer = 0;
        //state = NPCState.Idle;
            
    }

    private void Update() {
        Debug.Log(state);
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