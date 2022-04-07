using System;
using UnityEngine;

/// Static neuron data
[CreateAssetMenu(menuName = "Pond Water/Brain/Meta Neuron", fileName = "Meta Neuron")]
public class MetaNeuron : ScriptableObject
{
    [Tooltip("Must match variable name in corresponding module")]
    public new string name;
    public NeuronType io;
    /// Maps neurons to modules
    public BrainModuleID moduleID;
    public BrainIconID iconID;

    public Neuron GetNeuron(int index) { return new Neuron(this, index); }
}

public interface IBrainModule
{
    void SetNeuralValue(Neuron neuron);
}

// WPP: redundant with Neuron
/// Differentiates neuron templates with the same static data (needed for hidden neurons).
/*[Serializable]
public class NeuronGenome
{
    public BrainModuleID moduleID => data.moduleID;
    public NeuronType io => data.io;

    public MetaNeuron data;
    public int index;

    /// Create new NeuronGenome from static data
    public NeuronGenome(MetaNeuron data, int index)
    {
        if (!data) Debug.LogError("Neuron genome created without data!");
        //Debug.Log($"{data.name} {index}");
            
        this.data = data;
        this.index = index;
    }
    
    /// Copy existing NeuronGenome
    public NeuronGenome(NeuronGenome original)
    {
        if (original == null) 
            Debug.LogError("Attempting to copy null NeuronGenome");
            
        data = original.data;
        index = original.index;
    }
}*/
