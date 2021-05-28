using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeciesOverviewUI : MonoBehaviour {
    SimulationManager simulationManager => SimulationManager.instance;
    UIManager uiManager => UIManager.instance;
    Lookup lookup => Lookup.instance;
    GameObject genomeIcon => lookup.genomeIcon;
    CameraManager cameraManager => CameraManager.instance;
    
    Color CLEAR => Color.black;


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
    
    public GameObject panelGenomeViewer;
    public Image genomeLeaderboard;
    public Image imageLineageA;
    public Image imageLineageB;
    
    [SerializeField] int maxButtons = 24;
    [SerializeField] SelectionGroup defaultSelectionGroup = SelectionGroup.Founder;
    [SerializeField] SelectionGroupData[] selectionGroups;
    SelectionGroupData selectedButtonData;
    
    // WPP 5/25: Buttons delegated to nested class
    //public Button selectedButton;
    //public Button buttonFoundingGenome;
    //public Button buttonRepresentativeGenome;
    //public Button buttonLongestLivedGenome;
    //public Button buttonMostEatenGenome;
    
    // * WPP: references null in editor, not used
    public List<GenomeButtonPrefabScript> hallOfFameButtonsList;
    public List<GenomeButtonPrefabScript> leaderboardGenomeButtonsList;
    public List<GenomeButtonPrefabScript> candidateGenomeButtonsList;
    public Text textHallOfFameTitle;
    public Text textCurrentGenepoolTitle;
    public GameObject panelCurrentGenepool;
    public GameObject panelLineageGenomes;
    public Slider sliderLineageGenomes;

    public Text textSpeciesLineage;
    
    public bool isShowingLineage = false;
    
    private Texture2D speciesPoolGenomeTex; // speciesOverviewPanel
    public Material speciesPoolGenomeMat;
    
    //private SelectionGroup selectionGroup = SelectionGroup.Founder;
    
    List<GenomeButtonPrefabScript> buttons = new List<GenomeButtonPrefabScript>();

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
        SpeciesGenomePool pool = simulationManager.GetSelectedGenomePool(); 

        Vector3 hueA = pool.appearanceGenome.huePrimary; 
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
            if (parentSpeciesID < 0)
            {
                lineageTxt += "*";
                break;
            }

            SpeciesGenomePool parentPool = simulationManager.GetGenomePoolBySpeciesID(parentSpeciesID);
            lineageTxt += " <- " + parentPool.speciesID;

            savedSpeciesID = parentPool.speciesID;
            parentSpeciesID = parentPool.parentSpeciesID;
        }
        
        textSpeciesLineage.text = lineageTxt;

        // * WPP: remove obsolete comments
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
    
    private void GenerateButtonList()
    {
        foreach (Transform child in genomeLeaderboard.transform) {
            Destroy(child.gameObject);
        }
        
        for(int i = 0; i < maxButtons; i++) {
            GameObject buttonObj = Instantiate(genomeIcon, Vector3.zero, Quaternion.identity);
            buttonObj.transform.SetParent(genomeLeaderboard.transform, false);
            buttons.Add(buttonObj.GetComponent<GenomeButtonPrefabScript>());
            buttonObj.gameObject.SetActive(false);
        }
    }

    // Current Genepool
    private void RebuildGenomeButtonsCurrent(SpeciesGenomePool pool) {
        Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;            
        genomeLeaderboard.color = new Color(hue.x, hue.y, hue.z);
        
        // WPP 5/28: moved to UpdateButtons, applied pooling
        /*foreach (Transform child in genomeLeaderboard.transform) {
             Destroy(child.gameObject);
        }
        
        for(int i = 0; i < Mathf.Min(pool.candidateGenomesList.Count, MAX_BUTTONS); i++) {
            GameObject tempObj = Instantiate(genomeIcon, Vector3.zero, Quaternion.identity);
            tempObj.transform.SetParent(genomeLeaderboard.transform, false);
            
            GenomeButtonPrefabScript buttonScript = tempObj.GetComponent<GenomeButtonPrefabScript>();
            buttonScript.UpdateButtonPrefab(SelectionGroup.Candidates, i);

            CandidateAgentData candidate = pool.candidateGenomesList[i];
            buttonScript.SetDisplay(candidate);
        }*/
        
        UpdateButtons(pool.candidateGenomesList, SelectionGroup.Candidates);
    }
    
    // Hall of fame genomes (checkpoints .. every 50 years?)
    private void RebuildGenomeButtonsLineage(SpeciesGenomePool pool) {
        // WPP 5/28: moved to UpdateButtons, applied pooling
        /*foreach (Transform child in uiManager.panelHallOfFameGenomes.transform) {
             Destroy(child.gameObject);
        }
        
        for(int i = 0; i < pool.hallOfFameGenomesList.Count; i++) {
            GameObject tempObj = Instantiate(genomeIcon, Vector3.zero, Quaternion.identity);
            tempObj.transform.SetParent(uiManager.panelHallOfFameGenomes.transform, false);
            
            GenomeButtonPrefabScript genomeButton = tempObj.GetComponent<GenomeButtonPrefabScript>();
            genomeButton.UpdateButtonPrefab(SelectionGroup.HallOfFame, i);

            CandidateAgentData candidate = pool.hallOfFameGenomesList[i];
            genomeButton.SetDisplay(candidate);
            //uiManagerRef.speciesOverviewUI.hallOfFameButtonsList.Add(buttonScript);
        }*/
        
        UpdateButtons(pool.hallOfFameGenomesList, SelectionGroup.HallOfFame);
    }
    
    private void UpdateButtons(List<CandidateAgentData> candidates, SelectionGroup groupId) {
        for(int i = 0; i < buttons.Count; i++) {
            var active = i < candidates.Count;
            
            buttons[i].gameObject.SetActive(active);
            buttons[i].UpdateButtonPrefab(groupId, i);
            
            if (active)
                buttons[i].SetDisplay(candidates[i]);   
        } 
    }

    // * WPP: remove obsolete comments
    public void ChangeSelectedGenome(SelectionGroup group, int index) {
        //selectionGroup = group;    
        
        if(selectedButtonData != null && selectedButtonData.image != null) {
            selectedButtonData.image.color = Color.yellow;
        }            

        //clear all selections
       // isFoundingGenomeSelected = false;
       // isRepresentativeGenomeSelected = false;
       // isLongestLivedGenomeSelected = false;
       // isMostEatenGenomeSelected = false;
       // isHallOfFameSelected = false;    
        //isLeaderboardGenomesSelected = false;    
        //isCandidateGenomesSelected = false;
        
        SpeciesGenomePool pool = simulationManager.GetSelectedGenomePool();
        uiManager.SetFocusedCandidateGenome(pool, group, index);
        selectedButtonData = GetSelectionGroupData(group);

        #region Removed
        // WPP: using nested struct to set the button and avoid GetComponent<Image>
        // + moved focus selection to UIManager
        // + moved special case operation to if statement
        //switch (group) 
        //{
            //case SelectionGroup.Founder:
                //isFoundingGenomeSelected = true;
                //selectedButton = buttonFoundingGenome;
                //uiManager.SetFocusedCandidateGenome(spool.foundingCandidate);
            //    break;
            //case SelectionGroup.Representative:
               // isRepresentativeGenomeSelected = true;
                //selectedButton = buttonRepresentativeGenome;
                //uiManager.SetFocusedCandidateGenome(spool.representativeCandidate); // maybe weird if used as species-wide average? 
            //    break;
            //case SelectionGroup.LongestLived:
               // isLongestLivedGenomeSelected = true;
                //selectedButton = buttonLongestLivedGenome;
                //uiManager.SetFocusedCandidateGenome(spool.longestLivedCandidate);
            //    break;
            //case SelectionGroup.MostEaten:
                //isMostEatenGenomeSelected = true;
                //selectedButton = buttonMostEatenGenome;
                //uiManager.SetFocusedCandidateGenome(spool.mostEatenCandidate);
            //    break;
            //case SelectionGroup.HallOfFame:
                //isHallOfFameSelected = true;
                
                // WPP: variable set by never used
                //selectedHallOfFameIndex = index;
                //if(selectedHallOfFameIndex >= pool.hallOfFameGenomesList.Count) {
                //    selectedHallOfFameIndex = 0;
                //}
                
                //selectedButton = hallOfFameButtonsList[index].GetComponent<Button>();
                //uiManager.SetFocusedCandidateGenome(spool.hallOfFameGenomesList[index]);
                //Debug.Log("ChangeSelectedGenome: " + group + ", HallOfFame, #" + index);
                //break;
            //case SelectionGroup.Leaderboard:
                //isLeaderboardGenomesSelected = true;
                //selectedLeaderboardGenomeIndex = index;
                //SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.globalResourcesUI.selectedSpeciesIndex];
                //uiManager.SetFocusedCandidateGenome(spool.leaderboardGenomesList[index]);
                //Debug.Log("ChangeSelectedGenome: " + group + ", #" + index);
                //selectedButton = leaderboardGenomeButtonsList[index].GetComponent<Button>();
            //    break;
            //case SelectionGroup.Candidates:
                //isCandidateGenomesSelected = true;
                
                // WPP: variable set by never used
                //selectedCandidateGenomeIndex = index;
                //if(selectedCandidateGenomeIndex >= pool.candidateGenomesList.Count) {
                //    selectedCandidateGenomeIndex = 0;
                //}
                
                //cameraManager.SetTargetAgent(simulationManager.agentsArray[cameraManager.targetAgentIndex], cameraManager.targetAgentIndex);
                //uiManager.SetFocusedCandidateGenome(spool.candidateGenomesList[index]);
                //Debug.Log("ChangeSelectedGenome: " + group + ", #" + index);
                //selectedButton = candidateGenomeButtonsList[index].GetComponent<Button>();
                //break;
        //}
        #endregion
        
        // WPP 5/25: special case extracted from switch, added overload to cameraManager to cut down on arguments
        if (group == SelectionGroup.Candidates)
            cameraManager.SetTargetAgent();

        // * WPP 5/25: what is purpose of this? (also set to yellow earlier in method under same conditions)
        if(selectedButtonData != null && selectedButtonData.image != null) {
            selectedButtonData.image.color = Color.white;
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

    public void CreateSpeciesLeaderboardGenomeTexture() {
        int width = 32;
        int height = 96;
        speciesPoolGenomeTex.Resize(width, height); 
        SpeciesGenomePool pool = simulationManager.GetSelectedGenomePool();

        for(int x = 0; x < width; x++) 
        {
            for(int y = 0; y < height; y++) 
            {
                LinkGenome linkGenome = pool.GetLeaderboardLinkGenome(x, y);
                Color testColor = linkGenome == null ? CLEAR : Color.Lerp(negativeColor, positiveColor, linkGenome.normalizedWeight);
                speciesPoolGenomeTex.SetPixel(x, y, testColor);
            }
        }
        
        // Body Genome
        //int xI = curLinearIndex % speciesPoolGenomeTex.width;
        //int yI = Mathf.FloorToInt(curLinearIndex / speciesPoolGenomeTex.width);
        
        speciesPoolGenomeTex.Apply();
    }

    [SerializeField] Color negativeColor = Color.black;
    [SerializeField] Color positiveColor = Color.white;

    void Start () {
        GenerateButtonList();
    
        selectedButtonData = GetSelectionGroupData(defaultSelectionGroup);
        
        speciesPoolGenomeTex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        speciesPoolGenomeTex.filterMode = FilterMode.Point;
        speciesPoolGenomeTex.wrapMode = TextureWrapMode.Clamp;
        speciesPoolGenomeMat.SetTexture("_MainTex", speciesPoolGenomeTex);
	}
	
	SelectionGroupData GetSelectionGroupData(SelectionGroup id)
	{
	    foreach (var group in selectionGroups)
	        if (group.id == id)
	            return group;
        
        return null;
	}
	
	[Serializable]
	public class SelectionGroupData
	{
	    public SelectionGroup id;
	    public Button button;
	    public Image image;
	}
}

public enum SelectionGroup {
    Founder,
    Representative,
    LongestLived,
    MostEaten,
    HallOfFame,
    Leaderboard,
    Candidates
}