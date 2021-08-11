using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryPanelUI : MonoBehaviour
{
    public GameObject anchorGO;
    public GameObject prefabSpeciesIcon;
    public GameObject prefabCreatureIcon;

    public Button buttonToggleExtinct;
    public Button buttonSelCreatureEventsLink;

    private List<SpeciesIconUI> speciesIconsList;  // keeping track of spawned buttons
    //private List<CreatureIconUI> creatureIconsList;

    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    SimulationManager simManager => SimulationManager.instance;
    MasterGenomePool masterGenomePool => simManager.masterGenomePool;
    UIManager uiManagerRef => UIManager.instance;

    public float timelineStartTimeStep = 0f;

    [SerializeField]
    Text textPanelStateDebug;
    [SerializeField]
    private GameObject tempPanelSpeciesPop;
    [SerializeField]
    private GameObject tempPanelGraph;

    private HistoryPanelMode curPanelMode;
    
    public enum HistoryPanelMode {
        AllSpecies,
        ActiveSpecies,
        SpeciesPopulation,
        CreatureTimeline
    }
    
    public HistoryPanelMode GetCurPanelMode() {
        return curPanelMode;
    }
        
    public struct WorldTreeLineData {
        public Vector3 worldPos;
        public Vector4 color;
    }
    public ComputeBuffer worldTreeLineDataCBuffer;
    private int worldTreeNumPointsPerLine = 64;    
    private int worldTreeNumSpeciesLines = 32;
    private int worldTreeNumCreatureLines = 32;
    private int worldTreeBufferCount => worldTreeNumPointsPerLine * (worldTreeNumSpeciesLines * worldTreeNumCreatureLines);
    
    public struct TreeOfLifeEventLineData { //***EAC deprecate!
        public int timeStepActivated;
        public float eventCategory;  // minor major extreme 0, 0.5, 1.0
        public float isActive;
    }
    
    public bool isAllSpeciesMode => curPanelMode == HistoryPanelMode.AllSpecies;
    public bool isActiveSpeciesMode => curPanelMode == HistoryPanelMode.ActiveSpecies;

    // How to sync rendered geo with UI buttons???
    
    public void Awake() {
        speciesIconsList = new List<SpeciesIconUI>();
    }
    public void InitializeRenderBuffers() {
        //**** TEMP!!! TESTING!!!
        float cursorCoordsX = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().x) / 360f);
        float cursorCoordsY = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().y - 720f) / 360f);                
        //**** !!!!!!

        // WORLD TREE LINES SPECIES CREATURES
        WorldTreeLineData[] worldTreeLineDataArray = new WorldTreeLineData[worldTreeBufferCount];
        worldTreeLineDataCBuffer?.Release();
        worldTreeLineDataCBuffer = new ComputeBuffer(worldTreeBufferCount, sizeof(float) * 7);
        
        // SPECIES LINES:
        for(int line = 0; line < worldTreeNumSpeciesLines; line++) 
        {
            for (int i = 0; i < worldTreeNumPointsPerLine; i++) 
            {
                int index = line * worldTreeNumPointsPerLine + i;

                if (line >= simManager.masterGenomePool.completeSpeciesPoolsList.Count) 
                    continue;
                
                SpeciesGenomePool pool = simManager.masterGenomePool.completeSpeciesPoolsList[line];
                WorldTreeLineData data = new WorldTreeLineData();

                if (isAllSpeciesMode)
                {
                    // LINEAGE:
                    float xCoord = (float)i / (float)worldTreeNumPointsPerLine;
                    float yCoord = 1f - ((float)pool.speciesID / (float)Mathf.Max(simManager.masterGenomePool.completeSpeciesPoolsList.Count - 1, 1)); // Mathf.Sin(xCoord / orbitalPeriod * (simManager.simAgeTimeSteps) * animTimeScale) * 0.075f * (float)lineID + 0.5f;
                    float zCoord = 0f;
                                        
                    Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;

                    int timeStepStart = Mathf.RoundToInt(timelineStartTimeStep);

                    float xStart01 = (float)(pool.timeStepCreated - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
                    float xEnd01 = 1f;
                    if(pool.isExtinct) {
                        xEnd01 = (float)(pool.timeStepExtinct - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
                    }

                    if(xStart01 > xCoord || xEnd01 < xCoord) {
                        hue = Vector3.zero;
                    }
                    if(pool.speciesID == uiManagerRef.selectedSpeciesID) {
                        hue = Vector3.one;
                        zCoord = -0.1f;
                    }                        
                    data.color = new Color(hue.x, hue.y, hue.z); // Color.HSVToRGB(lerp, 1f - lerp, 1f); // Color.Lerp(Color.white, Color.black, lineID * 0.11215f);

                    xCoord = xCoord * 0.8f + 0.1f;  // rescaling --> make this more robust
                    yCoord = yCoord * 0.67f + 0.1f;
                    if((new Vector2(xCoord, yCoord) - new Vector2(cursorCoordsX, cursorCoordsY)).magnitude < 0.05f) {
                        data.color = Color.white;
                    }
                    data.worldPos = new Vector3(xCoord, yCoord, zCoord);
                }
                if (isActiveSpeciesMode)  
                {
                    int graphDataYearIndexStart = 0;
                    int graphDataYearIndexEnd = 0;
                    
                    if (pool.avgCandidateDataYearList.Count == 0) 
                        continue;
                    
                    float xCoord = (float)(i % worldTreeNumPointsPerLine) / (float)worldTreeNumPointsPerLine;
                    int count = Mathf.Max(0, pool.avgCandidateDataYearList.Count - 1);
                    graphDataYearIndexStart = Mathf.FloorToInt((float)count * xCoord);
                    graphDataYearIndexEnd = Mathf.CeilToInt((float)count * xCoord);
                    float frac = ((float)pool.avgCandidateDataYearList.Count * xCoord) % 1f;
                    float valStart = (float)pool.avgCandidateDataYearList[graphDataYearIndexStart].performanceData.totalTicksAlive / uiManagerRef.speciesGraphPanelUI.maxValuesStatArray[0];
                    float valEnd = (float)pool.avgCandidateDataYearList[graphDataYearIndexEnd].performanceData.totalTicksAlive / uiManagerRef.speciesGraphPanelUI.maxValuesStatArray[0];
                    valEnd = Mathf.Clamp(valEnd, 0, pool.avgCandidateDataYearList.Count - 1);
                    float yCoord = Mathf.Lerp(valStart, valEnd, frac); // Mathf.Sin(xCoord / orbitalPeriod * (simManager.simAgeTimeSteps) * animTimeScale) * 0.075f * (float)lineID + 0.5f;
                    float zCoord = 0f;
                    
                    Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
                    int timeStepStart = Mathf.RoundToInt(timelineStartTimeStep);

                    float xStart01 = (float)(pool.timeStepCreated - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
                    float xEnd01 = 1f;
                    if(pool.isExtinct) {
                        xEnd01 = (float)(pool.timeStepExtinct - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
                    }

                    if(pool.speciesID == uiManagerRef.selectedSpeciesID) {
                        hue = Vector3.one;
                        zCoord = -0.1f;
                    }
                    
                    if(xStart01 > xCoord || xEnd01 < xCoord) {
                        hue = Vector3.zero;
                    }

                    xCoord = xCoord * 0.8f + 0.1f;  // rescaling --> make this more robust
                    yCoord = yCoord * 0.67f + 0.1f;

                    data.worldPos = new Vector3(xCoord, yCoord, zCoord);
                    data.color = new Color(hue.x, hue.y, hue.z);// Color.HSVToRGB(lerp, 1f - lerp, 1f); // Color.Lerp(Color.white, Color.black, lineID * 0.11215f);

                    if((new Vector2(xCoord, yCoord) - new Vector2(cursorCoordsX, cursorCoordsY)).magnitude < 0.05f) {
                        data.color = Color.white;
                    }
                }

                if (curPanelMode == HistoryPanelMode.SpeciesPopulation || curPanelMode == HistoryPanelMode.CreatureTimeline) {
                    data.worldPos = Vector3.zero;
                    data.color = new Color(0f, 0f, 0f, 0f);
                }
                
                worldTreeLineDataArray[index] = data;
            }
        }
        
        // CREATURE LINES:::
        for(int line = 0; line < worldTreeNumCreatureLines; line++) 
        {
            for (int i = 0; i < worldTreeNumPointsPerLine; i++) 
            {
                int index = (line + worldTreeNumSpeciesLines) * worldTreeNumPointsPerLine + i;
                SpeciesGenomePool pool = simManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID];

                if (line >= pool.candidateGenomesList.Count) 
                    continue;
                
                WorldTreeLineData data = new WorldTreeLineData();
                CandidateAgentData cand = pool.candidateGenomesList[line];

                float xCoord = (float)(i) / (float)worldTreeNumPointsPerLine;
                int numAgentsDisplayed = 0;
                for(int a = 0; a < pool.candidateGenomesList.Count; a++) {
                    if(pool.candidateGenomesList[a].isBeingEvaluated) {
                        numAgentsDisplayed++;
                    }
                }
                numAgentsDisplayed = Mathf.Max(numAgentsDisplayed, 1); // avoid divide by 0
                float yCoord = 1f - (float)line / (float)numAgentsDisplayed;

                Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary * 1.5f;
                Color col = new Color(hue.x, hue.y, hue.z);

                int timeStepStart = Mathf.RoundToInt(timelineStartTimeStep);
                float xStart = (float)(pool.candidateGenomesList[line].performanceData.timeStepHatched - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
                float xEnd = 1f;
                if(pool.isExtinct || cand.performanceData.timeStepDied > 1) {
                    xEnd = (float)(cand.performanceData.timeStepDied - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
                }
                
                if(xStart > xCoord || xEnd < xCoord) {
                    //hue = Vector3.zero;
                    col.a = 0f;
                }
                else if(cand.candidateID == uiManagerRef.focusedCandidate.candidateID) {
                    //hue = Vector3.one;
                    col.r = 1f;
                    col.g = 1f;
                    col.b = 1f;
                    col.a = 1f;
                }
                if(!cand.isBeingEvaluated && cand.numCompletedEvaluations == 0) {
                    //hue = Vector3.zero;
                    col.a = 0f;
                }   
                if(cand.performanceData.timeStepHatched <= 1) {
                    //hue = Vector3.zero;
                    col.a = 0f;
                }
                if(cand.performanceData.totalTicksAlive >= 1) {
                    col.r *= 0.35f;     
                    col.g *= 0.35f; 
                    col.b *= 0.35f; 
                }

                data.color = col; // new Color(hue.x, hue.y, hue.z);// Color.HSVToRGB(lerp, 1f - lerp, 1f); // Color.Lerp(Color.white, Color.black, lineID * 0.11215f);
                xCoord = xCoord * 0.8f + 0.1f;  // rescaling --> make this more robust
                yCoord = yCoord * 0.67f + 0.1f;
                                
                data.worldPos = new Vector3(xCoord, yCoord, 0f);                     
                if((new Vector2(xCoord, yCoord) - new Vector2(cursorCoordsX, cursorCoordsY)).magnitude < 0.05f) { // Mouse hover highlight
                    data.color = Color.white;
                }

                if (curPanelMode == HistoryPanelMode.AllSpecies || curPanelMode == HistoryPanelMode.ActiveSpecies) {
                    data.worldPos = Vector3.zero;
                    data.color = new Color(0f, 0f, 0f, 0f);
                }
                worldTreeLineDataArray[index] = data;
            }
        }
        
        worldTreeLineDataCBuffer.SetData(worldTreeLineDataArray);
    }

    public void ClickSpeciesIcon(SpeciesIconUI iconUI) {

        if(iconUI.linkedPool.speciesID == uiManagerRef.selectedSpeciesID) {
            Debug.Log("ClickSpeciesIcon(SpeciesIconUI iconUI) " + curPanelMode);
            if (curPanelMode == HistoryPanelMode.SpeciesPopulation) {
                // Acting as a "BACK" button!
                curPanelMode = HistoryPanelMode.AllSpecies;
            }
            else if(curPanelMode == HistoryPanelMode.ActiveSpecies || curPanelMode == HistoryPanelMode.AllSpecies) {
                // Zoom into sel species pop
                curPanelMode = HistoryPanelMode.SpeciesPopulation;
            }
        }
        else {
            uiManagerRef.SetSelectedSpeciesUI(iconUI.linkedPool.speciesID);  
        }
        

        
    }
    public void ClickButtonToggleExtinct() {
        if(curPanelMode == HistoryPanelMode.AllSpecies) {
            curPanelMode = HistoryPanelMode.ActiveSpecies;
        }
        else if(curPanelMode == HistoryPanelMode.ActiveSpecies) {
            curPanelMode = HistoryPanelMode.AllSpecies;
        }
    }
    public void ClickButtonModeCycle() {
        curPanelMode++;
        if((int)curPanelMode >= 4) {
            curPanelMode = 0;
        }
    }
    public void Tick() {
        textPanelStateDebug.text = "MODE: " + curPanelMode;
        float targetStartTimeStep = 0f;
        tempPanelSpeciesPop.SetActive(false);
        tempPanelGraph.SetActive(false);

        if(curPanelMode == HistoryPanelMode.AllSpecies) {
            //UpdateSpeciesIconsDefault();
            UpdateSpeciesIconsLineageMode();
            buttonToggleExtinct.gameObject.SetActive(true);
        }
        else if(curPanelMode == HistoryPanelMode.ActiveSpecies) {
            UpdateSpeciesIconsGraphMode();
            targetStartTimeStep = simManager.masterGenomePool.completeSpeciesPoolsList[simManager.masterGenomePool.currentlyActiveSpeciesIDList[0]].timeStepCreated;
            buttonToggleExtinct.gameObject.SetActive(true);
            tempPanelGraph.SetActive(true);
            //Set(bool value)
        }
        else if(curPanelMode == HistoryPanelMode.SpeciesPopulation) {
            tempPanelSpeciesPop.SetActive(true);
            UpdateSpeciesIconsSinglePop();
            targetStartTimeStep = simManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID].candidateGenomesList[0].performanceData.timeStepHatched; //***EAC better less naive way to calculate this
            buttonToggleExtinct.gameObject.SetActive(false);
        }
        else if(curPanelMode == HistoryPanelMode.CreatureTimeline) {
            UpdateSpeciesIconsCreatureEvents();
            targetStartTimeStep = simManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID].candidateGenomesList[0].performanceData.timeStepHatched;
            buttonToggleExtinct.gameObject.SetActive(false);
        }

        foreach (var icon in speciesIconsList) {
            bool isSelected = false;
            if (icon.speciesID == uiManagerRef.selectedSpeciesID) {
                isSelected = true;
                icon.gameObject.transform.SetAsLastSibling();
            }
            icon.UpdateSpeciesIconDisplay(360, isSelected);
        }
        
        timelineStartTimeStep = Mathf.Lerp(timelineStartTimeStep, targetStartTimeStep, 0.15f);
    }

    private void CreateSpeciesIcon(SpeciesGenomePool pool) 
    {
        AgentGenome templateGenome = masterGenomePool.completeSpeciesPoolsList[pool.speciesID].leaderboardGenomesList[0].candidateGenome; //.bodyGenome.coreGenome.name;
        Color color = new Color(templateGenome.bodyGenome.appearanceGenome.huePrimary.x, templateGenome.bodyGenome.appearanceGenome.huePrimary.y, templateGenome.bodyGenome.appearanceGenome.huePrimary.z);

        GameObject obj = Instantiate(prefabSpeciesIcon, new Vector3(0f, 0f, 0f), Quaternion.identity);
        obj.transform.SetParent(anchorGO.transform, false);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = new Vector3(1f, 1f, 1f);
        obj.GetComponent<Image>().color = color;
          
        string labelText = "";
        labelText += "[" + pool.speciesID + "]";// " + masterGenomePool.completeSpeciesPoolsList[pool.speciesID].foundingCandidate.candidateGenome.bodyGenome.coreGenome.name;

        obj.GetComponentInChildren<Text>().text = labelText;
        SpeciesIconUI iconScript = obj.GetComponent<SpeciesIconUI>();
        speciesIconsList.Add(iconScript);

        iconScript.Initialize(speciesIconsList.Count - 1, masterGenomePool.completeSpeciesPoolsList[pool.speciesID]);
 
    }
    public void InitializeSpeciesIcons() {
        //Debug.Log("InitializeSpeciesListBars");
        int numSpecies = masterGenomePool.completeSpeciesPoolsList.Count;

        foreach (Transform child in anchorGO.transform) { // clear all GO's
            if(child.GetComponent<SpeciesIconUI>()) {
                Destroy(child.gameObject);
            }
        }

        for (int s = 0; s < numSpecies; s++) {
            int speciesID = s;
            int parentSpeciesID = masterGenomePool.completeSpeciesPoolsList[speciesID].parentSpeciesID;

            CreateSpeciesIcon(masterGenomePool.completeSpeciesPoolsList[speciesID]);
        }
    }

    public void AddNewSpeciesToPanel(SpeciesGenomePool pool) {
        //Debug.Log("AddNewSpeciesToPanelUI: " + pool.speciesID);
        CreateSpeciesIcon(pool);
    }
    
    private void UpdateSpeciesIconsLineageMode() {        
        for (int s = 0; s < speciesIconsList.Count; s++) {            
            float yCoord = 1f - (float)s / Mathf.Max(speciesIconsList.Count - 1, 1f);  
            float xCoord = 1f;
            if (speciesIconsList[s].linkedPool.isExtinct) {
                xCoord = (float)speciesIconsList[s].linkedPool.timeStepExtinct / Mathf.Max(1f, (float)simManager.simAgeTimeSteps);
            }

            //float indent = 0.05f;           

            xCoord = xCoord * 0.8f + 0.1f;
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
                xCoord = (float)pool.timeStepExtinct / Mathf.Max(1f, (float)simManager.simAgeTimeSteps);
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
            
            float yCoord = (float)s / Mathf.Max(speciesIconsList.Count - 1, 1f);      
            xCoord = xCoord * 0.8f + 0.1f;
            yCoord = yCoord * 0.67f + 0.1f;
            speciesIconsList[s].SetTargetCoords(new Vector2(xCoord, yCoord));
        }
    }
    private void UpdateSpeciesIconsSinglePop() {
        for (int s = 0; s < speciesIconsList.Count; s++) {        // simple list, evenly spaced    
            float xCoord = -0.2f;
            float yCoord = 0.2f;// (float)s / Mathf.Max(speciesIconsList.Count - 1, 1f);   // DEFAULTS

            int prevSpeciesIndex = uiManagerRef.selectedSpeciesID - 1;
            if (prevSpeciesIndex < 0) prevSpeciesIndex = masterGenomePool.completeSpeciesPoolsList.Count - 1;
            int nextSpeciesIndex = uiManagerRef.selectedSpeciesID + 1;
            if (nextSpeciesIndex >= masterGenomePool.completeSpeciesPoolsList.Count) nextSpeciesIndex = 0;

            if (speciesIconsList[s].linkedPool.speciesID == prevSpeciesIndex) {  // CYCLE PREV SPECIES
                xCoord = 0f;
                yCoord = 1f;
            }
            if (speciesIconsList[s].linkedPool.speciesID == uiManagerRef.selectedSpeciesID) {   // SELECTED
                xCoord = 0f;
                yCoord = 0.5f;
            }
            if (speciesIconsList[s].linkedPool.speciesID == nextSpeciesIndex) {   // CYCLE NEXT SPECIES
                xCoord = 0f;
                yCoord = 0f;
            }
            
            xCoord = xCoord * 0.8f + 0.1f;
            yCoord = yCoord * 0.67f + 0.1f;

            speciesIconsList[s].SetTargetCoords(new Vector2(xCoord, yCoord));
        }

        
        SpeciesGenomePool pool = simManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID];
        int numAgentsDisplayed = 0;
        for(int a = 0; a < pool.candidateGenomesList.Count; a++) {
            if(pool.candidateGenomesList[a].isBeingEvaluated) {
                numAgentsDisplayed++;
            }
        }
        numAgentsDisplayed = Mathf.Max(numAgentsDisplayed, 1); // avoid divide by 0
        
        for(int line = 0; line < worldTreeNumCreatureLines; line++) 
        {            
            
            if (line >= pool.candidateGenomesList.Count) 
                continue;
                            
            CandidateAgentData cand = pool.candidateGenomesList[line];

            float xCoord = 1f;                
            float yCoord = 1f - (float)line / (float)numAgentsDisplayed;

            Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary * 2f;

            int timeStepStart = Mathf.RoundToInt(timelineStartTimeStep);            
            float xEnd = 1f;
            if(pool.isExtinct || cand.performanceData.timeStepDied > 1) {
                xEnd = (float)(cand.performanceData.timeStepDied - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
            }
            
            xCoord = xCoord * 0.8f + 0.1f;  // rescaling --> make this more robust
            yCoord = yCoord * 0.67f + 0.1f;

            uiManagerRef.speciesOverviewUI.SetButtonPos(line, new Vector3(xCoord * 360f, yCoord * 360f, 0f));      
            
        }
    }

    private void UpdateSpeciesIconsCreatureEvents() {
        for (int s = 0; s < speciesIconsList.Count; s++) {        // simple list, evenly spaced    
            float xCoord = 0f;            
            float yCoord = 0f;   // DEFAULTS
            speciesIconsList[s].SetTargetCoords(new Vector2(xCoord, yCoord));
        }        
    }

    private void OnDisable() {
        worldTreeLineDataCBuffer?.Release();
    }
}
