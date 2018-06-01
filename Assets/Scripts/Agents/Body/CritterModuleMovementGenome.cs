using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleMovementGenome {

    public int parentID;
    public int inno;

    public CritterModuleMovementGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }	

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {

    }

    public void GenerateRandomGenome() {
        // Do stuff:

    }

    public void SetToMutatedCopyOfParentGenome(CritterModuleMovementGenome parentGenome, MutationSettings settings) {

    }
}
