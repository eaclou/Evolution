using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleEnergyGenome {

    public int parentID;
    public int inno;

	public CritterModuleEnergyGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {

    }

    public void GenerateRandomGenome() {
        // Do stuff:

    }

    public void SetToMutatedCopyOfParentGenome(CritterModuleEnergyGenome parentGenome, MutationSettings settings) {

    }
}
