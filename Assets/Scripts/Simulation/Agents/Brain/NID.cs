using System;

/// Neuron Identifier
[Serializable]
public struct NID 
{
    public BrainModuleID moduleID;
    
    /// Unique identifier for neurons within a module
    public int neuronID;

    public NID(BrainModuleID moduleID, int neuronID) {
        this.moduleID = moduleID;
        this.neuronID = neuronID;
    }
}