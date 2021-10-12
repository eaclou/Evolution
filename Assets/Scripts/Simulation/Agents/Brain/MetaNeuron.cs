using UnityEngine;

/// Static neuron data
[CreateAssetMenu(menuName = "Pond Water/Brain/Meta Neuron", fileName = "Meta Neuron")]
public class MetaNeuron : ScriptableObject
{
    public new string name;
    public NeuronType io;
    public BrainModuleID moduleID;
    public BrainIconID iconID;
    [Tooltip("Auto-generates index, probably obsolete")]
    public int id;
}

/// Differentiates neuron templates with the same static data (needed for hidden neurons).
public class NeuronGenome
{
    public BrainModuleID moduleID => data.moduleID;
    public NeuronType io => data.io;

    public MetaNeuron data;
    public int index;

    /// Create new NeuronGenome from static data
    public NeuronGenome(MetaNeuron data, int index = -1)
    {
        if (!data) 
            Debug.LogError("Neuron genome created without data!");
            
        this.data = data;
        this.index = index == -1 ? data.id : index;
    }
    
    /// Copy existing NeuronGenome
    public NeuronGenome(NeuronGenome original)
    {
        if (original == null) 
            Debug.LogError("Attempting to copy null NeuronGenome");
            
        data = original.data;
        index = original.index;
    }
}

public interface IBrainModule
{
    void MapNeuron(MetaNeuron data, Neuron neuron);
}
