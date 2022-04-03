using System;
using UnityEngine;

[Serializable]
public class Neuron 
{
    NeuronGenome _genome;
    public NeuronGenome genome
    {
        get => _genome;
        set
        {
            _genome = value;
            data = new NeuronData(this);
        }
    }
    
    /// Static data
    [Header("Static Data")] 
    [ReadOnly] public NeuronData data;
    public NeuronType io => data.io; 
    public string name => data.name;
    public int index => data.index;
    
    [Header("Dynamic Data")]
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
    
    public bool IsMe(NeuronGenome other) { return data.template == other.data && genome.index == other.index; }
}

/// Static data associated with a neuron
[Serializable]
public struct NeuronData
{
    public MetaNeuron template;
    public NeuronType io; 
    public BrainModuleID moduleID;  // * Not used
    public string name;
    public int index;
    // * TBD: add icon position
    
    public NeuronData(Neuron neuron)
    {
        template = neuron.genome.data;
        name = template.name;
        index = neuron.genome.index;
        io = template.io;
        moduleID = template.moduleID;        
    }    
}

// * WPP: need to modify TestBrainVisualization to condense into above constructor
/*public Neuron(int index, int inputCount)
{
    neuronType = index < inputCount ? NeuronType.In : NeuronType.Out;
    currentValue = new float[1];
    currentValue[0] = Random.Range(-2f, 2f);
}*/