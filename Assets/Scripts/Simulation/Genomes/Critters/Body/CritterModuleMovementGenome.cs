using System;
using System.Collections.Generic;

[Serializable]
public class CritterModuleMovementGenome 
{
    Lookup lookup => Lookup.instance;
    NeuralMap map => lookup.neuralMap;

    public readonly BrainModuleID moduleID = BrainModuleID.Movement;

    public float horsepower;
    public float turnRate;
    
    
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

    // * WPP: expose magic numbers
    public void InitializeRandom() {
        horsepower = 140f;
        turnRate = 55f;
    }

    public void SetToMutatedCopyOfParentGenome(CritterModuleMovementGenome parentGenome, MutationSettingsInstance settings) {
        horsepower = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.horsepower, settings.bodyModuleInternalMutationChance, settings.bodyModuleInternalMutationStepSize, 100f, 200f);
        turnRate = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.turnRate, settings.bodyModuleInternalMutationChance, settings.bodyModuleInternalMutationStepSize, 50f, 60f);
    }
}
