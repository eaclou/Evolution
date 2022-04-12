using UnityEngine;
using UnityEngine.UI;

public class CreaturePaperDollUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    Lookup lookup => Lookup.instance;
    SelectionManager selectionManager => SelectionManager.instance;

    
    Agent agent => SelectionManager.instance.currentSelection.agent;
    //CritterModuleCore coreModule => agent.coreModule;

    //public WidgetAgentStatus widgetAgentStatus;
    [SerializeField]
    GameObject panelStatusBars;

    [SerializeField]
    TooltipUI tooltipState;
    [SerializeField]
    TooltipUI tooltipHealth;
    [SerializeField]
    TooltipUI tooltipEnergy;
    [SerializeField]
    TooltipUI tooltipStomachFood;
    [SerializeField]
    TooltipUI tooltipWaste;

    [SerializeField]
    Image tooltipImage;
    [SerializeField]
    Image imageMeterBarHealth;
    [SerializeField]
    Image imageMeterBarEnergy;
    [SerializeField]
    Image imageMeterBarStomach;
    [SerializeField]
    Image imageMeterBarWaste;

    [SerializeField]
    Image imageGrowthPercent;
    
    public void Tick() {
        if (agent == null || agent.coreModule == null) return;
        CandidateAgentData candidate = selectionManager.currentSelection.candidate;
        if(agent.curLifeStage != AgentLifeStage.Mature) {
            panelStatusBars.SetActive(false);
        }
        else {
            panelStatusBars.SetActive(true);
        }

        if(agent.candidateRef.candidateID == candidate.candidateID) {
            
            var lifeStageData = lookup.GetAgentLifeStageData(agent.curLifeStage, agent.isYoung);

            
            tooltipState.tooltipString = lifeStageData.stateName + "\nSize: " + (agent.currentBiomass / agent.fullsizeBiomass * 100f).ToString("F0") + "% Grown";// + agent.currentBiomass.ToString("F3");
            tooltipImage.sprite = lifeStageData.icon;

            tooltipHealth.tooltipString = "Health: " + (agent.coreModule.health * 100f).ToString("F0") + "%";          
            imageMeterBarHealth.transform.localScale = new Vector3(1f, agent.coreModule.health, 1f);
        
            tooltipEnergy.tooltipString = "Energy: " + agent.coreModule.energy.ToString("F0");
            imageMeterBarEnergy.transform.localScale = new Vector3(1f, Mathf.Clamp01(agent.coreModule.energy * 0.005f), 1f);
        
            tooltipStomachFood.tooltipString = "Stomach: " + (agent.coreModule.stomachContentsPercent * 100f).ToString("F0");
            imageMeterBarStomach.transform.localScale = new Vector3(1f, agent.coreModule.stomachContentsPercent, 1f);
        
            tooltipWaste.tooltipString = "Waste: (tbd)"; // + agent.coreModule.was.ToString("F0");
            imageMeterBarWaste.transform.localScale = new Vector3(1f, 0f, 1f);

            imageGrowthPercent.transform.localScale = Vector3.one * agent.sizePercentage;
        }
        else {
            
            var lifeStageData = lookup.GetAgentLifeStageData(AgentLifeStage.Dead);
            tooltipState.tooltipString = lifeStageData.stateName + ", Age: " + candidate.performanceData.totalTicksAlive; // + ", Size: " + candidate.performanceData.grow.ToString("F3");
            tooltipImage.sprite = lifeStageData.icon;

            tooltipHealth.tooltipString = "Health:";
            imageMeterBarHealth.transform.localScale = new Vector3(1f, 0f, 1f);
        
            tooltipEnergy.tooltipString = "Energy:";
            imageMeterBarEnergy.transform.localScale = new Vector3(1f, 0f, 1f);
        
            tooltipStomachFood.tooltipString = "Stomach:";
            imageMeterBarStomach.transform.localScale = new Vector3(1f, 0f, 1f);
        
            tooltipWaste.tooltipString = "Waste: (tbd)";
            imageMeterBarWaste.transform.localScale = new Vector3(1f, 0f, 1f);
        
            
        }
        
        
    }
}
