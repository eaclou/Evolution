using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterModuleExteriorGenome {

    public int parentID;
    public int inno;

	public CritterModuleExteriorGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void GenerateRandomGenome() {
        // Do stuff:

    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {

    }

    public void SetToMutatedCopyOfParentGenome(CritterModuleExteriorGenome parentGenome, MutationSettings settings) {

    }
}
