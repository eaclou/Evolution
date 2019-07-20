using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLayerDecomposerGenome {

	public string name;
    public string textDescriptionMutation;
    public Color color;
    public float feedRate;
    public float killRate;
    public float scale;
    public float reactionRate;
	
    public WorldLayerDecomposerGenome() {   // construction
        color = Color.white;
        feedRate = 1f;
        killRate = 1f;
        scale = 1f;
        reactionRate = 1f;
    }
}
