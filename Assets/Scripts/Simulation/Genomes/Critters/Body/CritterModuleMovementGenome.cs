using System;
using System.Collections.Generic;

[Serializable]
public class CritterModuleMovementGenome 
{
    Lookup lookup => Lookup.instance;
    NeuralMap map => lookup.neuralMap;

    public int parentID;
    public readonly BrainModuleID moduleID = BrainModuleID.Movement;

    public float horsepower;
    public float turnRate;

    public CritterModuleMovementGenome(int parentID) {
        this.parentID = parentID;
    }
    
    List<NeuronGenome> masterList;
    public void AppendModuleNeuronsToMasterList(List<NeuronGenome> masterList)
    {
        this.masterList = masterList;
        
        AddNeuron("ownVelX");
        AddNeuron("ownVelY");
        AddNeuron("facingDirX");
        AddNeuron("facingDirY");
        AddNeuron("throttleX");
        AddNeuron("throttleY");
        AddNeuron("dash");
    }
    
    void AddNeuron(string name) { masterList.Add(map.GetData(name)); }
    
    /*List<NeuronGenome> neuronList;
    public void AppendModuleNeuronsToMasterList(List<NeuronGenome> neuronList) {
        this.neuronList = neuronList;

        AddNeuron("ownVelX");
        AddNeuron("ownVelY");
        AddNeuron("facingDirX");
        AddNeuron("facingDirY");
        AddNeuron("throttleX");
        AddNeuron("throttleY");
        AddNeuron("dash");
        //neuronList.Add(new NeuronGenome("ownVelX", NeuronType.In, moduleID, 20));
        //neuronList.Add(new NeuronGenome("ownVelY", NeuronType.In, moduleID, 21));
        //neuronList.Add(new NeuronGenome("facingDirX", NeuronType.In, moduleID, 207)); 
        //neuronList.Add(new NeuronGenome("facingDirY", NeuronType.In, moduleID, 208));
        //neuronList.Add(new NeuronGenome("throttleX", NeuronType.Out, moduleID, 100)); 
        //neuronList.Add(new NeuronGenome("throttleY", NeuronType.Out, moduleID, 101)); 
        //neuronList.Add(new NeuronGenome("dash", NeuronType.Out, moduleID, 102)); 

        // Should give this module Throttle & Dash output neurons?
        // currently, all within COREmoduleGenome
    }
    
    void AddNeuron(string name) { neuronList.Add(map.GetGenome(name)); }*/

    public void GenerateRandomInitialGenome() {
        horsepower = 140f;
        turnRate = 55f;
    }

    public void SetToMutatedCopyOfParentGenome(CritterModuleMovementGenome parentGenome, MutationSettingsInstance settings) {
        horsepower = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.horsepower, settings.bodyModuleInternalMutationChance, settings.bodyModuleInternalMutationStepSize, 100f, 200f);
        //horsepower = parentGenome.horsepower;
        turnRate = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.turnRate, settings.bodyModuleInternalMutationChance, settings.bodyModuleInternalMutationStepSize, 50f, 60f);
        //turnRate = parentGenome.turnRate;
    }
}
