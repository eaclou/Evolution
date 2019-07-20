using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLayerTerrainGenome {

    public string name;
    public string textDescriptionMutation;
    public Color color;
    public float elevationChange;

    
	
    public WorldLayerTerrainGenome() {   // construction
        color = Color.white;
        elevationChange = 1f;
    }

    /*public string GenerateTextDescription() {
        string txt = "";


        return txt;
    }*/
}
