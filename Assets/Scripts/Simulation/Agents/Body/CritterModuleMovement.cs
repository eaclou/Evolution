using UnityEngine;

public class CritterModuleMovement 
{
    Lookup lookup => Lookup.instance;
    NeuralMap neuralMap => lookup.neuralMap;

    public int parentID;
    public int inno;

    public float horsepower;
    public float turnRate;
    
    public float[] ownVelX;
    public float[] ownVelY;

    public float[] facingDirX; 
    public float[] facingDirY; 

    public float[] throttleX;
    public float[] throttleY;
    public float[] dash;

    public float smallestCreatureBaseSpeed = 75f;
    public float largestCreatureBaseSpeed = 150f;

    public float smallestCreatureBaseTurnRate = 32f;
    public float largestCreatureBaseTurnRate = 0.05f;

    public float accelBonus = 1f;
    public float speedBonus = 1f;
    public float turnBonus = 1f;
	
    public CritterModuleMovement(AgentGenome agentGenome, CritterModuleMovementGenome genome) {
        Initialize(agentGenome, genome);
    }

    public void Initialize(AgentGenome agentGenome, CritterModuleMovementGenome genome) {

        horsepower = genome.horsepower;               
        turnRate = genome.turnRate;
        
        ownVelX = new float[1]; // 20
        ownVelY = new float[1]; // 21

        facingDirX = new float[1];  // 207
        facingDirY = new float[1];  // 208
        
        throttleX = new float[1]; // 0
        throttleY = new float[1]; // 1

        dash = new float[1]; // 2
        
        parentID = genome.parentID;
        inno = genome.inno;

        float invAspectRatio = agentGenome.bodyGenome.coreGenome.creatureAspectRatio;

        speedBonus = Mathf.Lerp(0.7f, 1.4f, 1f - invAspectRatio);
    }

    public void MapNeuron(NID nid, Neuron neuron) 
    {
        if (inno != nid.moduleID) return;
        neuron.neuronType = GetIO(nid.neuronID);    
        neuron.currentValue = GetNeuralValue(nid.neuronID);
        
        /*if (inno == nid.moduleID) {            
            if (nid.neuronID == 20) {
                neuron.currentValue = ownVelX;
                neuron.neuronType = NeuronType.In;
            }
            if (nid.neuronID == 21) {
                neuron.currentValue = ownVelY;
                neuron.neuronType = NeuronType.In;
            }
            if (nid.neuronID == 207) {
                neuron.currentValue = facingDirX;
                neuron.neuronType = NeuronType.In;
            }
            if (nid.neuronID == 208) {
                neuron.currentValue = facingDirY;
                neuron.neuronType = NeuronType.In;
            }

            if (nid.neuronID == 100) {
                neuron.currentValue = throttleX;
                neuron.neuronType = NeuronType.Out;
            }
            if (nid.neuronID == 101) {
                neuron.currentValue = throttleY;
                neuron.neuronType = NeuronType.Out;
            }
            if (nid.neuronID == 102) {
                neuron.currentValue = dash;
                neuron.neuronType = NeuronType.Out;
            }
        }*/
    }
    
    // WPP: some assignments break pattern established in other modules
    NeuronType GetIO(int neuronID)
    {
        switch (neuronID)
        {
            case 207: return NeuronType.In;
            case 208: return NeuronType.In;
            default: return neuralMap.GetIO(neuronID);
        }
    }
    
    float[] GetNeuralValue(int neuronID)
    {
        switch(neuronID)
        {
            case 20: return ownVelX;
            case 21: return ownVelY;
            case 207: return facingDirX;
            case 208: return facingDirY;
            case 100: return throttleX;
            case 101: return throttleY;
            case 102: return dash;
            default: return null;
        }
    }

    public void Tick(Agent agent) 
    {
        //Vector2 ownPos = new Vector2(agent.rigidbodiesArray[0].transform.localPosition.x, agent.rigidbodiesArray[0].transform.localPosition.y);
        //Vector2 ownVel = new Vector2(rigidbody.velocity.x, rigidbody.velocity.y);

        ownVelX[0] = agent.ownVel.x / 15f;
        ownVelY[0] = agent.ownVel.y / 15f;

        facingDirX[0] = agent.facingDirection.x;
        facingDirY[0] = agent.facingDirection.y;
    }
}
