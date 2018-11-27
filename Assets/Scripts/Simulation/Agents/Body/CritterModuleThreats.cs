using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterModuleThreats {

	public int parentID;
    public int inno;

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
               
    }

    public void Initialize(CritterModuleThreatSensorsGenome genome, Agent agent) {
        enemyPosX = new float[1]; // 14
        enemyPosY = new float[1]; // 15
        enemyVelX = new float[1]; // 16
        enemyVelY = new float[1]; // 17
        enemyDirX = new float[1]; // 18
        enemyDirY = new float[1]; // 19

        enemyRelSize = new float[1];  // 200
        enemyHealth = new float[1];   // 201
        enemyGrowthStage = new float[1]; // 202
        enemyThreatRating = new float[1]; // 203

        this.parentID = genome.parentID;
        this.inno = genome.inno; 
    }

    public void MapNeuron(NID nid, Neuron neuron) {
        if (inno == nid.moduleID) {
            if (nid.neuronID == 14) {
                neuron.currentValue = enemyPosX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 15) {
                neuron.currentValue = enemyPosY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 16) {
                neuron.currentValue = enemyVelX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 17) {
                neuron.currentValue = enemyVelY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 18) {
                neuron.currentValue = enemyDirX;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 19) {
                neuron.currentValue = enemyDirY;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }

            if (nid.neuronID == 200) {
                neuron.currentValue = enemyRelSize;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 201) {
                neuron.currentValue = enemyHealth;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 202) {
                neuron.currentValue = enemyGrowthStage;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
            if (nid.neuronID == 203) {
                neuron.currentValue = enemyThreatRating;
                neuron.neuronType = NeuronGenome.NeuronType.In;
            }
        }
    }

    public void Tick(Agent agent) {
        Vector2 enemyPos = Vector2.zero;
        Vector2 enemyDir = Vector2.zero;
        Vector2 enemyVel = Vector2.zero;
        if(agent.coreModule.nearestEnemyAgent != null) {
            enemyPos = new Vector2(agent.coreModule.nearestEnemyAgent.bodyRigidbody.transform.localPosition.x - agent.ownPos.x, agent.coreModule.nearestEnemyAgent.bodyRigidbody.transform.localPosition.y - agent.ownPos.y);
            enemyDir = enemyPos.normalized;
            enemyVel = new Vector2(agent.coreModule.nearestEnemyAgent.bodyRigidbody.velocity.x, agent.coreModule.nearestEnemyAgent.bodyRigidbody.velocity.y);

            float ownSize = 1f; // currentBodySize.x;
            float enemySize = 1f; // nearestEnemyAgent.coreModule.currentBodySize.x;

            if(ownSize != 0f && enemySize != 0) {
                float sizeRatio = enemySize / ownSize - 1f; 
                if(enemySize < ownSize) {
                    sizeRatio = -1f * (ownSize / enemySize - 1f);
                }
                enemyRelSize[0] = TransferFunctions.Evaluate(TransferFunctions.TransferFunction.RationalSigmoid, sizeRatio);  // smaller creatures negative values, larger creatures positive, 0 = same size
            }
            else {

            }            

            enemyHealth[0] = agent.coreModule.nearestEnemyAgent.coreModule.hitPoints[0];
            enemyGrowthStage[0] = agent.coreModule.nearestEnemyAgent.sizePercentage;

            float threat = 1f;
            if(agent.coreModule.nearestEnemyAgent.mouthRef.isPassive) {
                threat = 0f;
            }
            enemyThreatRating[0] = threat;
        }

        enemyPosX[0] = enemyPos.x / 20f;
        enemyPosY[0] = enemyPos.y / 20f;        
        enemyVelX[0] = (enemyVel.x - agent.ownVel.x) / 15f;
        enemyVelY[0] = (enemyVel.y - agent.ownVel.y) / 15f;
        enemyDirX[0] = enemyDir.x;
        enemyDirY[0] = enemyDir.y;
    }
}
