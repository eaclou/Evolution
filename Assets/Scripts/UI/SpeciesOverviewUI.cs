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
    
    [SerializeField] [Range(0, 1)] float hueMin = 0.2f;
    
    
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
        
    public void RebuildGenomeButtons() 
    {
        SpeciesGenomePool pool = simulationManager.GetSelectedGenomePool(); 
        
        SetLineageColors(pool.appearanceGenome);

        textSpeciesLineage.gameObject.SetActive(true);
        textSpeciesLineage.text = GetLineageText(pool);

        RebuildGenomeButtonsLineage(pool);

        RebuildGenomeButtonsCurrent(pool);        
    }
    
    private void SetLineageColors(CritterModuleAppearanceGenome appearance) {
        imageLineageA.color = ColorFloor(appearance.huePrimary, hueMin);
        imageLineageB.color = ColorFloor(appearance.hueSecondary, hueMin);
    }
    
    // * Consider moving to static class if generally useful
    private Color ColorFloor(Vector3 hue, float min) {
        return new Color(Mathf.Max(min, hue.x),  Mathf.Max(min, hue.y), Mathf.Max(min, hue.z));
    }
    
    private string GetLineageText(SpeciesGenomePool pool) {
        int savedSpeciesID = pool.speciesID;
        int parentSpeciesID = pool.parentSpeciesID;
        string lineage = savedSpeciesID.ToString();
        
        for(int i = 0; i < 64; i++) 
        {
            if (parentSpeciesID < 0)
            {
                lineage += "*";
                break;
            }

            SpeciesGenomePool parentPool = simulationManager.GetGenomePoolBySpeciesID(parentSpeciesID);
            lineage += " <- " + parentPool.speciesID;

            savedSpeciesID = parentPool.speciesID;
            parentSpeciesID = parentPool.parentSpeciesID;
        }
        
        return lineage;
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
        UpdateButtons(pool.candidateGenomesList, SelectionGroup.Candidates);
    }
    
    // Hall of fame genomes (checkpoints .. every 50 years?)
    private void RebuildGenomeButtonsLineage(SpeciesGenomePool pool) {
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

    public void ChangeSelectedGenome(SelectionGroup group, int index) {
        SpeciesGenomePool pool = simulationManager.GetSelectedGenomePool();
        uiManager.SetFocusedCandidateGenome(pool, group, index);
        selectedButtonData = GetSelectionGroupData(group);

        if (group == SelectionGroup.Candidates)
            cameraManager.SetTargetAgent();

        if(selectedButtonData != null && selectedButtonData.image != null) {
            selectedButtonData.image.color = Color.white;
        }

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