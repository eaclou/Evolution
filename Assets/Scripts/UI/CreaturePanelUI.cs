using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CreaturePanelUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    UIManager uiManagerRef => UIManager.instance;
    TheRenderKing theRenderKing => TheRenderKing.instance;

    [SerializeField]
    GameObject panelPortrait;
    [SerializeField]
    GameObject panelGenome;
    [SerializeField]
    GameObject panelBrain;
    [SerializeField]
    GameObject panelPaperDoll;

    [SerializeField]
    Image imageAppearanceIcon;
    [SerializeField]
    Image imageGenomeIcon;
    [SerializeField]
    Image imageBrainIcon;

    [SerializeField]
    Image imageCurAction;

    public TooltipUI tooltipCurrentAction;
    public TooltipUI tooltipBrain;
    public TooltipUI tooltipGenome;
    public TooltipUI tooltipAppearance; 
    
    [SerializeField]
    Text textPanelStateDebug;

    public Sprite spriteIconCreatureStateEgg;
    public Sprite spriteIconCreatureStateYoung;
    public Sprite spriteIconCreatureStateMature;
    public Sprite spriteIconCreatureStateDecaying;
    public Sprite spriteIconCreatureStateFossil;

    public Sprite spriteIconCreatureActionAttack;
    public Sprite spriteIconCreatureActionDefend;
    public Sprite spriteIconCreatureActionDash;
    public Sprite spriteIconCreatureActionRest;
    public Sprite spriteIconCreatureActionFeed;

    public Sprite spriteBrainButton;

    private CreaturePanelMode curPanelMode;
    public enum CreaturePanelMode {
        Portrait,
        Genome,
        Brain
    }

    // PORTRAIT!!!!
    public ComputeBuffer portraitCritterInitDataCBuffer;
    public ComputeBuffer portraitCritterSimDataCBuffer;
    public ComputeBuffer critterPortraitStrokesCBuffer;

    public void InitializeRenderBuffers() {
        
        // INIT:: ugly :(
        portraitCritterInitDataCBuffer?.Release();
        portraitCritterInitDataCBuffer = new ComputeBuffer(6, SimulationStateData.GetCritterInitDataSize());

        critterPortraitStrokesCBuffer = new ComputeBuffer(1 * theRenderKing.GetNumStrokesPerCritter(), theRenderKing.GetMemorySizeCritterStrokeData());
    }
    
    public void Tick() {
        textPanelStateDebug.text = "MODE: " + curPanelMode;

        Color onColor = Color.gray;
        Color offColor = Color.white;
        imageCurAction.sprite = spriteBrainButton;
        Agent agent = simulationManager.agents[cameraManager.targetAgentIndex];
        if(agent.curActionState == Agent.AgentActionState.Attacking) {
            imageCurAction.sprite = spriteIconCreatureActionAttack;
        }
        if(agent.curActionState == Agent.AgentActionState.Defending) {
            imageCurAction.sprite = spriteIconCreatureActionDefend;
        }
        if(agent.curActionState == Agent.AgentActionState.Dashing) {
            imageCurAction.sprite = spriteIconCreatureActionDash;
        }
        if(agent.curActionState == Agent.AgentActionState.Resting) {
            imageCurAction.sprite = spriteIconCreatureActionRest;
        }
        if(agent.curActionState == Agent.AgentActionState.Default) {
            imageCurAction.sprite = spriteIconCreatureActionFeed;
        }
        
                
        if (curPanelMode == CreaturePanelMode.Portrait) {
            panelPortrait.SetActive(true);
            imageAppearanceIcon.color = onColor;

            panelGenome.SetActive(false);
            panelBrain.SetActive(false);
            imageGenomeIcon.color = offColor;
            imageBrainIcon.color = offColor;
        }
        else if(curPanelMode == CreaturePanelMode.Genome) {
            panelGenome.SetActive(true);
            imageGenomeIcon.color = onColor;

            panelPortrait.SetActive(false);////

            panelBrain.SetActive(false);
            imageAppearanceIcon.color = offColor;
            imageBrainIcon.color = offColor;
        }
        else if(curPanelMode == CreaturePanelMode.Brain) {
            panelBrain.SetActive(true);
            imageBrainIcon.color = onColor;

            panelGenome.SetActive(false);

            panelPortrait.SetActive(false);/////

            imageGenomeIcon.color = offColor;
            imageAppearanceIcon.color = offColor;
        }

        
        if (agent.coreModule == null) return;
        tooltipCurrentAction.tooltipString = "BRAIN\nAction: " + agent.curActionState;
        //textGeneration.text = "Gen: " + genome.generationCount;
        //textBodySize.text = "Size: " + (100f * core.creatureBaseLength).ToString("F0") + ", Aspect 1:" + (1f / core.creatureAspectRatio).ToString("F0");
        //textBrainSize.text = "Brain Size: " + brain.bodyNeuronList.Count + "--" + brain.linkList.Count;
        tooltipGenome.tooltipString = "GENOME\nGen#" + agent.candidateRef.candidateGenome.generationCount + ", DNA length: " + (agent.candidateRef.candidateGenome.brainGenome.linkList.Count + agent.candidateRef.candidateGenome.brainGenome.bodyNeuronList.Count);
        tooltipAppearance.tooltipString = "APPEARANCE";
    }

    public void SetPanelMode(int modeID) {
        curPanelMode = (CreaturePanelMode)modeID;
    }

    private void OnDisable() {
        critterPortraitStrokesCBuffer?.Release();
        portraitCritterInitDataCBuffer?.Release();
        portraitCritterSimDataCBuffer?.Release();
    }
    
}
