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

    // WPP: moved to PanelModeData
    /*[SerializeField]
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
    Image imageBrainIcon;*/
    
    [SerializeField] Color onColor = Color.white;
    [SerializeField] Color offColor = Color.gray;

    [SerializeField]
    Image imageCurAction;

    public TooltipUI tooltipCurrentAction;
    public TooltipUI tooltipBrain;
    public TooltipUI tooltipGenome;
    public TooltipUI tooltipAppearance;
    public TooltipUI tooltipSpeciesIcon;
    
    [SerializeField]
    Text textPanelStateDebug;

    // * WPP: move to Lookup
    /*public Sprite spriteIconCreatureStateEgg;
    public Sprite spriteIconCreatureStateYoung;
    public Sprite spriteIconCreatureStateMature;
    public Sprite spriteIconCreatureStateDecaying;
    public Sprite spriteIconCreatureStateFossil;*/
    
    [SerializeField] AgentActionStateData[] actionStates;
    [SerializeField] AgentActionStateData defaultActionState;
    
    [SerializeField] PanelModeData[] panelModes;
    [SerializeField] StringSO startingPanelMode;

    // WPP: moved to AgentActionStateData
    /*public Sprite spriteIconCreatureActionAttack;
    public Sprite spriteIconCreatureActionDefend;
    public Sprite spriteIconCreatureActionDash;
    public Sprite spriteIconCreatureActionRest;
    public Sprite spriteIconCreatureActionFeed;*/

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
    
    public void Tick() {
        textPanelStateDebug.text = "MODE: " + curPanelMode.value;

        var actionState = GetAgentActionStateData(agent.curActionState);
        imageCurAction.sprite = actionState.sprite;
        tooltipCurrentAction.tooltipString = actionState.text;

        tooltipSpeciesIcon.tooltipString = "Species #" + agent.speciesIndex + "\nAvg Life: " + totalTicksAlive.ToString("F0");

        foreach (var panelMode in panelModes)
            panelMode.SetActive(curPanelMode);

        if (agent.coreModule == null) return;

        tooltipBrain.tooltipString = "BRAIN";//\nAction: " + agent.curActionState;
        //textGeneration.text = "Gen: " + genome.generationCount;
        //textBodySize.text = "Size: " + (100f * core.creatureBaseLength).ToString("F0") + ", Aspect 1:" + (1f / core.creatureAspectRatio).ToString("F0");
        //textBrainSize.text = "Brain Size: " + brain.bodyNeuronList.Count + "--" + brain.linkList.Count;
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
