using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldTreePanelUI : MonoBehaviour
{
    [SerializeField] GameObject panelSpeciesTree;
    public bool isShowingExtinct = false;
    //public Text textSelectedSpeciesTitle;
    public Image imageSelectedSpeciesBG;
    public Text textSpeciationTree;    
    //public Text textStatsBody;
    public Text textTitle;
    public Image imageClockPlanet;
    public Image imageClockMoon;
    public Image imageClockSun;

    [SerializeField]
    Material clockPlanetMatA;
    [SerializeField]
    Material clockMoonMatA;
    [SerializeField]
    Material clockSunMatA;

    public GameObject anchorGO;
    public GameObject prefabSpeciesIcon;
    public GameObject prefabCreatureIcon;

    private List<SpeciesIconUI> speciesIconsList;  // keeping track of spawned buttons
    private List<CreatureIconUI> creatureIconsList;

    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    SimulationManager simulationManager => SimulationManager.instance;
    MasterGenomePool masterGenomePool => simulationManager.masterGenomePool;
    UIManager uiManagerRef => UIManager.instance;

    private int focusLevel = 0;  // ***TEMP!!!   0==species, 1==creatures, 2==selectedCreature

    public float timelineStartTimeStep = 0f;

    private int curPanelMode = 0;  // 0 == lineage, 1 == graph
    public int GetPanelMode() {
        return curPanelMode;
    }

    private float marginLeft = 0.1f;
    private float marginRight = 0.1f;
    private float marginBottom = 0.1f;
    private float graphHeight = 0.3f;
    private float orbitsHeight = 0.2f;

    public void Awake() {
        speciesIconsList = new List<SpeciesIconUI>();
    }
    
    public void Set(bool value) {
        panelSpeciesTree.SetActive(value);        
    }

    public void SetFocusLevel(int focusLvl) {
        focusLevel = focusLvl;
    }
    public int GetFocusLevel() {
        return focusLevel;
    }
    public void ToggleFocusLevel() {
        if(focusLevel == 0) {
            focusLevel = 1;
        }
        else if(focusLevel == 1) {
            focusLevel = 0;
        }
        else {
            focusLevel = 0;
        }        
    }

    public void UpdateUI() { //***EAC UpdateUI() AND RefreshUI() ?????? 
        textTitle.text = "mode: " + curPanelMode + ", focus: " + focusLevel + ", " + timelineStartTimeStep.ToString("F0");
        //*** update positions of buttons, etc.
        //**** TEMP!!! TESTING!!!

        float cursorCoordsX = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().x) / 360f);
        float cursorCoordsY = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().y - 720f) / 360f);                
        
        //**** PLANET!!!!!!
        if(imageClockPlanet) {            
            imageClockPlanet.rectTransform.localPosition = new Vector3(Mathf.Min(360f, theCursorCzar.GetCursorPixelCoords().x), 180f, 0f);
            //imageClockPlanet.rectTransform.localEulerAngles = new Vector3(0f, 0f, simulationManager.simAgeTimeSteps * 0.043f);
            float curFrame = ((simulationManager.simAgeTimeSteps * cursorCoordsX) / 2048f * 16f);
            clockPlanetMatA.SetFloat("_CurFrame", curFrame);
            clockPlanetMatA.SetFloat("_NumRows", 4f);
            clockPlanetMatA.SetFloat("_NumColumns", 4f);
        }
        // MOON:
        if(imageClockMoon) {
            Vector2 moonDir = uiManagerRef.clockPanelUI.GetMoonDir();
            imageClockMoon.rectTransform.localPosition = new Vector3(Mathf.Min(360f, theCursorCzar.GetCursorPixelCoords().x) + moonDir.x * 30f, 180f + moonDir.y * 30f, 0f);
            //imageClockPlanet.rectTransform.localEulerAngles = new Vector3(0f, 0f, simulationManager.simAgeTimeSteps * 0.043f);
            float curFrame = ((simulationManager.simAgeTimeSteps * cursorCoordsX) / 2048f * 16f);
            clockMoonMatA.SetFloat("_CurFrame", curFrame);
            clockMoonMatA.SetFloat("_NumRows", 4f);
            clockMoonMatA.SetFloat("_NumColumns", 4f);
        }
        // SUN:
        if(imageClockSun) {
            Vector2 sunDir = uiManagerRef.clockPanelUI.GetSunDir();
            imageClockSun.rectTransform.localPosition = new Vector3(Mathf.Min(360f, theCursorCzar.GetCursorPixelCoords().x) + sunDir.x * 60f, 180f + sunDir.y * 60f, 0f);
            //imageClockPlanet.rectTransform.localEulerAngles = new Vector3(0f, 0f, simulationManager.simAgeTimeSteps * 0.043f);
            float curFrame = ((simulationManager.simAgeTimeSteps * cursorCoordsX) / 2048f * 16f);
            clockSunMatA.SetFloat("_CurFrame", curFrame);
            clockSunMatA.SetFloat("_NumRows", 4f);
            clockSunMatA.SetFloat("_NumColumns", 4f);
        }
        

        for(int i = 0; i < speciesIconsList.Count; i++) {
            SpeciesIconUI icon = speciesIconsList[i];
            
            bool isSelected = false;
            if (icon.speciesID == uiManagerRef.selectedSpeciesID) {
                isSelected = true;
                icon.gameObject.transform.SetAsLastSibling();
            }
            icon.UpdateIconDisplay(360, isSelected);
        }

        float targetStartTimeStep = 0f;
        if(focusLevel == 0) {

        }
        else {
            if(simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID].candidateGenomesList.Count > 0) {
                targetStartTimeStep = simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID].candidateGenomesList[0].performanceData.timeStepHatched; //***EAC better less naive way to calculate this
            
            }
            
        }
        timelineStartTimeStep = Mathf.Lerp(timelineStartTimeStep, targetStartTimeStep, 0.125f);
    }
    
    private void UpdateSpeciesIconsLineageMode() {        
        for (int s = 0; s < speciesIconsList.Count; s++) {            
            float yCoord = 1f - (float)s / Mathf.Max(speciesIconsList.Count - 1, 1f);  
            float xCoord = 1f;
            if (speciesIconsList[s].linkedPool.isExtinct) {
                xCoord = (float)speciesIconsList[s].linkedPool.timeStepExtinct / Mathf.Max(1f, (float)simulationManager.simAgeTimeSteps);
            }

            float indent = 0.05f;
            if(focusLevel == 0) {

            }
            else {
                xCoord = 0f;
                if(speciesIconsList[s].linkedPool.speciesID == uiManagerRef.selectedSpeciesID) {
                    indent = 0.1f;
                }
            }

            
            
            xCoord = xCoord * 0.8f + indent;
            yCoord = yCoord * 0.67f + 0.1f;
            speciesIconsList[s].SetTargetCoords(new Vector2(xCoord, yCoord));
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
            if(focusLevel == 0) {

            }
            else {
                xCoord = 0f;
            }
            if(bestScore == 0f) {
                bestScore = 1f;
            }
            float yCoord = Mathf.Clamp01(valStat / bestScore);
            xCoord = xCoord * 0.8f + 0.1f;
            yCoord = yCoord * 0.67f + 0.1f;
            speciesIconsList[s].SetTargetCoords(new Vector2(xCoord, yCoord));
            
        }
    }
    private void UpdateSpeciesIconsDefault() {
        for (int s = 0; s < speciesIconsList.Count; s++) {        // simple list, evenly spaced    
            float xCoord = 1f;
            if(focusLevel == 0) {

            }
            else {
                xCoord = 0f;
            }
            float yCoord = (float)s / Mathf.Max(speciesIconsList.Count - 1, 1f);      
            xCoord = xCoord * 0.8f + 0.1f;
            yCoord = yCoord * 0.67f + 0.1f;
            speciesIconsList[s].SetTargetCoords(new Vector2(xCoord, yCoord));
        }
    }

    public void RefreshPanelUI() {
        UpdateSpeciesIconsTargetCoords();
        //textSelectedSpeciesTitle.text = "Selected Species: #" + uiManagerRef.selectedSpeciesID;

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
            if(child.GetComponent<SpeciesIconUI>()) {
                Destroy(child.gameObject);
            }
            else {

            }
            
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
