using UnityEngine;

// Future use
public class CritterModuleTarget : IBrainModule
{
    public BrainModuleID moduleID => BrainModuleID.Undefined;
    
    public void MapNeuron(MetaNeuron data, Neuron neuron) { }
}
