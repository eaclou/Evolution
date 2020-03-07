using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLayerAlgaeGenome {

    public string name;
    public float metabolicRate;
    public float growthEfficiency;
    public Color displayColorPri;
    public Color displayColorSec;
    public int patternRowID;
    public int patternColumnID;
    public float patternThreshold;
    
    public WorldLayerAlgaeGenome() {   // construction
        float minIntakeRate = 0.009f;
        float maxIntakeRate = 0.012f; // init around 1?
        float lnLerp = UnityEngine.Random.Range(0f, 1f);
        lnLerp *= lnLerp;
        metabolicRate = Mathf.Lerp(minIntakeRate, maxIntakeRate, lnLerp);        
        growthEfficiency = 1f;

        displayColorPri = Color.white;
        displayColorSec = Color.black;

        patternRowID = UnityEngine.Random.Range(0, 8);
        patternColumnID = UnityEngine.Random.Range(0, 8);
        patternThreshold = UnityEngine.Random.Range(0f, 1f);

    }
}
