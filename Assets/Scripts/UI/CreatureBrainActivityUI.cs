using UnityEngine;

public class CreatureBrainActivityUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;

    public AgentBehaviorOneHot agentBehaviorOneHot;

    public Material newInspectAgentCurActivityMat;
    public Material newInspectAgentThrottleMat;
    public Material newInspectAgentCommsMat;

    private int callTickCounter = 90;

    private int critterIndex;
    private Agent agent;

    public void Tick() {
        critterIndex = cameraManager.targetAgentIndex;
        agent = simulationManager.agentsArray[critterIndex];

        if (agent.coreModule == null || agent.communicationModule == null)
            return;

        int curActivityID = agent.GetActivityID();

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

        // * WPP: what concept does this condition represent? -> convert to getter in Agent
        callTickCounter = agent.communicationModule.outComm3[0] > 0.25f ? 
            Mathf.Min(200, callTickCounter++) : 
            Mathf.Max(0, callTickCounter--);
        
        agentBehaviorOneHot.UpdateExtras(agent);
    
    }

}
