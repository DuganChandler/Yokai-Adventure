using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EncounterChecker : MonoBehaviour
{
    public event Action OnEncounter; 
    public event Action<Collider> OnEnterTrainerView;

    private const int TallGrass = 10;
    private const int Fov = 12;
    private void OnTriggerEnter(Collider other) {
        // check if in grass
        if (other.gameObject.layer == TallGrass) {
            if (UnityEngine.Random.Range(1, 101) <= 10) {
                gameObject.GetComponent<Animator>().SetFloat("Speed", 0);
                OnEncounter();
            }
        // check if in trainer
        } else if (other.gameObject.layer == Fov) {
            gameObject.GetComponent<Animator>().SetFloat("Speed", 0);
            OnEnterTrainerView?.Invoke(other);
        }
    }
}
