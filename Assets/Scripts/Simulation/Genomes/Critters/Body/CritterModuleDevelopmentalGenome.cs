using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleDevelopmentalGenome {

    public int parentID;
    public int inno;

    public float gestationTimeMult;
    public float quantityQualityRatio;

    public CritterModuleDevelopmentalGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void GenerateRandomInitialGenome() {
        // Do stuff:
        gestationTimeMult = 1f;
        quantityQualityRatio = 1f;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {

    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleDevelopmentalGenome parentGenome, MutationSettings settings) {

    }
}
