using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
    [Header("Evolution UI")]
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image yokaiImage;

    [Header("Audio")]
    [SerializeField] AudioClip evolutionMusic;

    public event Action OnStartEvolution;
    public event Action OnCompleteEvolution;

    public static EvolutionManager i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public IEnumerator Evolve(Yokai yokai, Evolution evolution)
    {
        OnStartEvolution?.Invoke();
        evolutionUI.SetActive(true);

        // Play music
        AudioManager.i.PlayMusic(evolutionMusic);

        // TODO: show yokai on screen and remove
        yield return DialogManager.Instance.ShowDialogText($"{yokai.Base.Name} is evolving");

        var oldYokai = yokai.Base;
        yokai.Evolve(evolution);

        // TODO: show yokai on screen and remove
        yield return DialogManager.Instance.ShowDialogText($"{oldYokai.Name} has evolved into {yokai.Base.Name}");

        evolutionUI.SetActive(false);
        OnCompleteEvolution?.Invoke();
    }
}
