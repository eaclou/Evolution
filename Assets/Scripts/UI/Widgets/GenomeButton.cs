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
    
    public Button button;
    public Image backgroundImage;
    public TooltipUI tooltip;

    public Vector2 targetCoords; // UI canvas
    private Vector2 currentCoords;
    
    [SerializeField] BackgroundState selectedState;
    [SerializeField] BackgroundState[] lifeStageStates;
    [SerializeField] BackgroundState fossilState;
    [SerializeField] BackgroundState unbornState;
    
    public void UpdateButtonPrefab(SelectionGroup grp, int slotIndex) {
        index = slotIndex;
        group = grp;
    }

    /// Updates focusedCandidate in uiManager
    public void ClickedThisButton() 
    {
        uiManager.speciesOverviewUI.ChangeSelectedGenome(group, index);  
        
        // WPP: removed, function already called by above line
        //if (!selectionManager.currentSelection.candidate.isBeingEvaluated) return;
        //selectionManager.SetSelected(simulationManager.masterGenomePool.completeSpeciesPoolsList[selectionManager.currentSelection.historySelectedSpeciesID].candidateGenomesList[index]); // FindCorrespondingAgent();
    }

    //public void SetTargetCoords(Vector2 newCoords) {
    //    targetCoords = newCoords;
    //}
    
    // * Consider Refactor: move life stage to candidate    
    public void SetDisplay(CandidateAgentData candidate)
    {
        var iconSprite = lookup.GetAgentLifeStageIcon(AgentLifeStage.Dead, true);   // Fossil
        
        // POSITION
        //currentCoords = Vector2.Lerp(currentCoords, targetCoords, 0.75f);

        //gameObject.transform.localPosition = new Vector3(currentCoords.x * 360f, currentCoords.y * 360f, 0f);

        string statusStr = "";

        if (candidate.isBeingEvaluated) 
        {
            Agent matchingAgent = simulationManager.GetAgent(candidate);
            bool isFocus = selectionManager.IsSelected(candidate);
            
            ColorBlock block = button.colors;
            block.colorMultiplier = isFocus ? 2f : 1f;
            button.colors = block;
            
            statusStr = isFocus ? SetBackground(selectedState) : SetBackgroundByLifeStage(matchingAgent);
            
            iconSprite = lookup.GetAgentLifeStageIcon(matchingAgent.curLifeStage, matchingAgent.isYoung);
        }
        else 
        {
            var background = candidate.allEvaluationsComplete ? fossilState : unbornState;
            statusStr = SetBackground(background);

            if(!candidate.allEvaluationsComplete)
                gameObject.SetActive(false);
        }

        backgroundImage.sprite = iconSprite;
        //tooltip.genomeViewerUIRef = uiManagerRef.genomeViewerUI;

        tooltip.tooltipString = "" + candidate.candidateGenome.name + "-" + candidate.candidateID + "\nAge " + candidate.performanceData.totalTicksAlive;// + ", " + statusStr;
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
