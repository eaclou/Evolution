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

    [SerializeField]
    Image imageMeterBarHealth;
    [SerializeField]
    Image imageMeterBarEnergy;
    [SerializeField]
    Image imageMeterBarStomach;
    [SerializeField]
    Image imageMeterBarWaste;
    
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
        //Rect rect = imageMeterBarHealth.GetComponent<RectTransform>().rect;
        //rect.height = Mathf.RoundToInt(32f * agent.coreModule.health);
        imageMeterBarHealth.GetComponent<RectTransform>().transform.localScale = new Vector3(1f, agent.coreModule.health, 1f);
        tooltipEnergy.tooltipString = "Energy: " + agent.coreModule.energy.ToString("F0");
        imageMeterBarEnergy.GetComponent<RectTransform>().transform.localScale = new Vector3(1f, Mathf.Clamp01(agent.coreModule.energy * 0.05f), 1f);
        tooltipStomachFood.tooltipString = "Stomach: " + (agent.coreModule.stomachContentsPercent * 100f).ToString("F0");
        imageMeterBarStomach.GetComponent<RectTransform>().transform.localScale = new Vector3(1f, agent.coreModule.stomachContentsPercent, 1f);
        tooltipWaste.tooltipString = "Waste: (tbd)"; // + agent.coreModule.was.ToString("F0");
        imageMeterBarWaste.GetComponent<RectTransform>().transform.localScale = new Vector3(1f, 0f, 1f);
    }
}
