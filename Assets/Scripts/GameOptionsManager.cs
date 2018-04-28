using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GameOptionsManager : MonoBehaviour {

    public AudioManager audioManager;
    public Toggle toggleFullscreen;
    public Dropdown dropdownResolution;
    public Dropdown dropdownVSync;
    public Dropdown dropdownFluidPhysicsQuality;
    public Dropdown dropdownSimulationComplexity;
    public Slider sliderMasterVolume;
    public Slider sliderMusicVolume;
    public Slider sliderEffectsVolume;
    public Slider sliderAmbientVolume;
    public Resolution[] resolutionsArray;
    public GameOptions gameOptions;

    private void OnEnable() {

        gameOptions = new GameOptions();

        // example of hardcoded Ui listener rather than through inspector:
        //toggleFullscreen.onValueChanged.AddListener(delegate { OnToggleFullscreen(); });

        // Giving duplicates for some annoying reason, so manually set up resolutions list:
        SetResolutionsDropdown();

        // Set vSync dropdown to its current value: // **** Change all this when supporting Persistent Options!!!!! *****
        gameOptions.vSync = QualitySettings.vSyncCount;
        dropdownVSync.value = gameOptions.vSync;

        // Set Defaults:  ** CHANGE LATER WHEN SUPPORTING PERSISTENT OPTIONS!! ***
        gameOptions.fluidPhysicsQuality = 1; // medium 256x256
        dropdownFluidPhysicsQuality.value = gameOptions.fluidPhysicsQuality;
        gameOptions.simulationComplexity = 1; // medium 48 creatures, 16 hidden neurons
        dropdownSimulationComplexity.value = gameOptions.simulationComplexity;
                
    }

    private void Start() {
        //Initialize Audio Levels to 3/4:

        gameOptions.masterVolume = sliderMasterVolume.value;
        OnSliderMasterVolume(gameOptions.masterVolume);

        gameOptions.musicVolume = sliderMusicVolume.value;
        OnSliderMusicVolume(gameOptions.musicVolume);

        gameOptions.effectsVolume = sliderEffectsVolume.value;
        OnSliderEffectsVolume(gameOptions.effectsVolume);

        gameOptions.ambientVolume = sliderAmbientVolume.value;
        OnSliderAmbientVolume(gameOptions.ambientVolume);
    }

    private void SetResolutionsDropdown() {
        //resolutionsArray = GetSupportedResolutions();
        Resolution[] rawResolutionsArray = Screen.resolutions;
        List<Resolution> supportedResolutionsList = new List<Resolution>();         
        
        // a bit awkward but it should work for finding dropdownMenu index of current resolution:
        Dictionary<string, int> existingResolutionsDict = new Dictionary<string, int>();
        
        for(int i = 0; i < rawResolutionsArray.Length; i++) {
            int minResolutionWidth = 1024;
            int minResolutionHeight = 768;

            // Only consider resolutions larger than a minimum size:
            if (rawResolutionsArray[i].width >= minResolutionWidth && rawResolutionsArray[i].height >= minResolutionHeight) {
                if(existingResolutionsDict.ContainsKey(rawResolutionsArray[i].ToString())) {
                    //already in, do nothing
                }
                else {
                    // Add this to existing list:
                    existingResolutionsDict.Add(rawResolutionsArray[i].ToString(), supportedResolutionsList.Count); 
                    supportedResolutionsList.Add(rawResolutionsArray[i]);
                }
            }          
        }        
        resolutionsArray = supportedResolutionsList.ToArray();


        dropdownResolution.options.Clear(); // clear existing options (set in Editor)        
        foreach(Resolution resolution in resolutionsArray) {
            dropdownResolution.options.Add(new Dropdown.OptionData(resolution.ToString()));
        }
        // Start with current resolution selected?
        Resolution currentRes = Screen.currentResolution;
        int currentDropdownIndex = 0;
        if(existingResolutionsDict.ContainsKey(currentRes.ToString())) {
            currentDropdownIndex = existingResolutionsDict[currentRes.ToString()];
        }
        dropdownResolution.value = currentDropdownIndex;
        
        dropdownResolution.RefreshShownValue();
    }
    
    public void OnToggleFullscreen() {
        gameOptions.isFullscreen = toggleFullscreen.isOn;
        Screen.fullScreen = gameOptions.isFullscreen;
    }

    public void OnDropdownResolution() {
        gameOptions.resolutionIndex = dropdownResolution.value;
        Screen.SetResolution(resolutionsArray[dropdownResolution.value].width, resolutionsArray[dropdownResolution.value].height, Screen.fullScreen);
    }

    public void OnDropdownVSync() {
        gameOptions.vSync = dropdownVSync.value;
        QualitySettings.vSyncCount = gameOptions.vSync;
    }

    public void OnDropdownFluidPhysicsQuality() {
        gameOptions.fluidPhysicsQuality = dropdownFluidPhysicsQuality.value;

        // Actually change resolution in FluidManager!!! ******************************
    }

    public void OnDropdownSimulationComplexity() {
        gameOptions.simulationComplexity = dropdownSimulationComplexity.value;

        // Actually change resolution in FluidManager!!! ******************************
    }

    public void OnSliderMasterVolume(float value) {
        gameOptions.masterVolume = sliderMasterVolume.value;
        audioManager.AdjustMasterVolume(value); // converts 0-1 to decibel range        
    }

    public void OnSliderMusicVolume(float value) {
        gameOptions.masterVolume = sliderMusicVolume.value;

        audioManager.AdjustMusicVolume(value); // converts 0-1 to decibel range   
    }

    public void OnSliderEffectsVolume(float value) {
        gameOptions.masterVolume = sliderEffectsVolume.value;

        audioManager.AdjustEffectsVolume(value); // converts 0-1 to decibel range   
    }

    public void OnSliderAmbientVolume(float value) {
        gameOptions.masterVolume = sliderAmbientVolume.value;

        audioManager.AdjustAmbientVolume(value); // converts 0-1 to decibel range   
    }

    public void SaveOptions() {
        string jsonData = JsonUtility.ToJson(gameOptions, true);
        File.WriteAllText(Application.persistentDataPath + "/options.json", jsonData);
    }
    public void LoadOptions() {
        gameOptions = JsonUtility.FromJson<GameOptions>(File.ReadAllText(Application.persistentDataPath + "/options.json"));

        // Apply all settings here!!!! **********************************
    }
}
