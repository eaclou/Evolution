using UnityEngine;

public class CreatureBrainActivityUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;

    public AgentBehaviorOneHot agentBehaviorOneHot;

    //*** EAC REFACTOR THIS!!! *** first pass brute force approach just to get the Tooltips working initially!!!! ***
    /*[SerializeField]
    public TooltipUI sensorFoodPlant;
    [SerializeField]
    public TooltipUI sensorFoodMicrobe;
    [SerializeField]
    public TooltipUI sensorFoodEgg;
    [SerializeField]
    public TooltipUI sensorFoodAnimal;
    [SerializeField]
    public TooltipUI sensorFoodCorpse;
    [SerializeField]
    public TooltipUI sensorFriend;
    [SerializeField]
    public TooltipUI sensorFoe;
    [SerializeField]
    public TooltipUI sensorWalls;
    [SerializeField]
    public TooltipUI sensorWater;
    [SerializeField]
    public TooltipUI sensorInternals;
    [SerializeField]
    public TooltipUI sensorContact;
    [SerializeField]
    public TooltipUI sensorCommsIn;
    [SerializeField]
    public TooltipUI effectorBite;
    [SerializeField]
    public TooltipUI effectorAttack;
    [SerializeField]
    public TooltipUI effectorDefend;
    [SerializeField]
    public TooltipUI effectorDash;
    [SerializeField]
    public TooltipUI effectorRest;
    [SerializeField]
    public TooltipUI effectorComms0;
    [SerializeField]
    public TooltipUI effectorComms1;
    [SerializeField]
    public TooltipUI effectorComms2;
    [SerializeField]
    public TooltipUI effectorComms3;
    */
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

        int curActivityID = agent.GetActivityID();

        newInspectAgentCurActivityMat.SetInt("_CurActivityID", curActivityID);
        newInspectAgentThrottleMat.SetFloat("_ThrottleX", Mathf.Clamp01(agent.smoothedThrottle.x));
        newInspectAgentThrottleMat.SetFloat("_ThrottleY", Mathf.Clamp01(agent.smoothedThrottle.y));
        newInspectAgentThrottleMat.SetTexture("_VelocityTex", simulationManager.environmentFluidManager._VelocityPressureDivergenceMain);
        newInspectAgentThrottleMat.SetFloat("_AgentCoordX", agent.ownPos.x / SimulationManager._MapSize);
        newInspectAgentThrottleMat.SetFloat("_AgentCoordY", agent.ownPos.y / SimulationManager._MapSize);
        
        agentBehaviorOneHot.UpdateBars(agent);

        // * WPP: what concept does this condition represent? -> convert to getter in Agent
        callTickCounter = agent.communicationModule.outComm3[0] > 0.25f ? 
            Mathf.Min(200, callTickCounter++) : 
            Mathf.Max(0, callTickCounter--);
        
        agentBehaviorOneHot.UpdateExtras(agent);
    
    }

    public void TickTooltips() {

    }
}
