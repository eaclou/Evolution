using UnityEngine;
using UnityEngine.UI;

public class GenomeButtonPrefabScript : MonoBehaviour {
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    UIManager uiManagerRef => UIManager.instance;

    public int index = -1;
    private SpeciesOverviewUI.SelectionGroup group;
    //public Text textEventName;
    //public Text textEventCost;
    public Image imageBG;
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
        uiManagerRef.speciesOverviewUI.ChangeSelectedGenome(group, index);  // updates focusedCandidate in uiManager
        
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
    }
}
