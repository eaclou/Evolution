using System;

/// Neuron Identifier
[Serializable]
public struct NID 
{
    public int moduleID;
    public int neuronID;

    public NID(int moduleID, int neuronID) {
        this.moduleID = moduleID;
        this.neuronID = neuronID;
    }
}