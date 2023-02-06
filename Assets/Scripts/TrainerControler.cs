using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class TrainerControler : MonoBehaviour, IInteractable
{
    [SerializeField] new string name;
    [SerializeField] GameObject exlamation;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject fov;
    
    //State 
    bool battleLost = false;
    public string Name {
        get => name;
    }

    public void Interact()
    {
        // show dialog
        if (!battleLost){
            StartCoroutine (DialogManager.Instance.ShowDialog(dialog, () => {
                GameControler.Instance.StartTrainerBattle(this);
            }));
        } else {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialogAfterBattle));
        }
            
    }

    public void BattleLost() {
        battleLost = true;
        fov.SetActive(false);
    }

    public IEnumerator TriggerTrainerBattle(ThirdPersonController player) {
        // Show exclamation
        exlamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exlamation.SetActive(false);

        // show dialog
        StartCoroutine (DialogManager.Instance.ShowDialog(dialog, () => {
            GameControler.Instance.StartTrainerBattle(this);
        }));
    }
}
