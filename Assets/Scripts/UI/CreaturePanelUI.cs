﻿using System;
using UnityEngine;
using UnityEngine.UI;


public class CreaturePanelUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    
    Agent agent => simulationManager.agents[cameraManager.targetAgentIndex];
    float totalTicksAlive => simulationManager.masterGenomePool.completeSpeciesPoolsList[agent.speciesIndex].avgCandidateData.performanceData.totalTicksAlive;

    [SerializeField]
    Animator CreaturePanelAnimator;
    [SerializeField] Color onColor = Color.white;
    [SerializeField] Color offColor = Color.gray;

    [SerializeField]
    Image imageCurAction;

    public TooltipUI tooltipCurrentAction;
    public TooltipUI tooltipBrain;
    public TooltipUI tooltipGenome;
    public TooltipUI tooltipAppearance;
    public TooltipUI tooltipSpeciesIcon;
        
    [SerializeField] AgentActionStateData[] actionStates;
    [SerializeField] AgentActionStateData defaultActionState;
    
    [SerializeField] PanelModeData[] panelModes;
    [SerializeField] StringSO startingPanelMode;

    public Sprite spriteBrainButton;

    private StringSO curPanelMode;

    // PORTRAIT!!!!
    public ComputeBuffer portraitCritterInitDataCBuffer;
    public ComputeBuffer portraitCritterSimDataCBuffer;
    public ComputeBuffer critterPortraitStrokesCBuffer;
    
    void Start()
    {
        SetPanelMode(startingPanelMode);
    
        foreach (var panelMode in panelModes)
            panelMode.Initialize(onColor, offColor);
    }

    public void InitializeRenderBuffers() 
    {
        // INIT:: ugly :(
        portraitCritterInitDataCBuffer?.Release();
        portraitCritterInitDataCBuffer = new ComputeBuffer(6, SimulationStateData.GetCritterInitDataSize());
        critterPortraitStrokesCBuffer = new ComputeBuffer(1 * theRenderKing.GetNumStrokesPerCritter(), theRenderKing.GetMemorySizeCritterStrokeData());
    }

    public void ClickGenome() {
        CreaturePanelAnimator.SetBool("GenomeON", true);
        CreaturePanelAnimator.SetBool("BrainWiringON", false);
    }
    public void ClickBrain() {
        CreaturePanelAnimator.SetBool("GenomeON", false);
        CreaturePanelAnimator.SetBool("BrainWiringON", true);
    }
    
    public void Tick() 
    {
        if (!curPanelMode) return;
    
        //textPanelStateDebug.text = "MODE: " + curPanelMode.value;

        var actionState = GetAgentActionStateData(agent.curActionState);
        imageCurAction.sprite = actionState.sprite;
        tooltipCurrentAction.tooltipString = actionState.text;

        tooltipSpeciesIcon.tooltipString = "Species #" + agent.speciesIndex + "\nAvg Life: " + totalTicksAlive.ToString("F0");

        foreach (var panelMode in panelModes)
            panelMode.SetActive(curPanelMode);

        if (agent.coreModule == null) return;

        tooltipBrain.tooltipString = "BRAIN";//\nAction: " + agent.curActionState;
        tooltipGenome.tooltipString = "GENOME";//\nGen#" + agent.candidateRef.candidateGenome.generationCount + ", DNA length: " + (agent.candidateRef.candidateGenome.brainGenome.linkList.Count + agent.candidateRef.candidateGenome.brainGenome.bodyNeuronList.Count);
        tooltipAppearance.tooltipString = "APPEARANCE";
    }
    
    public void SetPanelMode(StringSO mode) {
        //Debug.Log($"CreaturePanelUI.SetPanelMode({mode})");
        curPanelMode = mode;
    }

    private void OnDisable() {
        critterPortraitStrokesCBuffer?.Release();
        portraitCritterInitDataCBuffer?.Release();
        portraitCritterSimDataCBuffer?.Release();
    }
    
    AgentActionStateData GetAgentActionStateData(AgentActionState id)
    {
        foreach (var actionState in actionStates)
            if (actionState.id == id)
                return actionState;
                
        return defaultActionState;
    }
    
    [Serializable]
    public struct AgentActionStateData
    {
        public AgentActionState id;
        public Sprite sprite;
        public string text;
    }
    
    [Serializable]
    public class PanelModeData
    {
        [SerializeField] ScriptableObject id;
        [SerializeField] GameObject panel;
        [SerializeField] Image icon;
        
        Color onColor;
        Color offColor;
        
        public void Initialize(Color onColor, Color offColor)
        {
            this.onColor = onColor;
            this.offColor=  offColor;
        }
        
        public void SetActive(ScriptableObject id) { SetActive(this.id == id); }
        
        public void SetActive(bool value)
        {
            panel.SetActive(value);
            icon.color = value ? onColor : offColor;
        }
    }
}
