using UnityEngine;
using UnityEngine.UI;

public class CreaturePaperDollUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;

    public WidgetAgentStatus widgetAgentStatus;

    public TooltipUI tooltipState;
    public TooltipUI tooltipHealth;
    public TooltipUI tooltipEnergy;
    public TooltipUI tooltipStomachFood;
    public TooltipUI tooltipWaste;
    
    public void Tick() {
        Agent agent = simulationManager.agents[cameraManager.targetAgentIndex];
        if (agent.coreModule == null) return;
        
        widgetAgentStatus.UpdateBars(agent); 
        
        string lifeStage = agent.curLifeStage.ToString();
        if(agent.curLifeStage == Agent.AgentLifeStage.Mature) {
            if(agent.isSexuallyMature) {
                lifeStage = "Mature";
            }
            else {
                lifeStage = "Young";
            }
        }
        tooltipState.tooltipString = lifeStage + ", Age: " + agent.ageCounter + ", Size: " + agent.currentBiomass.ToString("F3");

        tooltipHealth.tooltipString = "Health: " + (agent.coreModule.health * 100f).ToString("F0") + "%";
        tooltipEnergy.tooltipString = "Energy: " + agent.coreModule.energy.ToString("F0");
        tooltipStomachFood.tooltipString = "Stomach: " + (agent.coreModule.stomachContentsPercent * 100f).ToString("F0");
        tooltipWaste.tooltipString = "Waste: (tbd)"; // + agent.coreModule.was.ToString("F0");
    }
}
