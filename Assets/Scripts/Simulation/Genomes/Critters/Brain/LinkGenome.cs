using System;
using System.Collections.Generic;

[Serializable]
public class LinkGenome 
{
    public BrainModuleID fromModuleID => from.moduleID;
    public int fromNeuronID => from.index;
    public BrainModuleID toModuleID => to.moduleID;
    public int toNeuronID => to.index;
    
    public NeuronGenome from;
    public NeuronGenome to;
    
    public float weight;    // multiplier on signal
    public bool enabled;
    
    public float normalizedWeight => weight * 0.5f + 0.5f;
    
    public LinkGenome(NeuronGenome from, NeuronGenome to, float weight, bool enabled)
    {
        this.from = from;
        this.to = to;
        this.weight = weight;
        this.enabled = enabled;
    }
    
    public bool IsInList(List<NeuronGenome> list)
    {
        foreach (var item in list)
            if (item == from || item == to)
                return true;
                
        return false;
    }
    
    // WPP: remove
    /*public LinkGenome(BrainModuleID fromModuleID, int fromNeuronID, BrainModuleID toModuleID, int toNeuronID, float weight, bool enabled) {
        this.fromModuleID = fromModuleID;
        this.fromNeuronID = fromNeuronID;
        this.toModuleID = toModuleID;
        this.toNeuronID = toNeuronID;
        this.weight = weight;
        this.enabled = enabled;
    }*/
}
