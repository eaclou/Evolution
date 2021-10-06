using System;
using System.Collections.Generic;

[Serializable]
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
        gestationTimeMult = 1f;
        quantityQualityRatio = 1f;
    }

    public void AppendModuleNeuronsToMasterList(List<NeuronGenome> neuronList) {

    }
	
    public void SetToMutatedCopyOfParentGenome(CritterModuleDevelopmentalGenome parentGenome, MutationSettingsInstance settings) {

    }
}
