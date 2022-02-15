using System;
using System.Collections.Generic;

[Serializable]
public class CritterModuleDevelopmentalGenome 
{
    public BrainModuleID moduleID => BrainModuleID.Developmental;

    public float gestationTimeMult;
    public float quantityQualityRatio;

    public void Initialize() {
        gestationTimeMult = 1f;
        quantityQualityRatio = 1f;
    }

    // Future use
    public void AppendModuleNeuronsToMasterList(List<NeuronGenome> neuronList) { }
    public void SetToMutatedCopyOfParentGenome(CritterModuleDevelopmentalGenome parentGenome, MutationSettingsInstance settings) { }
}
