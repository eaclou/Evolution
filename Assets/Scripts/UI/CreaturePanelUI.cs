using System;
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
    public AgentActionStateData mostRecentActionState;
    
    [SerializeField] PanelModeData[] panelModes;
    [SerializeField] StringSO startingPanelMode;

    private bool isBrainWiringOpen = false;

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

    public void UpdateAgentActionStateData(int candID, AgentActionState actionState) {
        if(SelectionManager.instance.focusedCandidate.candidateID != candID) return;
        
        for(int i = 0; i < actionStates.Length; i++) {
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

    //public void ClickGenome() {
    //    CreaturePanelAnimator.SetBool("GenomeON", true);
    //    CreaturePanelAnimator.SetBool("BrainWiringON", false);
    //}
    public void ClickBrain() {
        isBrainWiringOpen = !isBrainWiringOpen;
        CreaturePanelAnimator.SetBool("GenomeON", !isBrainWiringOpen);
        CreaturePanelAnimator.SetBool("BrainWiringON", isBrainWiringOpen);
    }
    
    public void Tick() 
    {
        if (!curPanelMode) return;
    
        //textPanelStateDebug.text = "MODE: " + curPanelMode.value;

        //AgentActionStateData actionState = GetAgentActionStateData(agent.curActionState);
        imageCurAction.sprite = mostRecentActionState.sprite;
        tooltipCurrentAction.tooltipString = mostRecentActionState.text;

        tooltipSpeciesIcon.tooltipString = "Species #" + agent.speciesIndex + "\nAvg Life: " + totalTicksAlive.ToString("F0");

        foreach (var panelMode in panelModes)
            panelMode.SetActive(curPanelMode);

        if (agent.coreModule == null) return;
                
        tooltipBrain.tooltipString = "BRAIN";//\nAction: " + agent.curActionState;
        tooltipGenome.tooltipString = "Genome???";
        tooltipAppearance.tooltipString = "GEN " + agent.candidateRef.candidateGenome.generationCount + ", Axons: " + (agent.candidateRef.candidateGenome.brainGenome.links.Count + ", IO: " + agent.candidateRef.candidateGenome.brainGenome.inOutNeurons.Count + ", H: " + agent.candidateRef.candidateGenome.brainGenome.hiddenNeurons.Count);//"APPEARANCE";
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
