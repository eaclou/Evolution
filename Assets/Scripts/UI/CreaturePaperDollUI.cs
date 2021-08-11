using UnityEngine;
using UnityEngine.UI;

public class CreaturePaperDollUI : MonoBehaviour
{
    UIManager uiManager => UIManager.instance;
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
        Sprite creatureStateSprite = uiManager.creaturePanelUI.spriteIconCreatureStateEgg;
        if (agent.coreModule == null) return;
        
        widgetAgentStatus.UpdateBars(agent); 
        
        string lifeStage = agent.curLifeStage.ToString();
        if(agent.curLifeStage == Agent.AgentLifeStage.Mature) {
            if(agent.isSexuallyMature) {
                lifeStage = "Mature";
                creatureStateSprite = uiManager.creaturePanelUI.spriteIconCreatureStateMature;
            }
            else {
                lifeStage = "Young";
                creatureStateSprite = uiManager.creaturePanelUI.spriteIconCreatureStateYoung;
            }
        }
        else if(agent.curLifeStage == Agent.AgentLifeStage.Dead) {
            creatureStateSprite = uiManager.creaturePanelUI.spriteIconCreatureStateDecaying;
        }

        tooltipState.tooltipString = lifeStage + ", Age: " + agent.ageCounter + ", Size: " + agent.currentBiomass.ToString("F3");

        tooltipState.gameObject.GetComponent<Image>().sprite = creatureStateSprite;

        tooltipHealth.tooltipString = "Health: " + (agent.coreModule.health * 100f).ToString("F0") + "%";
        tooltipEnergy.tooltipString = "Energy: " + agent.coreModule.energy.ToString("F0");
        tooltipStomachFood.tooltipString = "Stomach: " + (agent.coreModule.stomachContentsPercent * 100f).ToString("F0");
        tooltipWaste.tooltipString = "Waste: (tbd)"; // + agent.coreModule.was.ToString("F0");
    }
}
