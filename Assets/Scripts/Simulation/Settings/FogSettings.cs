using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Game Settings/Fog")]
public class FogSettings : ScriptableObject
{
    SimulationManager simulation => SimulationManager.instance;
    float globalPlantParticles => simulation.simResourceManager.curGlobalPlantParticles;

    [Header("Color")]
    public Color minColor = new Color(0.15f, 0.25f, 0.52f);
    public Color maxColor = new Color(0.07f, 0.27f, 0.157f);
    [Tooltip("Multiplied by global plant particles to set lerp percent")]
    [Range(0, 1)] public float colorMultiplier = 0.035f;
    
    [Header("Intensity")]
    [Range(0, 1)] public float minIntensity = 0.3f;
    [Range(0, 1)] public float maxIntensity = .55f;
    [Tooltip("Multiplied by global plant particles to set lerp percent")]
    [Range(0, 1)] public float intensityMultiplier = 0.0036f;
    
    public Color fogColor => Color.Lerp(minColor, maxColor, globalPlantParticles * colorMultiplier);
    public float fogIntensity => Mathf.Lerp(minIntensity, maxIntensity, Mathf.Clamp01(globalPlantParticles * intensityMultiplier));
}
