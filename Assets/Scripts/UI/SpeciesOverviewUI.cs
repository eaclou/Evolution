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
    SelectionManager selectionManager => SelectionManager.instance;

    Color CLEAR => Color.black;

    public GameObject panelGenomeViewer;
    public Image genomeLeaderboard;
    
    [SerializeField] int maxButtons = 32;
    [SerializeField] SelectionGroup defaultSelectionGroup = SelectionGroup.Founder;
    [SerializeField] SelectionGroupData[] selectionGroups;
    SelectionGroupData selectedButtonData;
    
    [SerializeField] [Range(0, 1)] float hueMin = 0.2f;
    
    public Text textSpeciesLineage;
    
    public bool isShowingLineage = false;
    
    //private Texture2D speciesPoolGenomeTex; // speciesOverviewPanel
    
    public List<GenomeButton> candidateGenomeButtons = new List<GenomeButton>();

    public void ClickButtonToggleLineage() {
        isShowingLineage = !isShowingLineage;
        RefreshGenomeButtons();
    }
    
    public void SetButtonPosition(int id, Vector3 position) { // set by historyPanelUI atm
        if (id < 0 || id >= candidateGenomeButtons.Count) return;
        candidateGenomeButtons[id].gameObject.transform.localPosition = position;
    }
        
    public void RefreshGenomeButtons() // ***EAC how is this diff from Update??
    {
        SpeciesGenomePool pool = simulationManager.masterGenomePool.completeSpeciesPoolsList[selectionManager.currentSelection.candidate.speciesID]; 
        RefreshGenomeButtonsCurrent(pool);
    }

    public void RequestNewCandidateGenomeButton(CandidateAgentData cand) {
        
        if(candidateGenomeButtons.Count >= maxButtons) {
            
            int index = 0;
            //find oldest non-living candidateButton
            for(int i = 0; i < candidateGenomeButtons.Count; i++) {
                //check if still alive:
                bool safeToRemove = true;
                if(candidateGenomeButtons[i].candidateRef.isBeingEvaluated) {
                    safeToRemove = false;
                    if(candidateGenomeButtons[i].candidateRef.candidateID == SelectionManager.instance.currentSelection.candidate.candidateID) {
                        safeToRemove = false;
                    }
                    else {
                        
                    }
                    
                }
                
                if(!safeToRemove) {
                    continue;
                }
                //else:
                index = i;
                break;
            }
            GenomeButton candGenomeButton = candidateGenomeButtons[index]; // save ref to button
            candidateGenomeButtons.RemoveAt(index);
            //delete oldest non-recordholding creature
            candGenomeButton.SetCandidate(cand);
            candidateGenomeButtons.Add(candGenomeButton); // move to end of list -- temp
        }
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
            candidateGenomeButtons.Add(buttonObj.GetComponent<GenomeButton>());
            buttonObj.gameObject.SetActive(false);
        }
        Debug.Log("GenerateButtonList Genome Buttons!");
    }

    // Current Genepool
    private void RefreshGenomeButtonsCurrent(SpeciesGenomePool pool) {
        Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;            
        genomeLeaderboard.color = new Color(hue.x, hue.y, hue.z);
        UpdateButtons();
    }
    
    private void UpdateButtons() { //List<CandidateAgentData> candidates, SelectionGroup groupId
        for(int i = 0; i < candidateGenomeButtons.Count; i++) {
            
            var active = false;
            
            if(candidateGenomeButtons[i].candidateRef == null) {
                
            }
            else {
                if (uiManager.historyPanelUI.isPopulationMode) {
                    //active = false;
                    if (candidateGenomeButtons[i].candidateRef.speciesID == SelectionManager.instance.currentSelection.candidate.speciesID) {
                        active = true;
                    }
                    else {
                        active = false;
                    }
                }
                if(candidateGenomeButtons[i].candidateRef.candidateID == SelectionManager.instance.currentSelection.candidate.candidateID) {
                    active = true;
                }
            }
            
            candidateGenomeButtons[i].gameObject.SetActive(active);
            //candidateButtons[i].UpdateButtonPrefab(groupId, i);
            
            //if (active)
            candidateGenomeButtons[i].SetDisplay();   
        } 
    }

    public void ChangeSelectedGenome(SelectionGroup group, int index, GenomeButton button) {
        
        selectedButtonData = GetSelectionGroupData(group);
        selectionManager.SetSelected(button.candidateRef);
        
        if (selectedButtonData != null && selectedButtonData.image != null) {
            //selectedButtonData.image.color = Color.white;
        }
        button.backgroundImage.color = Color.white;
    }
   
    [SerializeField] Color negativeColor = Color.black;
    [SerializeField] Color positiveColor = Color.white;

    void Start () {
        GenerateButtonList();    
        selectedButtonData = GetSelectionGroupData(defaultSelectionGroup);        
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