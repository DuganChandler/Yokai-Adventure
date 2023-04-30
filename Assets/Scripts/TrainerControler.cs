using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class TrainerControler : MonoBehaviour, IInteractable, ISavable
{
    [Header("Trainer Name")]
    [SerializeField] new string name;

    [Header("Trainer Start Exclamation")]
    [SerializeField] GameObject exlamation;

    [Header("Dialog")]
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;

    [Header("FOV for Battle Start")]
    [SerializeField] GameObject fov;

    [Header("Audio")]
    [SerializeField] AudioClip trainerAppearsClip;

    [Header("Kind of Battle")]
    [SerializeField] int battleUnitCount = 1;
    
    //State 
    bool battleLost = false;
    public string Name {
        get => name;
    }

    public IEnumerator Interact(Transform initiator)
    {
        // show dialog
        if (battleLost == false)
        {
            AudioManager.i.PlayMusic(trainerAppearsClip);

            // Show exclamation
            exlamation.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            exlamation.SetActive(false);

            yield return DialogManager.Instance.ShowDialog(dialog);
            GameControler.Instance.StartTrainerBattle(this, battleUnitCount);
        } 
        else 
        {
            yield return DialogManager.Instance.ShowDialog(dialogAfterBattle);
        }
            
    }

    public void BattleLost() {
        battleLost = true;
        fov.SetActive(false);
    }

    public IEnumerator TriggerTrainerBattle(ThirdPersonController player) 
    {
        AudioManager.i.PlayMusic(trainerAppearsClip);

        // Show exclamation
        exlamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exlamation.SetActive(false);

        // show dialog
        yield return DialogManager.Instance.ShowDialog(dialog);
        GameControler.Instance.StartTrainerBattle(this, battleUnitCount);
    }

    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state)
    {
        battleLost = (bool)state;

        if (battleLost) {
            fov.SetActive(false);
        }
    }
}
