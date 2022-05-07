using UnityEngine;

public class CritterModuleCommunication : IBrainModule
{
    public BrainModuleID moduleID => BrainModuleID.Communication;

    public float[] inComm0;
    public float[] inComm1;
    public float[] inComm2;
    public float[] inComm3;
        
    public float[] outComm0;
    public float[] outComm1;
    public float[] outComm2;
    public float[] outComm3;
    
    bool canCommunicate;
    
    public void Initialize(bool canCommunicate) {
        this.canCommunicate = canCommunicate;
    
        inComm0 = new float[1];
        inComm1 = new float[1];
        inComm2 = new float[1];
        inComm3 = new float[1]; 
        // 44 Total Inputs                
        outComm0 = new float[1];
        outComm1 = new float[1];
        outComm2 = new float[1];
        outComm3 = new float[1]; 
    }
    
    public void SetNeuralValue(Neuron neuron) {
        neuron.currentValue = ((float[])GetType().GetField(neuron.name).GetValue(this))[0];
    }

    public void Tick(Agent agent) 
    {
        if (!canCommunicate) 
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
    
    public float GetInChannelValue(int index)
    {
        switch (index)
        {
            case 0: return inComm0[0];
            case 1: return inComm1[0];
            case 2: return inComm2[0];
            case 3: return inComm3[0];
            default: Debug.LogError($"Invalid in communication channel {index}"); return 0f; 
        }
    }
    
    public float GetOutChannelValue(int index)
    {
        switch (index)
        {
            case 0: return outComm0[0];
            case 1: return outComm1[0];
            case 2: return outComm2[0];
            case 3: return outComm3[0];
            default: Debug.LogError($"Invalid out communication channel {index}"); return 0f; 
        }
    }
}
