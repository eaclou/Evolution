using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatureIconUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    UIManager uiManagerRef => UIManager.instance;

    public int index = -1;
    //private SpeciesOverviewUI.SelectionGroup group;
    
    public Image imageBG;
    public bool isSelected = false;

	public void UpdateButtonPrefab(int slotIndex) {
        index = slotIndex;
        //group = grp;
    }

    public void Clicked() {
        /*uiManagerRef.speciesOverviewUI.ChangeSelectedGenome(group, index);  // updates focusedCandidate in uiManager
        
        // find corresponding agent:
        if(uiManagerRef.focusedCandidate.isBeingEvaluated) {

            bool isFound = false;
            int agentIndex = -1;
            for(int i = 0; i < simulationManager.agentsArray.Length; i++) {
                if(simulationManager.agentsArray[i].candidateRef.candidateID == uiManagerRef.focusedCandidate.candidateID) {
                    isFound = true;
                    agentIndex = i;
                    break;
                }
            }
            if(isFound) {
                cameraManager.SetTargetAgent(simulationManager.agentsArray[agentIndex], agentIndex);
                uiManagerRef.SetFocusedCandidateGenome(uiManagerRef.focusedCandidate); //.StartFollowingAgent();
                cameraManager.isFollowingAgent = true;
            }
        } 
        */
    }
}
