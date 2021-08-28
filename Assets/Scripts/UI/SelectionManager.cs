using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    UIManager uiManager => UIManager.instance;
    SimulationManager simulationManager => SimulationManager.instance;
    MasterGenomePool genomePool => simulationManager.masterGenomePool;
    //TrophicLayersManager trophicLayersManager => simulationManager.trophicLayersManager;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    //TheCursorCzar theCursorCzar => TheCursorCzar.instance;

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
        
        if(focusedCandidate != null && focusedCandidate.candidateGenome != null) {
            uiManager.genomeViewerUI.UpdateUI();

            if(uiManager.isRebuildTimeStep) {
                uiManager.speciesOverviewUI.RebuildGenomeButtons();  
            }
        } 
    }
    
    public bool IsFocus(CandidateAgentData candidate) { return candidate.candidateID == focusedCandidate.candidateID; }
    
    public void SetSelectedSpeciesUI(int id) {
        
        selectedSpeciesID = id;
        uiManager.speciesOverviewUI.RebuildGenomeButtons();
    }

    public void Tick() {

    }
}
