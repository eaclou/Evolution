using Playcraft;

public class SelectionManager : Singleton<SelectionManager>
{
    UIManager uiManager => UIManager.instance;
    TheRenderKing theRenderKing => TheRenderKing.instance;

    //public CandidateAgentData focusedCandidate;     
    public bool isFollowing;
    public SelectionData currentSelection;

    public void Start() {
        
    }
    public void ResetCurrentFocusedCandidateGenome() { SetSelected(currentSelection.candidate); }
    
    /*public void SetFocusedCandidateGenome(SpeciesGenomePool selectedPool, SelectionGroup group, int index) {
        var candidate = selectedPool.GetFocusedCandidate(group, index);
        SetSelected(candidate);
        UnityEngine.Debug.Log($"Selecting agent index {index}");
    }*/

    public void SetSelected(CandidateAgentData candidate) {
        if(currentSelection == null) {
            currentSelection = new SelectionData();
        }
        currentSelection.candidate = candidate;

        //check if corresponding agent exists:
        bool hasAgent = false;
        for(int i = 0; i < SimulationManager.instance.agents.Length; i++) {
            if(currentSelection.candidate.candidateID == SimulationManager.instance.agents[i].candidateRef.candidateID) {
                hasAgent = true;
                currentSelection.agent = SimulationManager.instance.agents[i];
                theRenderKing.InitializeCreaturePortrait(currentSelection.candidate.candidateGenome);
                uiManager.genomeViewerUI.brainGenomeImage.SetTexture(currentSelection.candidate.candidateGenome.brainGenome);
            }
        }
        
        currentSelection.isGenomeOnly = !hasAgent;
        SetHistorySelectedSpeciesUI(candidate.speciesID);
        uiManager.historyPanelUI.InitializePanel();
        //uiManager.historyPanelUI.RefreshFocusedAgent(currentSelection.agent);
        uiManager.speciesOverviewUI.RebuildGenomeButtons();
        
                
    }

    public void FollowedCreatureDied() {
        currentSelection.isGenomeOnly = true;
    }
    
    public bool IsSelected(CandidateAgentData candidate) { return candidate.candidateID == currentSelection.candidate.candidateID; }
    
    public void SetHistorySelectedSpeciesUI(int id) {
        currentSelection.historySelectedSpeciesID = id;        
    }
    
    public bool SelectedAgentHasTech(TechElementId techId) { return currentSelection.candidate.candidateGenome.bodyGenome.data.HasTech(techId); }

    public class SelectionData {
        public int historySelectedSpeciesID;
        public CandidateAgentData candidate;
        public Agent agent;
        public bool isGenomeOnly;
    }
}
