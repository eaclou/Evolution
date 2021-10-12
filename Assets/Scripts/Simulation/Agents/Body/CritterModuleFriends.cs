using UnityEngine;

public class CritterModuleFriends : IBrainModule
{
    Lookup lookup => Lookup.instance;
    NeuralMap neuralMap => lookup.neuralMap;

    public int parentID;
    public BrainModuleID moduleID { get; private set; }

    public float[] friendPosX;
    public float[] friendPosY;
    public float[] friendVelX;
    public float[] friendVelY;
    public float[] friendDirX;
    public float[] friendDirY;

    public CritterModuleFriends(CritterModuleFriendSensorsGenome genome) {
        Initialize(genome);
    }

    public void Initialize(CritterModuleFriendSensorsGenome genome) {
        friendPosX = new float[1]; // 8
        friendPosY = new float[1]; // 9
        friendVelX = new float[1]; // 10
        friendVelY = new float[1]; // 11
        friendDirX = new float[1]; // 12
        friendDirY = new float[1]; // 13

        parentID = genome.parentID;
        moduleID = genome.moduleID; 
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
        neuron.neuronType = neuralMap.GetIO(nid.neuronID);    
        neuron.currentValue = GetNeuralValue(nid.neuronID);
    }*/
    
    float[] GetNeuralValue(int neuronID)
    {
        switch(neuronID)
        {
            case 8: return friendPosX;
            case 9: return friendPosY;
            case 10: return friendVelX;
            case 11: return friendVelY;
            case 12: return friendDirX;
            case 13: return friendDirY;
            default: return null;
        }
    }

    public void Tick(Agent agent) {
        Vector2 friendPos = Vector2.zero;
        Vector2 friendDir = Vector2.zero;
        Vector2 friendVel = Vector2.zero;
        if(agent.coreModule.nearestFriendAgent) {
            friendPos = new Vector2(agent.coreModule.nearestFriendAgent.bodyRigidbody.transform.localPosition.x - agent.ownPos.x, agent.coreModule.nearestFriendAgent.bodyRigidbody.transform.localPosition.y - agent.ownPos.y);
            friendDir = friendPos.normalized;
            friendVel = new Vector2(agent.coreModule.nearestFriendAgent.bodyRigidbody.velocity.x, agent.coreModule.nearestFriendAgent.bodyRigidbody.velocity.y);
        }

        friendPosX[0] = friendPos.x / 20f;
        friendPosY[0] = friendPos.y / 20f;
        friendVelX[0] = (friendVel.x - agent.ownVel.x) / 15f;
        friendVelY[0] = (friendVel.y - agent.ownVel.y) / 15f;
        friendDirX[0] = friendDir.x;
        friendDirY[0] = friendDir.y;
    }
}
