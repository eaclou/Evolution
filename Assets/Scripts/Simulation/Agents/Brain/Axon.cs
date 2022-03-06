using System;

[Serializable]
public class Axon 
{
    public Neuron from;
    public Neuron to;
    public float weight;

    public Axon(Neuron from, Neuron to, float weight)
    {
        this.from = from;
        this.to = to;
        this.weight = weight;
        
        if (from == null || to == null)
            UnityEngine.Debug.LogError("Initializing axon to null neuron");
    }
}
