using UnityEngine;

public class CritterModuleCommunication : IBrainModule
{
    Lookup lookup => Lookup.instance;
    NeuralMap neuralMap => lookup.neuralMap;

    public CritterModuleCommunicationGenome genome;
	public int parentID;
    public BrainModuleID moduleID => genome.moduleID;

    public float[] inComm0;
    public float[] inComm1;
    public float[] inComm2;
    public float[] inComm3;  // 44 In?
        
    public float[] outComm0;
    public float[] outComm1;
    public float[] outComm2;
    public float[] outComm3;  // 7 Out?
    
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

        parentID = genome.parentID;
    }
    
    public void MapNeuron(MetaNeuron data, Neuron neuron)
    {
        if (moduleID != data.moduleID) return;
        neuron.currentValue = GetNeuralValue(data.id);
        //neuron.neuronType = data.io;
    }

    /*public void MapNeuron(NID nid, Neuron neuron) 
    {
        if (moduleID != nid.moduleID) return;
        neuron.currentValue = GetNeuralValue(nid.neuronID);
        neuron.neuronType = neuralMap.GetIO(nid.neuronID);
    }*/
    
    // * Use name from MetaNeuron
    // + consider using Reflection (match SO name field to variable name) to eliminate switch statement
    float[] GetNeuralValue(int neuronID)
    {
        switch (neuronID)
        {
            case 40: return inComm0;
            case 41: return inComm1;
            case 42: return inComm2;
            case 43: return inComm3;
            case 103: return outComm0;
            case 104: return outComm1;
            case 105: return outComm2;
            case 106: return outComm3;
            default: return null;
        }
    }

    public void Tick(Agent agent) 
    {
        if (!genome.useComms) 
            return;

        if (agent.coreModule.nearestFriendAgent) // && agent.coreModule.nearestFriendAgent.isDefending) {
        {
            var communication = agent.coreModule.nearestFriendAgent.communicationModule;
            inComm0[0] = Mathf.Round(communication.outComm0[0]); 
            inComm1[0] = Mathf.Round(communication.outComm1[0]);
            inComm2[0] = Mathf.Round(communication.outComm2[0]);
            inComm3[0] = Mathf.Round(communication.outComm3[0]);   
        } 
        else 
        {
            inComm0[0] = 0f;
            inComm1[0] = 0f;
            inComm2[0] = 0f;
            inComm3[0] = 0f; 
        }
    }
}
