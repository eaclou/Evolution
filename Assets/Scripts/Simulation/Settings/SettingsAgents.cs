using UnityEngine;

[CreateAssetMenu(fileName = "Agent Settings", menuName = "Pond Water/Game Settings/Agent", order = 1)]
public class SettingsAgents : ScriptableObject 
{
    public float _BaseOxygenUsage = 0.004f;
    public float _OxygenEnergyMask = 0.01f;
    [Tooltip("Proportion of input food mass converted to waste")]
    public float _DigestionWasteEfficiency = 0.25f;  
    [Tooltip("Proportion of consumed food converted to biomass growth")]
    public float _GrowthEfficiency = 0.2f;  
    [Tooltip("1 unit food = X units energy")]
    public float _DigestionEnergyEfficiency = 15f;  
    public float _BaseEnergyCost = 0.02f;
    public float _BaseDecompositionRate = 0.0003f;
    public float _BaseDigestionRate = 0.0025f;
    public float _BaseInitMass = 0.05f;
    [Tooltip("Egg sack cannot be more than x% of body mass")]
    public float _MaxPregnancyProportion = 0.2f;
    [Tooltip("Egg sack must be able to spawn x hatchings (initial mass)")]
    public float _MinPregnancyFactor = 5f;
    
}
