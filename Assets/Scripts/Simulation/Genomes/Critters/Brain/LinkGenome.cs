using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LinkGenome {

    public int fromModuleID;  // id of node from which this connection originates
    public int fromNeuronID;
    public int toModuleID;  // id of node to which this connection flows
    public int toNeuronID;
    public float weight;  // multiplier on signal
    public bool enabled;

    public LinkGenome() {

    }

    public LinkGenome(int fromModuleID, int fromNeuronID, int toModuleID, int toNeuronID, float weight, bool enabled) {
        this.fromModuleID = fromModuleID;
        this.fromNeuronID = fromNeuronID;
        this.toModuleID = toModuleID;
        this.toNeuronID = toNeuronID;
        this.weight = weight;
        this.enabled = enabled;
    }
}
