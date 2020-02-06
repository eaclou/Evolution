using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLayerPlantGenome {

    public string name;
    public string textDescriptionMutation;
    public float growthRate;
    
    public Color displayColorPri;
    public Color displayColorSec;
    public int patternRowID;
    public int patternColumnID;
    public float patternThreshold;
    public VegetationManager.PlantParticleData plantRepData;
   
    public WorldLayerPlantGenome() {   // construction
        growthRate = 1f;

        displayColorPri = Color.white;
        displayColorSec = Color.black;

        patternRowID = UnityEngine.Random.Range(0, 8);
        patternColumnID = UnityEngine.Random.Range(0, 8);
        patternThreshold = UnityEngine.Random.Range(0f, 1f);
    }
}
