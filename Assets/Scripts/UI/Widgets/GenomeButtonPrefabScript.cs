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
    public bool isSelected = false;
    
    
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
    
    // * WPP: Refactor repeating logic (color/scale/etc setting) into method
    //  and expose values with nested class
    public void SetDisplay(CandidateAgentData candidate)
    {
        string statusStr = "";

        if (candidate.isBeingEvaluated) 
        {
            // WPP 5/22/21: Simplified, moved to SimulationManager
            /* // find Agent:
            int matchingAgentIndex = -1;
            Agent matchingAgent = null;
            
            for(int a = 0; a < simulationManager.agentsArray.Length; a++) 
            {
                Agent agent = simulationManager.agentsArray[a];

                if(agent.candidateRef == null)
                    continue;

                if(iCand.candidateID == agent.candidateRef.candidateID) 
                {
                    matchingAgentIndex = a;
                    matchingAgent = simulationManager.agentsArray[matchingAgentIndex];
                    break;
                }
            }*/
            Agent matchingAgent = simulationManager.GetAgent(candidate);
            
            // *********** WRITE IT DOWN!!! **************
            // WPP 5/22/21: moved check to UIManager 
            //if(iCand.candidateID == uiManagerRef.focusedCandidate.candidateID) 
            if (uiManager.IsFocus(candidate))
            {
                backgroundImage.color = Color.white * 1f;
                ColorBlock block = button.colors;
                block.colorMultiplier = 2f;
                button.colors = block;
                gameObject.transform.localScale = Vector3.one * 1.3f;
                statusStr = "\n(SELECTED)";
            }
            else 
            {
                ColorBlock block = button.colors;
                block.colorMultiplier = 1f;
                button.colors = block;
                
                // WPP 5/22/21: delegated to GenomeButtonPrefabScript
                if (matchingAgent.isDead)
                {
                    gameObject.transform.localScale = Vector3.one * 0.9f;
                    backgroundImage.color = Color.red;
                    statusStr = "\n(DEAD!)";
                }
                else if(matchingAgent.isEgg) 
                {
                    gameObject.transform.localScale = Vector3.one * 1f;
                    backgroundImage.color = Color.yellow;
                    statusStr = "\n(EGG!)";
                }
                else 
                {  
                    gameObject.transform.localScale = Vector3.one * 1f;
                    backgroundImage.color = Color.green;
                    statusStr = "\n(ALIVE!)";
                }
                
                //buttonScript.GetComponent<Image>().color = Color.white;
                //statusStr = "\n(Under Evaluation)";
            }
        }
        else 
        {
            gameObject.transform.localScale = Vector3.one;
            if(candidate.allEvaluationsComplete) 
            {
                backgroundImage.color = Color.gray;
                statusStr = "\n(Fossil)";
            }
            else 
            {
                gameObject.SetActive(false);
                backgroundImage.color = Color.black;
                statusStr = "\n(Unborn)";
            }
        }        

        //tooltip.genomeViewerUIRef = uiManagerRef.genomeViewerUI;
        tooltip.tooltipString ="Creature #" + candidate.candidateID + statusStr;
        //uiManagerRef.speciesOverviewUI.leaderboardGenomeButtonsList.Add(buttonScript);
    }

    // WPP: work-in-progress, delete if not used when refactor complete
    /*public string SetDisplay(Agent agent)
    {
        string status;
    
        // WPP 5/22/21: simplified checks with Agent getters
        if (agent.isDead)
        {
            gameObject.transform.localScale = Vector3.one * 0.9f;
            backgroundImage.color = Color.red;
            status = "\n(DEAD!)";
        }
        else if(agent.isEgg) 
        {
            gameObject.transform.localScale = Vector3.one * 1f;
            backgroundImage.color = Color.yellow;
            status = "\n(EGG!)";
        }
        else 
        {  
            gameObject.transform.localScale = Vector3.one * 1f;
            backgroundImage.color = Color.green;
            status = "\n(ALIVE!)";
        }
        
        return status;
    }*/
}
