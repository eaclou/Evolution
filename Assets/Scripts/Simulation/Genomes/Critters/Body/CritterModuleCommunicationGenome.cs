using System;
using System.Collections.Generic;
using Playcraft;

[Serializable]
public class CritterModuleCommunicationGenome 
{
    NeuralMap map => Lookup.instance.neuralMap;

    public int parentID;
    public readonly BrainModuleID moduleID = BrainModuleID.Communication;

    public bool useComms;

	public CritterModuleCommunicationGenome(int parentID) {
        this.parentID = parentID;
    }

    public void GenerateRandomInitialGenome() {
        useComms = RandomStatics.CoinToss();
    }
    
    public void AppendModuleNeuronsToMasterList(List<NeuronGenome> masterList)
    {
        if(!useComms) 
            return;
            
        var toAdd = map.GetAllByModule(moduleID);
        foreach (var item in toAdd)
            masterList.Add(item);    
    }
    
    /*public void AppendModuleNeuronsToMasterList(List<NeuronGenome> neuronList) {
        if(!useComms) 
            return;
            
        var toAdd = map.GetAllByModule(moduleID);
        foreach (var item in toAdd)
            neuronList.Add(new NeuronGenome(item));
        
        neuronList.Add(new NeuronGenome("InComm0", NeuronType.In, moduleID, 40)); 
        neuronList.Add(new NeuronGenome("InComm1", NeuronType.In, moduleID, 41));
        neuronList.Add(new NeuronGenome("InComm2", NeuronType.In, moduleID, 42)); 
        neuronList.Add(new NeuronGenome("InComm3", NeuronType.In, moduleID, 43));
        // 44 Total Inputs
        neuronList.Add(new NeuronGenome("OutComm0", NeuronType.Out, moduleID, 103)); 
        neuronList.Add(new NeuronGenome("OutComm1", NeuronType.Out, moduleID, 104)); 
        neuronList.Add(new NeuronGenome("OutComm2", NeuronType.Out, moduleID, 105)); 
        neuronList.Add(new NeuronGenome("OutComm3", NeuronType.Out, moduleID, 106));
    }*/

    public void SetToMutatedCopyOfParentGenome(CritterModuleCommunicationGenome parentGenome, MutationSettingsInstance settings) {
        var mutate = RandomStatics.CoinToss(settings.bodyModuleInternalMutationChance);
        useComms = mutate ? !parentGenome.useComms : parentGenome.useComms;
    }
}
