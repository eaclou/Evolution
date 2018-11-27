using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterModuleEnvironment {

	public int parentID;
    public int inno;

    public float[] distUp; // start up and go clockwise!
    public float[] distTopRight;
    public float[] distRight;
    public float[] distBottomRight;
    public float[] distDown;    
    public float[] distBottomLeft;
    public float[] distLeft;
    public float[] distTopLeft;

    public CritterModuleEnvironment() {
                
    }

    public void Initialize(CritterModuleEnvironmentSensorsGenome genome, Agent agent) {
        distUp = new float[1]; // 32 // start up and go clockwise!
        distTopRight = new float[1]; // 33
        distRight = new float[1]; // 34
        distBottomRight = new float[1]; // 35
        distDown = new float[1]; // 36
        distBottomLeft = new float[1]; // 37
        distLeft = new float[1]; // 38
        distTopLeft = new float[1]; // 39

        this.parentID = genome.parentID;
        this.inno = genome.inno;
    }

    public void MapNeuron(NID nid, Neuron neuron) {
        if (inno == nid.moduleID) {
            if (nid.neuronID == 32) {
                neuron.currentValue = distUp;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 33) {
                neuron.currentValue = distTopRight;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 34) {
                neuron.currentValue = distRight;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 35) {
                neuron.currentValue = distBottomRight;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 36) {
                neuron.currentValue = distDown;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 37) {
                neuron.currentValue = distBottomLeft;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 38) {
                neuron.currentValue = distLeft;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 39) {
                neuron.currentValue = distTopLeft;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
        }
    }

    public void Tick(Agent agent) {
        int rayLayer = LayerMask.GetMask("EnvironmentCollision");
        //Debug.Log(LayerMask.GetMask("EnvironmentCollision"));
        //Debug.Log(mask.ToString());
                
        // TOP
        float raycastMaxLength = 10f;
        RaycastHit2D hitTop = Physics2D.Raycast(agent.ownPos, Vector2.up, raycastMaxLength, rayLayer);  // UP
        float distance = raycastMaxLength;
        if (hitTop.collider != null && hitTop.collider.tag == "HazardCollider") {
            distance = (hitTop.point - agent.ownPos).magnitude;            
        }
        distUp[0] = (raycastMaxLength - distance) / raycastMaxLength;  // Mathf.Abs(40f - yPos) / 40f;        
        // TOP RIGHT
        RaycastHit2D hitTopRight = Physics2D.Raycast(agent.ownPos, new Vector2(1f,1f), raycastMaxLength, rayLayer);  //  + / +
        distance = raycastMaxLength;
        if (hitTopRight.collider != null && hitTopRight.collider.tag == "HazardCollider") {
            distance = (hitTopRight.point - agent.ownPos).magnitude;
        }
        distTopRight[0] = (raycastMaxLength - distance) / raycastMaxLength;
        // RIGHT
        RaycastHit2D hitRight = Physics2D.Raycast(agent.ownPos, new Vector2(1f, 0f), raycastMaxLength, rayLayer);  //  + / +
        distance = raycastMaxLength;
        if (hitRight.collider != null && hitRight.collider.tag == "HazardCollider") {
            distance = (hitRight.point - agent.ownPos).magnitude;
        }
        distRight[0] = (raycastMaxLength - distance) / raycastMaxLength;
        // BOTTOM RIGHT
        RaycastHit2D hitBottomRight = Physics2D.Raycast(agent.ownPos, new Vector2(1f, -1f), raycastMaxLength, rayLayer);  //  + / +
        distance = raycastMaxLength;
        if (hitBottomRight.collider != null && hitBottomRight.collider.tag == "HazardCollider") {
            distance = (hitBottomRight.point - agent.ownPos).magnitude;
        }
        distBottomRight[0] = (raycastMaxLength - distance) / raycastMaxLength;
        // BOTTOM
        RaycastHit2D hitBottom = Physics2D.Raycast(agent.ownPos, new Vector2(0f, -1f), raycastMaxLength, rayLayer);  //  + / +
        distance = raycastMaxLength;
        if (hitBottom.collider != null && hitBottom.collider.tag == "HazardCollider") {
            distance = (hitBottom.point - agent.ownPos).magnitude;
            //Debug.Log("HIT BOTTOM! " + ((raycastMaxLength - distance) / raycastMaxLength).ToString());
        }
        distDown[0] = (raycastMaxLength - distance) / raycastMaxLength;
        // BOTTOM LEFT
        RaycastHit2D hitBottomLeft = Physics2D.Raycast(agent.ownPos, new Vector2(-1f, -1f), raycastMaxLength, rayLayer);  //  + / +
        distance = raycastMaxLength;
        if (hitBottomLeft.collider != null && hitBottomLeft.collider.tag == "HazardCollider") {
            distance = (hitBottomLeft.point - agent.ownPos).magnitude;
        }
        distBottomLeft[0] = (raycastMaxLength - distance) / raycastMaxLength;
        // LEFT
        RaycastHit2D hitLeft = Physics2D.Raycast(agent.ownPos, new Vector2(-1f, 0f), raycastMaxLength, rayLayer);  //  + / +
        distance = raycastMaxLength;
        if (hitLeft.collider != null && hitLeft.collider.tag == "HazardCollider") {
            distance = (hitLeft.point - agent.ownPos).magnitude;
        }
        distLeft[0] = (raycastMaxLength - distance) / raycastMaxLength;
        // TOP LEFT
        RaycastHit2D hitTopLeft = Physics2D.Raycast(agent.ownPos, new Vector2(-1f, 1f), raycastMaxLength, rayLayer);  //  + / +
        distance = raycastMaxLength;
        if (hitTopLeft.collider != null && hitTopLeft.collider.tag == "HazardCollider") {
            distance = (hitTopLeft.point - agent.ownPos).magnitude;
        }
        distTopLeft[0] = (raycastMaxLength - distance) / raycastMaxLength;
        
        
    }
}
