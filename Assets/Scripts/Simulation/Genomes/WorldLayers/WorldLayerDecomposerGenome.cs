using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLayerDecomposerGenome {

	public string name;
    public string textDescriptionMutation;
    public Color displayColor;
    public float decomposerUpkeep;
    public float decomposerIntakeRate;
    public float decomposerGrowthEfficiency;
	
    public WorldLayerDecomposerGenome() {   // construction
        displayColor = Color.white;
        
    }
}
