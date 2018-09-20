using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NID {
    // Neuron Identifier
    public int moduleID;
    public int neuronID;

    public NID(int moduleID, int neuronID) {
        this.moduleID = moduleID;
        this.neuronID = neuronID;
    }
}