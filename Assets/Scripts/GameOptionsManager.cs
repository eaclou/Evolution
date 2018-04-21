using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GameOptionsManager : MonoBehaviour {

    public Toggle toggleFullscreen;
    public Dropdown dropdownResolution;
    public Dropdown dropdownVSync;
    public Dropdown dropdownFluidSimQuality;
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
        
        resolutionsArray = Screen.resolutions;
        
        foreach(Resolution resolution in resolutionsArray) {
            dropdownResolution.options.Add(new Dropdown.OptionData(resolution.ToString()));
        }

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

    public void OnDropdownFluidSimQuality() {
        gameOptions.fluidSimQuality = dropdownFluidSimQuality.value;

        // Actually change resolution in FluidManager!!! ******************************
    }

    public void OnSliderMasterVolume() {
        gameOptions.masterVolume = sliderMasterVolume.value;

        // *** Actually change Volume!!!! ***
    }

    public void OnSliderMusicVolume() {
        gameOptions.masterVolume = sliderMusicVolume.value;

        // *** Actually change Volume!!!! ***
    }

    public void OnSliderEffectsVolume() {
        gameOptions.masterVolume = sliderEffectsVolume.value;

        // *** Actually change Volume!!!! ***
    }

    public void OnSliderAmbientVolume() {
        gameOptions.masterVolume = sliderAmbientVolume.value;

        // *** Actually change Volume!!!! ***
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
