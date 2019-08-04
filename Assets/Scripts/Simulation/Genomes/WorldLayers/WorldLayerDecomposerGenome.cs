using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLayerDecomposerGenome {

	public string name;
    public string textDescriptionMutation;
    public Color displayColor;
    public float metabolicRate;
    //public float decomposerIntakeRate;
    public float growthEfficiency;
	
    public WorldLayerDecomposerGenome() {   // construction
        displayColor = Color.white;
        
    }
}
