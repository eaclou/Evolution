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

    public Neuron GetNeuron() { return new Neuron(this); }
}

public interface IBrainModule
{
    void SetNeuralValue(Neuron neuron);
}