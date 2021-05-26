using System;
using UnityEngine;
using UnityEngine.UI;

public class GenomeButtonPrefabScript : MonoBehaviour {
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    UIManager uiManager => UIManager.instance;

    public int index = -1;
    private SelectionGroup group;
    
    public Button button;
    public Image backgroundImage;
    public GenomeButtonTooltipSource tooltip;
    
    [SerializeField] BackgroundState selectedState;
    [SerializeField] BackgroundState[] lifeStageStates;
    [SerializeField] BackgroundState fossilState;
    [SerializeField] BackgroundState unbornState;
    
    //public Color testColor; void OnValidate() { testColor = Color.gray; }

    public void UpdateButtonPrefab(SelectionGroup grp, int slotIndex) {
        index = slotIndex;
        group = grp;
    }

    public void ClickedThisButton() {
        // updates focusedCandidate in uiManager
        uiManager.speciesOverviewUI.ChangeSelectedGenome(group, index);  
        
        if(!uiManager.focusedCandidate.isBeingEvaluated) 
            return;

        FindCorrespondingAgent();
    }

    void FindCorrespondingAgent()
    {
        var agentIndex = simulationManager.GetIndexOfFocusedAgent();
        if (agentIndex == -1) return;
        
        cameraManager.SetTargetAgent(simulationManager.agentsArray[agentIndex], agentIndex);
        
        uiManager.ResetCurrentFocusedCandidateGenome();
        cameraManager.isFollowingAgent = true; 
    }
    
    // * Consider Refactor: move life stage to candidate    
    public void SetDisplay(CandidateAgentData candidate)
    {
        string statusStr = "";

        if (candidate.isBeingEvaluated) 
        {
            Agent matchingAgent = simulationManager.GetAgent(candidate);
            bool isFocus = uiManager.IsFocus(candidate);
            
            ColorBlock block = button.colors;
            block.colorMultiplier = isFocus ? 2f : 1f;
            button.colors = block;
            
            statusStr = isFocus ? SetBackground(selectedState) : SetBackgroundByLifeStage(matchingAgent);
        }
        else 
        {
            var background = candidate.allEvaluationsComplete ? fossilState : unbornState;
            statusStr = SetBackground(background);

            if(!candidate.allEvaluationsComplete)
                gameObject.SetActive(false);
        }        

        //tooltip.genomeViewerUIRef = uiManagerRef.genomeViewerUI;
        tooltip.tooltipString ="Creature #" + candidate.candidateID + "\n" + statusStr;
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
        public Agent.AgentLifeStage lifeStage;
        public float scale;
        public Color color;
        public string status;
    }
}
