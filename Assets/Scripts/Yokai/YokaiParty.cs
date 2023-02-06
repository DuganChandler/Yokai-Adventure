using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 
using System;

public class YokaiParty : MonoBehaviour
{
    [SerializeField] List<Yokai> yokaiList;
    public event Action OnUpdated;

    public List<Yokai> YokaiList{
        get {
            return yokaiList;
        }
    }

    private void Start() {
        foreach (var yokai in yokaiList) {
            yokai.Init();
        }
    }

    public Yokai GetHealthyYokai() {
        return yokaiList.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddYokai(Yokai newYokai) {
        if (yokaiList.Count < 6) {
            yokaiList.Add(newYokai);
        } else {
            // TODO: TRANSFER
        }
    }

    public void PartyUpdated() {
        OnUpdated?.Invoke();
    }
}
