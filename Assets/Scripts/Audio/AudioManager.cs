using System.Collections;
using Playcraft;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager> 
{
    CameraManager cameraManager => CameraManager.instance;
    Vector3 cameraFocus => cameraManager.curCameraFocusPivotPos;
    Vector2 cameraFocus2D => new Vector2(cameraFocus.x, cameraFocus.y);

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
    public AudioSource[] soundtrackSources; // <--

    public AudioSource curTrackPlaying;
    
    public int frameCounter = 10;
    private int framesPerTrack = 1024 * 256 / 12; //MONTHLY 12//SimulationManager.instance.GetNumTimeStepsPerYear();
    private float distSqrFalloff = 110f;

    private int curTrackID = 0;

    public int GetFramesPerTrack() {
        return framesPerTrack;
    }

    private float GetSFXVolumeFromPos(Vector2 position) {
        float distSqr = (position - cameraFocus2D).sqrMagnitude;
        float dist = Mathf.Sqrt(distSqr);
        float falloffDist = Mathf.Sqrt(distSqrFalloff);
        float amplitude01 = 1f - Mathf.Clamp01(dist / falloffDist);

        return amplitude01;
    }
    
    public void PlayCritterDeath(Vector2 position) { PlaySound(audioSourceCritterDeath01, position); }
    public void PlayCritterDamage(Vector2 position) { PlaySound(audioSourceCritterDamage01, position); }
    public void PlayCritterDash(Vector2 position) { PlaySound(audioSourceCritterDash01, position); }
    public void PlayCritterSpawn(Vector2 position) { PlaySound(audioSourceCritterSpawn01, position); }
    public void PlayCritterBite(Vector2 position) { PlaySound(audioSourceCritterBite01, position); }
    public void PlayCritterAttack(Vector2 position) { PlaySound(audioSourceCritterAttack01, position); }
    public void PlayCritterDefend(Vector2 position) { PlaySound(audioSourceCritterDefend01, position); }
    
    public void PlayCritterAction(Vector2 position, AgentActionState actionState)
    {
        switch (actionState)
        {
            case AgentActionState.Attacking: PlayCritterAttack(position); break;
            case AgentActionState.Dashing: PlayCritterDash(position); break;
            case AgentActionState.Defending: PlayCritterDefend(position); break;
        }
    }
    
    void PlaySound(AudioSource sound, Vector2 position)
    {
        float volume = GetSFXVolumeFromPos(position);
        if (volume <= .01f) return;
        sound.volume = volume;
        sound.Play();
    }

    public void PlayMainMenuMusic() {

        //PlayNextSong(); // randomly picked
        int trackIndex = 7;
        curTrackPlaying = soundtrackSources[trackIndex]; 
        curTrackPlaying.Play();
        
        Debug.Log(trackIndex + " PlayMainMenuMusic() " + curTrackPlaying.name);
    }

    public void Tick() {
        
        if(curTrackPlaying != null) {
            if(curTrackPlaying.isPlaying) {

            }
            else {
                Debug.Log("PlayNextSONG");
                PlayNextSong();
            }
        }
        else {

        }
        //Check prograss of play:
        if(frameCounter > framesPerTrack) {
            //PlayNextSong();
            frameCounter = 0;
        }

        frameCounter++;
    }
    
    private void PlayNextSong() {
        curTrackPlaying = GetNextSong(); // GetRandomSong();
        curTrackPlaying.Play();
        curTrackID++;
        if(curTrackID >= soundtrackSources.Length) {
            curTrackID = 0;
        }
    }

    private AudioSource GetNextSong() {
        if(soundtrackSources.Length == 0) {
            return null;
        }

        Debug.Log("AudioManager Play Song #" + (curTrackID));// + " of " + numTracks);

        
        return soundtrackSources[curTrackID];

    }
    
    private AudioSource GetRandomSong() {
        if(soundtrackSources.Length == 0) {
            return null;
        }

        int numTracks = soundtrackSources.Length;
        int randTrackIndex = Random.Range(0, numTracks);
        Debug.Log("AudioManager Play Song #" + (randTrackIndex+1) + " of " + numTracks);
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
