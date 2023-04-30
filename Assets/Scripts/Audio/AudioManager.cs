using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField]List<AudioData> sfxList;

    // get audio sources
    [Header("Audio Sources")]
    [SerializeField] AudioSource musicPlayer;
    [SerializeField] AudioSource sfxPlayer;

    // fader duration
    [Header("Fader")]
    [SerializeField] float fadeDuration;

    AudioClip currentMusic;

    float originalMusicVol;
    Dictionary<AudioID, AudioData> sfxLookup;

    public static AudioManager i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    private void Start()
    {
        originalMusicVol = musicPlayer.volume;

        sfxLookup = sfxList.ToDictionary(x => x.id);
    }

    // play music function
    public void PlayMusic(AudioClip clip, bool loop=true, bool fade=false)
    {
        if (clip == null || clip == currentMusic) return;

        currentMusic = clip;
        StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    public void PlaySfx(AudioClip clip, bool pauseMusic = false)
    {
        if (clip == null) return;

        if (pauseMusic)
        {
            musicPlayer.Pause();
            StartCoroutine(UnPauseMusic(clip.length));
        }

        sfxPlayer.PlayOneShot(clip);
    }

    public void PlaySfx(AudioID audioId, bool pauseMusic=false)
    {
        if (!sfxLookup.ContainsKey(audioId)) return;

        var audioData = sfxLookup[audioId];

        PlaySfx(audioData.clip, pauseMusic);
    }

    IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade)
    {
        if (fade)
        {
            yield return musicPlayer.DOFade(0, fadeDuration).WaitForCompletion();
        }

        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.Play();

        if (fade)
        {
            yield return musicPlayer.DOFade(originalMusicVol, fadeDuration).WaitForCompletion();
        }
    }

    IEnumerator UnPauseMusic(float delay)
    {
        yield return new WaitForSeconds(delay);

        musicPlayer.volume = 0;
        musicPlayer.UnPause();
        musicPlayer.DOFade(originalMusicVol, fadeDuration);
    }
}

public enum AudioID { UISelect, Hit, Faint, ExpGain, ItemObtained, YokaiObtained }

[Serializable]
public class AudioData
{
    public AudioID id;
    public AudioClip clip;

}
