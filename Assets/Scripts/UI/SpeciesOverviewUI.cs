using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeciesOverviewUI : MonoBehaviour {
    SimulationManager simulationManager => SimulationManager.instance;
    UIManager uiManagerRef => UIManager.instance;
    Lookup lookup => Lookup.instance;
    GameObject genomeIcon => lookup.genomeIcon;
    CameraManager cameraManager => CameraManager.instance;

    //private bool isFoundingGenomeSelected = false;
    //private bool isRepresentativeGenomeSelected = false;
    //private bool isLongestLivedGenomeSelected = false;
    //private bool isMostEatenGenomeSelected = false;
    //private bool isHallOfFameSelected = false;
    private int selectedHallOfFameIndex = 0;
    //private bool isLeaderboardGenomesSelected = false;
    //private int selectedLeaderboardGenomeIndex = 0;
    //private bool isCandidateGenomesSelected = false;
    private int selectedCandidateGenomeIndex = 0;

    public GameObject panelCurrentGenepool;
    public GameObject panelLineageGenomes;
    public GameObject panelGenomeViewer;
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

    
    private Texture2D speciesPoolGenomeTex; // speciesOverviewPanel
    public Material speciesPoolGenomeMat;

    //private SelectionGroup selectionGroup = SelectionGroup.Founder;
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

    public void RefreshPanelUI() {
        RebuildGenomeButtons();

    }
    public void RebuildGenomeButtons() { // **** CHANGE to properly pooled, only create as needed, etc. ****
        
        SpeciesGenomePool pool = simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID];

        Vector3 hueA = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
        imageLineageA.color = new Color(Mathf.Max(0.2f, hueA.x), Mathf.Max(0.2f, hueA.y), Mathf.Max(0.2f, hueA.z));
        Vector3 hueB = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary;
        imageLineageB.color = new Color(Mathf.Max(0.2f, hueB.x), Mathf.Max(0.2f, hueB.y), Mathf.Max(0.2f, hueB.z));

        // Update Species Panel UI:
        textSpeciesLineage.gameObject.SetActive(true);
        int savedSpeciesID = pool.speciesID;
        int parentSpeciesID = pool.parentSpeciesID;
        string lineageTxt = savedSpeciesID.ToString();
        for(int i = 0; i < 64; i++) {
                
            if (parentSpeciesID >= 0) {
                //get parent pool:
                SpeciesGenomePool parentPool = simulationManager.masterGenomePool.completeSpeciesPoolsList[parentSpeciesID];
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
    
    private void UpdateGenomeButton(SpeciesGenomePool pool, CandidateAgentData iCand, Button buttonScript) {

        
        string statusStr = "";


        if (iCand.isBeingEvaluated) {

            // find Agent:
            int matchingAgentIndex = -1;
            Agent matchingAgent = null;
            for(int a = 0; a < simulationManager.agentsArray.Length; a++) {

                Agent gent = simulationManager.agentsArray[a];

                if(gent.candidateRef != null) {
                    if(iCand.candidateID == gent.candidateRef.candidateID) {
                        matchingAgentIndex = a;
                        matchingAgent = simulationManager.agentsArray[matchingAgentIndex];
                        break;
                            
                            
                    }
                        
                }                    
            }
            // *********** WRITE IT DOWN!!! **************

            if(iCand.candidateID == uiManagerRef.focusedCandidate.candidateID) {
                buttonScript.GetComponent<Image>().color = Color.white * 1f;
                ColorBlock block = buttonScript.GetComponent<Button>().colors;
                block.colorMultiplier = 2f;
                buttonScript.GetComponent<Button>().colors = block;
                buttonScript.gameObject.transform.localScale = Vector3.one * 1.3f;
                statusStr = "\n(SELECTED)";
            }
            else {
                ColorBlock block = buttonScript.GetComponent<Button>().colors;
                block.colorMultiplier = 1f;
                buttonScript.GetComponent<Button>().colors = block;
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
                buttonScript.gameObject.SetActive(false);
                buttonScript.GetComponent<Image>().color = Color.black;
                statusStr = "\n(Unborn)";
            }
        }        

        GenomeButtonTooltipSource tooltip = buttonScript.GetComponent<GenomeButtonTooltipSource>();
        //tooltip.genomeViewerUIRef = uiManagerRef.genomeViewerUI;
        tooltip.tooltipString ="Creature #" + iCand.candidateID.ToString() + statusStr;
        //uiManagerRef.speciesOverviewUI.leaderboardGenomeButtonsList.Add(buttonScript);
    }
    private void RebuildGenomeButtonsCurrent(SpeciesGenomePool pool) {
        Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;            
        uiManagerRef.panelLeaderboardGenomes.GetComponent<Image>().color = new Color(hue.x, hue.y, hue.z);
        // Current Genepool:
        foreach (Transform child in uiManagerRef.panelLeaderboardGenomes.transform) {
             Destroy(child.gameObject);
        }
        for(int i = 0; i < Mathf.Min(pool.candidateGenomesList.Count, 24); i++) {
            GameObject tempObj = Instantiate(genomeIcon, new Vector3(0, 0, 0), Quaternion.identity);
            tempObj.transform.SetParent(uiManagerRef.panelLeaderboardGenomes.transform, false);
            GenomeButtonPrefabScript buttonScript = tempObj.GetComponent<GenomeButtonPrefabScript>();
            buttonScript.UpdateButtonPrefab(SelectionGroup.Candidates, i);

            CandidateAgentData iCand = pool.candidateGenomesList[i];
            UpdateGenomeButton(pool, iCand, buttonScript.GetComponent<Button>());
            
        }
    }
    private void RebuildGenomeButtonsLineage(SpeciesGenomePool pool) {
        // Hall of fame genomes (checkpoints .. every 50 years?)
        foreach (Transform child in uiManagerRef.panelHallOfFameGenomes.transform) {
             Destroy(child.gameObject);
        }
        for(int i = 0; i < pool.hallOfFameGenomesList.Count; i++) {
            GameObject tempObj = Instantiate(genomeIcon, new Vector3(0, 0, 0), Quaternion.identity);
            tempObj.transform.SetParent(uiManagerRef.panelHallOfFameGenomes.transform, false);
            GenomeButtonPrefabScript buttonScript = tempObj.GetComponent<GenomeButtonPrefabScript>();
            buttonScript.UpdateButtonPrefab(SelectionGroup.HallOfFame, i);

            CandidateAgentData iCand = pool.hallOfFameGenomesList[i];
            UpdateGenomeButton(pool, iCand, buttonScript.GetComponent<Button>());

            //uiManagerRef.speciesOverviewUI.hallOfFameButtonsList.Add(buttonScript);
        }
    }

    public void ChangeSelectedGenome(SelectionGroup group, int index) {
        //selectionGroup = group;                

        if(selectedButton != null) {
            selectedButton.GetComponent<Image>().color = Color.yellow;
        }
        //clear all selections
       // isFoundingGenomeSelected = false;
       // isRepresentativeGenomeSelected = false;
       // isLongestLivedGenomeSelected = false;
       // isMostEatenGenomeSelected = false;
       // isHallOfFameSelected = false;    
        //isLeaderboardGenomesSelected = false;    
        //isCandidateGenomesSelected = false;    

        SpeciesGenomePool spool = simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID];


        switch(group) {
            case SelectionGroup.Founder:
                //isFoundingGenomeSelected = true;
                selectedButton = buttonFoundingGenome;
                uiManagerRef.SetFocusedCandidateGenome(spool.foundingCandidate);
                break;
            case SelectionGroup.Representative:
               // isRepresentativeGenomeSelected = true;
                selectedButton = buttonRepresentativeGenome;
                uiManagerRef.SetFocusedCandidateGenome(spool.representativeCandidate); // maybe weird if used as species-wide average? 
                break;
            case SelectionGroup.LongestLived:
               // isLongestLivedGenomeSelected = true;
                selectedButton = buttonLongestLivedGenome;
                uiManagerRef.SetFocusedCandidateGenome(spool.longestLivedCandidate);
                break;
            case SelectionGroup.MostEaten:
                //isMostEatenGenomeSelected = true;
                selectedButton = buttonMostEatenGenome;
                uiManagerRef.SetFocusedCandidateGenome(spool.mostEatenCandidate);
                break;
            case SelectionGroup.HallOfFame:
                //isHallOfFameSelected = true;
                selectedHallOfFameIndex = index;
                if(selectedHallOfFameIndex >= spool.hallOfFameGenomesList.Count) {
                    selectedHallOfFameIndex = 0;
                }
                //selectedButton = hallOfFameButtonsList[index].GetComponent<Button>();
                
                uiManagerRef.SetFocusedCandidateGenome(spool.hallOfFameGenomesList[index]);

                Debug.Log("ChangeSelectedGenome: " + group.ToString() + ", HallOfFame, #" + index.ToString());
                break;
            case SelectionGroup.Leaderboard:
                //isLeaderboardGenomesSelected = true;
                //selectedLeaderboardGenomeIndex = index;

                //SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.globalResourcesUI.selectedSpeciesIndex];
                uiManagerRef.SetFocusedCandidateGenome(spool.leaderboardGenomesList[index]);

                Debug.Log("ChangeSelectedGenome: " + group.ToString() + ", #" + index.ToString());
                //selectedButton = leaderboardGenomeButtonsList[index].GetComponent<Button>();
                break;
            case SelectionGroup.Candidates:
                //isCandidateGenomesSelected = true;
                selectedCandidateGenomeIndex = index;
                if(selectedCandidateGenomeIndex >= spool.candidateGenomesList.Count) {
                    selectedCandidateGenomeIndex = 0;
                }
                cameraManager.SetTargetAgent(simulationManager.agentsArray[cameraManager.targetAgentIndex], cameraManager.targetAgentIndex);
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

        //uiManagerRef.ClickButtonOpenGenome();
        panelGenomeViewer.SetActive(true);
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
    
    
    public void CreateSpeciesLeaderboardGenomeTexture() {
        int width = 32;
        int height = 96;
        speciesPoolGenomeTex.Resize(width, height); // pool.leaderboardGenomesList.Count);
        SpeciesGenomePool pool = simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID];
        //for(int i = 0; i < pool.leaderboardGenomesList.Count; i++) {
        for(int x = 0; x < width; x++) {
                        
            for(int y = 0; y < height; y++) {

                int xIndex = x; 
                int yIndex = y;            
                              
                Color testColor;

                if(x < pool.leaderboardGenomesList.Count) {
                    AgentGenome genome = pool.leaderboardGenomesList[x].candidateGenome;
                    if (genome.brainGenome.linkList.Count > y) {

                        float weightVal = genome.brainGenome.linkList[y].weight;
                        testColor = new Color(weightVal * 0.5f + 0.5f, weightVal * 0.5f + 0.5f, weightVal * 0.5f + 0.5f);
                        if(weightVal < -0.25f) {
                            testColor = Color.Lerp(testColor, Color.black, 0.15f);
                        }
                        else if(weightVal > 0.25f) {
                            testColor = Color.Lerp(testColor, Color.white, 0.15f);
                        }
                        else {
                            testColor = Color.Lerp(testColor, Color.gray, 0.15f);
                        }
                    }
                    else {
                        testColor = Color.black; // CLEAR
                    
                        //break;
                    }
                }
                else {
                    testColor = Color.black; // CLEAR
                }

                
                
                speciesPoolGenomeTex.SetPixel(xIndex, yIndex, testColor);
            }

        }
        
       
        //}
          
            
        
        // Body Genome
        //int xI = curLinearIndex % speciesPoolGenomeTex.width;
        //int yI = Mathf.FloorToInt(curLinearIndex / speciesPoolGenomeTex.width);
        
        speciesPoolGenomeTex.Apply();
    }

	// Use this for initialization
	void Start () {
        selectedButton = buttonFoundingGenome; // default

        
        speciesPoolGenomeTex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        speciesPoolGenomeTex.filterMode = FilterMode.Point;
        speciesPoolGenomeTex.wrapMode = TextureWrapMode.Clamp;
        speciesPoolGenomeMat.SetTexture("_MainTex", speciesPoolGenomeTex);
	}
}
