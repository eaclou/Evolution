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
