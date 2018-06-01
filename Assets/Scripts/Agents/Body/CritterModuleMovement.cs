using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterModuleMovement {

    public int parentID;
    public int inno;
    
    public float[] ownVelX;
    public float[] ownVelY;

    public float[] throttleX;
    public float[] throttleY;
    public float[] dash;

	
    public CritterModuleMovement() {
        
    }

    public void Initialize(CritterModuleMovementGenome genome) {
        
        ownVelX = new float[1]; // 20
        ownVelY = new float[1]; // 21
        
        throttleX = new float[1]; // 0
        throttleY = new float[1]; // 1
        dash = new float[1]; // 2
        
        parentID = genome.parentID;
        inno = genome.inno;
    }

    public void MapNeuron(NID nid, Neuron neuron) {

        if (inno == nid.moduleID) {            
            if (nid.neuronID == 20) {
                neuron.currentValue = ownVelX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 21) {
                neuron.currentValue = ownVelY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }

            if (nid.neuronID == 100) {
                neuron.currentValue = throttleX;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            if (nid.neuronID == 101) {
                neuron.currentValue = throttleY;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
            if (nid.neuronID == 102) {
                neuron.currentValue = dash;
                neuron.neuronType = NeuronGenome.NeuronType.Out;
            }
        }
    }

    public void Tick(bool isPlayer, Vector2 ownVel) {

        //Vector2 ownPos = new Vector2(agent.rigidbodiesArray[0].transform.localPosition.x, agent.rigidbodiesArray[0].transform.localPosition.y);
        //Vector2 ownVel = new Vector2(rigidbody.velocity.x, rigidbody.velocity.y);

        ownVelX[0] = ownVel.x / 15f;
        ownVelY[0] = ownVel.y / 15f;
    }
}
