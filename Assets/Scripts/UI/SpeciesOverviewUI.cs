using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeciesOverviewUI : MonoBehaviour {
    SimulationManager simulationManager => SimulationManager.instance;
    UIManager uiManager => UIManager.instance;
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

    // * WPP: references null in editor, intent unclear
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
    
    Color CLEAR => Color.black;

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
    
    // **** CHANGE to properly pooled, only create as needed, etc. ****
    public void RebuildGenomeButtons() 
    {
        // WPP: shortened references with getters
        SpeciesGenomePool pool = simulationManager.GetSelectedGenomePool(); //masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID];

        Vector3 hueA = pool.appearanceGenome.huePrimary; //foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
        imageLineageA.color = new Color(Mathf.Max(0.2f, hueA.x), Mathf.Max(0.2f, hueA.y), Mathf.Max(0.2f, hueA.z));
        Vector3 hueB = pool.appearanceGenome.hueSecondary;
        imageLineageB.color = new Color(Mathf.Max(0.2f, hueB.x), Mathf.Max(0.2f, hueB.y), Mathf.Max(0.2f, hueB.z));

        // Update Species Panel UI:
        textSpeciesLineage.gameObject.SetActive(true);
        int savedSpeciesID = pool.speciesID;
        int parentSpeciesID = pool.parentSpeciesID;
        string lineageTxt = savedSpeciesID.ToString();
        
        for(int i = 0; i < 64; i++) 
        {
            // WPP: inverted logic to remove nesting
            if (parentSpeciesID < 0)
            {
                lineageTxt += "*";
                break;
            }
        
            //if (parentSpeciesID >= 0) 
            //{
            
            SpeciesGenomePool parentPool = simulationManager.GetGenomePoolBySpeciesID(parentSpeciesID); //masterGenomePool.completeSpeciesPoolsList[parentSpeciesID];
            lineageTxt += " <- " + parentPool.speciesID;

            savedSpeciesID = parentPool.speciesID;
            parentSpeciesID = parentPool.parentSpeciesID;
            
            //}
            //else 
            //{
            //    lineageTxt += "*";
            //    break;
            //}
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
    
    // WPP: moved to GenomeButtonPrefabScript
    private void UpdateGenomeButton(CandidateAgentData candidate, GenomeButtonPrefabScript genomeButton) {
        genomeButton.SetDisplay(candidate);
        /*string statusStr = "";

        if (iCand.isBeingEvaluated) 
        {
            // WPP 5/22/21: Simplified, moved to SimulationManager
            
            // find Agent:
            int matchingAgentIndex = -1;
            Agent matchingAgent = null;
            
            for(int a = 0; a < simulationManager.agentsArray.Length; a++) 
            {
                Agent agent = simulationManager.agentsArray[a];

                if(agent.candidateRef == null)
                    continue;

                if(iCand.candidateID == agent.candidateRef.candidateID) 
                {
                    matchingAgentIndex = a;
                    matchingAgent = simulationManager.agentsArray[matchingAgentIndex];
                    break;
                }
            }
            Agent matchingAgent = simulationManager.GetAgent(iCand);
            
            // *********** WRITE IT DOWN!!! **************
            if (uiManager.IsFocus(iCand))
            {
                genomeButton.backgroundImage.color = Color.white * 1f;
                ColorBlock block = genomeButton.button.colors;
                block.colorMultiplier = 2f;
                genomeButton.button.colors = block;
                genomeButton.gameObject.transform.localScale = Vector3.one * 1.3f;
                statusStr = "\n(SELECTED)";
            }
            else 
            {
                ColorBlock block = genomeButton.button.colors;
                block.colorMultiplier = 1f;
                genomeButton.button.colors = block;
                
                statusStr = genomeButton.SetDisplay(matchingAgent);
                if (matchingAgent.isDead)
                {
                    genomeButton.gameObject.transform.localScale = Vector3.one * 0.9f;
                    genomeButton.backgroundImage.color = Color.red;
                    statusStr = "\n(DEAD!)";
                }
                else if(matchingAgent.isEgg) 
                {
                    genomeButton.gameObject.transform.localScale = Vector3.one * 1f;
                    genomeButton.backgroundImage.color = Color.yellow;
                    statusStr = "\n(EGG!)";
                }
                else 
                {  
                    genomeButton.gameObject.transform.localScale = Vector3.one * 1f;
                    genomeButton.backgroundImage.color = Color.green;
                    statusStr = "\n(ALIVE!)";
                }
                
                //buttonScript.GetComponent<Image>().color = Color.white;
                //statusStr = "\n(Under Evaluation)";
            }
        }
        else 
        {
            genomeButton.gameObject.transform.localScale = Vector3.one;
            if(iCand.allEvaluationsComplete) 
            {
                genomeButton.backgroundImage.color = Color.gray;
                statusStr = "\n(Fossil)";
            }
            else 
            {
                genomeButton.gameObject.SetActive(false);
                genomeButton.backgroundImage.color = Color.black;
                statusStr = "\n(Unborn)";
            }
        }        

        //tooltip.genomeViewerUIRef = uiManagerRef.genomeViewerUI;
        genomeButton.tooltip.tooltipString ="Creature #" + iCand.candidateID + statusStr;
        //uiManagerRef.speciesOverviewUI.leaderboardGenomeButtonsList.Add(buttonScript);
        */
    }

    private void RebuildGenomeButtonsCurrent(SpeciesGenomePool pool) {
        Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;            
        uiManager.panelLeaderboardGenomes.GetComponent<Image>().color = new Color(hue.x, hue.y, hue.z);
        
        // Current Genepool:
        foreach (Transform child in uiManager.panelLeaderboardGenomes.transform) {
             Destroy(child.gameObject);
        }
        
        for(int i = 0; i < Mathf.Min(pool.candidateGenomesList.Count, 24); i++) {
            GameObject tempObj = Instantiate(genomeIcon, new Vector3(0, 0, 0), Quaternion.identity);
            tempObj.transform.SetParent(uiManager.panelLeaderboardGenomes.transform, false);
            
            GenomeButtonPrefabScript buttonScript = tempObj.GetComponent<GenomeButtonPrefabScript>();
            buttonScript.UpdateButtonPrefab(SelectionGroup.Candidates, i);

            CandidateAgentData iCand = pool.candidateGenomesList[i];
            UpdateGenomeButton(iCand, buttonScript);
        }
    }
    
    private void RebuildGenomeButtonsLineage(SpeciesGenomePool pool) {
        // Hall of fame genomes (checkpoints .. every 50 years?)
        foreach (Transform child in uiManager.panelHallOfFameGenomes.transform) {
             Destroy(child.gameObject);
        }
        
        for(int i = 0; i < pool.hallOfFameGenomesList.Count; i++) {
            GameObject tempObj = Instantiate(genomeIcon, new Vector3(0, 0, 0), Quaternion.identity);
            tempObj.transform.SetParent(uiManager.panelHallOfFameGenomes.transform, false);
            GenomeButtonPrefabScript buttonScript = tempObj.GetComponent<GenomeButtonPrefabScript>();
            buttonScript.UpdateButtonPrefab(SelectionGroup.HallOfFame, i);

            CandidateAgentData iCand = pool.hallOfFameGenomesList[i];
            UpdateGenomeButton(iCand, buttonScript);

            //uiManagerRef.speciesOverviewUI.hallOfFameButtonsList.Add(buttonScript);
        }
    }

    public void ChangeSelectedGenome(SelectionGroup group, int index) {
        //selectionGroup = group;                

        if(selectedButton != null) 
        {
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

        SpeciesGenomePool spool = simulationManager.GetSelectedGenomePool();

        switch (group) 
        {
            case SelectionGroup.Founder:
                //isFoundingGenomeSelected = true;
                selectedButton = buttonFoundingGenome;
                uiManager.SetFocusedCandidateGenome(spool.foundingCandidate);
                break;
            case SelectionGroup.Representative:
               // isRepresentativeGenomeSelected = true;
                selectedButton = buttonRepresentativeGenome;
                uiManager.SetFocusedCandidateGenome(spool.representativeCandidate); // maybe weird if used as species-wide average? 
                break;
            case SelectionGroup.LongestLived:
               // isLongestLivedGenomeSelected = true;
                selectedButton = buttonLongestLivedGenome;
                uiManager.SetFocusedCandidateGenome(spool.longestLivedCandidate);
                break;
            case SelectionGroup.MostEaten:
                //isMostEatenGenomeSelected = true;
                selectedButton = buttonMostEatenGenome;
                uiManager.SetFocusedCandidateGenome(spool.mostEatenCandidate);
                break;
            case SelectionGroup.HallOfFame:
                //isHallOfFameSelected = true;
                selectedHallOfFameIndex = index;
                if(selectedHallOfFameIndex >= spool.hallOfFameGenomesList.Count) 
                {
                    selectedHallOfFameIndex = 0;
                }
                //selectedButton = hallOfFameButtonsList[index].GetComponent<Button>();
                
                uiManager.SetFocusedCandidateGenome(spool.hallOfFameGenomesList[index]);

                Debug.Log("ChangeSelectedGenome: " + group + ", HallOfFame, #" + index);
                break;
            case SelectionGroup.Leaderboard:
                //isLeaderboardGenomesSelected = true;
                //selectedLeaderboardGenomeIndex = index;

                //SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.globalResourcesUI.selectedSpeciesIndex];
                uiManager.SetFocusedCandidateGenome(spool.leaderboardGenomesList[index]);

                Debug.Log("ChangeSelectedGenome: " + group + ", #" + index);
                //selectedButton = leaderboardGenomeButtonsList[index].GetComponent<Button>();
                break;
            case SelectionGroup.Candidates:
                //isCandidateGenomesSelected = true;
                selectedCandidateGenomeIndex = index;
                if(selectedCandidateGenomeIndex >= spool.candidateGenomesList.Count) 
                {
                    selectedCandidateGenomeIndex = 0;
                }
                cameraManager.SetTargetAgent(simulationManager.agentsArray[cameraManager.targetAgentIndex], cameraManager.targetAgentIndex);
                uiManager.SetFocusedCandidateGenome(spool.candidateGenomesList[index]);
                Debug.Log("ChangeSelectedGenome: " + group + ", #" + index);
                //selectedButton = candidateGenomeButtonsList[index].GetComponent<Button>();
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
        SpeciesGenomePool pool = simulationManager.GetSelectedGenomePool();
        //for(int i = 0; i < pool.leaderboardGenomesList.Count; i++) {
        for(int x = 0; x < width; x++) {
            for(int y = 0; y < height; y++) {

                int xIndex = x; 
                int yIndex = y;            
                              
                Color testColor;

                if(x < pool.leaderboardGenomesList.Count) 
                {
                    AgentGenome genome = pool.leaderboardGenomesList[x].candidateGenome;
                    if (genome.brainGenome.linkList.Count > y) 
                    {
                        float weightVal = genome.brainGenome.linkList[y].weight;
                        testColor = new Color(weightVal * 0.5f + 0.5f, weightVal * 0.5f + 0.5f, weightVal * 0.5f + 0.5f);
                        if(weightVal < -0.25f) 
                        {
                            testColor = Color.Lerp(testColor, Color.black, 0.15f);
                        }
                        else if(weightVal > 0.25f) 
                        {
                            testColor = Color.Lerp(testColor, Color.white, 0.15f);
                        }
                        else 
                        {
                            testColor = Color.Lerp(testColor, Color.gray, 0.15f);
                        }
                    }
                    else 
                    {
                        testColor = CLEAR;
                    }
                }
                else 
                {
                    testColor = CLEAR;
                }

                speciesPoolGenomeTex.SetPixel(xIndex, yIndex, testColor);
            }
        }
        
        // Body Genome
        //int xI = curLinearIndex % speciesPoolGenomeTex.width;
        //int yI = Mathf.FloorToInt(curLinearIndex / speciesPoolGenomeTex.width);
        
        speciesPoolGenomeTex.Apply();
    }

	void Start () {
        selectedButton = buttonFoundingGenome; // default
        
        speciesPoolGenomeTex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        speciesPoolGenomeTex.filterMode = FilterMode.Point;
        speciesPoolGenomeTex.wrapMode = TextureWrapMode.Clamp;
        speciesPoolGenomeMat.SetTexture("_MainTex", speciesPoolGenomeTex);
	}
}
