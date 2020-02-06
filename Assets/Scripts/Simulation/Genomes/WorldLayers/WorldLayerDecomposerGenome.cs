using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLayerDecomposerGenome {

	public string name;
    public string textDescriptionMutation;
    public Color displayColorPri;
    public Color displayColorSec;
    public int patternRowID;
    public int patternColumnID;
    public float patternThreshold;
    public float metabolicRate;
    public float growthEfficiency;
	
    public WorldLayerDecomposerGenome() {   // construction
        displayColorPri = Color.white;
        displayColorSec = Color.black;

        patternRowID = UnityEngine.Random.Range(0, 8);
        patternColumnID = UnityEngine.Random.Range(0, 8);
        patternThreshold = UnityEngine.Random.Range(0f, 1f);

    }
}
