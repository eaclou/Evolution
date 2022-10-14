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
            uiManager.OnAgentSelected?.Invoke(agent);
            theRenderKing.InitializeCreaturePortrait(currentSelection.candidate.candidateGenome);
            uiManager.genomeViewerUI.brainGenomeImage.SetTexture(currentSelection.candidate.candidateGenome.brainGenome);
            CameraManager.instance.targetAgent = agent;
            CameraManager.instance.targetAgentTransform = agent.bodyGO.transform;
            CameraManager.instance.targetAgentIndex = agent.index;
            CameraManager.instance.SetFollowing(KnowledgeMapId.Animals);
            break;
        }
        
        currentSelection.isGenomeOnly = !hasAgent;
        currentSelection.historySelectedSpeciesID = currentSelection.candidate.speciesID;  
        

        if (hasAgent) 
        {            
            unlockedTech = currentSelection.candidate.candidateGenome.bodyGenome.unlockedTech;
            //PrintTech();
        }
        else {
            uiManager.brainVisualizationRef.RefreshCandidate(currentSelection.candidate);
            Debug.Log("RefreshCandidate! " + currentSelection.candidate.candidateID);
            CameraManager.instance.isFollowingAgent = false;
        }

        //uiManager.historyPanelUI.UpdateTargetGraphBounds();
        uiManager.historyPanelUI.RebuildRenderBuffers();
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
        //currentSelection.isGenomeOnly = true;
        //CameraManager.instance.isFollowingAgent = false;
        //next creature??
        SetSelected(simulation.masterGenomePool.completeSpeciesPoolsList[currentSelection.candidate.speciesID].candidateGenomesList[1]);
        //SetSelectedFromSpeciesUI(currentSelection.candidate.speciesID);
        //CameraManager.instance.isFollowingAgent = true;
    }

    public bool IsSelected(CandidateAgentData candidate) { return candidate.candidateID == currentSelection.candidate.candidateID; }
    
    public void SetSelectedFromSpeciesUI(int speciesID) {
        //currentSelection.historySelectedSpeciesID = id;  
        if(speciesID == currentSelection.historySelectedSpeciesID) {
            return;
        }

        SpeciesGenomePool pool = simulation.masterGenomePool.completeSpeciesPoolsList[speciesID];
        CandidateAgentData cand = pool.foundingCandidate;
        if(!pool.isExtinct && pool.candidateGenomesList.Count > 0) {
            cand = pool.candidateGenomesList[0];
            foreach(var candidate in pool.candidateGenomesList) {
                if(candidate.causeOfDeath == "Alive!") {
                    cand = candidate;
                    break;
                }
            }            
        }
        SetSelected(cand);
        //Debug.Log("Selected! " + speciesID + ",  " + cand.candidateID);
    }
    
    // WPP: simplified search by using cached values
    //public bool SelectedAgentHasTech(TechElementId techId) { return currentSelection.candidate.candidateGenome.bodyGenome.data.HasTech(techId); }
    public bool SelectedAgentHasTech(TechElement value) { return unlockedTech.Contains(value); }
    
    public string SelectedTechValue(TechElement tech)
    {
        if (currentSelection == null || currentSelection.agent == null) return "";
        var neurons = currentSelection.agent.brain.GetNeuronsByTechElement(tech);
        
        // Set neuron text and color
        var values = "";
        for (int i = 0; i < neurons.Count; i++)
        {
            //textStatsGraphLegend.text = "<color=#ff0000ff>Species A -----</color>\n\n<color=#00ff00ff>Species B -----</color>\n\n<color=#0000ffff>Species C -----</color>\n\n<color=#ffffffff>Species D -----</color>";
            Color textColor = new Color(0.67f, 0.67f, 0.67f);
            float neuronVal = 100f * neurons[i].currentValue;
            string plusMinus = "";
            if (neuronVal < 0f) {
                textColor = new Color(0.9f, 0.8f, 0.6f);
            }            
            else if (neuronVal > 0f) {  
                textColor = new Color(0.5f, 0.8f, 0.96f);
                plusMinus = "+";
            }
            string hexColorCode = ColorUtility.ToHtmlStringRGB(textColor);
            values += "<color=#" + hexColorCode + ">" + plusMinus + neuronVal.ToString("F0") + "</color>";  // neurons[i].name + " " + 
            if (i < neurons.Count - 1) values += ", ";
        }
        return values;
    }
}

    public class SelectionData {
        public int historySelectedSpeciesID;
        public CandidateAgentData candidate;
        public Agent agent;
        public bool isGenomeOnly;
    }

