using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleDigestiveGenome {

    public int parentID;
    public int inno;

    public CritterModuleDigestiveGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {

    }
	
    public void GenerateRandomGenome() {
        // Do stuff:

    }

    public void SetToMutatedCopyOfParentGenome(CritterModuleDigestiveGenome parentGenome, MutationSettings settings) {

    }
}
