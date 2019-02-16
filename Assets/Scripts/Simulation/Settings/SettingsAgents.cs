using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AgentSettings", menuName = "ScriptableObjects/AgentSettings", order = 1)]
public class SettingsAgents : ScriptableObject {

    public float _BaseOxygenUsage = 0.004f;
    public float _OxygenEnergyMask = 0.01f;
    public float _DigestionWasteEfficiency = 0.25f;  // what proportion of input food mass converted to waste?
    public float _GrowthEfficiency = 0.2f;  // what proportion of consumed food --> biomass growth?
    public float _DigestionEnergyEfficiency = 15f;  // 1 unit food = X units energy
    public float _BaseEnergyCost = 0.02f;
    public float _BaseDecompositionRate = 0.0003f;
    public float _BaseDigestionRate = 0.0025f;
    public float _BaseInitMass = 0.05f;
    public float _MaxPregnancyProportion = 0.2f;  // egg sack can't be more than x% of body mass
    public float _MinPregnancyFactor = 5f;  // egg sack must be able to spawn x hatchings (init mass)
    
}
