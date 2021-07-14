using UnityEngine;
using UnityEngine.UI;

public class CreaturePaperDollUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;

    public WidgetAgentStatus widgetAgentStatus;

    #region Unused variables
    public Text textStomachContents;
    public Text textEnergy;
    public Text textHealth;
   
    public Material newInspectAgentEnergyMat;
    public Material newInspectAgentStaminaMat;
    public Material newInspectAgentStomachMat;
    public Material newInspectAgentAgeMat;
    public Material newInspectAgentHealthMat;
    
    public Material newInspectAgentStateMat;
    
    public Material newInspectAgentWasteMat;
    public Material newInspectAgentBrainMat;
        
    #endregion

    public void Tick() {
        Agent agent = simulationManager.agents[cameraManager.targetAgentIndex];
        if (agent.coreModule == null) return;
        
        widgetAgentStatus.UpdateBars(agent.coreModule.health,
                                     agent.coreModule.energy,
                                     agent.coreModule.stomachContentsPercent,
                                     agent.currentBiomass,
                                     agent.coreModule.stamina[0]);           
    }
}
