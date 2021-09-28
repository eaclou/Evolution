using System.Collections.Generic;
using UnityEngine;
using Playcraft;

[System.Serializable]
public class CritterModuleCommunicationGenome 
{
    public int parentID;
    public int inno;

    public bool useComms;

	public CritterModuleCommunicationGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void GenerateRandomInitialGenome() {
        useComms = Random.Range(0f, 1f) >= 0.5f;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if(!useComms) 
            return;
        
        NeuronGenome inComm0 = new NeuronGenome("InComm0", NeuronType.In, inno, 40); // 40
        NeuronGenome inComm1 = new NeuronGenome("InComm1", NeuronType.In, inno, 41);// 41
        NeuronGenome inComm2 = new NeuronGenome("InComm2", NeuronType.In, inno, 42); // 42
        NeuronGenome inComm3 = new NeuronGenome("InComm3", NeuronType.In, inno, 43); // 43 
        // 44 Total Inputs
        NeuronGenome outComm0 = new NeuronGenome("OutComm0", NeuronType.Out, inno, 103); // 103
        NeuronGenome outComm1 = new NeuronGenome("OutComm1", NeuronType.Out, inno, 104); // 104
        NeuronGenome outComm2 = new NeuronGenome("OutComm2", NeuronType.Out, inno, 105); // 105
        NeuronGenome outComm3 = new NeuronGenome("OutComm3", NeuronType.Out, inno, 106); // 106

        neuronList.Add(inComm0); // 40
        neuronList.Add(inComm1); // 41
        neuronList.Add(inComm2); // 42
        neuronList.Add(inComm3); // 43
        // 44 Total Inputs
            
        neuronList.Add(outComm0); // 103
        neuronList.Add(outComm1); // 104
        neuronList.Add(outComm2); // 105
        neuronList.Add(outComm3); // 106
    }

    public void SetToMutatedCopyOfParentGenome(CritterModuleCommunicationGenome parentGenome, MutationSettingsInstance settings) {
        var mutate = RandomStatics.CoinToss(settings.bodyModuleInternalMutationChance);
        useComms = mutate ? !parentGenome.useComms : parentGenome.useComms;
    }
}
