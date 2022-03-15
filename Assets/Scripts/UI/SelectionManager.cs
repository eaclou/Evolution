using UnityEngine;
using Playcraft;

public class SelectionManager : Singleton<SelectionManager>
{
    UIManager uiManager => UIManager.instance;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    SimulationManager simulation => SimulationManager.instance;
    
    public bool isFollowing;    // * Not used
    public SelectionData currentSelection;
    
    public void ResetCurrentFocusedCandidateGenome() { SetSelected(currentSelection.candidate); }

    public void SetSelected(CandidateAgentData candidate) {
        if (currentSelection == null) {
            currentSelection = new SelectionData();
        }
        currentSelection.candidate = candidate;

        // Check if corresponding agent exists:
        bool hasAgent = false;
        foreach (var agent in simulation.agents) {
            if (currentSelection.candidate.candidateID != agent.candidateRef.candidateID) 
                continue;
            
            hasAgent = true;
            currentSelection.agent = agent;
            theRenderKing.InitializeCreaturePortrait(currentSelection.candidate.candidateGenome);
            uiManager.genomeViewerUI.brainGenomeImage.SetTexture(currentSelection.candidate.candidateGenome.brainGenome);
            break;
        }
        
        currentSelection.isGenomeOnly = !hasAgent;
        
        SetHistorySelectedSpeciesUI(candidate.speciesID);
        uiManager.historyPanelUI.InitializePanel();
        //uiManager.historyPanelUI.RefreshFocusedAgent(currentSelection.agent);
        uiManager.speciesOverviewUI.RebuildGenomeButtons();
        
        if (hasAgent) 
        {
            unlockedTech = currentSelection.candidate.candidateGenome.bodyGenome.unlockedTech;
            PrintTech();
        }
    }
    
    [ReadOnly] public UnlockedTech unlockedTech;
    
    /// For debugging unlocked tech on selected agent
    void PrintTech()
    {
        Debug.Log($"Selected agent {currentSelection.agent.transform.GetSiblingIndex()}, " +
                  $"has {unlockedTech.values.Count} tech:");
                  
        foreach (var tech in unlockedTech.values)
            Debug.Log(tech);
    }

    public void FollowedCreatureDied() {
        currentSelection.isGenomeOnly = true;
    }

    public bool IsSelected(CandidateAgentData candidate) { return candidate.candidateID == currentSelection.candidate.candidateID; }
    
    public void SetHistorySelectedSpeciesUI(int id) {
        currentSelection.historySelectedSpeciesID = id;        
    }
    
    // WPP: simplified search by using cached values
    //public bool SelectedAgentHasTech(TechElementId techId) { return currentSelection.candidate.candidateGenome.bodyGenome.data.HasTech(techId); }
    public bool SelectedAgentHasTech(TechElement value) { return unlockedTech.Contains(value); }
    
    public string SelectedTechValue(TechElement tech)
    {
        if (currentSelection == null || currentSelection.agent == null) return "";
        var neurons = currentSelection.agent.brain.GetNeuronsByTechElement(tech);
        
        var values = "";
        for (int i = 0; i < neurons.Count; i++)
        {
            //textStatsGraphLegend.text = "<color=#ff0000ff>Species A -----</color>\n\n<color=#00ff00ff>Species B -----</color>\n\n<color=#0000ffff>Species C -----</color>\n\n<color=#ffffffff>Species D -----</color>";
            Color textColor = new Color(0.67f, 0.67f, 0.67f);
            float neuronVal = (100f * neurons[i].currentValue[0]);
            string plusMinus = "";
            if(neuronVal < 0f) {
                textColor = new Color(0.9f, 0.8f, 0.6f);
            }            
            if(neuronVal > 0f) {  
                textColor = new Color(0.5f, 0.8f, 0.96f);
                plusMinus = "+";
            }
            string hexColorCode = ColorUtility.ToHtmlStringRGB(textColor);
            values += "<color=#" + hexColorCode + ">" + plusMinus + neuronVal.ToString("F0") + "</color>";  // neurons[i].name + " " + 
            if (i < neurons.Count - 1) values += ", ";
        }
        return values;
    }

    public class SelectionData {
        public int historySelectedSpeciesID;
        public CandidateAgentData candidate;
        public Agent agent;
        public bool isGenomeOnly;
    }
}

//public CandidateAgentData focusedCandidate; 
/*public void SetFocusedCandidateGenome(SpeciesGenomePool selectedPool, SelectionGroup group, int index) {
    var candidate = selectedPool.GetFocusedCandidate(group, index);
    SetSelected(candidate);
    UnityEngine.Debug.Log($"Selecting agent index {index}");
}*/
