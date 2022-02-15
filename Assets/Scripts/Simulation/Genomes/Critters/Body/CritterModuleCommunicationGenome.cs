using System;
using System.Collections.Generic;
using Playcraft;

[Serializable]
public class CritterModuleCommunicationGenome 
{
    NeuralMap map => Lookup.instance.neuralMap;

    public readonly BrainModuleID moduleID = BrainModuleID.Communication;

    public bool useComms;

    // Deprecate
    public void InitializeRandom() {
        useComms = RandomStatics.CoinToss();
    }
    
    public void Initialize(UnlockedTech unlockedTech) {
        useComms = unlockedTech.Contains(TechElementId.VocalCords);
    }
    
    public void AppendModuleNeuronsToMasterList(List<NeuronGenome> masterList)
    {
        if(!useComms) 
            return;
            
        var toAdd = map.GetAllByModule(moduleID);
        foreach (var item in toAdd)
            masterList.Add(item);    
    }

    public void SetToMutatedCopyOfParentGenome(CritterModuleCommunicationGenome parentGenome, MutationSettingsInstance settings) 
    {
        var mutate = RandomStatics.CoinToss(settings.bodyModuleInternalMutationChance);
        useComms = mutate ? !parentGenome.useComms : parentGenome.useComms;
    }
}
