using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterModuleFriends {

	public int parentID;
    public int inno;

    public float[] friendPosX;
    public float[] friendPosY;
    public float[] friendVelX;
    public float[] friendVelY;
    public float[] friendDirX;
    public float[] friendDirY;

    public CritterModuleFriends(CritterModuleFriendSensorsGenome genome, Agent agent) {
        Initialize(genome, agent);
    }

    public void Initialize(CritterModuleFriendSensorsGenome genome, Agent agent) {
        friendPosX = new float[1]; // 8
        friendPosY = new float[1]; // 9
        friendVelX = new float[1]; // 10
        friendVelY = new float[1]; // 11
        friendDirX = new float[1]; // 12
        friendDirY = new float[1]; // 13

        parentID = genome.parentID;
        inno = genome.inno; 
    }

    public void MapNeuron(NID nid, Neuron neuron) {
        if (inno == nid.moduleID) {
            if (nid.neuronID == 8) {
                neuron.currentValue = friendPosX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 9) {
                neuron.currentValue = friendPosY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 10) {
                neuron.currentValue = friendVelX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 11) {
                neuron.currentValue = friendVelY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 12) {
                neuron.currentValue = friendDirX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 13) {
                neuron.currentValue = friendDirY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
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
