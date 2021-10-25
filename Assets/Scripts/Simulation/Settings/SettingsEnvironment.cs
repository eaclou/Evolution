using UnityEngine;

[CreateAssetMenu(fileName = "Environment Settings", menuName = "Pond Water/Game Settings/Environment", order = 1)]
public class SettingsEnvironment : ScriptableObject 
{
    // Resources:
    public float _BaseSolarEnergy = 1f;

    public float _DecomposersOxygenMask = 0.01f;
    public float _DecomposersDetritusMask = 0.01f;
    public float _BaseDecompositionRate = 0.00075f;
    public float _DetritusToNutrientsEfficiency = 1f;   
    public float _DecompositionOxygenUsage = 1f;
}
