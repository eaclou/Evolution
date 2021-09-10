using System;
using UnityEngine;
using UnityEngine.UI;

public class GenomeButtonPrefabScript : MonoBehaviour {
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    UIManager uiManager => UIManager.instance;
    Lookup lookup => Lookup.instance;

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
    
    //public Color testColor; void OnValidate() { testColor = Color.gray; }

    public void UpdateButtonPrefab(SelectionGroup grp, int slotIndex) {
        index = slotIndex;
        group = grp;
    }

    public void ClickedThisButton() {
        

        // updates focusedCandidate in uiManager
        uiManager.speciesOverviewUI.ChangeSelectedGenome(group, index);  
        
        if(!uiManager.selectionManager.focusedCandidate.isBeingEvaluated) 
            return;

        FindCorrespondingAgent();
    }

    void FindCorrespondingAgent()
    {
        var agentIndex = simulationManager.GetIndexOfFocusedAgent();
        if (agentIndex == -1) return;
        
        cameraManager.SetTargetAgent(simulationManager.agents[agentIndex], agentIndex);
        uiManager.selectionManager.ResetCurrentFocusedCandidateGenome();
        cameraManager.isFollowingAgent = true; 
    }

    public void SetTargetCoords(Vector2 newCoords) {
        targetCoords = newCoords;
    }
    
    // * Consider Refactor: move life stage to candidate    
    public void SetDisplay(CandidateAgentData candidate)
    {
        var iconSprite = lookup.GetAgentLifeStageIcon(AgentLifeStage.Dead, true);   // Fossil
        
        // POSITION
        currentCoords = Vector2.Lerp(currentCoords, targetCoords, 0.75f);

        gameObject.transform.localPosition = new Vector3(currentCoords.x * 360f, currentCoords.y * 360f, 0f);

        string statusStr = "";

        if (candidate.isBeingEvaluated) 
        {
            Agent matchingAgent = simulationManager.GetAgent(candidate);
            bool isFocus = uiManager.selectionManager.IsFocus(candidate);
            
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

        tooltip.tooltipString = "" + candidate.name + "\nAge " + candidate.performanceData.totalTicksAlive;// + ", " + statusStr;
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
