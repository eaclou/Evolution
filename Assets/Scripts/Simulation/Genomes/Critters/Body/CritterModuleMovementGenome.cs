/*
using System;
using System.Collections.Generic;

/// DEPRECATE
[Serializable]
public class CritterModuleMovementGenome 
{
    public readonly BrainModuleID moduleID = BrainModuleID.Movement;

    /// Not used
    public float horsepower;
    /// Not used
    public float turnRate;
    
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
*/

#region Obsolete: remove on verify
    /*
    Lookup lookup => Lookup.instance;
    NeuralMap map => lookup.neuralMap;    
    
    List<NeuronGenome> masterList;
    public void AppendModuleNeuronsToMasterList(List<NeuronGenome> masterList)
    {
        this.masterList = masterList;
        
        // * WPP: no condition, move to central location
        AddNeuron("ownVelX");
        AddNeuron("ownVelY");
        AddNeuron("facingDirX");
        AddNeuron("facingDirY");
        AddNeuron("throttleX");
        AddNeuron("throttleY");
        AddNeuron("dash");
    }
    
    void AddNeuron(string name) { masterList.Add(map.GetData(name)); }
    */
#endregion