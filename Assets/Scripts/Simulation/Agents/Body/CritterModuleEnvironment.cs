
public class CritterModuleEnvironment 
{
    public int parentID;
    public int inno;

    public CritterModuleEnvironmentSensorsGenome genome;

    public float[] waterDepth;
    public float[] waterVelX;
    public float[] waterVelY;

    public float[] waterGradX;  // plan B
    public float[] waterGradY;  // plan B

    public float[] depthGradX; // start up and go clockwise!    
    public float[] depthGradY;
    public float[] depthSouth;
    public float[] depthWest;

    public float[] velTopRightX;
    public float[] velTopLeftY;    
    public float[] velBottomLeftX;
    public float[] velBottomRightY;
    
    public CritterModuleEnvironment(CritterModuleEnvironmentSensorsGenome genome, Agent agent) {
        Initialize(genome, agent);
    }

    public void Initialize(CritterModuleEnvironmentSensorsGenome genome, Agent agent) {
        this.genome = genome;

        waterDepth = new float[1];
        waterVelX = new float[1];
        waterVelY = new float[1];

        depthGradX = new float[1]; 
        depthGradY = new float[1]; 
        depthSouth = new float[1];       
        depthWest = new float[1]; 

        velTopRightX = new float[1]; 
        velTopLeftY = new float[1]; 
        velBottomLeftX = new float[1];
        velBottomRightY = new float[1]; 
        
        this.parentID = genome.parentID;
        this.inno = genome.inno;
    }

    public void MapNeuron(NID nid, Neuron neuron) {
        if (inno == nid.moduleID) {
            if (nid.neuronID == 1) {                
                neuron.currentValue = waterDepth;
                neuron.neuronType = NeuronType.In;
            }
            if (nid.neuronID == 2) {
                neuron.currentValue = waterVelX;
                neuron.neuronType = NeuronType.In;
            }
            if (nid.neuronID == 3) {
                neuron.currentValue = waterVelY;
                neuron.neuronType = NeuronType.In;
            }
            if (nid.neuronID == 4) {
                neuron.currentValue = depthGradX;
                neuron.neuronType = NeuronType.In;
            }
            if (nid.neuronID == 5) {
                neuron.currentValue = depthGradY;
                neuron.neuronType = NeuronType.In;
            }
            if (nid.neuronID == 6) {
                neuron.currentValue = depthSouth;
                neuron.neuronType = NeuronType.In;
            }
            if (nid.neuronID == 7) {
                neuron.currentValue = depthWest;
                neuron.neuronType = NeuronType.In;
            }

            if (nid.neuronID == 8) {
                neuron.currentValue = velTopRightX;
                neuron.neuronType = NeuronType.In;
            }            
            if (nid.neuronID == 9) {
                neuron.currentValue = velTopLeftY;
                neuron.neuronType = NeuronType.In;
            }            
            if (nid.neuronID == 10) {
                neuron.currentValue = velBottomLeftX;
                neuron.neuronType = NeuronType.In;
            }            
            if (nid.neuronID == 11) {
                neuron.currentValue = velBottomRightY;
                neuron.neuronType = NeuronType.In;
            }
        }
    }

    public void Tick(Agent agent) {
        if(genome.useWaterStats) {
            waterDepth[0] = agent.waterDepth;
            waterVelX[0] = agent.avgFluidVel.x * 10f;
            waterVelY[0] = agent.avgFluidVel.y * 10f; // *** *10f to get closer to 0-1 range since values are very low

            depthGradX[0] = agent.depthGradient.x;
            depthGradY[0] = agent.depthGradient.y;
            //depthEast[0] = agent.depthEast;
            //depthSouth[0] = agent.depthSouth;
            //depthWest[0] = agent.depthWest;
        }

        //int rayLayer = LayerMask.GetMask("EnvironmentCollision");
        //float raycastMaxLength = genome.maxRange;        
        //Debug.Log(LayerMask.GetMask("EnvironmentCollision"));
        //Debug.Log(mask.ToString());
        /*if(genome.useCardinals) {
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
        }*/
    }
}
