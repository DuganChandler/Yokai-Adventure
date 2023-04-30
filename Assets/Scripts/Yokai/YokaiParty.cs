using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 
using System;
using StarterAssets;

public class YokaiParty : MonoBehaviour
{
    [Header("Yokai List")]
    [SerializeField] List<Yokai> yokaiList;

    // on updated event
    public event Action OnUpdated;

    // Yokai list
    public List<Yokai> YokaiList{
        get {
            return yokaiList;
        }
        set {
            yokaiList = value;
            OnUpdated?.Invoke();
        }
    }

    private void Awake() {
        foreach (var yokai in yokaiList) {
            yokai.Init();
        }
    }

    public Yokai GetHealthyYokai(List<Yokai> dontInclude=null) 
    {
       var healthyYokai = yokaiList.Where(x => x.HP > 0);
        if (dontInclude != null)
        {
            healthyYokai = healthyYokai.Where(y => !dontInclude.Contains(y));
        }
        return healthyYokai.FirstOrDefault();
    }

    public List<Yokai> GetHealthyYokais(int unitCount)
    {
        return yokaiList.Where(x => x.HP > 0).Take(unitCount).ToList();
    }

    public void AddYokai(Yokai newYokai) {
        if (yokaiList.Count < 6) {
            yokaiList.Add(newYokai);
            OnUpdated?.Invoke();
        } else {
            // TODO: TRANSFER
        }
    }

    public void PartyUpdated() {
        OnUpdated?.Invoke();
    }

    public bool CheckForEvolution()
    {
        return yokaiList.Any(y => y.CheckForEvolution() != null);
    }

    public IEnumerator RunEvolutions() { 
        foreach (var yokai in yokaiList)
        {
            var evolution = yokai.CheckForEvolution();
            if (evolution != null)
            {
               yield return EvolutionManager.i.Evolve(yokai, evolution);
            }
        }
    }

    public static YokaiParty GetPlayerParty() {
        return FindObjectOfType<ThirdPersonController>().GetComponent<YokaiParty>();
    }
}
