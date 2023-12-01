using System;
using UnityEngine;
using UnityEngine.UI;

public class GenomeButton : MonoBehaviour 
{
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    UIManager uiManager => UIManager.instance;
    Lookup lookup => Lookup.instance;
    SelectionManager selectionManager => SelectionManager.instance;

    public int index = -1;
    private SelectionGroup group;

    public CandidateAgentData candidateRef;
    public Button button;
    public Image backgroundImage;
    public TooltipUI tooltip;

    public BezierCurve bezierCurve; //CreatureButton history panel Line

    public Vector2 targetCoords; // UI canvas
    private Vector2 currentCoords;
    
    [SerializeField] BackgroundState selectedState;
    [SerializeField] BackgroundState[] lifeStageStates;
    [SerializeField] BackgroundState fossilState;
    [SerializeField] BackgroundState unbornState;

    public void SetCandidate(CandidateAgentData candidate) {
       candidateRef = candidate;
    }
    public void UpdateButtonPrefab(SelectionGroup grp, int slotIndex) {
        index = slotIndex;
        group = grp;
    }

    /// Updates focusedCandidate in uiManager
    public void ClickedThisButton() 
    {
        // is this candidate already selected?
        
        uiManager.speciesOverviewUI.ChangeSelectedGenome(group, index, this);
        Debug.Log("uiManager.speciesOverviewUI.ChangeSelectedGenome(group, index);  ");
        // WPP: removed, function already called by above line
        //if (!selectionManager.currentSelection.candidate.isBeingEvaluated) return;
        //selectionManager.SetSelected(simulationManager.masterGenomePool.completeSpeciesPoolsList[selectionManager.currentSelection.historySelectedSpeciesID].candidateGenomesList[index]); // FindCorrespondingAgent();
    }

    //public void SetTargetCoords(Vector2 newCoords) {
    //    targetCoords = newCoords;
    //}
    
    // * Consider Refactor: move life stage to candidate    
    public void SetDisplay()
    {
        var iconSprite = lookup.GetAgentLifeStageIcon(AgentLifeStage.Dead, true);   // Fossil
        
        string statusStr = "";
        bool isFocus = selectionManager.IsSelected(candidateRef);
        if (candidateRef.isBeingEvaluated) 
        {
            Agent matchingAgent = simulationManager.GetAgent(candidateRef);            
            
            ColorBlock block = button.colors;
            block.colorMultiplier = isFocus ? 2f : 1f;
            button.colors = block;
            
            statusStr = isFocus ? SetBackground(selectedState) : SetBackgroundByLifeStage(matchingAgent);
            
            iconSprite = lookup.GetAgentLifeStageIcon(matchingAgent.curLifeStage, matchingAgent.isYoung);
        }
        else 
        {
            var background = candidateRef.allEvaluationsComplete ? fossilState : unbornState;
            statusStr = isFocus ? SetBackground(selectedState) : SetBackground(background);
        }

        backgroundImage.sprite = iconSprite;
        //tooltip.genomeViewerUIRef = uiManagerRef.genomeViewerUI;
        string ageString = uiManager.clockPanelUI.ConvertFramesToAgeString(candidateRef.performanceData.totalTicksAlive);
        
        tooltip.tooltipString = "" + candidateRef.candidateGenome.name + "-" + candidateRef.candidateID + "\nAge: " + ageString.ToString();// + ", " + statusStr;
        //uiManagerRef.speciesOverviewUI.leaderboardGenomeButtonsList.Add(buttonScript);
    }
    
    string SetBackgroundByLifeStage(Agent agent)
    {
        
        foreach (var state in lifeStageStates)
            
            if (state.lifeStage == agent.curLifeStage)
                return SetBackground(state);
        
        return "";
    }
    
    string SetBackground(BackgroundState state)
    {
        gameObject.transform.localScale = Vector3.one * state.scale;
        backgroundImage.color = state.color;
        return state.status;        
    }

    [Serializable]
    public struct BackgroundState
    {
        public AgentLifeStage lifeStage;
        public float scale;
        public Color color;
        public string status;
        //public Sprite icon;
    }
}
