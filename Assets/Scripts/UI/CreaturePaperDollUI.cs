using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreaturePaperDollUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;

    public WidgetAgentStatus widgetAgentStatus;

    public Text textStomachContents;
    public Text textEnergy;
    public Text textHealth;
    public Text textWaste;
   
    //public GameObject panelNewInspect;
    //public Text textNewInspectAgentName;
    public Material newInspectAgentEnergyMat;
    public Material newInspectAgentStaminaMat;
    public Material newInspectAgentStomachMat;
    public Material newInspectAgentAgeMat;
    public Material newInspectAgentHealthMat;
    
    public Material newInspectAgentStateMat;
    
    public Material newInspectAgentWasteMat;
    public Material newInspectAgentBrainMat;

    //public Text textNewInspectLog;
    public Text textVertebrateLifestage;
    public Text textVertebrateStatus;

    public void Tick() {

        //int critterIndex = cameraManager.targetAgentIndex;
        Agent agent = simulationManager.agentsArray[cameraManager.targetAgentIndex]; // simulationManager.agentsArray[critterIndex];

        if (agent.coreModule != null) {
            
            widgetAgentStatus.UpdateBars(agent.coreModule.health,
                                            agent.coreModule.energy,
                                            agent.coreModule.stomachContentsNorm,
                                            agent.currentBiomass,
                                            agent.coreModule.stamina[0]);   
        }        
    }
}
