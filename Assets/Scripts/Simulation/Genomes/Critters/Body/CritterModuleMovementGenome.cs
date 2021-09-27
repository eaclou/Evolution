using System;
using System.Collections.Generic;

[Serializable]
public class CritterModuleMovementGenome 
{
    public int parentID;
    public int inno;

    public float horsepower;
    public float turnRate;

    public CritterModuleMovementGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }	

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {

        NeuronGenome ownVelX = new NeuronGenome("ownVelX", NeuronType.In, inno, 20); // 20
        NeuronGenome ownVelY = new NeuronGenome("ownVelY", NeuronType.In, inno, 21); // 21

        NeuronGenome facingDirX = new NeuronGenome("facingDirX", NeuronType.In, inno, 207); // 20
        NeuronGenome facingDirY = new NeuronGenome("facingDirY", NeuronType.In, inno, 208); // 21

        NeuronGenome throttleX = new NeuronGenome("throttleX", NeuronType.Out, inno, 100); // 100
        NeuronGenome throttleY = new NeuronGenome("throttleY", NeuronType.Out, inno, 101); // 101
        NeuronGenome dash = new NeuronGenome("dash", NeuronType.Out, inno, 102); // 102

        neuronList.Add(ownVelX); // 20
        neuronList.Add(ownVelY); // 21

        neuronList.Add(facingDirX); // 207
        neuronList.Add(facingDirY); // 208

        neuronList.Add(throttleX); // 100
        neuronList.Add(throttleY); // 101
        neuronList.Add(dash); // 102
        
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
