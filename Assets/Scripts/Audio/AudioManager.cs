using System.Collections;
using System.Collections.Generic;
using Playcraft;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager> {

    public AudioMixer masterAudioMixer;

    public GameObject menuMusicGroupGO;
    public GameObject gameplayAudioGroupGO;

    public AudioSource audioSourcePlayerSwimLoop;
    public AudioSource audioSourcePlayerSwimStart;

    private float Range01toDecibels(float value) {
        return 20f * Mathf.Log10(value);
    }

    public void SetPlayerSwimLoopVolume(float volume) {
        audioSourcePlayerSwimLoop.volume = volume;
    }
    public void PlaySwimStart() {
        audioSourcePlayerSwimStart.Play();
    }
	
    public void AdjustMasterVolume(float value) {
        masterAudioMixer.SetFloat("masterVol", Range01toDecibels(value));
    }
    public void AdjustGameplayVolume(float value) {
        masterAudioMixer.SetFloat("gameplayVol", Range01toDecibels(value));
    }
    public void AdjustMusicVolume(float value) {
        masterAudioMixer.SetFloat("musicVol", Range01toDecibels(value));
    }
    public void AdjustEffectsVolume(float value) {
        masterAudioMixer.SetFloat("effectsVol", Range01toDecibels(value));
    }
    public void AdjustAmbientVolume(float value) {
        masterAudioMixer.SetFloat("ambientVol", Range01toDecibels(value));
    }

    public void AdjustMenuVolume(float value) {
        masterAudioMixer.SetFloat("menuVol", Range01toDecibels(value));

        //Debug.Log("menuAudioLevel DB: " + Range01toDecibels(value).ToString());
    }

    public void TurnOnMenuAudioGroup() {
        menuMusicGroupGO.SetActive(true);
    }
    public void TurnOffMenuAudioGroup() {
        menuMusicGroupGO.SetActive(false);
    }
    public void TurnOnGameplayAudioGroup() {
        gameplayAudioGroupGO.SetActive(true);
    }
    public void TurnOffGameplayAudioGroup() {
        gameplayAudioGroupGO.SetActive(false);
    }

    public void BeginFadeMenuToGame(float duration) { StartCoroutine(FadeMenuToGame(duration)); }

    // * Replace method calls with delegates so this can be reused for all fades
    IEnumerator FadeMenuToGame(float duration) {
        float percent = 0f;
        float startTime = Time.time;
    
        while (percent < 1f) {
            AdjustMenuVolume(1f - percent);
            AdjustGameplayVolume(percent);
            percent = Time.time - startTime / duration;
            yield return null;
        }
    }
}
