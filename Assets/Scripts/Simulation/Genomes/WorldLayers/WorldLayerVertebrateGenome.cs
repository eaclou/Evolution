using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLayerVertebrateGenome {

    public string name;
    public string textDescriptionMutation;
    public Color displayColor;
    public AgentGenome representativeGenome;
    
    public WorldLayerVertebrateGenome() {   // construction
        
    }

    public void UpdateDisplayColor() {
        Vector3 hue = representativeGenome.bodyGenome.appearanceGenome.huePrimary;
        displayColor = new Color(hue.x, hue.y, hue.z);
    }

    public void SetRepresentativeGenome(AgentGenome rep) {
        representativeGenome = rep;

        UpdateDisplayColor();
    }
}
