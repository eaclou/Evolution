using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

    public AudioMixer masterAudioMixer;

    private float Range01toDecibels(float value) {
        return 20f * Mathf.Log10(value);
    }
	
    public void AdjustMasterVolume(float value) {
        masterAudioMixer.SetFloat("masterVol", Range01toDecibels(value));
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

}
