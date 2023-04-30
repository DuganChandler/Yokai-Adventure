using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using StarterAssets;

public class Portal : MonoBehaviour
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;
    ThirdPersonController player;

    private void OnTriggerEnter(Collider other) {
        player = other.GetComponent<ThirdPersonController>();
        StartCoroutine(SwitchScene());
    }

    Fader fader;

    private void Start() {
        fader = FindObjectOfType<Fader>();
        
    }

    IEnumerator SwitchScene() {
        DontDestroyOnLoad(gameObject);

        yield return fader.FadeIn(0.5f);

        // async is the coroutine. Wont complete until scene is completley loaded.
        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        // first portal of current scene that is not the tranfer portal
        var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        
        player.transform.position = destPortal.SpawnPoint.position;
        
        yield return fader.FadeOut(0.5f);

        Destroy(gameObject);
    }

    public Transform SpawnPoint => spawnPoint;
}

public enum DestinationIdentifier {A, B, C, D, E}