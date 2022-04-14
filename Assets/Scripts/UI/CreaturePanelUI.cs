﻿using System;
using UnityEngine;
using UnityEngine.UI;


public class CreaturePanelUI : MonoBehaviour
{
    SelectionManager selection => SelectionManager.instance;
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    
    Agent agent => selection.currentSelection.agent;
    float totalTicksAlive => simulationManager.masterGenomePool.completeSpeciesPoolsList[agent.speciesIndex].avgCandidateData.performanceData.totalTicksAlive;
    
    [SerializeField] Color onColor = Color.white;
    [SerializeField] Color offColor = Color.gray;

    [SerializeField]
    Image imageCurAction;

    public TooltipUI tooltipCurrentAction;
    public TooltipUI tooltipBrain;
    public TooltipUI tooltipGenome;
    public TooltipUI tooltipAppearance;
    public TooltipUI tooltipGenerationCount;

    [SerializeField]
    Text textGenerationCount;

    [SerializeField]
    OpenCloseButton openCloseButton;
        
    [SerializeField] AgentActionStateData[] actionStates;
    [SerializeField] AgentActionStateData defaultActionState;
    public AgentActionStateData mostRecentActionState;
    
    [SerializeField] PanelModeData[] panelModes;
    [SerializeField] StringSO startingPanelMode;

    private bool isPanelOpen => openCloseButton.isOpen;

    public Sprite spriteBrainButton;

    [SerializeField]
    public SpeciesIconUI speciesIconUI;

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

    public void UpdateAgentActionStateData(int candID, AgentActionState actionState) {
        if (selection.currentSelection.candidate.candidateID != candID) return;
        
        for (int i = 0; i < actionStates.Length; i++) {
            if (actionStates[i].id == actionState) {
                mostRecentActionState = actionStates[i];
            }
        }
    }

    public void InitializeRenderBuffers() 
    {
        // INIT:: ugly :(
        portraitCritterInitDataCBuffer?.Release();
        portraitCritterInitDataCBuffer = new ComputeBuffer(6, SimulationStateData.GetCritterInitDataSize());
        critterPortraitStrokesCBuffer = new ComputeBuffer(1 * theRenderKing.GetNumStrokesPerCritter(), theRenderKing.GetMemorySizeCritterStrokeData());
    }

    public void Tick() 
    {
        if (!curPanelMode) return;
    
        var mouseInOpenCloseArea = Screen.height - Input.mousePosition.y < 64 && Math.Abs((Screen.width / 2) - Input.mousePosition.x) < 128;
        openCloseButton.SetMouseEnter(mouseInOpenCloseArea);
        
        imageCurAction.sprite = mostRecentActionState.sprite;
        tooltipCurrentAction.tooltipString = mostRecentActionState.text;

        foreach (var panelMode in panelModes)
            panelMode.SetActive(curPanelMode);

        if (!agent) return;

        speciesIconUI.GetComponent<Image>().material = simulationManager.masterGenomePool.completeSpeciesPoolsList[agent.speciesIndex].coatOfArmsMat;
        speciesIconUI.tooltip.tooltipString = "Species " + simulationManager.masterGenomePool.completeSpeciesPoolsList[agent.speciesIndex].representativeCandidate.candidateGenome.name.Substring(0, 1);
        
        tooltipBrain.tooltipString = "BRAIN:\n" + 
            (agent.candidateRef.candidateGenome.brainGenome.neurons.hiddenCount + 
            agent.candidateRef.candidateGenome.brainGenome.neurons.inOutCount) + " Neurons, " + 
            agent.candidateRef.candidateGenome.brainGenome.axonCount + " Axons\n" + "Action: " + 
            mostRecentActionState.id;
        //tooltipGenome.tooltipString = "Genome???";
        //tooltipAppearance.tooltipString = "GEN " + agent.candidateRef.candidateGenome.generationCount + ", Axons: " + (agent.candidateRef.candidateGenome.brainGenome.links.Count + ", IO: " + agent.candidateRef.candidateGenome.brainGenome.inOutNeurons.Count + ", H: " + agent.candidateRef.candidateGenome.brainGenome.hiddenNeurons.Count);//"APPEARANCE";

        tooltipGenerationCount.tooltipString = "Generation " + agent.candidateRef.candidateGenome.generationCount.ToString();
        textGenerationCount.text = agent.candidateRef.candidateGenome.generationCount.ToString();
    }
    
    public void SetPanelMode(StringSO mode) {
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
            this.offColor = offColor;
        }
        
        public void SetActive(ScriptableObject id) { SetActive(this.id == id); }
        
        public void SetActive(bool value)
        {
            panel.SetActive(value);
            icon.color = value ? onColor : offColor;
        }
    }
}
