using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, IInteractable, ISavable
{
    [SerializeField] ItemBase item;

    public bool Used { get; set; } = false;

    public IEnumerator Interact(Transform initiator)
    {
        if (!Used)
        {
            initiator.GetComponent<Inventory>().AddItem(item);

            Used = true;

            var renderComponennts = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderComponennts)
            {
                renderer.enabled = false;
            }

            GetComponent<MeshCollider>().enabled = false;

            AudioManager.i.PlaySfx(AudioID.ItemObtained, pauseMusic: true);

            yield return DialogManager.Instance.ShowDialogText($"You found a {item.Name}");
        }
    }

    public object CaptureState()
    {
        return Used;
    }

    public void RestoreState(object state)
    {
        Used = (bool)state;

        if (Used)
        {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<MeshCollider>().enabled = false;
        }
    }
}