using UnityEngine;

public class CritterModuleThreats : IBrainModule
{
    public float[] enemyPosX;
    public float[] enemyPosY;
    public float[] enemyVelX;
    public float[] enemyVelY;
    public float[] enemyDirX;
    public float[] enemyDirY;

    public float[] enemyRelSize;
    public float[] enemyHealth;
    public float[] enemyGrowthStage;
    public float[] enemyThreatRating;

    public CritterModuleThreats() {
        enemyPosX = new float[1];
        enemyPosY = new float[1];
        enemyVelX = new float[1];
        enemyVelY = new float[1];
        enemyDirX = new float[1];
        enemyDirY = new float[1];

        enemyRelSize = new float[1];
        enemyHealth = new float[1];
        enemyGrowthStage = new float[1];
        enemyThreatRating = new float[1];
    }

    public void SetNeuralValue(Neuron neuron) {
        neuron.currentValues = (float[])GetType().GetField(neuron.name).GetValue(this);
    }
    
    public void Tick(Agent agent) 
    {
        Vector2 enemyPos = Vector2.zero;
        Vector2 enemyDir = Vector2.zero;
        Vector2 enemyVel = Vector2.zero;
        
        if(agent.coreModule.nearestEnemyAgent) 
        {
            enemyPos = new Vector2(agent.coreModule.nearestEnemyAgent.bodyRigidbody.transform.localPosition.x - agent.ownPos.x, agent.coreModule.nearestEnemyAgent.bodyRigidbody.transform.localPosition.y - agent.ownPos.y);
            enemyDir = enemyPos.normalized;
            enemyVel = new Vector2(agent.coreModule.nearestEnemyAgent.bodyRigidbody.velocity.x, agent.coreModule.nearestEnemyAgent.bodyRigidbody.velocity.y);

            float ownSize = 1f; // currentBodySize.x;
            float enemySize = 1f; // nearestEnemyAgent.coreModule.currentBodySize.x;

            if(ownSize != 0f && enemySize != 0) 
            {
                float sizeRatio = enemySize / ownSize - 1f; 
                
                if(enemySize < ownSize) 
                {
                    sizeRatio = -1f * (ownSize / enemySize - 1f);
                }
                enemyRelSize[0] = TransferFunctions.Evaluate(TransferFunctions.TransferFunction.RationalSigmoid, sizeRatio);  // smaller creatures negative values, larger creatures positive, 0 = same size
            }

            enemyHealth[0] = 0f;
            enemyGrowthStage[0] = 0f;
            
            if(agent.coreModule != null && agent.coreModule.nearestEnemyAgent) 
            {
                if (agent.coreModule.nearestEnemyAgent.coreModule != null) 
                {
                    enemyHealth[0] = agent.coreModule.nearestEnemyAgent.coreModule.hitPoints[0];              
                }
                
                enemyGrowthStage[0] = agent.coreModule.nearestEnemyAgent.sizePercentage;
            }
            
            //float threat = 1f;
            //if(agent.coreModule.nearestEnemyAgent.mouthRef.isPassive) {
            //    threat = 0f;
            //}
            enemyThreatRating[0] = 0f; // threat;
        }

        enemyPosX[0] = enemyPos.x / 20f;
        enemyPosY[0] = enemyPos.y / 20f;        
        enemyVelX[0] = (enemyVel.x - agent.ownVel.x) / 15f;
        enemyVelY[0] = (enemyVel.y - agent.ownVel.y) / 15f;
        enemyDirX[0] = enemyDir.x;
        enemyDirY[0] = enemyDir.y;
    }
}
