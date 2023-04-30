using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using StarterAssets;

public class LocationPortal : MonoBehaviour
{
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;
    ThirdPersonController player;

    private void OnTriggerEnter(Collider other) {
        player = other.GetComponent<ThirdPersonController>();
        StartCoroutine(Teleport());
    }

    Fader fader;

    private void Start() {
        fader = FindObjectOfType<Fader>();
        
    }

    IEnumerator Teleport() {

        yield return fader.FadeIn(0.5f);

        // first portal of current scene that is not the tranfer portal
        var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        
        player.transform.position = destPortal.SpawnPoint.position;
        
        yield return fader.FadeOut(0.5f);

    }

    public Transform SpawnPoint => spawnPoint;
}