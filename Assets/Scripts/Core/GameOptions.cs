using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameOptions {

    public bool isFullscreen;
    public int vSync;
    public QualitySettingId fluidPhysicsQuality;     // ***WPP replace with enum
    public QualitySettingId simulationComplexity;
    
    public int resolutionIndex;    
    public float masterVolume;
    public float musicVolume;
    public float effectsVolume;
    public float ambientVolume;
}
