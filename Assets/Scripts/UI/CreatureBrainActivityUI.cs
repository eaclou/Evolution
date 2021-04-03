using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatureBrainActivityUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;

    public AgentBehaviorOneHot agentBehaviorOneHot;

    public Material newInspectAgentCurActivityMat;
    public Material newInspectAgentThrottleMat;
    public Material newInspectAgentCommsMat;

    private int callTickCounter = 90;

    private void UpdateUI() {

        int critterIndex = cameraManager.targetAgentIndex;
        Agent agent = simulationManager.agentsArray[critterIndex];

        if (agent.coreModule != null) {

            
            int curActivityID = 0;
            if (agent.curLifeStage == Agent.AgentLifeStage.Dead) {
                
                //curActivityID = 7;
            }
            if (agent.curLifeStage == Agent.AgentLifeStage.Mature) {

                // curActivity
                if (agent.isPregnantAndCarryingEggs) {
                    curActivityID = 6;
                }
                if (agent.isFeeding) {
                    curActivityID = 1;
                }
                if (agent.isAttacking) {
                    curActivityID = 2;
                }
                if (agent.isDashing) {
                    curActivityID = 3;
                }
                if (agent.isDefending) {
                    curActivityID = 4;
                }
                if (agent.isResting) {
                    curActivityID = 5;
                }

                if (agent.isCooldown) {
                    curActivityID = 7;
                }
            }
            
            newInspectAgentCurActivityMat.SetInt("_CurActivityID", curActivityID);
            newInspectAgentThrottleMat.SetFloat("_ThrottleX", Mathf.Clamp01(agent.smoothedThrottle.x));
            newInspectAgentThrottleMat.SetFloat("_ThrottleY", Mathf.Clamp01(agent.smoothedThrottle.y));
            newInspectAgentThrottleMat.SetTexture("_VelocityTex", simulationManager.environmentFluidManager._VelocityPressureDivergenceMain);
            newInspectAgentThrottleMat.SetFloat("_AgentCoordX", agent.ownPos.x / SimulationManager._MapSize);
            newInspectAgentThrottleMat.SetFloat("_AgentCoordY", agent.ownPos.y / SimulationManager._MapSize);

            agentBehaviorOneHot.UpdateBars( agent.coreModule.healEffector[0],
                                            agent.coreModule.dashEffector[0],
                                            agent.coreModule.defendEffector[0],
                                            agent.coreModule.mouthFeedEffector[0],
                                            agent.coreModule.mouthAttackEffector[0],
                                            agent.communicationModule.outComm0[0], agent.isCooldown);

            if(agent.communicationModule.outComm3[0] > 0.25f) {
                callTickCounter = Mathf.Min(200, callTickCounter++);
                            
            }
            else {
                callTickCounter = Mathf.Max(0, callTickCounter--);
            }
               
            agentBehaviorOneHot.UpdateExtras(agent);
        }        
    }

}
