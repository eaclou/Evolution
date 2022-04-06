using System;
using UnityEngine;

[Serializable]
public class Neuron 
{
    UIManager ui => UIManager.instance;
    PlaceNeuronsAtUI placement => ui.placeNeuronsAtUI;

    NeuronGenome _genome;
    public NeuronGenome genome
    {
        get => _genome;
        set
        {
            _genome = value;
            data = new NeuronData(this);
            data.iconPosition = placement.GetNeuronPosition(this);
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
    
    // WPP: Added to eliminate need for external null reference checking and simplify interface
    public float[] currentValues = new float[1];
    public float currentValue
    {
        get
        {
            if (currentValues == null) currentValues = new float[1];
            return currentValues[0];
        }
        set
        {
            if (currentValues == null) currentValues = new float[1];
            currentValues[0] = value;
        }
    }
    
    public float previousValue;  // * Not used
    
    public Neuron(NeuronGenome genome) { this.genome = genome; }
    public Neuron(Neuron original) { genome = original.genome; }

    public void Zero()
    {
        currentValue = 0f;
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
    public string name;
    public int index;
    public Vector3 iconPosition;
    
    public NeuronData(Neuron neuron)
    {
        template = neuron.genome.data;
        name = template.name;
        index = neuron.genome.index;
        io = template.io;
        iconPosition = Vector3.zero;
    }
}