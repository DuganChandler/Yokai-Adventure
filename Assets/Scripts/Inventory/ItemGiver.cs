using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGiver : MonoBehaviour, ISavable
{
    [SerializeField] ItemBase item;
    [SerializeField] Dialog dialog;
    [SerializeField] int count = 1;

    bool used = false;

    public IEnumerator GiveItem(ThirdPersonController player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);

        player.GetComponent<Inventory>().AddItem(item, count);

        used = true;

        AudioManager.i.PlaySfx(AudioID.ItemObtained, pauseMusic: true);

        string dialogText = $"You recieved a {item.Name}";
        if (count > 1)
        {
            dialogText = $"You recieved {count} {item.Name}s";
        }
        yield return DialogManager.Instance.ShowDialogText(dialogText);
    }

    public bool CanBeGiven()
    {
        return item != null && count > 0 && !used;
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}
