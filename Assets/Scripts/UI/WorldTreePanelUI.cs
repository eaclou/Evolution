using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldTreePanelUI : MonoBehaviour
{
    [SerializeField] GameObject panelSpeciesTree;
    public bool isShowingExtinct = false;
    public Text textSelectedSpeciesTitle;
    public Image imageSelectedSpeciesBG;
    public Text textSpeciationTree;    
    public Text textStatsBody;
    public Text textTitle;

    public GameObject anchorGO;
    public GameObject prefabSpeciesIcon;
    public GameObject prefabCreatureIcon;

    private List<SpeciesIconUI> speciesIconsList;  // keeping track of spawned buttons
    private List<CreatureIconUI> creatureIconsList;

    SimulationManager simulationManager => SimulationManager.instance;
    MasterGenomePool masterGenomePool => simulationManager.masterGenomePool;
    UIManager uiManagerRef => UIManager.instance;



    private int curPanelMode = 0;  // 0 == lineage, 1 == graph
    public int GetPanelMode() {
        return curPanelMode;
    }


    public void Awake() {
        speciesIconsList = new List<SpeciesIconUI>();
    }
    

    public void Set(bool value) {
        panelSpeciesTree.SetActive(value);        
    }

    public void UpdateUI() { //***EAC UpdateUI() AND RefreshUI() ?????? 
        textTitle.text = "mode: " + curPanelMode;
        //*** update positions of buttons, etc.
        
        for(int i = 0; i < speciesIconsList.Count; i++) {
            SpeciesIconUI icon = speciesIconsList[i];
            
            bool isSelected = false;
            if (icon.speciesID == uiManagerRef.selectedSpeciesID) {
                isSelected = true;
                icon.gameObject.transform.SetAsLastSibling();
            }
            icon.UpdateIconDisplay(256, isSelected);
        }
    }
    private void UpdateSpeciesIconsLineageMode() {        
        for (int s = 0; s < speciesIconsList.Count; s++) {            
            float yCoord = (float)s / Mathf.Max(speciesIconsList.Count - 1, 1f);  
            float xCoord = 1f;
            if (speciesIconsList[s].linkedPool.isExtinct) {
                xCoord = (float)speciesIconsList[s].linkedPool.timeStepExtinct / Mathf.Max(1f, (float)simulationManager.simAgeTimeSteps);
            }
            speciesIconsList[s].SetTargetCoords(new Vector2(xCoord, 1f - yCoord));
        }
    }
    private void UpdateSpeciesIconsGraphMode() {
        if (speciesIconsList[0].linkedPool.avgCandidateDataYearList.Count < 1) {
            UpdateSpeciesIconsDefault();
            return;
        }
        float bestScore = 0f;
        for (int s = 0; s < speciesIconsList.Count; s++) {
            SpeciesGenomePool pool = speciesIconsList[s].linkedPool;
            float valStat = (float)pool.avgCandidateDataYearList[pool.avgCandidateDataYearList.Count - 1].performanceData.totalTicksAlive;

            if(valStat > bestScore) {
                bestScore = valStat;
            }
        }

        // SORT
        for (int s = 0; s < speciesIconsList.Count; s++) {
            SpeciesGenomePool pool = speciesIconsList[s].linkedPool;
            float valStat = (float)pool.avgCandidateDataYearList[pool.avgCandidateDataYearList.Count - 1].performanceData.totalTicksAlive;

            float xCoord = 1f;
            if (pool.isExtinct) {
                xCoord = (float)pool.timeStepExtinct / Mathf.Max(1f, (float)simulationManager.simAgeTimeSteps);
            }

            if(bestScore == 0f) {
                bestScore = 1f;
            }
            speciesIconsList[s].SetTargetCoords(new Vector2(xCoord, Mathf.Clamp01(valStat / bestScore)));
            
        }
    }
    private void UpdateSpeciesIconsDefault() {
        for (int s = 0; s < speciesIconsList.Count; s++) {        // simple list, evenly spaced    
            float yCoord = (float)s / Mathf.Max(speciesIconsList.Count - 1, 1f);            
            speciesIconsList[s].SetTargetCoords(new Vector2(1f, yCoord));
        }
    }

    public void RefreshPanelUI() {
        UpdateSpeciesIconsTargetCoords();
        textSelectedSpeciesTitle.text = "Selected Species: #" + uiManagerRef.selectedSpeciesID;

        Vector3 hue = simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID].foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
        imageSelectedSpeciesBG.color = new Color(hue.x, hue.y, hue.z);
    }
    public void UpdateSpeciesIconsTargetCoords() {

        if(curPanelMode == 0) {
            UpdateSpeciesIconsLineageMode();
        }
        else {
            UpdateSpeciesIconsGraphMode();
        }


        
    }
    private void CreateSpeciesIcon(SpeciesGenomePool pool) {
        
            AgentGenome templateGenome = masterGenomePool.completeSpeciesPoolsList[pool.speciesID].leaderboardGenomesList[0].candidateGenome; //.bodyGenome.coreGenome.name;
            Color color = new Color(templateGenome.bodyGenome.appearanceGenome.huePrimary.x, templateGenome.bodyGenome.appearanceGenome.huePrimary.y, templateGenome.bodyGenome.appearanceGenome.huePrimary.z);

            GameObject obj = Instantiate(prefabSpeciesIcon, new Vector3(0f, 0f, 0f), Quaternion.identity);
            obj.transform.SetParent(anchorGO.transform, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            obj.GetComponent<Image>().color = color;
          
            string labelText = "";
            labelText += "[" + pool.speciesID.ToString() + "]";// " + masterGenomePool.completeSpeciesPoolsList[pool.speciesID].foundingCandidate.candidateGenome.bodyGenome.coreGenome.name;

            obj.GetComponentInChildren<Text>().text = labelText;
            SpeciesIconUI iconScript = obj.GetComponent<SpeciesIconUI>();
            speciesIconsList.Add(iconScript);

            iconScript.Initialize(speciesIconsList.Count - 1, masterGenomePool.completeSpeciesPoolsList[pool.speciesID]);
            
    }
    public void InitializeSpeciesIcons() {
        Debug.Log("InitializeSpeciesListBarsInitializeSpeciesListBarsInitializeSpeciesListBars");
        int numSpecies = masterGenomePool.completeSpeciesPoolsList.Count;

        foreach (Transform child in anchorGO.transform) { // clear all GO's
            Destroy(child.gameObject);
        }

        for (int s = 0; s < numSpecies; s++) {
            int speciesID = s;
            int parentSpeciesID = masterGenomePool.completeSpeciesPoolsList[speciesID].parentSpeciesID;

            CreateSpeciesIcon(masterGenomePool.completeSpeciesPoolsList[speciesID]);
            
        }
    }

    public void AddNewSpeciesToPanel(SpeciesGenomePool pool) {
        Debug.Log("AddNewSpeciesToPanelUI: " + pool.speciesID);

        CreateSpeciesIcon(pool);
    }
    
    public void ClickButtonToggleExtinct() {
        isShowingExtinct = !isShowingExtinct;
        if(isShowingExtinct) { // was extinct, switch to current:
            if(masterGenomePool.currentlyActiveSpeciesIDList.Count < masterGenomePool.completeSpeciesPoolsList.Count) {
                int defaultSpeciesID = 0;
                for(int i = 0; i < masterGenomePool.completeSpeciesPoolsList.Count; i++) {
                    if(masterGenomePool.completeSpeciesPoolsList[i].isExtinct) {
                        defaultSpeciesID = i;
                        break;
                    }
                }
                //SetSelectedSpeciesUI(defaultSpeciesID);
            }
        }
        else {
            int defaultSpeciesID = masterGenomePool.currentlyActiveSpeciesIDList[0];
            //SetSelectedSpeciesUI(defaultSpeciesID);   
        }  
    }

    public void ClickButtonGraphModeToggle() {
        if(curPanelMode == 0) {
            curPanelMode = 1;
        }
        else {
            curPanelMode = 0;
        }
        Debug.Log("WorldTreeUI Panel MODE: " + curPanelMode);

        UpdateSpeciesIconsTargetCoords();
    }
}
