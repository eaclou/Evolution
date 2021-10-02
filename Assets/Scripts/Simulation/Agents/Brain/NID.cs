using System;

/// Neuron Identifier
[Serializable]
public struct NID 
{
    public BrainModuleID moduleID;
    public int neuronID;

    public NID(BrainModuleID moduleID, int neuronID) {
        this.moduleID = moduleID;
        this.neuronID = neuronID;
    }
}