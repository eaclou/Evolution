using System;

[Serializable]
public class LinkGenome 
{
    public BrainModuleID fromModuleID; // id of node from which this connection originates
    public int fromNeuronID;
    public BrainModuleID toModuleID;  // id of node to which this connection flows
    public int toNeuronID;
    public float weight;    // multiplier on signal
    public bool enabled;
    
    public float normalizedWeight => weight * 0.5f + 0.5f;

    public LinkGenome(BrainModuleID fromModuleID, int fromNeuronID, BrainModuleID toModuleID, int toNeuronID, float weight, bool enabled) {
        this.fromModuleID = fromModuleID;
        this.fromNeuronID = fromNeuronID;
        this.toModuleID = toModuleID;
        this.toNeuronID = toNeuronID;
        this.weight = weight;
        this.enabled = enabled;
    }
}
