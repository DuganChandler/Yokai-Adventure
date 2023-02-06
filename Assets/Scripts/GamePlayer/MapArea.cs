using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Yokai> wildYokais;

    public Yokai GetRandomWildYokai() {
        var wildYokai = wildYokais[Random.Range(0, wildYokais.Count)];
        wildYokai.Init();
        return wildYokai;
    }
}
