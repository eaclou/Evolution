using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AlgaeParticleSettings", menuName = "ScriptableObjects/AlgaeParticleSettings", order = 1)]
public class SettingsAlgaeParticles : ScriptableObject {

	public float _AlgaeGrowthEfficiency = 0.1f;  // Energy --> biomass conversion
    public float _AlgaeGrowthNutrientsMask = 0.01f;
    //float nutrientsGrowthMask = saturate(_GlobalNutrients * 0.01);
    public float _AlgaeReservoirOxygenProductionEfficiency = 0.001f;
    
    // Simulation:    
	//float lightGrowthMask = saturate((altitudeRaw - 0.25) * 4.0 + 0.25); // refactor this later to use turbidity (water clarity)
	public float _AlgaeBaseGrowthRate = 0.0002f;
    public float _AlgaeGrowthNutrientUsage = 1f;
    public float _AlgaeGrowthOxygenProduction = 1f;
    public float _AlgaeAgingRate = 0.0001f;
    public float _AlgaeDecayRate = 0.0001f;
    public float _AlgaeSpawnMaxAltitude = 0.48f;
    public float _AlgaeParticleInitMass = 0.01f;
        
}
