﻿using System.Collections;
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

        NeuronGenome ownVelX = new NeuronGenome("ownVelX", NeuronGenome.NeuronType.In, inno, 20); // 20
        NeuronGenome ownVelY = new NeuronGenome("ownVelY", NeuronGenome.NeuronType.In, inno, 21); // 21

        NeuronGenome facingDirX = new NeuronGenome("facingDirX", NeuronGenome.NeuronType.In, inno, 207); // 20
        NeuronGenome facingDirY = new NeuronGenome("facingDirY", NeuronGenome.NeuronType.In, inno, 208); // 21

        NeuronGenome throttleX = new NeuronGenome("throttleX", NeuronGenome.NeuronType.Out, inno, 100); // 100
        NeuronGenome throttleY = new NeuronGenome("throttleY", NeuronGenome.NeuronType.Out, inno, 101); // 101
        NeuronGenome dash = new NeuronGenome("dash", NeuronGenome.NeuronType.Out, inno, 102); // 102

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
        // Do stuff:
        horsepower = 140f;
        turnRate = 55f;
    }

    public void SetToMutatedCopyOfParentGenome(CritterModuleMovementGenome parentGenome, MutationSettings settings) {
        horsepower = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.horsepower, settings.bodyModuleInternalMutationChance, settings.bodyModuleInternalMutationStepSize, 100f, 200f);
        //horsepower = parentGenome.horsepower;
        turnRate = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.turnRate, settings.bodyModuleInternalMutationChance, settings.bodyModuleInternalMutationStepSize, 50f, 60f);
        //turnRate = parentGenome.turnRate;
    }
}
