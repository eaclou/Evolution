using UnityEngine;

public class CritterModuleMovement : IBrainModule
{
    public BrainModuleID moduleID => BrainModuleID.Movement;

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
    
    public Vector2 throttle => new Vector2(throttleX[0], throttleY[0]);
	
    public CritterModuleMovement(AgentGenome agentGenome) 
    {
        ownVelX = new float[1];
        ownVelY = new float[1];
        facingDirX = new float[1];
        facingDirY = new float[1];
        throttleX = new float[1];
        throttleY = new float[1];
        dash = new float[1];
        
        float invAspectRatio = agentGenome.bodyGenome.coreGenome.creatureAspectRatio;
        speedBonus = Mathf.Lerp(0.7f, 1.4f, 1f - invAspectRatio);
    }

    public void GetNeuralValue(MetaNeuron data, Neuron neuron)
    {
        if (moduleID != data.moduleID) return;
        neuron.currentValues = (float[])GetType().GetField(neuron.name).GetValue(this);
        //GetNeuralValue(neuron.name);
    }

    /*float[] GetNeuralValue(string neuronID)
    {
        switch(neuronID)
        {
            case "ownVelX": return ownVelX;
            case "ownVelY": return ownVelY;
            case "facingDirX": return facingDirX;
            case "facingDirY": return facingDirY;
            case "throttleX": return throttleX;
            case "throttleY": return throttleY;
            case "dash": return dash;
            default: return null;
        }
    }*/

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
