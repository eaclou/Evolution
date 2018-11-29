using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterModuleEnvironment {

	public int parentID;
    public int inno;

    public CritterModuleEnvironmentSensorsGenome genome;

    public float[] waterDepth;
    public float[] waterVelX;
    public float[] waterVelY;

    public float[] distUp; // start up and go clockwise!    
    public float[] distRight;
    public float[] distDown;
    public float[] distLeft;

    public float[] distTopRight;
    public float[] distBottomRight;    
    public float[] distBottomLeft;
    public float[] distTopLeft;
    
    public CritterModuleEnvironment() {
                
    }

    public void Initialize(CritterModuleEnvironmentSensorsGenome genome, Agent agent) {
        this.genome = genome;

        waterDepth = new float[1];
        waterVelX = new float[1];
        waterVelY = new float[1];

        distUp = new float[1]; 
        distRight = new float[1]; 
        distDown = new float[1];       
        distLeft = new float[1]; 

        distTopRight = new float[1]; 
        distBottomRight = new float[1]; 
        distBottomLeft = new float[1];
        distTopLeft = new float[1]; 
        
        this.parentID = genome.parentID;
        this.inno = genome.inno;
    }

    public void MapNeuron(NID nid, Neuron neuron) {
        if (inno == nid.moduleID) {
            if (nid.neuronID == 1) {
                neuron.currentValue = waterDepth;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 2) {
                neuron.currentValue = waterVelX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 3) {
                neuron.currentValue = waterVelY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 4) {
                neuron.currentValue = distUp;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 5) {
                neuron.currentValue = distRight;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 6) {
                neuron.currentValue = distDown;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 7) {
                neuron.currentValue = distLeft;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }

            if (nid.neuronID == 8) {
                neuron.currentValue = distTopRight;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }            
            if (nid.neuronID == 9) {
                neuron.currentValue = distBottomRight;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }            
            if (nid.neuronID == 10) {
                neuron.currentValue = distBottomLeft;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }            
            if (nid.neuronID == 11) {
                neuron.currentValue = distTopLeft;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
        }
    }

    public void Tick(Agent agent) {
        if(genome.useWaterStats) {
            waterDepth[0] = agent.depth;
            waterVelX[0] = agent.avgFluidVel.x * 10f;
            waterVelY[0] = agent.avgFluidVel.y * 10f; // *** *10f to get closer to 0-1 range since values are very low
        }

        int rayLayer = LayerMask.GetMask("EnvironmentCollision");
        float raycastMaxLength = genome.maxRange;        
        //Debug.Log(LayerMask.GetMask("EnvironmentCollision"));
        //Debug.Log(mask.ToString());
        if(genome.useCardinals) {
            // TOP
            
            RaycastHit2D hitTop = Physics2D.Raycast(agent.ownPos, Vector2.up, raycastMaxLength, rayLayer);  // UP
            float distance = raycastMaxLength;
            if (hitTop.collider != null && hitTop.collider.tag == "HazardCollider") {
                distance = (hitTop.point - agent.ownPos).magnitude;            
            }
            distUp[0] = (raycastMaxLength - distance) / raycastMaxLength;  // Mathf.Abs(40f - yPos) / 40f;        
            // RIGHT
            RaycastHit2D hitRight = Physics2D.Raycast(agent.ownPos, new Vector2(1f, 0f), raycastMaxLength, rayLayer);  //  + / +
            distance = raycastMaxLength;
            if (hitRight.collider != null && hitRight.collider.tag == "HazardCollider") {
                distance = (hitRight.point - agent.ownPos).magnitude;
            }
            distRight[0] = (raycastMaxLength - distance) / raycastMaxLength;
            // BOTTOM
            RaycastHit2D hitBottom = Physics2D.Raycast(agent.ownPos, new Vector2(0f, -1f), raycastMaxLength, rayLayer);  //  + / +
            distance = raycastMaxLength;
            if (hitBottom.collider != null && hitBottom.collider.tag == "HazardCollider") {
                distance = (hitBottom.point - agent.ownPos).magnitude;
                //Debug.Log("HIT BOTTOM! " + ((raycastMaxLength - distance) / raycastMaxLength).ToString());
            }
            distDown[0] = (raycastMaxLength - distance) / raycastMaxLength;
            // LEFT
            RaycastHit2D hitLeft = Physics2D.Raycast(agent.ownPos, new Vector2(-1f, 0f), raycastMaxLength, rayLayer);  //  + / +
            distance = raycastMaxLength;
            if (hitLeft.collider != null && hitLeft.collider.tag == "HazardCollider") {
                distance = (hitLeft.point - agent.ownPos).magnitude;
            }
            distLeft[0] = (raycastMaxLength - distance) / raycastMaxLength;

        }
        if(genome.useDiagonals) {
            // TOP RIGHT
            RaycastHit2D hitTopRight = Physics2D.Raycast(agent.ownPos, new Vector2(1f,1f), raycastMaxLength, rayLayer);  //  + / +
            float distance = raycastMaxLength;
            if (hitTopRight.collider != null && hitTopRight.collider.tag == "HazardCollider") {
                distance = (hitTopRight.point - agent.ownPos).magnitude;
            }
            distTopRight[0] = (raycastMaxLength - distance) / raycastMaxLength;        
            // BOTTOM RIGHT
            RaycastHit2D hitBottomRight = Physics2D.Raycast(agent.ownPos, new Vector2(1f, -1f), raycastMaxLength, rayLayer);  //  + / +
            distance = raycastMaxLength;
            if (hitBottomRight.collider != null && hitBottomRight.collider.tag == "HazardCollider") {
                distance = (hitBottomRight.point - agent.ownPos).magnitude;
            }
            distBottomRight[0] = (raycastMaxLength - distance) / raycastMaxLength;        
            // BOTTOM LEFT
            RaycastHit2D hitBottomLeft = Physics2D.Raycast(agent.ownPos, new Vector2(-1f, -1f), raycastMaxLength, rayLayer);  //  + / +
            distance = raycastMaxLength;
            if (hitBottomLeft.collider != null && hitBottomLeft.collider.tag == "HazardCollider") {
                distance = (hitBottomLeft.point - agent.ownPos).magnitude;
            }
            distBottomLeft[0] = (raycastMaxLength - distance) / raycastMaxLength;        
            // TOP LEFT
            RaycastHit2D hitTopLeft = Physics2D.Raycast(agent.ownPos, new Vector2(-1f, 1f), raycastMaxLength, rayLayer);  //  + / +
            distance = raycastMaxLength;
            if (hitTopLeft.collider != null && hitTopLeft.collider.tag == "HazardCollider") {
                distance = (hitTopLeft.point - agent.ownPos).magnitude;
            }
            distTopLeft[0] = (raycastMaxLength - distance) / raycastMaxLength;  
        }
    }
}
