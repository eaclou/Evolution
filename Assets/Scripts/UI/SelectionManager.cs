using Playcraft;

public class SelectionManager : Singleton<SelectionManager>
{
    UIManager uiManager => UIManager.instance;
    TheRenderKing theRenderKing => TheRenderKing.instance;

    public CandidateAgentData focusedCandidate; // *** TRANSITION TO THIS?
    public int selectedSpeciesID;

    public void ResetCurrentFocusedCandidateGenome() { SetFocusedCandidateGenome(focusedCandidate); }
    
    public void SetFocusedCandidateGenome(SpeciesGenomePool selectedPool, SelectionGroup group, int index) {
        var candidate = selectedPool.GetFocusedCandidate(group, index);
        SetFocusedCandidateGenome(candidate);
    }

    public void SetFocusedCandidateGenome(CandidateAgentData candidate) {
        focusedCandidate = candidate;
        
        theRenderKing.InitializeCreaturePortrait(focusedCandidate.candidateGenome);
        
        uiManager.speciesOverviewUI.RebuildGenomeButtons();
        uiManager.genomeViewerUI.brainGenomeImage.SetTexture(focusedCandidate.candidateGenome.brainGenome);

        SetSelectedSpeciesUI(focusedCandidate.speciesID);
    }

    public void SetFocus()
    {
        //pool = genomePool.completeSpeciesPoolsList[selectedSpeciesID];
        
        if (focusedCandidate?.candidateGenome == null) 
            return;
        
        uiManager.genomeViewerUI.UpdateUI();

        if(uiManager.isRebuildTimeStep) {
            uiManager.speciesOverviewUI.RebuildGenomeButtons();  
        }
    }
    
    public bool IsFocus(CandidateAgentData candidate) { return candidate.candidateID == focusedCandidate.candidateID; }
    
    public void SetSelectedSpeciesUI(int id) {
        selectedSpeciesID = id;
        uiManager.speciesOverviewUI.RebuildGenomeButtons();
    }
}
