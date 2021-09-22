using UnityEngine;

public class CreatureBrainActivityUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    EnvironmentFluidManager fluidManager => EnvironmentFluidManager.instance;
    SelectionManager selectionManager => SelectionManager.instance;

    public AgentBehaviorOneHot agentBehaviorOneHot;

    public Material newInspectAgentCurActivityMat;
    public Material newInspectAgentThrottleMat;
    public Material newInspectAgentCommsMat;

    private int callTickCounter = 90;

    private int critterIndex;
    private Agent agent;

    public void Tick() {
        critterIndex = cameraManager.targetAgentIndex;
        agent = simulationManager.agents[critterIndex];
        
        if (agent.coreModule == null || agent.communicationModule == null)
            return;

        if(agent.candidateRef.candidateID == selectionManager.focusedCandidate.candidateID) {
            UpdateBrainLive(agent);
        }
        else {
            UpdateBrainFossil(selectionManager.focusedCandidate);
        }
    }
    
    private void UpdateBrainLive(Agent agent) {
        int curActivityID = agent.GetActivityID();

        newInspectAgentCurActivityMat.SetInt("_CurActivityID", curActivityID);
        newInspectAgentThrottleMat.SetFloat("_ThrottleX", Mathf.Clamp01(agent.smoothedThrottle.x));
        newInspectAgentThrottleMat.SetFloat("_ThrottleY", Mathf.Clamp01(agent.smoothedThrottle.y));
        newInspectAgentThrottleMat.SetTexture("_VelocityTex", fluidManager._VelocityPressureDivergenceMain);
        newInspectAgentThrottleMat.SetFloat("_AgentCoordX", agent.ownPos.x / SimulationManager._MapSize);
        newInspectAgentThrottleMat.SetFloat("_AgentCoordY", agent.ownPos.y / SimulationManager._MapSize);
        
        agentBehaviorOneHot.UpdateBars(agent);

        // * WPP: what concept does this condition represent? -> convert to getter in Agent
        callTickCounter = agent.communicationModule.outComm3[0] > 0.25f ? 
            Mathf.Min(200, callTickCounter++) : 
            Mathf.Max(0, callTickCounter--);
        
        agentBehaviorOneHot.UpdateExtras(agent);
    }
    
    private void UpdateBrainFossil(CandidateAgentData candidate) {
        newInspectAgentCurActivityMat.SetInt("_CurActivityID", 0);
        newInspectAgentThrottleMat.SetFloat("_ThrottleX", 0f);
        newInspectAgentThrottleMat.SetFloat("_ThrottleY", 0f);
        newInspectAgentThrottleMat.SetTexture("_VelocityTex", fluidManager._VelocityPressureDivergenceMain);
        newInspectAgentThrottleMat.SetFloat("_AgentCoordX", 0f);
        newInspectAgentThrottleMat.SetFloat("_AgentCoordY", 0f);
        
        agentBehaviorOneHot.UpdateBars(candidate);

        // * WPP: what concept does this condition represent? -> convert to getter in Agent
        callTickCounter = agent.communicationModule.outComm3[0] > 0.25f ? 
            Mathf.Min(200, callTickCounter++) : 
            Mathf.Max(0, callTickCounter--);
        
        agentBehaviorOneHot.UpdateExtras(candidate);
    }

    public void TickTooltips() {

    }
}
