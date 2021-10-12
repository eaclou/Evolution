using System;
using System.Collections.Generic;

[Serializable]
public class CritterModuleDevelopmentalGenome 
{
    public int parentID;
    public BrainModuleID moduleID => BrainModuleID.Developmental;

    public float gestationTimeMult;
    public float quantityQualityRatio;

    public CritterModuleDevelopmentalGenome(int parentID) {
        this.parentID = parentID;
    }

    public void GenerateRandomInitialGenome() {
        gestationTimeMult = 1f;
        quantityQualityRatio = 1f;
    }

    // Future use
    public void AppendModuleNeuronsToMasterList(List<NeuronGenome> neuronList) { }
    public void SetToMutatedCopyOfParentGenome(CritterModuleDevelopmentalGenome parentGenome, MutationSettingsInstance settings) { }
}
