using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeciesOverviewUI : MonoBehaviour {
    public UIManager uiManagerRef;

    private bool isFoundingGenomeSelected = false;
    private bool isRepresentativeGenomeSelected = false;
    private bool isLongestLivedGenomeSelected = false;
    private bool isMostEatenGenomeSelected = false;
    private bool isHallOfFameSelected = false;
    private int selectedHallOfFameIndex = 0;
    private bool isLeaderboardGenomesSelected = false;
    private int selectedLeaderboardGenomeIndex = 0;
    private bool isCandidateGenomesSelected = false;
    private int selectedCandidateGenomeIndex = 0;

    public Button selectedButton;

    public Button buttonFoundingGenome;
    public Button buttonRepresentativeGenome;
    public Button buttonLongestLivedGenome;
    public Button buttonMostEatenGenome;
    public List<GenomeButtonPrefabScript> hallOfFameButtonsList;
    public List<GenomeButtonPrefabScript> leaderboardGenomeButtonsList;
    public List<GenomeButtonPrefabScript> candidateGenomeButtonsList;

    private SelectionGroup selectionGroup = SelectionGroup.Founder;
    public enum SelectionGroup {
        Founder,
        Representative,
        LongestLived,
        MostEaten,
        HallOfFame,
        Leaderboard,
        Candidates
    }

    public void RebuildGenomeButtons() {
        
        SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.globalResourcesUI.selectedSpeciesIndex];

        // Update Species Panel UI:
        // Hall of fame genomes (checkpoints .. every 50 years?)
        foreach (Transform child in uiManagerRef.panelHallOfFameGenomes.transform) {
             GameObject.Destroy(child.gameObject);
        }
        for(int i = 0; i < pool.hallOfFameGenomesList.Count; i++) {
            GameObject tempObj = Instantiate(uiManagerRef.prefabGenomeIcon, new Vector3(0, 0, 0), Quaternion.identity);
            tempObj.transform.SetParent(uiManagerRef.panelHallOfFameGenomes.transform, false);
            GenomeButtonPrefabScript buttonScript = tempObj.GetComponent<GenomeButtonPrefabScript>();
            buttonScript.UpdateButtonPrefab(uiManagerRef, SpeciesOverviewUI.SelectionGroup.HallOfFame, i);
            uiManagerRef.speciesOverviewUI.hallOfFameButtonsList.Add(buttonScript);
        }
        // Current Leaderboard:
        foreach (Transform child in uiManagerRef.panelLeaderboardGenomes.transform) {
             GameObject.Destroy(child.gameObject);
        }
        for(int i = 0; i < pool.leaderboardGenomesList.Count; i++) {
            GameObject tempObj = Instantiate(uiManagerRef.prefabGenomeIcon, new Vector3(0, 0, 0), Quaternion.identity);
            tempObj.transform.SetParent(uiManagerRef.panelLeaderboardGenomes.transform, false);

            GenomeButtonPrefabScript buttonScript = tempObj.GetComponent<GenomeButtonPrefabScript>();
            buttonScript.UpdateButtonPrefab(uiManagerRef, SpeciesOverviewUI.SelectionGroup.Leaderboard, i);
            uiManagerRef.speciesOverviewUI.leaderboardGenomeButtonsList.Add(buttonScript);
        }
    }


    public void ChangeSelectedGenome(SelectionGroup group, int index) {
        selectionGroup = group;
                

        if(selectedButton != null) {
            selectedButton.GetComponent<Image>().color = Color.yellow;
        }
        //clear all selections
        isFoundingGenomeSelected = false;
        isRepresentativeGenomeSelected = false;
        isLongestLivedGenomeSelected = false;
        isMostEatenGenomeSelected = false;
        isHallOfFameSelected = false;    
        isLeaderboardGenomesSelected = false;    
        isCandidateGenomesSelected = false;    

        SpeciesGenomePool spool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.globalResourcesUI.selectedSpeciesIndex];

        switch(group) {
            case SelectionGroup.Founder:
                isFoundingGenomeSelected = true;
                selectedButton = buttonFoundingGenome;
                uiManagerRef.globalResourcesUI.SetFocusedAgentGenome(spool.foundingGenome);
                break;
            case SelectionGroup.Representative:
                isRepresentativeGenomeSelected = true;
                selectedButton = buttonRepresentativeGenome;
                uiManagerRef.globalResourcesUI.SetFocusedAgentGenome(spool.representativeGenome);
                break;
            case SelectionGroup.LongestLived:
                isLongestLivedGenomeSelected = true;
                selectedButton = buttonLongestLivedGenome;
                uiManagerRef.globalResourcesUI.SetFocusedAgentGenome(spool.longestLivedGenome);
                break;
            case SelectionGroup.MostEaten:
                isMostEatenGenomeSelected = true;
                selectedButton = buttonMostEatenGenome;
                uiManagerRef.globalResourcesUI.SetFocusedAgentGenome(spool.mostEatenGenome);
                break;
            case SelectionGroup.HallOfFame:
                isHallOfFameSelected = true;
                selectedHallOfFameIndex = index;
                //selectedButton = hallOfFameButtonsList[index].GetComponent<Button>();
                
                uiManagerRef.globalResourcesUI.SetFocusedAgentGenome(spool.hallOfFameGenomesList[index]);

                Debug.Log("ChangeSelectedGenome: " + group.ToString() + ", HallOfFame, #" + index.ToString());
                break;
            case SelectionGroup.Leaderboard:
                isLeaderboardGenomesSelected = true;
                selectedLeaderboardGenomeIndex = index;

                //SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.globalResourcesUI.selectedSpeciesIndex];
                uiManagerRef.globalResourcesUI.SetFocusedAgentGenome(spool.leaderboardGenomesList[index].candidateGenome);

                Debug.Log("ChangeSelectedGenome: " + group.ToString() + ", #" + index.ToString());
                //selectedButton = leaderboardGenomeButtonsList[index].GetComponent<Button>();
                break;
            case SelectionGroup.Candidates:
                isCandidateGenomesSelected = true;
                selectedCandidateGenomeIndex = index;
                //selectedButton = candidateGenomeButtonsList[index].GetComponent<Button>();
                break;
            default:
                //sda
                break;
        }

        if(selectedButton != null) {
            selectedButton.GetComponent<Image>().color = Color.white;
        }

        uiManagerRef.gameManager.simulationManager.theRenderKing.InitializeCreaturePortraitGenomes(uiManagerRef.globalResourcesUI.focusedAgentGenome);
    }

    
        
        

    public void UpdateUI(SpeciesGenomePool pool) {
        UpdateLeaderboardGenomesUI(pool);
    }
    private void UpdateLeaderboardGenomesUI(SpeciesGenomePool pool) {

    }
    
	// Use this for initialization
	void Start () {
        selectedButton = buttonFoundingGenome; // default
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
