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
            /*
            textStomachContents.text = "STOMACH " + Mathf.Clamp01(agent.coreModule.stomachContentsNorm * 1f).ToString("F5");
            textEnergy.text = "ENERGY " + (agent.coreModule.energy / agent.currentBiomass).ToString("F5");
            textHealth.text = "HEALTH " + agent.coreModule.healthBody.ToString("F5");
            //textWaste.text = "WASTE " + agent.wasteProducedLastFrame.ToString("F5");

            //textNewInspectLog.text = agent.candidateRef.causeOfDeath + ", " + agent.cooldownFrameCounter + " / " + agent.cooldownDuration; // agent.lastEvent;
            newInspectAgentEnergyMat.SetFloat("_Value", Mathf.Clamp01((agent.coreModule.energy * Mathf.Sqrt(agent.currentBiomass)) * 0.33f));
            newInspectAgentStaminaMat.SetFloat("_Value", Mathf.Clamp01(agent.coreModule.stamina[0] * 1f));
            newInspectAgentStomachMat.SetFloat("_Value", Mathf.Clamp01(agent.coreModule.stomachContentsNorm * 1f));

            newInspectAgentHealthMat.SetFloat("_HealthHead", Mathf.Clamp01(agent.coreModule.healthHead));
            newInspectAgentHealthMat.SetFloat("_HealthBody", Mathf.Clamp01(agent.coreModule.healthBody));
            newInspectAgentHealthMat.SetFloat("_HealthExternal", Mathf.Clamp01(agent.coreModule.healthExternal));
            newInspectAgentAgeMat.SetFloat("_Value", Mathf.Clamp01((float)agent.ageCounter * 0.0005f));
            newInspectAgentAgeMat.SetFloat("_Age", Mathf.RoundToInt(agent.ageCounter * 0.1f));
            int developmentStateID = 0;
            
            if (agent.curLifeStage == Agent.AgentLifeStage.Dead) {
                developmentStateID = 7;              
            }
            if (agent.curLifeStage == Agent.AgentLifeStage.Mature) {

                // state
                if (agent.ageCounter < 1000) {
                    developmentStateID = 1;
                }
                else {
                    if (agent.ageCounter < 10000) {
                        developmentStateID = 2;
                    }
                }

                if (agent.sizePercentage > 0.5f) {
                    developmentStateID = 3;
                }

            }
            newInspectAgentStateMat.SetInt("_StateID", developmentStateID);
            //newInspectAgentBrainMat.SetFloat("_Value", Mathf.Clamp01((float)agent.brain.axonList.Count * 0.05f + (float)agent.brain.neuronList.Count * 0.05f));
            //newInspectAgentWasteMat.SetFloat("_Value", Mathf.Clamp01(agent.wasteProducedLastFrame * 1000f));
            

            string statusStr = "Alive!";
            Color statusColor = Color.white;
            Color healthColor = Color.green;

            if (agent.curLifeStage == Agent.AgentLifeStage.Dead) {
                statusStr = "Dead! (Decaying)";
                statusColor = Color.red;
                healthColor = Color.red;
            }
            if (agent.curLifeStage == Agent.AgentLifeStage.Egg) {
                statusStr = "Egg!";
                healthColor = Color.yellow;
            }
            
            //textVertebrateGen.text = "Gen #" + agent.candidateRef.candidateGenome.bodyGenome.coreGenome.generation.ToString();
            //textVertebrateLifestage.text = "Age: " + (0.1f * agent.ageCounter).ToString("F0");// + ", stateID: " + developmentStateID;
            //textVertebrateLifestage.color = healthColor;
            textVertebrateStatus.text = statusStr; // "activity: " + curActivityID;
               */         

            widgetAgentStatus.UpdateBars((agent.coreModule.healthBody + agent.coreModule.healthHead + agent.coreModule.healthExternal) / 3f,
                                            agent.coreModule.energy,
                                            agent.coreModule.stomachContentsNorm,
                                            agent.currentBiomass,
                                            agent.coreModule.stamina[0]);   
        }        
    }
}
