using System;
using System.Collections.Generic;

// * WPP: redundant, serves same purpose as Axon

[Serializable]
public class LinkGenome 
{
    public NeuronGenome from;
    public NeuronGenome to;
    
    public float weight;    // multiplier on signal
    
    public float normalizedWeight => weight * 0.5f + 0.5f;
    
    public LinkGenome(NeuronGenome from, NeuronGenome to, float weight)
    {
        this.from = from;
        this.to = to;
        this.weight = weight;
    }
    
    public bool IsInList(List<NeuronGenome> list)
    {
        foreach (var item in list)
            if (item == from || item == to)
                return true;
                
        return false;
    }
}
