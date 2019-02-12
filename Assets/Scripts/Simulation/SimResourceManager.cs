using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimResourceManager {

    public float baseSolarEnergy = 1f;
    public float dissolvedOxygenAmount = 1f;
    public float dissolvedNutrientsAmount = 1f;
    public float availableDetritusAmount = 1f;
    public float currentDecomposersAmount = 1f;

    
	public SimResourceManager() {
        // constructor
        dissolvedOxygenAmount = 100f;
        dissolvedNutrientsAmount = 100f;
        currentDecomposersAmount = 10f;
        availableDetritusAmount = 100f;
    }


}
