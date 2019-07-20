using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLayerVertebrateGenome {

    public string name;
    public string textDescriptionMutation;
    public AgentGenome representativeGenome;
    
    public WorldLayerVertebrateGenome() {   // construction
        
    }

    public void SetRepresentativeGenome(AgentGenome rep) {
        representativeGenome = rep;
    }
}
