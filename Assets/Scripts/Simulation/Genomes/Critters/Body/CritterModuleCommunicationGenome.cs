using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleCommunicationGenome {

	public int parentID;
    public int inno;

    public bool useComms;

	public CritterModuleCommunicationGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
    }

    public void GenerateRandomInitialGenome() {
        // Do stuff:

        useComms = false;
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        if(useComms) {
            NeuronGenome inComm0 = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 40); // 40
            NeuronGenome inComm1 = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 41);// 41
            NeuronGenome inComm2 = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 42); // 42
            NeuronGenome inComm3 = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 43); // 43 
            // 44 Total Inputs
            NeuronGenome outComm0 = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 103); // 103
            NeuronGenome outComm1 = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 104); // 104
            NeuronGenome outComm2 = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 105); // 105
            NeuronGenome outComm3 = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 106); // 106

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
    }

    public void SetToMutatedCopyOfParentGenome(CritterModuleCommunicationGenome parentGenome, MutationSettings settings) {
        this.useComms = parentGenome.useComms;

        float randChance = UnityEngine.Random.Range(0f, 1f);
        if(randChance < settings.bodyModuleMutationChance) {
            this.useComms = !this.useComms;
        }
    }
}
