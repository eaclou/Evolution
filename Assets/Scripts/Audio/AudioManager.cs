using System.Collections;
using Playcraft;
using UnityEngine;
using UnityEngine.Audio;

// * WPP: refactor to eliminate repetition
public class AudioManager : Singleton<AudioManager> 
{
    public AudioMixer masterAudioMixer;

    public GameObject menuMusicGroupGO;
    public GameObject gameplayAudioGroupGO;

    public AudioSource audioSourcePlayerSwimLoop;
    public AudioSource audioSourcePlayerSwimStart;
    public AudioSource audioSourceCritterSpawn01;
    public AudioSource audioSourceCritterDeath01;
    public AudioSource audioSourceCritterBite01;
    public AudioSource audioSourceCritterAttack01;
    public AudioSource audioSourceCritterDefend01;
    public AudioSource audioSourceCritterDash01;
    public AudioSource audioSourceCritterDamage01;

    [SerializeField]
    public AudioSource[] soundtrackSources;
    
    private int frameCounter = 12400;
    private int framesPerTrack = 12500;
    private float distSqrFalloff = 110f;

    private float GetSFXVolumeFromPos(Vector2 pos) {
        float distSqr = (pos - new Vector2(CameraManager.instance.curCameraFocusPivotPos.x, CameraManager.instance.curCameraFocusPivotPos.y)).sqrMagnitude;
        float dist = Mathf.Sqrt(distSqr);
        float falloffDist = Mathf.Sqrt(distSqrFalloff);
        float amplitude01 = 1f - Mathf.Clamp01(dist / falloffDist);

        return amplitude01;
    }
    
    public void PlayCritterDeath(Vector2 pos) {
        float volume = GetSFXVolumeFromPos(pos);
        if(volume > 0.01f) {
            audioSourceCritterDeath01.volume = volume;
            audioSourceCritterDeath01.Play();
        }
    }
    
    public void PlayCritterDamage(Vector2 pos) {
        float volume = GetSFXVolumeFromPos(pos);
        if (volume > 0.01f) {
            audioSourceCritterDamage01.volume = volume;
            audioSourceCritterDamage01.Play();
        }
    }
    
    public void PlayCritterDash(Vector2 pos) {
        float volume = GetSFXVolumeFromPos(pos);
        if (volume > 0.01f) {
            audioSourceCritterDash01.volume = volume;
            audioSourceCritterDash01.Play();
        }
    }
    
    public void PlayCritterSpawn(Vector2 pos) {
        float volume = GetSFXVolumeFromPos(pos);
        if (volume > 0.01f) {
            audioSourceCritterSpawn01.volume = volume;
            audioSourceCritterSpawn01.Play();
        }
    }
    
    public void PlayCritterBite(Vector2 pos) {
        float volume = GetSFXVolumeFromPos(pos);
        if (volume > 0.01f) {
            audioSourceCritterBite01.volume = volume;
            audioSourceCritterBite01.Play();
        }
    }
    
    public void PlayCritterAttack(Vector2 pos) {
        float volume = GetSFXVolumeFromPos(pos);
        if (volume > 0.01f) {
            audioSourceCritterAttack01.volume = volume;
            audioSourceCritterAttack01.Play();
        }
    }
    
    public void PlayCritterDefend(Vector2 pos) {
        float volume = GetSFXVolumeFromPos(pos);
        if (volume > 0.01f) {
            audioSourceCritterDefend01.volume = volume;
            audioSourceCritterDefend01.Play();
        }
    }

    public void Tick() {
        //Check prograss of play:
        if(frameCounter > framesPerTrack) {
            PlayNextSong();
            frameCounter = 0;
        }

        frameCounter++;
    }
    
    private void PlayNextSong() {
        GetRandomSong().Play();
        //Debug.Log("PLAY SONG)");
    }
    
    private AudioSource GetRandomSong() {
        if(soundtrackSources.Length == 0) {
            return null;
        }

        int numTracks = soundtrackSources.Length;
        int randTrackIndex = Random.Range(0, numTracks - 1);

        return soundtrackSources[randTrackIndex];
    }

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
