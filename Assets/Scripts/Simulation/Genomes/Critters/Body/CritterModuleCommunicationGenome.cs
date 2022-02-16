/*
using System;
using System.Collections.Generic;
using Playcraft;

/// DEPRECATE
[Serializable]
public class CritterModuleCommunicationGenome 
{
    public readonly BrainModuleID moduleID = BrainModuleID.Communication;

    /// Deprecate
    public bool useComms;

    public void SetToMutatedCopyOfParentGenome(CritterModuleCommunicationGenome parentGenome, MutationSettingsInstance settings) 
    {
        var mutate = RandomStatics.CoinToss(settings.bodyModuleInternalMutationChance);
        useComms = mutate ? !parentGenome.useComms : parentGenome.useComms;
    }
}
*/