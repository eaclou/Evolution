using UnityEngine;
using UnityEngine.UI;

public class CreaturePaperDollUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    Lookup lookup => Lookup.instance;
    
    Agent agent => simulationManager.agents[cameraManager.targetAgentIndex];
    CritterModuleCore coreModule => agent.coreModule;

    public WidgetAgentStatus widgetAgentStatus;

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
    
    public void Tick() {
        if (coreModule == null) return;
        
        widgetAgentStatus.UpdateBars(agent); 
        
        // WPP: moved to lookup
        //var lifeStage = agent.curLifeStage.ToString();
        /*if(agent.curLifeStage == AgentLifeStage.Mature) {
            if(agent.isSexuallyMature) {
                lifeStage = "Mature";
                creatureStateSprite = uiManager.creaturePanelUI.spriteIconCreatureStateMature;
            }
            else {
                lifeStage = "Young";
                creatureStateSprite = uiManager.creaturePanelUI.spriteIconCreatureStateYoung;
            }
        }
        else if(agent.curLifeStage == AgentLifeStage.Dead) {
            creatureStateSprite = uiManager.creaturePanelUI.spriteIconCreatureStateDecaying;
        }*/
        var lifeStageData = lookup.GetAgentLifeStageData(agent.curLifeStage, agent.isYoung);

        tooltipState.tooltipString = lifeStageData.stateName + ", Age: " + agent.ageCounter + ", Size: " + agent.currentBiomass.ToString("F3");
        tooltipImage.sprite = lifeStageData.icon;

        tooltipHealth.tooltipString = "Health: " + (coreModule.health * 100f).ToString("F0") + "%";
        //Rect rect = imageMeterBarHealth.GetComponent<RectTransform>().rect;
        //rect.height = Mathf.RoundToInt(32f * agent.coreModule.health);
        imageMeterBarHealth.transform.localScale = new Vector3(1f, coreModule.health, 1f);
        
        tooltipEnergy.tooltipString = "Energy: " + coreModule.energy.ToString("F0");
        imageMeterBarEnergy.transform.localScale = new Vector3(1f, Mathf.Clamp01(coreModule.energy * 0.05f), 1f);
        
        tooltipStomachFood.tooltipString = "Stomach: " + (coreModule.stomachContentsPercent * 100f).ToString("F0");
        imageMeterBarStomach.transform.localScale = new Vector3(1f, coreModule.stomachContentsPercent, 1f);
        
        tooltipWaste.tooltipString = "Waste: (tbd)"; // + agent.coreModule.was.ToString("F0");
        imageMeterBarWaste.transform.localScale = new Vector3(1f, 0f, 1f);
    }
}
