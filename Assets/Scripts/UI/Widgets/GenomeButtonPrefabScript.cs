﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenomeButtonPrefabScript : MonoBehaviour {

    public int index = -1;
    private SpeciesOverviewUI.SelectionGroup group;
    //public Text textEventName;
    //public Text textEventCost;
    public Image imageBG;
    public bool isSelected = false;

    public UIManager uiManagerRef;

	public void UpdateButtonPrefab(UIManager uiManager, SpeciesOverviewUI.SelectionGroup grp, int slotIndex) {
        uiManagerRef = uiManager;
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
            for(int i = 0; i < uiManagerRef.gameManager.simulationManager.agentsArray.Length; i++) {
                if(uiManagerRef.gameManager.simulationManager.agentsArray[i].candidateRef.candidateID == uiManagerRef.focusedCandidate.candidateID) {
                    isFound = true;
                    agentIndex = i;
                    break;
                }
            }
            if(isFound) {
                uiManagerRef.cameraManager.SetTargetAgent(uiManagerRef.gameManager.simulationManager.agentsArray[agentIndex], agentIndex);
                uiManagerRef.watcherUI.StartFollowingAgent();
            }


        }
        
    }
}
