﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryPanelUI : MonoBehaviour
{
    public Transform anchor;
    public GameObject prefabSpeciesIcon;
    public GameObject prefabCreatureIcon;
    public GameObject prefabCreatureEventIconUI;

    public Button buttonToggleExtinct;
    public Button buttonToggleGraphMode;
    public Button buttonSelCreatureEventsLink;
    public Button buttonBack;
    
    // keeps track of spawned buttons
    private List<SpeciesIconUI> speciesIcons = new List<SpeciesIconUI>();  
    private List<CreatureEventIconUI> creatureEventIcons = new List<CreatureEventIconUI>();
    //private List<CreatureIconUI> creatureIconsList;

    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    SimulationManager simManager => SimulationManager.instance;
    MasterGenomePool masterGenomePool => simManager.masterGenomePool;
    UIManager uiManagerRef => UIManager.instance;
    SelectionManager selectionManager => SelectionManager.instance;

    public float timelineStartTimeStep = 0f;
    
    private const int panelSizePixels = 360;
    [SerializeField]
    float marginLeft = 0.1f;
    [SerializeField]
    float marginRight = 0.1f;
    [SerializeField]
    float marginTop = 0.05f;
    [SerializeField]
    float marginMiddle = 0.025f;
    [SerializeField]
    float marginBottom = 0.1f;
    [SerializeField] 
    float clockHeight = 0.2f;
    [SerializeField]
    Text textPanelStateDebug;
    [SerializeField]
    GameObject tempPanelSpeciesPop;
    [SerializeField]
    GameObject tempPanelGraph;
    [SerializeField]
    GameObject tempPanelLifeEvents;
    [SerializeField] int maxNumCreatureEventIcons;

    private float displayWidth => 1f - marginLeft - marginRight;
    private float displayHeight => 1f - marginTop - marginMiddle - marginBottom - clockHeight;
    
    public static int GetPanelSizePixels() {
        return panelSizePixels;
    }
    
    public enum HistoryPanelMode {
        AllSpecies,
        //ActiveSpecies,
        SpeciesPopulation,
        CreatureTimeline
    }

    private HistoryPanelMode curPanelMode;

    public void SetCurPanelMode(HistoryPanelMode mode) {
        curPanelMode = mode;
    }
        
    public struct WorldTreeLineData {
        public Vector3 worldPos;
        public Vector4 color;
    }
    
    public ComputeBuffer worldTreeLineDataCBuffer;
    private int worldTreeNumPointsPerLine = 128;    
    private int worldTreeNumSpeciesLines = 32;
    private int worldTreeNumCreatureLines = 32;
    private int worldTreeBufferCount => worldTreeNumPointsPerLine * (worldTreeNumSpeciesLines * worldTreeNumCreatureLines);
    
    public struct TreeOfLifeEventLineData { //***EAC deprecate!
        public int timeStepActivated;
        public float eventCategory;  // minor major extreme 0, 0.5, 1.0
        public float isActive;
    }
    
    public bool isAllSpeciesMode => curPanelMode == HistoryPanelMode.AllSpecies;
    //public bool isActiveSpeciesMode => curPanelMode == HistoryPanelMode.ActiveSpecies;
    public bool isGraphMode;

    // How to sync rendered geo with UI buttons???

    void Start() {
        uiManagerRef.OnAgentSelected += RefreshFocusedAgent;
    }
    
    void OnDestroy() {
        if (UIManager.exists) uiManagerRef.OnAgentSelected -= RefreshFocusedAgent;
    }
    
    public void InitializePanel() {
        InitializeSpeciesIcons();
        GenerateCreatureEventIcons();
    }
    
    private void GenerateCreatureEventIcons() {
        for(int i = 0; i < maxNumCreatureEventIcons; i++) {
            CreateCreatureEventIcon();
        }
    }
    
    private void CreateCreatureEventIcon() {
        var icon = Instantiate(prefabCreatureEventIconUI).GetComponent<CreatureEventIconUI>();
        icon.OnCreate(anchor);
        creatureEventIcons.Add(icon);
    }
    
    private void UpdateCreatureEventIcons(CandidateAgentData candidate) {
        if (creatureEventIcons.Count == 0 || candidate.candidateEventDataList.Count == 0) return;
        
        ClearDeadCreatureEventIcons();
        
        for (int i = 0; i < creatureEventIcons.Count; i++) {
            if(curPanelMode == HistoryPanelMode.CreatureTimeline) {
                var active = i < candidate.candidateEventDataList.Count;
            
                creatureEventIcons[i].gameObject.SetActive(active);

                if (active) {
                    Vector2 eventCoords = Vector2.zero;
                    eventCoords.x = (float)(candidate.candidateEventDataList[i].eventFrame - candidate.performanceData.timeStepHatched) / (float)(simManager.simAgeTimeSteps - candidate.performanceData.timeStepHatched);
                    eventCoords.x = eventCoords.x * displayWidth + marginLeft;
                    eventCoords.y = (1f - ( (float)candidate.candidateEventDataList[i].type / 12f)) * displayHeight + marginBottom;
                    creatureEventIcons[i].UpdateIconPrefabData(candidate.candidateEventDataList[i], i);
                    creatureEventIcons[i].SetTargetCoords(eventCoords);
                    creatureEventIcons[i].SetDisplay();
                }
            }
            else {
                // Hide icons when not on that screen
                creatureEventIcons[i].gameObject.SetActive(false); 
            }
        } 
    }
    
    public void RefreshFocusedAgent(Agent agent) {
        RefreshFocusedAgent(selectionManager.IsFocus(agent.candidateRef));
    }
    
    public void RefreshFocusedAgent(bool focusHasChanged)
    {
        HistoryPanelMode panelMode = curPanelMode;
    
        // * WPP: comment suggests checking for matching species, but code is checking candidate ID, not species ID
        // + design goals of this if/else branch unclear
        // Same species
        if (focusHasChanged)
        {
            switch (panelMode)
            {
                case HistoryPanelMode.AllSpecies: SetCurPanelMode(HistoryPanelMode.SpeciesPopulation); break;
                case HistoryPanelMode.SpeciesPopulation: SetCurPanelMode(HistoryPanelMode.CreatureTimeline); break;
            }
        }
        // different species
        else 
        {
            switch (panelMode)
            {
                case HistoryPanelMode.AllSpecies: SetCurPanelMode(HistoryPanelMode.SpeciesPopulation); break;
                case HistoryPanelMode.SpeciesPopulation: break;
                case HistoryPanelMode.CreatureTimeline: SetCurPanelMode(HistoryPanelMode.SpeciesPopulation); break;
            }
        }
    }
    
    // * Rename to suggest this is a per-frame update, break out the initialization logic
    // * APPLICATION BOTTLENECK: focus optimization efforts here!
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
                    if(isGraphMode) {
                        int graphDataYearIndexStart = 0;
                        int graphDataYearIndexEnd = 0;
                    
                        if (pool.avgCandidateDataYearList.Count == 0) 
                            continue;
                    
                        float xCoord = (float)(i % worldTreeNumPointsPerLine) / (float)worldTreeNumPointsPerLine;
                        int count = Mathf.Max(0, pool.avgCandidateDataYearList.Count - 1);
                        graphDataYearIndexStart = Mathf.FloorToInt((float)count * xCoord);
                        graphDataYearIndexEnd = Mathf.CeilToInt((float)count * xCoord);
                        float frac = ((float)count * xCoord) % 1f;
                        float valStart = (float)pool.avgCandidateDataYearList[graphDataYearIndexStart].performanceData.totalTicksAlive / uiManagerRef.speciesGraphPanelUI.maxValuesStatArray[0];
                        float valEnd = (float)pool.avgCandidateDataYearList[graphDataYearIndexEnd].performanceData.totalTicksAlive / uiManagerRef.speciesGraphPanelUI.maxValuesStatArray[0];
                        //valEnd = Mathf.Clamp(valEnd, 0, pool.avgCandidateDataYearList.Count - 1);
                        float yCoord = Mathf.Lerp(valStart, valEnd, frac); // Mathf.Sin(xCoord / orbitalPeriod * (simManager.simAgeTimeSteps) * animTimeScale) * 0.075f * (float)lineID + 0.5f;
                        float zCoord = 0f;
                    
                        Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
                        float alpha = 1f;
                        int timeStepStart = Mathf.RoundToInt(timelineStartTimeStep);

                        float xStart01 = (float)(pool.timeStepCreated - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
                        float xEnd01 = 1f;
                        if(pool.isExtinct) {
                            xEnd01 = (float)(pool.timeStepExtinct - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
                        }

                        if(pool.speciesID == selectionManager.selectedSpeciesID) {
                            hue = Vector3.one;
                            zCoord = -0.1f;
                        }
                    
                        if(xStart01 > xCoord || xEnd01 < xCoord) {
                            hue = Vector3.zero;
                            alpha = 0f;
                        }

                        xCoord = xCoord * displayWidth + marginLeft;  // rescaling --> make this more robust
                        yCoord = yCoord * displayHeight + marginBottom;

                        if((new Vector2(xCoord, yCoord) - new Vector2(cursorCoordsX, cursorCoordsY)).magnitude < 0.05f) {
                            //data.color = Color.white;
                            hue = Vector3.Lerp(hue, Vector3.one, 0.8f);
                            //alpha = 1f;
                        }

                        data.worldPos = new Vector3(xCoord, yCoord, zCoord);
                        data.color = new Color(hue.x, hue.y, hue.z, alpha);// Color.HSVToRGB(lerp, 1f - lerp, 1f); // Color.Lerp(Color.white, Color.black, lineID * 0.11215f);
                    }
                    else {
                        // LINEAGE:
                        float xCoord = (float)i / (float)worldTreeNumPointsPerLine;
                        float yCoord = 1f - ((float)pool.speciesID / (float)Mathf.Max(simManager.masterGenomePool.completeSpeciesPoolsList.Count - 1, 1)); // Mathf.Sin(xCoord / orbitalPeriod * (simManager.simAgeTimeSteps) * animTimeScale) * 0.075f * (float)lineID + 0.5f;
                        float zCoord = 0f;
                                        
                        Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
                        float alpha = 1f;
                        int timeStepStart = Mathf.RoundToInt(timelineStartTimeStep);

                        float xStart01 = (float)(pool.timeStepCreated - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
                        float xEnd01 = 1f;
                        if(pool.isExtinct) {
                            xEnd01 = (float)(pool.timeStepExtinct - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
                        }

                        if(xStart01 > xCoord || xEnd01 < xCoord) {
                            hue = Vector3.zero;
                            alpha = 0f;
                        }
                        if(pool.speciesID == selectionManager.selectedSpeciesID) {
                            hue = Vector3.one;
                            zCoord = -0.1f;
                        }                        
                        data.color = new Color(hue.x, hue.y, hue.z, alpha); // Color.HSVToRGB(lerp, 1f - lerp, 1f); // Color.Lerp(Color.white, Color.black, lineID * 0.11215f);

                        xCoord = xCoord * displayWidth + marginLeft;  // rescaling --> make this more robust
                        yCoord = yCoord * displayHeight + marginBottom;
                        if((new Vector2(xCoord, yCoord) - new Vector2(cursorCoordsX, cursorCoordsY)).magnitude < 0.05f) {
                            data.color = Color.white;
                        }
                        data.worldPos = new Vector3(xCoord, yCoord, zCoord);
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
                SpeciesGenomePool pool = simManager.masterGenomePool.completeSpeciesPoolsList[selectionManager.selectedSpeciesID];

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
                else if(cand.candidateID == selectionManager.focusedCandidate.candidateID) {
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
                xCoord = xCoord * displayWidth + marginLeft;  // rescaling --> make this more robust
                yCoord = yCoord * displayHeight + marginBottom;
                                
                data.worldPos = new Vector3(xCoord, yCoord, 0f);                     
                if((new Vector2(xCoord, yCoord) - new Vector2(cursorCoordsX, cursorCoordsY)).magnitude < 0.05f) { // Mouse hover highlight
                    data.color = Color.white;
                }

                if (curPanelMode == HistoryPanelMode.AllSpecies || curPanelMode == HistoryPanelMode.CreatureTimeline) {
                    data.worldPos = Vector3.zero;
                    data.color = new Color(0f, 0f, 0f, 0f);
                }
                worldTreeLineDataArray[index] = data;
            }
        }
        
        worldTreeLineDataCBuffer.SetData(worldTreeLineDataArray);
    }

    private void CreateSpeciesIcon(SpeciesGenomePool pool) 
    {
        AgentGenome templateGenome = masterGenomePool.completeSpeciesPoolsList[pool.speciesID].leaderboardGenomesList[0].candidateGenome; 
        Color color = new Color(templateGenome.bodyGenome.appearanceGenome.huePrimary.x, templateGenome.bodyGenome.appearanceGenome.huePrimary.y, templateGenome.bodyGenome.appearanceGenome.huePrimary.z);

        // WPP: moved to SpeciesIconUI.Initialize
        /*GameObject obj = Instantiate(prefabSpeciesIcon, new Vector3(0f, 0f, 0f), Quaternion.identity);
        obj.transform.SetParent(anchor.transform, false);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = new Vector3(1f, 1f, 1f);
        obj.GetComponent<Image>().color = color;
          
        string labelText = "";
        labelText += "[" + pool.speciesID + "]";// " + masterGenomePool.completeSpeciesPoolsList[pool.speciesID].foundingCandidate.candidateGenome.bodyGenome.coreGenome.name;

        obj.GetComponentInChildren<Text>().text = labelText;
        SpeciesIconUI iconScript = obj.GetComponent<SpeciesIconUI>();*/
        var icon = Instantiate(prefabSpeciesIcon).GetComponent<SpeciesIconUI>();
        icon.Initialize(speciesIcons.Count - 1, masterGenomePool.completeSpeciesPoolsList[pool.speciesID], anchor, color);
        speciesIcons.Add(icon);
    }
    
    public void InitializeSpeciesIcons() {
        //Debug.Log("InitializeSpeciesListBars");
        int numSpecies = masterGenomePool.completeSpeciesPoolsList.Count;
        
        foreach (var icon in speciesIcons)
            Destroy(icon.gameObject);
            
        speciesIcons.Clear();

        for (int s = 0; s < numSpecies; s++) {
            int speciesID = s;
            int parentSpeciesID = masterGenomePool.completeSpeciesPoolsList[speciesID].parentSpeciesID;
            CreateSpeciesIcon(masterGenomePool.completeSpeciesPoolsList[speciesID]);
        }
    }
    
    public void ClickSpeciesIcon(SpeciesIconUI iconUI) {
        if(iconUI.speciesID == selectionManager.selectedSpeciesID) {
            Debug.Log("ClickSpeciesIcon(SpeciesIconUI iconUI) " + curPanelMode);
            if (curPanelMode == HistoryPanelMode.SpeciesPopulation) {
                // Acting as a "BACK" button!
                curPanelMode = HistoryPanelMode.AllSpecies;
            }
            else if(curPanelMode == HistoryPanelMode.AllSpecies) {
                // Zoom into sel species pop
                curPanelMode = HistoryPanelMode.SpeciesPopulation;
                //int indexLast = Mathf.Max(0, iconUI.linkedPool.candidateGenomesList.Count - 1);
                if (iconUI.linkedPool.candidateGenomesList.Count == 0) return;

                //uiManagerRef.selectionManager.SetFocusedCandidateGenome(iconUI.linkedPool.candidateGenomesList[0]);
                //uiManagerRef.speciesOverviewUI.buttons[0].ClickedThisButton();
            }
        }
        else {
            selectionManager.SetSelectedSpeciesUI(iconUI.speciesID);
            uiManagerRef.speciesOverviewUI.RebuildGenomeButtons();
        }
    }
    
    public void ClickButtonToggleGraphMode() {
        isGraphMode = !isGraphMode;
    }
    
    public void ClickButtonToggleExtinct() { }
    
    public void ClickButtonBack() {
        if(curPanelMode == HistoryPanelMode.CreatureTimeline) {
            curPanelMode = HistoryPanelMode.SpeciesPopulation;
        }        
    }
    
    public void ClickButtonModeCycle() {
        curPanelMode++;
        if((int)curPanelMode >= 4) {
            curPanelMode = 0;
        }
    }
    
    public void ClickedSelectedCreatureEvents() {
        curPanelMode = HistoryPanelMode.CreatureTimeline;
    }
    
    public void Tick() 
    {
        textPanelStateDebug.text = "MODE: " + selectionManager.selectedSpeciesID + selectionManager.focusedCandidate.speciesID;
        buttonToggleExtinct.gameObject.SetActive(false);
        float targetStartTimeStep = 0f;
        tempPanelSpeciesPop.SetActive(false);
        tempPanelGraph.SetActive(false);
        buttonBack.gameObject.SetActive(false);
        buttonSelCreatureEventsLink.gameObject.SetActive(false);
        tempPanelLifeEvents.gameObject.SetActive(false);
        
        if (curPanelMode == HistoryPanelMode.AllSpecies) 
        {
            buttonToggleGraphMode.gameObject.SetActive(true);
            //buttonToggleExtinct.gameObject.SetActive(true);
            if (isGraphMode) {
                UpdateSpeciesIconsGraphMode();
                targetStartTimeStep = simManager.masterGenomePool.completeSpeciesPoolsList[simManager.masterGenomePool.currentlyActiveSpeciesIDList[0]].timeStepCreated;
                tempPanelGraph.SetActive(true);
            }
            else {
                UpdateSpeciesIconsLineageMode();
            }
        }
        else if(curPanelMode == HistoryPanelMode.SpeciesPopulation) {
            tempPanelSpeciesPop.SetActive(true);
            UpdateSpeciesIconsSinglePop();
            targetStartTimeStep = simManager.masterGenomePool.completeSpeciesPoolsList[selectionManager.selectedSpeciesID].candidateGenomesList[0].performanceData.timeStepHatched; //***EAC better less naive way to calculate this
            //buttonToggleExtinct.gameObject.SetActive(false);
            buttonToggleGraphMode.gameObject.SetActive(false);
            buttonSelCreatureEventsLink.gameObject.SetActive(true);
        }
        else if(curPanelMode == HistoryPanelMode.CreatureTimeline) {
            tempPanelSpeciesPop.SetActive(false);
            UpdateSpeciesIconsCreatureEvents();
            targetStartTimeStep = selectionManager.focusedCandidate.performanceData.timeStepHatched;
            //buttonToggleExtinct.gameObject.SetActive(false);
            buttonToggleGraphMode.gameObject.SetActive(false);
            buttonBack.gameObject.SetActive(true);
            tempPanelLifeEvents.gameObject.SetActive(true);
            buttonSelCreatureEventsLink.gameObject.SetActive(false);
        }
        
        ClearDeadSpeciesIcons();

        foreach (var icon in speciesIcons) {
            bool isSelected = false;
            if (icon.speciesID == selectionManager.selectedSpeciesID) {
                isSelected = true;
                icon.transform.SetAsLastSibling();
            }
            icon.UpdateSpeciesIconDisplay(panelSizePixels, isSelected);
        }

        if(selectionManager.focusedCandidate != null) UpdateCreatureEventIcons(selectionManager.focusedCandidate);
        
        timelineStartTimeStep = Mathf.Lerp(timelineStartTimeStep, targetStartTimeStep, 0.15f);
    }

    public void AddNewSpeciesToPanel(SpeciesGenomePool pool) {
        //Debug.Log("AddNewSpeciesToPanelUI: " + pool.speciesID);
        CreateSpeciesIcon(pool);
    }
    
    private void UpdateSpeciesIconsLineageMode() {        
        for (int s = 0; s < speciesIcons.Count; s++) {            
            float yCoord = 1f - (float)s / Mathf.Max(speciesIcons.Count - 1, 1f);  
            float xCoord = 1f;
            if (speciesIcons[s].linkedPool.isExtinct) {
                xCoord = (float)speciesIcons[s].linkedPool.timeStepExtinct / Mathf.Max(1f, (float)simManager.simAgeTimeSteps);
            }

            //float indent = 0.05f;           
            xCoord = xCoord * displayWidth + marginLeft;
            yCoord = yCoord * displayHeight + marginBottom;
            speciesIcons[s].SetTargetCoords(new Vector2(xCoord, yCoord));
        }
    }
    
    private void UpdateSpeciesIconsGraphMode() {
        if (speciesIcons[0].linkedPool.avgCandidateDataYearList.Count < 1) {
            UpdateSpeciesIconsDefault();
            return;
        }
        
        float bestScore = 0f;
        foreach (var icon in speciesIcons) {
            SpeciesGenomePool pool = icon.linkedPool;
            float valStat = (float)pool.avgCandidateDataYearList[pool.avgCandidateDataYearList.Count - 1].performanceData.totalTicksAlive;

            if(valStat > bestScore) {
                bestScore = valStat;
            }
        }

        // SORT
        foreach (var icon in speciesIcons) {
            SpeciesGenomePool pool = icon.linkedPool;
            float valStat = pool.avgCandidateDataYearList[pool.avgCandidateDataYearList.Count - 1].performanceData.totalTicksAlive;

            float xCoord = 1f;
            if (pool.isExtinct) {
                xCoord = (float)pool.timeStepExtinct / Mathf.Max(1f, (float)simManager.simAgeTimeSteps);
            }
            
            if(bestScore == 0f) {
                bestScore = 1f;
            }
            
            float yCoord = Mathf.Clamp01(valStat / bestScore);
            xCoord = xCoord * displayWidth + marginLeft;
            yCoord = yCoord * displayHeight + marginBottom;
            icon.SetTargetCoords(new Vector2(xCoord, yCoord));
        }
    }
    
    /// Simple list, evenly spaced 
    private void UpdateSpeciesIconsDefault() {
        for (int s = 0; s < speciesIcons.Count; s++) {           
            float yCoord = (float)s / Mathf.Max(speciesIcons.Count - 1, 1f);
            float xCoord = displayWidth + marginLeft;
            yCoord = yCoord * displayHeight + marginBottom;
            speciesIcons[s].SetTargetCoords(new Vector2(xCoord, yCoord));
        }
    }
    
    /// Simple list, evenly spaced 
    private void UpdateSpeciesIconsSinglePop() {
        foreach (var icon in speciesIcons) {         
            // DEFAULTS
            float xCoord = -0.2f;
            float yCoord = 0.2f;// (float)s / Mathf.Max(speciesIconsList.Count - 1, 1f);   

            int prevSpeciesIndex = selectionManager.selectedSpeciesID - 1;
            if (prevSpeciesIndex < 0) prevSpeciesIndex = masterGenomePool.completeSpeciesPoolsList.Count - 1;
            int nextSpeciesIndex = selectionManager.selectedSpeciesID + 1;
            if (nextSpeciesIndex >= masterGenomePool.completeSpeciesPoolsList.Count) nextSpeciesIndex = 0;

            // CYCLE PREV SPECIES
            if (icon.linkedPool.speciesID == prevSpeciesIndex) {  
                xCoord = 0f;
                yCoord = 1f;
            }
            // SELECTED
            if (icon.linkedPool.speciesID == selectionManager.selectedSpeciesID) {   
                xCoord = 0f;
                yCoord = 0.5f;
            }
            // CYCLE NEXT SPECIES
            if (icon.linkedPool.speciesID == nextSpeciesIndex) {   
                xCoord = 0f;
                yCoord = 0f;
            }
            
            xCoord = xCoord * displayWidth + marginLeft;
            yCoord = yCoord * displayHeight + marginBottom;

            icon.SetTargetCoords(new Vector2(xCoord, yCoord));
        }
        
        //*** UPDATE CREATURE ICONS!!!!!! v v v
        SpeciesGenomePool pool = simManager.masterGenomePool.completeSpeciesPoolsList[selectionManager.selectedSpeciesID];
        int numAgentsDisplayed = 0;
        foreach (var candidate in pool.candidateGenomesList) {
            if(candidate.isBeingEvaluated) {
                numAgentsDisplayed++;
            }
        }
        numAgentsDisplayed = Mathf.Max(numAgentsDisplayed, 1); // avoid divide by 0
        
        for (int line = 0; line < worldTreeNumCreatureLines; line++) 
        {
            if (line >= pool.candidateGenomesList.Count) 
                continue;
                            
            CandidateAgentData cand = pool.candidateGenomesList[line];

            float xCoord = 1f;                
            float yCoord = 1f - (float)line / (float)numAgentsDisplayed;

            Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary * 2f;

            int timeStepStart = Mathf.RoundToInt(timelineStartTimeStep);            
            if(pool.isExtinct || cand.performanceData.timeStepDied > 1) {
                xCoord = (float)(cand.performanceData.timeStepDied - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
            }
            
            xCoord = xCoord * displayWidth + marginLeft;
            yCoord = yCoord * displayHeight + marginBottom;

            //***EAC FIX!
            uiManagerRef.speciesOverviewUI.SetButtonPos(line, new Vector3(xCoord * (float)panelSizePixels, yCoord * (float)panelSizePixels, 0f));
        }
    }
    
    /// Simple list, evenly spaced
    /// DEFAULTS
    private void UpdateSpeciesIconsCreatureEvents() {
        foreach (var icon in speciesIcons) {           
            float xCoord = 0f;            
            float yCoord = 0f;   
            icon.SetTargetCoords(new Vector2(xCoord, yCoord));
        }        
    }
    
    // WPP: prevent attempts to access destroyed objects
    void ClearDeadSpeciesIcons()
    {
        for (int i = speciesIcons.Count - 1; i >= 0; i--)
            if (speciesIcons[i].flaggedForDestruction)
                speciesIcons.Remove(speciesIcons[i]);
    }
    
    void ClearDeadCreatureEventIcons()
    {
        for (int i = creatureEventIcons.Count - 1; i >= 0; i--)
            if (creatureEventIcons[i].flaggedForDestruction)
                creatureEventIcons.Remove(creatureEventIcons[i]);
    }

    private void OnDisable() {
        worldTreeLineDataCBuffer?.Release();
    }
}
