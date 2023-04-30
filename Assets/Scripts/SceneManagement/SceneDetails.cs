using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
public class SceneDetails : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] List<SceneDetails> connectedScenes;

    [Header("Audio")]
    [SerializeField] AudioClip sceneMusic;

    List<SavableEntity> savableEntities;

    public bool isLoaded { get; private set; }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            Debug.Log($"Entered {gameObject.name}");

            // load the scene
            LoadScene();
            GameControler.Instance.SetCurrentScene(this);

            // play music
            if (sceneMusic != null ) 
                AudioManager.i.PlayMusic(sceneMusic, fade: true);

            // load all connected scenes
            foreach (var scene in connectedScenes) {
                scene.LoadScene();
            }

            // Unload unneaded scenes
            var previousScene = GameControler.Instance.PreviousScene;
            if (GameControler.Instance.PreviousScene != null) {
                var previouslyLoadedScenes = GameControler.Instance.PreviousScene.connectedScenes;
                foreach (var scene in previouslyLoadedScenes) {
                    if (!connectedScenes.Contains(scene) && scene != this) {
                        scene.UnLoadScene();
                    }
                }
                
                if (!connectedScenes.Contains(previousScene)){
                    previousScene.UnLoadScene();
                }
                    
            }
        }
    }

    public void LoadScene() {
        if(!isLoaded) {
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            isLoaded = true;

            operation.completed += (AsyncOperation op) => {
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };

            
        }
    }

    public void UnLoadScene() {
        if(isLoaded) {
            SavingSystem.i.CaptureEntityStates(savableEntities);

            SceneManager.UnloadSceneAsync(gameObject.name);
            isLoaded = false;
         }
    }

    public List<SavableEntity> GetSavableEntitiesInScene() {
        var currentScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currentScene).ToList();
        return savableEntities;
    }

    public AudioClip SceneMusic => sceneMusic;
}
