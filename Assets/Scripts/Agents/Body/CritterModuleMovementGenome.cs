using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleMovementGenome {

    public int parentID;
    public int inno;

    public float horsepower;
    public float turnRate;

    public CritterModuleMovementGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }	

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {

        NeuronGenome ownVelX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 20); // 20
        NeuronGenome ownVelY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 21); // 21

        NeuronGenome facingDirX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 207); // 20
        NeuronGenome facingDirY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 208); // 21

        NeuronGenome throttleX = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 100); // 100
        NeuronGenome throttleY = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 101); // 101
        NeuronGenome dash = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 102); // 102

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

    public void GenerateRandomGenome() {
        // Do stuff:
        horsepower = 160f;
        turnRate = 16f;
    }

    public void SetToMutatedCopyOfParentGenome(CritterModuleMovementGenome parentGenome, MutationSettings settings) {
        //horsepower = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.horsepower, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 1f, 1f);
        horsepower = parentGenome.horsepower;
        //turnRate = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.turnRate, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 1f, 1f);
        turnRate = parentGenome.turnRate;
    }
}
