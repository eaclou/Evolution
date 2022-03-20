using System;

[Serializable]
public class Neuron 
{
    public NeuronGenome genome;

    public MetaNeuron data => genome.data;
    public NeuronType neuronType => data.io;
    public BrainModuleID moduleID => data.moduleID;
    public string name => data.name;
    public int index => genome.index;
    
    public float inputTotal;
    public float[] currentValue = new float[1];
    public float previousValue;  // * Not used

    public Neuron(NeuronGenome genome) { this.genome = genome; }
    public Neuron(Neuron original) { genome = original.genome; }

    public void Zero()
    {
        currentValue = new float[1];
        previousValue = 0f;
    }
    
    public bool IsMe(NeuronGenome other) { return data == other.data && genome.index == other.index; }
}

// * WPP: need to modify TestBrainVisualization to condense into above constructor
/*public Neuron(int index, int inputCount)
{
    neuronType = index < inputCount ? NeuronType.In : NeuronType.Out;
    currentValue = new float[1];
    currentValue[0] = Random.Range(-2f, 2f);
}*/