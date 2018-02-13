using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GenePool {

    public AgentGenome[] genomeArray;
    
    public GenePool(AgentGenome[] genomeArray) {
        this.genomeArray = genomeArray;
    }
}
