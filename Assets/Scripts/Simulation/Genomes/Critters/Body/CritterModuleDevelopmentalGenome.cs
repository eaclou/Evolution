using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleDevelopmentalGenome {

    public int parentID;
    public int inno;

    public CritterModuleDevelopmentalGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void GenerateRandomGenome() {
        // Do stuff:

    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {

    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleDevelopmentalGenome parentGenome, MutationSettings settings) {

    }
}
