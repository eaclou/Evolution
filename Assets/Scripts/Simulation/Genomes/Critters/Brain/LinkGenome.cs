using System;
using System.Collections.Generic;

[Serializable]
public class LinkGenome 
{
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
}
