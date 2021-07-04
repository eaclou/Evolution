using UnityEngine;

public class CritterModuleCommunication {

    public CritterModuleCommunicationGenome genome;
	public int parentID;
    public int inno;

    public float[] inComm0;
    public float[] inComm1;
    public float[] inComm2;
    public float[] inComm3;  // 44 In?
        
    public float[] outComm0;
    public float[] outComm1;
    public float[] outComm2;
    public float[] outComm3;  // 7 Out?

    public CritterModuleCommunication() {
               
    }

    public void Initialize(CritterModuleCommunicationGenome genome) {
        this.genome = genome;

        inComm0 = new float[1]; // 40
        inComm1 = new float[1]; // 41
        inComm2 = new float[1]; // 42
        inComm3 = new float[1]; // 43 
        // 44 Total Inputs                
        outComm0 = new float[1]; // 3
        outComm1 = new float[1]; // 4
        outComm2 = new float[1]; // 5
        outComm3 = new float[1]; // 6 

        this.parentID = genome.parentID;
        this.inno = genome.inno; 
    }

    public void MapNeuron(NID nid, Neuron neuron) {
        if (inno == nid.moduleID) {
            if (nid.neuronID == 40) {
                neuron.currentValue = inComm0;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 41) {
                neuron.currentValue = inComm1;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 42) {
                neuron.currentValue = inComm2;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 43) {
                neuron.currentValue = inComm3;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }

            if (nid.neuronID == 103) {
                neuron.currentValue = outComm0;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            if (nid.neuronID == 104) {
                neuron.currentValue = outComm1;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            if (nid.neuronID == 105) {
                neuron.currentValue = outComm2;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            if (nid.neuronID == 106) {
                neuron.currentValue = outComm3;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
        }
    }

    public void Tick(Agent agent) {
        if(genome.useComms) {
            if(agent.coreModule.nearestFriendAgent) { // && agent.coreModule.nearestFriendAgent.isDefending) {
                //Debug.Log("does this happen? yes");
                inComm0[0] = Mathf.Round(agent.coreModule.nearestFriendAgent.communicationModule.outComm0[0]); 
                inComm1[0] = Mathf.Round(agent.coreModule.nearestFriendAgent.communicationModule.outComm1[0]);
                inComm2[0] = Mathf.Round(agent.coreModule.nearestFriendAgent.communicationModule.outComm2[0]);
                inComm3[0] = Mathf.Round(agent.coreModule.nearestFriendAgent.communicationModule.outComm3[0]);   
            } 
            else {
                inComm0[0] = 0f;
                inComm1[0] = 0f;
                inComm2[0] = 0f;
                inComm3[0] = 0f; 
            }
        }
    }
}
