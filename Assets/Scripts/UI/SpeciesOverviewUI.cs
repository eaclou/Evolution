﻿using System.Collections;
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

    public GameObject panelCurrentGenepool;
    public GameObject panelLineageGenomes;
    public Image imageLineageA;
    public Image imageLineageB;

    public Slider sliderLineageGenomes;

    public Button selectedButton;

    public Button buttonFoundingGenome;
    public Button buttonRepresentativeGenome;
    public Button buttonLongestLivedGenome;
    public Button buttonMostEatenGenome;
    public List<GenomeButtonPrefabScript> hallOfFameButtonsList;
    public List<GenomeButtonPrefabScript> leaderboardGenomeButtonsList;
    public List<GenomeButtonPrefabScript> candidateGenomeButtonsList;

    public Text textHallOfFameTitle;
    public Text textCurrentGenepoolTitle;

    public Text textSpeciesLineage;
    
    public bool isShowingLineage = false;

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

    public void ClickButtonToggleLineage() {
        isShowingLineage = !isShowingLineage;

        RebuildGenomeButtons();
    }

    public void RebuildGenomeButtons() { // **** CHANGE to properly pooled, only create as needed, etc. ****
        
        SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID];

        Vector3 hueA = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
        imageLineageA.color = new Color(hueA.x, hueA.y, hueA.z);
        Vector3 hueB = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary;
        imageLineageB.color = new Color(hueB.x, hueB.y, hueB.z);

        // Update Species Panel UI:
        textSpeciesLineage.gameObject.SetActive(true);
        int savedSpeciesID = pool.speciesID;
        int parentSpeciesID = pool.parentSpeciesID;
        string lineageTxt = savedSpeciesID.ToString();
        for(int i = 0; i < 64; i++) {
                
            if (parentSpeciesID >= 0) {
                //get parent pool:
                SpeciesGenomePool parentPool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[parentSpeciesID];
                lineageTxt += " <- " + parentPool.speciesID.ToString();

                savedSpeciesID = parentPool.speciesID;
                parentSpeciesID = parentPool.parentSpeciesID;
            }
            else {
                lineageTxt += "*";
                break;
            }
        }
        textSpeciesLineage.text = lineageTxt;

        //panelCurrentGenepool.SetActive(!isShowingLineage);
        //panelLineageGenomes.SetActive(isShowingLineage);
        //if(isShowingLineage) {

            //uiManagerRef.panelHallOfFameGenomes.SetActive(true);
            //textHallOfFameTitle.gameObject.SetActive(true);

            //uiManagerRef.panelLeaderboardGenomes.SetActive(false);
            //textCurrentGenepoolTitle.gameObject.SetActive(false);

        RebuildGenomeButtonsLineage(pool);

            //float timeLerp = Mathf.Clamp01(sliderLineageGenomes.value);

            
        //}
        //else {
            //uiManagerRef.panelHallOfFameGenomes.SetActive(false);
            //textHallOfFameTitle.gameObject.SetActive(false);

            //uiManagerRef.panelLeaderboardGenomes.SetActive(true);
            //textCurrentGenepoolTitle.gameObject.SetActive(true);

        RebuildGenomeButtonsCurrent(pool);
        //}
        
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
       
    }
    public void SliderChange(float val) {
        int num = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID].avgPerformanceDataYearList.Count;
        int index = Mathf.RoundToInt((float)num * val);

        //uiManagerRef.SetFocusedCandidateGenome(uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID].avgPerformanceDataYearList[index])
    }
    private void UpdateGenomeButton(SpeciesGenomePool pool, CandidateAgentData iCand, Button buttonScript) {

        
        string statusStr = "";


        if (iCand.isBeingEvaluated) {

            // find Agent:
            int matchingAgentIndex = -1;
            Agent matchingAgent = null;
            for(int a = 0; a < uiManagerRef.gameManager.simulationManager.agentsArray.Length; a++) {

                Agent gent = uiManagerRef.gameManager.simulationManager.agentsArray[a];

                if(gent.candidateRef != null) {
                    if(iCand.candidateID == gent.candidateRef.candidateID) {
                        matchingAgentIndex = a;
                        matchingAgent = uiManagerRef.gameManager.simulationManager.agentsArray[matchingAgentIndex];
                        break;
                            
                            
                    }
                        
                }                    
            }
            // *********** WRITE IT DOWN!!! **************

            if(iCand.candidateID == uiManagerRef.focusedCandidate.candidateID) {
                buttonScript.GetComponent<Image>().color = Color.white;
                buttonScript.gameObject.transform.localScale = Vector3.one * 1.3f;
                statusStr = "\n(SELECTED)";
            }
            else {
                if(matchingAgent.curLifeStage == Agent.AgentLifeStage.Dead) {
                    buttonScript.gameObject.transform.localScale = Vector3.one * 0.9f;
                    buttonScript.GetComponent<Image>().color = Color.red;
                    statusStr = "\n(DEAD!)";
                }
                else if(matchingAgent.curLifeStage == Agent.AgentLifeStage.Egg) {
                    buttonScript.gameObject.transform.localScale = Vector3.one * 1f;
                    buttonScript.GetComponent<Image>().color = Color.yellow;
                    statusStr = "\n(EGG!)";
                }
                else {  // alive
                    buttonScript.gameObject.transform.localScale = Vector3.one * 1f;
                    buttonScript.GetComponent<Image>().color = Color.green;
                    statusStr = "\n(ALIVE!)";
                }
                //buttonScript.GetComponent<Image>().color = Color.white;
                //statusStr = "\n(Under Evaluation)";
            }

        }
        else {
            buttonScript.gameObject.transform.localScale = Vector3.one;
            if(iCand.allEvaluationsComplete) {
                buttonScript.GetComponent<Image>().color = Color.gray;
                statusStr = "\n(Fossil)";
            }
            else {
                buttonScript.GetComponent<Image>().color = Color.gray;
                statusStr = "\n(Unborn)";
            }
        }        

        GenomeButtonTooltipSource tooltip = buttonScript.GetComponent<GenomeButtonTooltipSource>();
        tooltip.genomeViewerUIRef = uiManagerRef.genomeViewerUI;
        tooltip.tooltipString = pool.speciesID.ToString() + "-" + iCand.candidateID.ToString() + statusStr;
        //uiManagerRef.speciesOverviewUI.leaderboardGenomeButtonsList.Add(buttonScript);
    }
    private void RebuildGenomeButtonsCurrent(SpeciesGenomePool pool) {
         Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;            
        uiManagerRef.panelLeaderboardGenomes.GetComponent<Image>().color = new Color(hue.x, hue.y, hue.z);
        // Current Genepool:
        foreach (Transform child in uiManagerRef.panelLeaderboardGenomes.transform) {
             GameObject.Destroy(child.gameObject);
        }
        for(int i = 0; i < Mathf.Min(pool.candidateGenomesList.Count, 24); i++) {
            GameObject tempObj = Instantiate(uiManagerRef.prefabGenomeIcon, new Vector3(0, 0, 0), Quaternion.identity);
            tempObj.transform.SetParent(uiManagerRef.panelLeaderboardGenomes.transform, false);
            GenomeButtonPrefabScript buttonScript = tempObj.GetComponent<GenomeButtonPrefabScript>();
            buttonScript.UpdateButtonPrefab(uiManagerRef, SpeciesOverviewUI.SelectionGroup.Candidates, i);

            CandidateAgentData iCand = pool.candidateGenomesList[i];
            UpdateGenomeButton(pool, iCand, buttonScript.GetComponent<Button>());
            
        }
    }
    private void RebuildGenomeButtonsLineage(SpeciesGenomePool pool) {
        // Hall of fame genomes (checkpoints .. every 50 years?)
        foreach (Transform child in uiManagerRef.panelHallOfFameGenomes.transform) {
             GameObject.Destroy(child.gameObject);
        }
        for(int i = 0; i < pool.hallOfFameGenomesList.Count; i++) {
            GameObject tempObj = Instantiate(uiManagerRef.prefabGenomeIcon, new Vector3(0, 0, 0), Quaternion.identity);
            tempObj.transform.SetParent(uiManagerRef.panelHallOfFameGenomes.transform, false);
            GenomeButtonPrefabScript buttonScript = tempObj.GetComponent<GenomeButtonPrefabScript>();
            buttonScript.UpdateButtonPrefab(uiManagerRef, SpeciesOverviewUI.SelectionGroup.HallOfFame, i);

            CandidateAgentData iCand = pool.hallOfFameGenomesList[i];
            UpdateGenomeButton(pool, iCand, buttonScript.GetComponent<Button>());

            //uiManagerRef.speciesOverviewUI.hallOfFameButtonsList.Add(buttonScript);
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
                if(selectedHallOfFameIndex >= spool.hallOfFameGenomesList.Count) {
                    selectedHallOfFameIndex = 0;
                }
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
                if(selectedCandidateGenomeIndex >= spool.candidateGenomesList.Count) {
                    selectedCandidateGenomeIndex = 0;
                }

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

    public void CycleHallOfFame() {
        ChangeSelectedGenome(SelectionGroup.HallOfFame, selectedHallOfFameIndex + 1); 
    }
    public void CycleCurrentGenome() {
        ChangeSelectedGenome(SelectionGroup.Candidates, selectedCandidateGenomeIndex + 1); 
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
