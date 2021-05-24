using System;
using UnityEngine;
using UnityEngine.UI;

// * WPP: remove from name "Script" (redundant) and "Prefab" (misleading)
public class GenomeButtonPrefabScript : MonoBehaviour {
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    UIManager uiManager => UIManager.instance;

    public int index = -1;
    private SpeciesOverviewUI.SelectionGroup group;
    
    public Button button;
    
    // * WPP: not used, remove
    //public Text textEventName;
    //public Text textEventCost;
    public Image backgroundImage;
    public GenomeButtonTooltipSource tooltip;
    //public bool isSelected = false;
    
    [SerializeField] BackgroundState selectedState;
    [SerializeField] BackgroundState[] lifeStageStates;
    [SerializeField] BackgroundState fossilState;
    [SerializeField] BackgroundState unbornState;
    
    //public Color testColor; void OnValidate() { testColor = Color.gray; }

    public void UpdateButtonPrefab(SpeciesOverviewUI.SelectionGroup grp, int slotIndex) {
        index = slotIndex;
        group = grp;

        //textEventName.text = data.name;
        //textEventCost.text = "$" + data.cost.ToString();

        //if(isSelected) {
            //bgColor *= 2f;
        //    imageBG.color = new Color(0.3f, 1f, 1f);
        //}
        //else {
        //    imageBG.color = new Color(0.3f, 0.3f, 0.3f);
        //}
        //imageBG.color = bgColor;
    }

    public void ClickedThisButton() {
        // updates focusedCandidate in uiManager
        uiManager.speciesOverviewUI.ChangeSelectedGenome(group, index);  
        
        // WPP: early exit
        if(!uiManager.focusedCandidate.isBeingEvaluated) 
            return;

        // WPP: use method for self-commenting
        FindCorrespondingAgent();

        // WPP: moved to simulationManager
        /*
         //bool isFound = false;
         //int agentIndex = -1;
         for(int i = 0; i < simulationManager.agentsArray.Length; i++) {
            if(simulationManager.agentsArray[agentIndex].candidateRef.candidateID == uiManager.focusedCandidate.candidateID) {
                isFound = true;
                agentIndex = i;
                break;
            }
        }
        
        if(isFound) {
            cameraManager.SetTargetAgent(simulationManager.agentsArray[agentIndex], agentIndex);
            uiManagerRef.SetFocusedCandidateGenome(uiManagerRef.focusedCandidate); //.StartFollowingAgent();
            cameraManager.isFollowingAgent = true;
        }*/
    }

    void FindCorrespondingAgent()
    {
        var agentIndex = simulationManager.GetIndexOfFocusedAgent();
        if (agentIndex == -1) return;
        
        cameraManager.SetTargetAgent(simulationManager.agentsArray[agentIndex], agentIndex);
        
        // WPP: renamed to clarify intent
        uiManager.ResetCurrentFocusedCandidateGenome(); //.StartFollowingAgent();
        cameraManager.isFollowingAgent = true; 
    }
    
    // WPP: common logic applied via nested struct
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
