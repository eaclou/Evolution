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
        
        SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID];

        // Update Species Panel UI:
        // Hall of fame genomes (checkpoints .. every 50 years?)
        /*foreach (Transform child in uiManagerRef.panelHallOfFameGenomes.transform) {
             GameObject.Destroy(child.gameObject);
        }
        for(int i = 0; i < pool.hallOfFameGenomesList.Count; i++) {
            GameObject tempObj = Instantiate(uiManagerRef.prefabGenomeIcon, new Vector3(0, 0, 0), Quaternion.identity);
            tempObj.transform.SetParent(uiManagerRef.panelHallOfFameGenomes.transform, false);
            GenomeButtonPrefabScript buttonScript = tempObj.GetComponent<GenomeButtonPrefabScript>();
            buttonScript.UpdateButtonPrefab(uiManagerRef, SpeciesOverviewUI.SelectionGroup.HallOfFame, i);
            uiManagerRef.speciesOverviewUI.hallOfFameButtonsList.Add(buttonScript);
        }*/
        // Current Leaderboard:
        /*foreach (Transform child in uiManagerRef.panelLeaderboardGenomes.transform) {
             GameObject.Destroy(child.gameObject);
        }
        for(int i = 0; i < Mathf.Min(32, pool.candidateGenomesList.Count); i++) {
            GameObject tempObj = Instantiate(uiManagerRef.prefabGenomeIcon, new Vector3(0, 0, 0), Quaternion.identity);
            tempObj.transform.SetParent(uiManagerRef.panelLeaderboardGenomes.transform, false);

            GenomeButtonPrefabScript buttonScript = tempObj.GetComponent<GenomeButtonPrefabScript>();
            buttonScript.UpdateButtonPrefab(uiManagerRef, SpeciesOverviewUI.SelectionGroup.Candidates, i);
            uiManagerRef.speciesOverviewUI.leaderboardGenomeButtonsList.Add(buttonScript);
        }*/
        Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;            
        uiManagerRef.panelLeaderboardGenomes.GetComponent<Image>().color = new Color(hue.x, hue.y, hue.z);
        // Current Genepool:
        foreach (Transform child in uiManagerRef.panelLeaderboardGenomes.transform) {
             GameObject.Destroy(child.gameObject);
        }
        for(int i = 0; i < Mathf.Min(pool.candidateGenomesList.Count, 32); i++) {
            GameObject tempObj = Instantiate(uiManagerRef.prefabGenomeIcon, new Vector3(0, 0, 0), Quaternion.identity);
            tempObj.transform.SetParent(uiManagerRef.panelLeaderboardGenomes.transform, false);
            GenomeButtonPrefabScript buttonScript = tempObj.GetComponent<GenomeButtonPrefabScript>();
            buttonScript.UpdateButtonPrefab(uiManagerRef, SpeciesOverviewUI.SelectionGroup.Candidates, i);
            CandidateAgentData iCand = pool.candidateGenomesList[i];
            string statusStr = "";
            if (iCand.isBeingEvaluated) {
                buttonScript.GetComponent<Image>().color = Color.white;
                statusStr = "\n(Under Evaluation)";
                
            }
            else {
                if(iCand.allEvaluationsComplete) {
                    buttonScript.GetComponent<Image>().color = Color.green;
                    statusStr = "\n(Fossil)";
                }
                else {
                    buttonScript.GetComponent<Image>().color = Color.gray;
                    statusStr = "\n(Unborn)";
                }
            }

            if(iCand.candidateID == uiManagerRef.focusedCandidate.candidateID) {
                buttonScript.GetComponent<Image>().color = Color.yellow;
                buttonScript.gameObject.transform.localScale = Vector3.one * 1.33f;
                statusStr = "\n(SELECTED)";
            }
            else { buttonScript.gameObject.transform.localScale = Vector3.one; }
            //buttonScript
            GenomeButtonTooltipSource tooltip = tempObj.GetComponent<GenomeButtonTooltipSource>();
            tooltip.genomeViewerUIRef = uiManagerRef.genomeViewerUI;
            tooltip.tooltipString = pool.speciesID.ToString() + "-" + pool.candidateGenomesList[i].candidateID.ToString() + statusStr;
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

        SpeciesGenomePool spool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID];


        switch(group) {
            case SelectionGroup.Founder:
                isFoundingGenomeSelected = true;
                selectedButton = buttonFoundingGenome;
                uiManagerRef.SetFocusedCandidateGenome(spool.foundingCandidate);
                break;
            case SelectionGroup.Representative:
                isRepresentativeGenomeSelected = true;
                selectedButton = buttonRepresentativeGenome;
                uiManagerRef.SetFocusedCandidateGenome(spool.representativeCandidate); // maybe weird if used as species-wide average? 
                break;
            case SelectionGroup.LongestLived:
                isLongestLivedGenomeSelected = true;
                selectedButton = buttonLongestLivedGenome;
                uiManagerRef.SetFocusedCandidateGenome(spool.longestLivedCandidate);
                break;
            case SelectionGroup.MostEaten:
                isMostEatenGenomeSelected = true;
                selectedButton = buttonMostEatenGenome;
                uiManagerRef.SetFocusedCandidateGenome(spool.mostEatenCandidate);
                break;
            case SelectionGroup.HallOfFame:
                isHallOfFameSelected = true;
                selectedHallOfFameIndex = index;
                //selectedButton = hallOfFameButtonsList[index].GetComponent<Button>();
                
                uiManagerRef.SetFocusedCandidateGenome(spool.hallOfFameGenomesList[index]);

                Debug.Log("ChangeSelectedGenome: " + group.ToString() + ", HallOfFame, #" + index.ToString());
                break;
            case SelectionGroup.Leaderboard:
                isLeaderboardGenomesSelected = true;
                selectedLeaderboardGenomeIndex = index;

                //SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.globalResourcesUI.selectedSpeciesIndex];
                uiManagerRef.SetFocusedCandidateGenome(spool.leaderboardGenomesList[index]);

                Debug.Log("ChangeSelectedGenome: " + group.ToString() + ", #" + index.ToString());
                //selectedButton = leaderboardGenomeButtonsList[index].GetComponent<Button>();
                break;
            case SelectionGroup.Candidates:
                isCandidateGenomesSelected = true;
                selectedCandidateGenomeIndex = index;

                uiManagerRef.SetFocusedCandidateGenome(spool.candidateGenomesList[index]);
                Debug.Log("ChangeSelectedGenome: " + group.ToString() + ", #" + index.ToString());
                //selectedButton = candidateGenomeButtonsList[index].GetComponent<Button>();
                break;
            default:
                //sda
                break;
        }

        if(selectedButton != null) {
            selectedButton.GetComponent<Image>().color = Color.white;
        }


        uiManagerRef.ClickButtonOpenGenome();
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
