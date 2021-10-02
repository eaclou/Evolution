using System;
using System.Collections.Generic;

[Serializable]
public class CritterModuleMovementGenome 
{
    public int parentID;
    public readonly BrainModuleID moduleID = BrainModuleID.Movement;

    public float horsepower;
    public float turnRate;

    public CritterModuleMovementGenome(int parentID) {
        this.parentID = parentID;
    }	

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {

        neuronList.Add(new NeuronGenome("ownVelX", NeuronType.In, moduleID, 20));
        neuronList.Add(new NeuronGenome("ownVelY", NeuronType.In, moduleID, 21)); 

        neuronList.Add(new NeuronGenome("facingDirX", NeuronType.In, moduleID, 207)); 
        neuronList.Add(new NeuronGenome("facingDirY", NeuronType.In, moduleID, 208));

        neuronList.Add(new NeuronGenome("throttleX", NeuronType.Out, moduleID, 100)); 
        neuronList.Add(new NeuronGenome("throttleY", NeuronType.Out, moduleID, 101)); 
        neuronList.Add(new NeuronGenome("dash", NeuronType.Out, moduleID, 102)); 

        // Should give this module Throttle & Dash output neurons?
        // currently, all within COREmoduleGenome
    }

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
