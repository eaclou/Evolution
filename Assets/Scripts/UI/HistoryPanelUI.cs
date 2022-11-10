using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryPanelUI : MonoBehaviour
{
    public GameObject anchorGO;
    public GameObject prefabSpeciesIcon;
    public GameObject prefabCreatureEventIconUI;

    public Button buttonToggleExtinct;
    public Button buttonToggleGraphMode;
    public Button buttonSelCreatureEventsLink;
    public Button buttonBack;
    public Button buttonToggleResourceView;
    
    /// Keeps track of spawned buttons
    private List<SpeciesIconUI> speciesIcons = new List<SpeciesIconUI>();  
    private List<CreatureEventIconUI> creatureEventIcons = new List<CreatureEventIconUI>();

    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    SimulationManager simManager => SimulationManager.instance;
    MasterGenomePool masterGenomePool => simManager.masterGenomePool;
    UIManager uiManagerRef => UIManager.instance;
    SelectionManager selectionManager => SelectionManager.instance;
    SelectionData currentSelection => selectionManager.currentSelection;
    int historySelectedSpeciesID => currentSelection.historySelectedSpeciesID;

    //public float timelineStartTimeStep = 0f;
        
    public static int panelSizePixels => 360;
    
    [SerializeField]
    public float marginLeft = 0.1f;    
    [SerializeField]
    public float marginRight = 0.1f;
    [SerializeField]
    public float marginTop = 0.05f;
    [SerializeField]
    public float marginMiddle = 0.025f;
    [SerializeField]
    public float marginBottom = 0.1f;
    [SerializeField] 
    public float clockHeight = 0.2f;
    [SerializeField]
    Text textPanelStateDebug;
    [SerializeField]
    GameObject tempPanelSpeciesPop;
    [SerializeField]
    GameObject tempPanelGraph;
    [SerializeField]
    GameObject tempPanelLifeEvents;
    [SerializeField] int maxNumCreatureEventIcons;

    [SerializeField]
    GameObject panelNudgeMessage;
    [SerializeField]
    Text textNudgeMessage;
    [SerializeField]
    TooltipUI tooltipNudgeMessage;

    public float displayWidth => 1f - marginLeft - marginRight;
    public float displayHeight => 1f - marginTop - marginMiddle - marginBottom - clockHeight;

    public enum HistoryPanelMode {
        EntireSimulation, // fully zoomed out
        ActiveSpecies,
        SelSpeciesPopulation,
        SelCreatureTimeline // fully zoomed in
    }
    
    private HistoryPanelMode curPanelMode = HistoryPanelMode.SelSpeciesPopulation;

    public void SetCurPanelMode(HistoryPanelMode mode) {
        curPanelMode = mode;
    }
        
    public struct SpeciesLineData { //species + maybe creatures?
        public Vector3 worldPos;
        public Vector4 color;
        public int speciesID;
        public int candidateID;
        public int isAlive; // wasteful, bools dont work
        public int isSelected; // wasteful, bools dont work
    }
    public struct CreatureLineData { //species + maybe creatures?
        public Vector3 worldPos;
        public Vector4 color;
        public int speciesID;
        public int candidateID;
        public int isAlive; // wasteful, bools dont work
        public int isSelected; // wasteful, bools dont work
        public int isVisible;
    }
    public struct ResourceLineData {
        public Vector3 worldPos; // timestep. value. depth
        public Vector4 color;
    }
    public struct GridLineData {
        public Vector3 worldPos;
        public Vector4 color;
        public float scale; // ?? what else?
    }
    
    public ComputeBuffer speciesLineDataCBuffer; // species+creature lines
    public ComputeBuffer creatureLineDataCBuffer;
    public ComputeBuffer resourceLineDataCBuffer;
    public ComputeBuffer gridLineDataCBuffer;
    private int worldTreeNumPointsPerSpeciesLine = 64;    
    private int worldTreeNumSpeciesLines = 32;
    private int worldTreeNumCreatureLines = 256; // should be same size as GenomeButtonUI collection
    private int worldTreeNumPointsPerCreatureLine = 32;

    private int worldTreeNumGridLines100 = 16;
    private int worldTreeNumGridLines1000 = 16;
    private int worldTreeNumGridLinesDays = 32;
    private int worldTreeNumGridLinesMonths = 16;
    private int worldTreeNumGridLinesYears = 32;
    private int worldTreeNumGridLines => worldTreeNumGridLines100 + worldTreeNumGridLines1000 + worldTreeNumGridLinesDays + worldTreeNumGridLinesMonths + worldTreeNumGridLinesYears;
    private int worldTreeNumPointsPerGridLine = 4;
    
    private int worldTreeNumResourceLines = 8;
    private int worldTreeNumPointsPerResourceLine = 128;
    private int speciesBufferCount => worldTreeNumPointsPerSpeciesLine * worldTreeNumSpeciesLines;
    private int creatureBufferCount => worldTreeNumPointsPerCreatureLine * worldTreeNumCreatureLines;
    private int gridLineBufferCount => worldTreeNumGridLines * worldTreeNumPointsPerGridLine;
    private int resourceLineBufferCount => worldTreeNumResourceLines * worldTreeNumPointsPerResourceLine;
    private int sizeOfSpeciesLineDataStruct => sizeof(float) * 7 + sizeof(int) * 4;
    private int sizeOfCreatureLineDataStruct => sizeof(float) * 7 + sizeof(int) * 5;
    private int sizeOfResourceLineDataStruct => sizeof(float) * 7;
    private int sizeOfGridLineDataStruct => sizeof(float) * 8;
    public bool isEntireSimulationMode => curPanelMode == HistoryPanelMode.EntireSimulation;
    public bool isActiveSpeciesMode => curPanelMode == HistoryPanelMode.ActiveSpecies;
    public bool isPopulationMode => curPanelMode == HistoryPanelMode.SelSpeciesPopulation;
    public bool isTimelineMode => curPanelMode == HistoryPanelMode.SelCreatureTimeline;
    
    public bool isGraphMode;
    public bool isResourceMode;

    bool isPanelOpen => openCloseButton.isOpen;

    [SerializeField]
    OpenCloseButton openCloseButton;
    [SerializeField]
    TooltipUI tooltipOpenCloseButton;

    private bool isNudgeOn = false;
    [ReadOnly]
    public int nudgeCounter;

    public float graphBoundsMinY = 500f;
    public float graphBoundsMaxY = 1000f;
    public float graphBoundsMinX;// => Mathf.Max(0f, simManager.simAgeTimeSteps - (maxScoreValue - minScoreValue) * 31.4f);
    public float graphBoundsMaxX;// => simManager.simAgeTimeSteps;

    private float targetGraphBoundsMinY;
    private float targetGraphBoundsMaxY;
    private float targetGraphBoundsMinX;
    private float targetGraphBoundsMaxX;

    private float alltimeHighestScore = 0f;

    [ReadOnly]
    public bool mouseWithinPanelBounds;
    [ReadOnly]
    public Vector2 mousePosPanelCoords;
    
    public void Start() {
        openCloseButton.SetHighlight(true);
    }

    public void UpdateTargetGraphBounds() {
        // UPDATE TARGET GRAPH BOUNDS:::
        // Selected Species?
        CandidateAgentData selectedCand = selectionManager.currentSelection.candidate;
        SpeciesGenomePool selectedPool = masterGenomePool.completeSpeciesPoolsList[selectedCand.speciesID];
        
        targetGraphBoundsMaxX = simManager.simAgeTimeSteps;
        foreach(var species in masterGenomePool.completeSpeciesPoolsList) {
            if (species.speciesAllTimeMaxScore > alltimeHighestScore)
                alltimeHighestScore = species.speciesAllTimeMaxScore;
        }
        
        // 
        //AllSpeciesView  // change how this is structured!:
        if(isEntireSimulationMode) {
            targetGraphBoundsMinY = 1500f;
            targetGraphBoundsMinX = 0f;
            targetGraphBoundsMaxY = alltimeHighestScore + 256;
        }
        if(isActiveSpeciesMode) { // all species shown, but auto-zoom
            targetGraphBoundsMinY = 10000f; //pos infinity
            targetGraphBoundsMaxY = 0f;
            foreach(SpeciesGenomePool pool in masterGenomePool.completeSpeciesPoolsList) {
                if(pool.isExtinct || pool.speciesDataPointsList.Count == 0) {
                    continue;
                }
                if(pool.avgLifespan < targetGraphBoundsMinY) {
                    targetGraphBoundsMinY = pool.avgLifespan;
                }
                if(pool.avgLifespan > targetGraphBoundsMaxY) {
                    targetGraphBoundsMaxY = pool.avgLifespan + 128;
                }
            }
            // X range based on Y-range for now:
            targetGraphBoundsMinX = Mathf.Max(0f, simManager.simAgeTimeSteps - (targetGraphBoundsMaxY - targetGraphBoundsMinY) * 33f);
            targetGraphBoundsMaxX = simManager.simAgeTimeSteps;
        }
        CandidateAgentData oldestActiveCand = null;
        if (uiManagerRef.speciesOverviewUI.candidateGenomeButtons.Count > 0) {
            oldestActiveCand = uiManagerRef.speciesOverviewUI.candidateGenomeButtons[0].candidateRef;
        }
        if(isPopulationMode) { //***EAC START HERE!!!
            //is species extinct?
            selectedPool.UpdateOldestActiveCandidateAndGraphBounds();
            
            targetGraphBoundsMinX = selectedPool.curActiveGraphBoundsMinX - 8;
            targetGraphBoundsMinY = selectedPool.curActiveGraphBoundsMinY - 8;
            targetGraphBoundsMaxX = selectedPool.curActiveGraphBoundsMaxX + 8;
            targetGraphBoundsMaxY = selectedPool.curActiveGraphBoundsMaxY + 8;

            //targetGraphBoundsMaxX = simManager.simAgeTimeSteps;
            //targetGraphBoundsMinX = selectedCand.performanceData.timeStepHatched; // simManager.masterGenomePool.completeSpeciesPoolsList[oldestActiveCand.speciesID].speciesAllTimeMinScore;// performanceData.creatureDataPointsList[0].timestep;
            //targetGraphBoundsMinY = Mathf.Min(selectedCand.performanceData.p0y, selectedCand.performanceData.p1y)-16;
            //targetGraphBoundsMaxY = Mathf.Max(selectedCand.performanceData.p0y, selectedCand.performanceData.p1y)+16;
            
            
            
            // targetGraphBoundsMinY = selectedCand.performanceData.p0y - 16f;// selectedPool.avgLifespan-64f;//0f; // simManager.masterGenomePool.completeSpeciesPoolsList[oldestActiveCand.speciesID].speciesCurAliveMinScore;                
            //targetGraphBoundsMaxY = selectedCand.performanceData.p1y + 16f;// selectedPool.avgLifespan + 64f;// speciesCurAliveMaxScore + 3280;
            // Mathf.Max(0f, oldestCand.performanceData.timeStepHatched);// uiManager.historyPanelUI.maxScoreValue - uiManager.historyPanelUI.minScoreValue) * 33f);
                          
        }
        if(isTimelineMode) {
            targetGraphBoundsMinX = Mathf.Max(0f, selectionManager.currentSelection.candidate.performanceData.timeStepHatched);// uiManager.historyPanelUI.maxScoreValue - uiManager.historyPanelUI.minScoreValue) * 33f);
            targetGraphBoundsMinY = simManager.masterGenomePool.completeSpeciesPoolsList[oldestActiveCand.speciesID].avgLifespan - 20f;//.speciesCurAliveMinScore;
            targetGraphBoundsMaxY = simManager.masterGenomePool.completeSpeciesPoolsList[oldestActiveCand.speciesID].avgLifespan + 128f;//.speciesCurAliveMaxScore + 1290;// selectedCand.performanceData.creatureDataPointsList[selectedCand.performanceData.creatureDataPointsList.Count - 1].lifespan;
            //graphBoundsMinY = selectedPool.minScoreValue - 33f;
            //graphBoundsMaxY = selectedPool.maxScoreValue + 33f;
        }
    }
        
    public void SetNudgeTooltip(string str) {
        tooltipNudgeMessage.tooltipString = str;
    }
    
    public void TriggerNudgeMessage(string str) {
        textNudgeMessage.text = str;
        panelNudgeMessage.SetActive(true);
        isNudgeOn = true;
        nudgeCounter = 0;
    }
    
    public void CloseNudgeMessage() {
        textNudgeMessage.text = "";
        panelNudgeMessage.SetActive(false);
        isNudgeOn = false;
        nudgeCounter = 0;
    }
    
    public void InitializePanel() {
        InitializeSpeciesIcons();
        GenerateCreatureEventIcons();
    }
    
    private void GenerateCreatureEventIcons() {
        for (int i = 0; i < maxNumCreatureEventIcons; i++) {
            CreateCreatureEventIcon();
        }
    }
    
    private void CreateCreatureEventIcon() {
        var icon = Instantiate(prefabCreatureEventIconUI).GetComponent<CreatureEventIconUI>();
        icon.OnCreate(anchorGO.transform);
        creatureEventIcons.Add(icon);
    }
    
    private void UpdateCreatureEventIcons(CandidateAgentData candidate) {
        if (candidate?.candidateEventDataList == null ||
            creatureEventIcons.Count == 0 || candidate.candidateEventDataList.Count == 0) 
            return;
        
        ClearDeadCreatureEventIcons();
        
        for (int i = 0; i < creatureEventIcons.Count; i++) {
            if (curPanelMode != HistoryPanelMode.SelCreatureTimeline) {
                // Hide icons when not on Timeline screen
                creatureEventIcons[i].gameObject.SetActive(false); 
                continue;
            }
            
            var active = i < candidate.candidateEventDataList.Count;
            creatureEventIcons[i].gameObject.SetActive(active);
            if (!active) continue;
                
            Vector2 eventCoords = Vector2.zero;
            eventCoords.x = (float)(candidate.candidateEventDataList[i].eventFrame - candidate.performanceData.timeStepHatched) / (float)(simManager.simAgeTimeSteps - candidate.performanceData.timeStepHatched);
            eventCoords.x *= displayWidth + marginLeft;
            eventCoords.y = (1f - ((float)candidate.candidateEventDataList[i].type / 12f)) * displayHeight + marginBottom;
            creatureEventIcons[i].UpdateIconPrefabData(candidate.candidateEventDataList[i], i);
            creatureEventIcons[i].SetTargetCoords(eventCoords);
            creatureEventIcons[i].SetDisplay();
        } 
    }
    
    public void RefreshFocusedAgent(Agent agent) {
        if (agent == null) return;
        RefreshFocusedAgent(selectionManager.IsSelected(agent.candidateRef));
    }
    
    public void RefreshFocusedAgent(bool focusHasChanged)
    {
        // * WPP: comment suggests checking for matching species, but code is checking candidate ID, not species ID
        // + design goals of this if/else branch unclear
        
        // Same species
        if (focusHasChanged)
        {
            switch (curPanelMode)
            {
                case HistoryPanelMode.EntireSimulation: SetCurPanelMode(HistoryPanelMode.SelSpeciesPopulation); break;
                case HistoryPanelMode.SelSpeciesPopulation: SetCurPanelMode(HistoryPanelMode.SelCreatureTimeline); break;
            }
        }
        // different species
        else 
        {
            switch (curPanelMode)
            {
                case HistoryPanelMode.EntireSimulation: SetCurPanelMode(HistoryPanelMode.ActiveSpecies); break;
                case HistoryPanelMode.SelSpeciesPopulation: break;
                case HistoryPanelMode.SelCreatureTimeline: SetCurPanelMode(HistoryPanelMode.SelSpeciesPopulation); break;
            }
        }
    }
    
    // * Rename to suggest this is a per-frame update, break out the initialization logic
    // * APPLICATION BOTTLENECK: focus optimization efforts here!
        // Since this is for visuals, could call on a fixed interval, e.g. 1/5 second
    public void RebuildRenderBuffers() 
    {        
        // Initialize world tree line data
        var speciesLines = new SpeciesLineData[speciesBufferCount];
        speciesLineDataCBuffer?.Release();
        speciesLineDataCBuffer = new ComputeBuffer(speciesBufferCount, sizeOfSpeciesLineDataStruct);

        var creatureLines = new CreatureLineData[creatureBufferCount];
        creatureLineDataCBuffer?.Release();
        creatureLineDataCBuffer = new ComputeBuffer(creatureBufferCount, sizeOfCreatureLineDataStruct);
        
        var gridLines = new GridLineData[gridLineBufferCount];
        gridLineDataCBuffer?.Release();
        gridLineDataCBuffer = new ComputeBuffer(gridLineBufferCount, sizeOfGridLineDataStruct);

        // Create world resource stat lines
        var resourceLines = new ResourceLineData[resourceLineBufferCount];
        resourceLineDataCBuffer?.Release();
        resourceLineDataCBuffer = new ComputeBuffer(resourceLineBufferCount, sizeOfResourceLineDataStruct);
        

        // Create species lines
        for (int line = 0; line < worldTreeNumSpeciesLines; line++) {
            for (int i = 0; i < worldTreeNumPointsPerSpeciesLine; i++) {
                CreateSpeciesLine(line, i, mousePosPanelCoords, speciesLines);
            }
        }
        // Create creature lines
        for (int line = 0; line < worldTreeNumCreatureLines; line++) {
            if (line >= uiManagerRef.speciesOverviewUI.candidateGenomeButtons.Count) continue;
            CandidateAgentData cand = uiManagerRef.speciesOverviewUI.candidateGenomeButtons[line].candidateRef;
            if(cand != null) {
                for (int i = 0; i < worldTreeNumPointsPerCreatureLine; i++) {     // this number replaces maxnumdataentries           
                    CreateCreatureLine(line, i, mousePosPanelCoords, creatureLines, cand);
                }
            }            
        }
        // Create Horizontal Gridlines Small (100f span):
        for (int line = 0; line < worldTreeNumGridLines100; line++) {
            for (int i = 0; i < worldTreeNumPointsPerGridLine; i++) {
                int lineIndex = line;
                float val = line * 100f;
                float size = 0.005f;
                CreateHorizontalGridLine(new Color(0.35f, 0.35f, 0.35f, 0.65f), val, size, lineIndex, i, mousePosPanelCoords, gridLines);
            }
        }
        // Create Horizontal Gridlines Large (1000f span):
        for (int line = 0; line < worldTreeNumGridLines1000; line++) {
            for (int i = 0; i < worldTreeNumPointsPerGridLine; i++) {
                int lineIndex = line + worldTreeNumGridLines100;
                float val = line * 1000f;
                float size = 0.0045f;
                CreateHorizontalGridLine(new Color(0.5f, 0.5f, 0.5f, 0.8f), val, size, lineIndex, i, mousePosPanelCoords, gridLines);
            }
        }
        // Create Vertical Gridlines Days
        for (int line = 0; line < worldTreeNumGridLinesDays; line++) {
            for (int i = 0; i < worldTreeNumPointsPerGridLine; i++) {
                int lineIndex = line + worldTreeNumGridLines100 + worldTreeNumGridLines1000;
                float val = line * simManager.GetNumTimeStepsPerYear() / 365f;
                float size = 0.0014f;
                CreateVerticalGridLine(new Color(0.15f, 0.33f, 0.15f, 0.65f), val, size, lineIndex, i, mousePosPanelCoords, gridLines);
            }
        }
        // Create Vertical Gridlines Months
        for (int line = 0; line < worldTreeNumGridLinesMonths; line++) {
            for (int i = 0; i < worldTreeNumPointsPerGridLine; i++) {
                int lineIndex = line + worldTreeNumGridLines100 + worldTreeNumGridLines1000 + worldTreeNumGridLinesDays;
                float val = (line + 1f) * simManager.GetNumTimeStepsPerYear() / 12f;
                float size = 0.005f;
                CreateVerticalGridLine(new Color(0.35f, 0.35f, 0.25f, 0.75f), val, size, lineIndex, i, mousePosPanelCoords, gridLines);
            }
        }
        // Create Vertical Gridlines Years
        for (int line = 0; line < worldTreeNumGridLinesYears; line++) {
            for (int i = 0; i < worldTreeNumPointsPerGridLine; i++) {
                int lineIndex = line + worldTreeNumGridLines100 + worldTreeNumGridLines1000 + worldTreeNumGridLinesDays + worldTreeNumGridLinesMonths;
                float val = (line + 1f) * simManager.GetNumTimeStepsPerYear();
                float size = 0.0025f;
                CreateVerticalGridLine(new Color(0.55f, 0.55f, 0.55f, 0.8f), val, size, lineIndex, i, mousePosPanelCoords, gridLines);
            }
        }
        
        for (int i = 0; i < worldTreeNumResourceLines; i++) {
            if(i >= simManager.simResourceManager.simResourcesArray.Length) {
                continue;
            }
            for(int j = 0; j < worldTreeNumPointsPerResourceLine; j++) {
                CreateResourceLine(i, j, mousePosPanelCoords, resourceLines, simManager.simResourceManager.simResourcesArray[i]);
            }
        }

        gridLineDataCBuffer.SetData(gridLines);
        resourceLineDataCBuffer.SetData(resourceLines);
        speciesLineDataCBuffer.SetData(speciesLines);
        creatureLineDataCBuffer.SetData(creatureLines);

        uiManagerRef.clockPanelUI.UpdateResourceStatsText();

        TheRenderKing.instance.gridLineDataMat.SetBuffer("quadVerticesCBuffer", TheRenderKing.instance.quadVerticesCBuffer);
        TheRenderKing.instance.gridLineDataMat.SetBuffer("gridLineDataCBuffer", gridLineDataCBuffer);

        TheRenderKing.instance.resourceLineDataMat.SetBuffer("quadVerticesCBuffer", TheRenderKing.instance.quadVerticesCBuffer);
        TheRenderKing.instance.resourceLineDataMat.SetBuffer("resourceLineDataCBuffer", resourceLineDataCBuffer);
   
        TheRenderKing.instance.creatureLineDataMat.SetBuffer("quadVerticesCBuffer", TheRenderKing.instance.quadVerticesCBuffer);
        TheRenderKing.instance.creatureLineDataMat.SetBuffer("worldTreeLineDataCBuffer", creatureLineDataCBuffer);
  
        TheRenderKing.instance.speciesLineDataMat.SetBuffer("quadVerticesCBuffer", TheRenderKing.instance.quadVerticesCBuffer);
        TheRenderKing.instance.speciesLineDataMat.SetBuffer("worldTreeLineDataCBuffer", speciesLineDataCBuffer);
    }
    void CreateVerticalGridLine(Color color, float timeStepCoord, float lineSize, int line, int pIndex, Vector2 cursorCoords, GridLineData[] gridLines) {
         int index = (line) * worldTreeNumPointsPerGridLine + pIndex;
        
        if (line >= worldTreeNumGridLines) 
            return;
        //current bounds of graph: minValue/MaxValue, minTimestep/curTime
        //figure out what kind of line this needs to be based on line index
        GridLineData data = new GridLineData();        
        float frac = (float)pIndex / (float)worldTreeNumPointsPerGridLine;
        float x = timeStepCoord;
        float y = frac * graphBoundsMaxY * 2f;
                
        data.color = color;
        data.scale = lineSize;

        if (pIndex == 0) data.color.w = 0f;
        var coordinates = AnchorBottomLeft(x, y);
        data.worldPos = new Vector3(coordinates.x, coordinates.y, 1f);   
       
        gridLines[index] = data;
    }
    void CreateHorizontalGridLine(Color color, float lifespanCoord, float lineSize, int line, int pIndex, Vector2 cursorCoords, GridLineData[] gridLines) {
         int index = (line) * worldTreeNumPointsPerGridLine + pIndex;
        
        if (line >= worldTreeNumGridLines) 
            return;
        //current bounds of graph: minValue/MaxValue, minTimestep/curTime
        //figure out what kind of line this needs to be based on line index
        GridLineData data = new GridLineData();        
        float frac = (float)pIndex / (float)worldTreeNumPointsPerGridLine;
        float x = frac * simManager.simAgeTimeSteps * 1.5f;
        float y = lifespanCoord;

        data.color = color; // new Vector4(0.35f, 0.35f, 0.35f, 1f);
        data.scale = lineSize;

        if (pIndex == 0) data.color.w = 0f;
        var coordinates = AnchorBottomLeft(x, y);
        data.worldPos = new Vector3(coordinates.x, coordinates.y, 1f);   
       
        gridLines[index] = data;
    }
    void CreateResourceLine(int line, int pIndex, Vector2 cursorCoords, ResourceLineData[] resourceLines, SimResource resource) {
        int index = (line) * worldTreeNumPointsPerResourceLine + pIndex;            
        
        if (line >= worldTreeNumResourceLines) 
            return;
        List<ResourceDataPoint> dataPointsList = resource.resourceDataPointList;
        ResourceLineData data = new ResourceLineData();
        if (dataPointsList.Count == 0 || pIndex >= dataPointsList.Count) 
            return;
        //float x = pool.speciesDataPointsList[point].timestep; 
        float x = (float)dataPointsList[pIndex].timestep;
        float y = (dataPointsList[pIndex].value - resource.GetMinValue()) / ((resource.GetMaxValue() - resource.GetMinValue()));
        //y = y * (maxScoreValue - minScoreValue) + minScoreValue; // match to current graph bounds - TEMP!!!
        data.color = resource.GetColor(); // Vector4.Lerp(new Vector4(1f,0f,0.2f,1f), new Vector4(0f,1f,0.8f,1f), (float)line / 6f); // GetCreatureLineColor(hue, cand, inXBounds);         
        if (!isResourceMode) {
            //data.worldPos = Vector3.zero;
            data.color = resource.GetColor(); // new Color(0f, 0f, 0f, 0f);
            data.color.w = 0f;
        }
        
        if (pIndex == 0) data.color.w = 0f;
        var coordinates = AnchorBottomLeft(x, y);
        data.worldPos = new Vector3(coordinates.x, coordinates.y, 1f);   
        
        resourceLines[index] = data;
    }
    void CreateSpeciesLine(int line, int point, Vector2 cursorCoords, SpeciesLineData[] speciesLines)
    {
        int index = line * worldTreeNumPointsPerSpeciesLine + point;

        if (line >= simManager.masterGenomePool.completeSpeciesPoolsList.Count) 
            return;
        
        SpeciesGenomePool pool = simManager.masterGenomePool.completeSpeciesPoolsList[line];
        SpeciesLineData data = new SpeciesLineData();
                    
        if (pool.speciesDataPointsList.Count == 0 || point >= pool.speciesDataPointsList.Count) 
            return;

        float x = pool.speciesDataPointsList[point].timestep; // (float)pool.speciesDataPointsList[point].timestep / (float)simManager.simAgeTimeSteps;
        //float val = (float)(pool.speciesDataPointsList[point].lifespan - minScoreValue) / (maxScoreValue - minScoreValue);
        float y = pool.speciesDataPointsList[point].lifespan; // Mathf.Lerp(valStart, valEnd, frac); // Mathf.Sin(xCoord / orbitalPeriod * (simManager.simAgeTimeSteps) * animTimeScale) * 0.075f * (float)lineID + 0.5f;
        float z = 0f;
            
        Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
        //hue.x = Mathf.Clamp01(hue.x + 0.4f);
        //hue.y = Mathf.Clamp01(hue.y + 0.4f);
        //hue.z = Mathf.Clamp01(hue.z + 0.4f);
        float alpha = 1f;
        //int timeStepStart = Mathf.RoundToInt(timelineStartTimeStep);
         
        data.isSelected = 0;
        if (pool.speciesID == historySelectedSpeciesID) {
            //hue = Vector3.one;
            z = -1.25f;
            data.isSelected = 1;
        }
        data.isAlive = 1;
        if (point == 0) {
            //data.isAlive = 0;
            z = 0.1f;
            alpha = 0f;
        }
        if(pool.isExtinct) {
            //data.isAlive = 0;
            hue *= 0.8f;
        }
      
        var coordinates = AnchorBottomLeft(x, y); // new Vector2(x, y); // 
        //z = -1.1f * data.isAlive;
        data.worldPos = new Vector3(coordinates.x, coordinates.y, z);
        data.color = new Color(hue.x, hue.y, hue.z, alpha);// Color.HSVToRGB(lerp, 1f - lerp, 1f); // Color.Lerp(Color.white, Color.black, lineID * 0.11215f);
        if(isResourceMode) {
            data.color *= 0.25f;
            z = 0.2f;
        }
        speciesLines[index] = data;
       
    }
    
    void CreateCreatureLine(int line, int point, Vector2 cursorCoords, CreatureLineData[] creatureLines, CandidateAgentData cand)
    {
        int index = (worldTreeNumCreatureLines - 1 - line) * worldTreeNumPointsPerCreatureLine + point;        
        CreatureLineData data = new CreatureLineData();

        // OLD::: 
        /*if (cand == null || cand.performanceData.creatureDataPointsList == null || point >= cand.performanceData.creatureDataPointsList.Count) {
            data.worldPos = new Vector3(graphBoundsMaxX, graphBoundsMaxY, 0f);//.zero;
            data.color = Vector4.zero;
            creatureLines[index] = data;
            return;
        }*/
        data.isVisible = 1;

        if (cand == null) {
            data.worldPos = Vector3.zero;// new Vector3(graphBoundsMaxX, graphBoundsMaxY, 0f);//.zero;
            data.color = Vector4.zero;
            creatureLines[index] = data;
            data.isVisible = 0;
            return;
        }
        
        float x = 0f;// pointWorldPos.x;// cand.performanceData.creatureDataPointsList[point].lifespan;        
        float y = 0f;// pointWorldPos.y; // cand.performanceData.creatureDataPointsList[point].timestep; // (float)pool.speciesDataPointsList[point].timestep / (float)simManager.simAgeTimeSteps;
        float z = 0f;

        if(cand.performanceData.bezierCurve != null) {
            float frac = (float)point / (float)(worldTreeNumPointsPerCreatureLine-1); // along length of bezier curve
            //float y01 = 1f - (float)line / (float)worldTreeNumCreatureLines;

            if(SimulationManager.instance.masterGenomePool.completeSpeciesPoolsList.Count < 1) {
                    //cand.performanceData.scoreStart = 0f;
            }
            else {
                
                //float y01 = line % 24;
                //cand.SetCurveData(cand.performanceData.timeStepHatched, cand.performanceData.timeStepDied, graphBoundsMinY, Mathf.Lerp(graphBoundsMinY, graphBoundsMaxY, y01));
                cand.performanceData.p0x = cand.performanceData.timeStepHatched;
                cand.performanceData.p1x = cand.performanceData.timeStepDied;
                SpeciesGenomePool poolRef = SimulationManager.instance.masterGenomePool.completeSpeciesPoolsList[cand.speciesID];
                //if(cand.isBeingEvaluated) { // stop updating line positions after creature death
                    //cand.performanceData.scoreStart = graphBoundsMinY; // need avgLifespan of species at time of this creature's birth
                    //y01 = y01 * 228f - 146f + SimulationManager.instance.masterGenomePool.completeSpeciesPoolsList[cand.speciesID].avgLifespan;
                float y01 = Mathf.Clamp01((cand.candidateID % 47) / 46f);
                //cand.performanceData.p1y = SimulationManager.instance.masterGenomePool.completeSpeciesPoolsList[cand.speciesID].avgLifespan + (cand.candidateID % 47);// Mathf.Clamp01((y01 - graphBoundsMinY) / (graphBoundsMaxY - graphBoundsMinY)); //SS  // SimulationManager.instance.masterGenomePool.completeSpeciesPoolsList[cand.speciesID].avgLifespan;
                float posMin = poolRef.curActiveGraphBoundsMinY;
                float posMax = poolRef.curActiveGraphBoundsMaxY;
                if(posMax - posMin < 16f) {
                    posMin -= 8f;
                    posMax += 8f;
                }
                //cand.performanceData.p1y = Mathf.Lerp(cand.performanceData.p0y, Mathf.Lerp(posMin, posMax, y01), 0.1f); 
                cand.performanceData.p1y = Mathf.Lerp(cand.performanceData.p1y, cand.performanceData.p0y, 0.2f);
                //collision??
                //COLLISION!!!!!!!!!!:: 
                // this isn't going to work, need a different approach
                float collisionSize = 0.06f;
                float collisionForce = 1f;
                for(int j = 0; j < uiManagerRef.speciesOverviewUI.candidateGenomeButtons.Count; j++) {
                    Vector2 vecToIcon = uiManagerRef.speciesOverviewUI.candidateGenomeButtons[j].targetCoords -
                                        uiManagerRef.speciesOverviewUI.candidateGenomeButtons[line].targetCoords;
                    float iconDistanceSquared = vecToIcon.sqrMagnitude;
                    if(iconDistanceSquared < collisionSize) {
                        //x -= vecToIcon.x * collisionForce;
                        //y -= vecToIcon.y * collisionForce;
                        cand.performanceData.p1y -= vecToIcon.y * collisionForce;
                    }
                    else {
                        
                    }
                }
                //}
                
                cand.UpdateDisplayCurve();
            }
            
            Vector3 pointWorldPos = cand.performanceData.bezierCurve.GetPoint(frac);
            x = pointWorldPos.x;
            y = pointWorldPos.y; 
        }
        //Debug.Log(x + ", " + y);
        
        SpeciesGenomePool pool = simManager.masterGenomePool.completeSpeciesPoolsList[cand.speciesID];
        Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
        //hue.x = Mathf.Clamp01(hue.x + 0.4f);
        //hue.y = Mathf.Clamp01(hue.y + 0.4f);
        //hue.z = Mathf.Clamp01(hue.z + 0.4f);
        
         // fog, placement
        data.isAlive = 1;
        data.color = new Color(hue.x, hue.y, hue.z);
        data.color.w = 0;
        
        if(cand.speciesID != selectionManager.currentSelection.candidate.speciesID) {
            ///hue *= 0.5f;
            //data.color *= 0.275f;//.w = 1f;
            data.color.w = 0.75f; // fog, placement
            data.color.x *= 0.46f;
            data.color.y *= 0.46f;
            data.color.z *= 0.46f;
            z = -1.0f;
        }
        if (cand.performanceData.timeStepHatched <= 1 || point < 1) {
            data.isAlive = 0;
            data.color = Vector3.zero;
            data.color.w = 1.0f; // fog, placement
            data.isVisible = 0;
            z = 0.2f;
        }
        if(!cand.isBeingEvaluated) {
            //data.isAlive = 0;
            // dead!
            data.color.x = Mathf.Lerp(data.color.x, 0.378f, 0.655f) * 0.8f;
            data.color.y = Mathf.Lerp(data.color.y, 0.378f, 0.655f) * 0.8f;
            data.color.z = Mathf.Lerp(data.color.z, 0.378f, 0.655f) * 0.8f;
            z = -0.071f;
            data.color.w = 0.255f; // fog, placement
        }
        else {
            z -= 0.3f;
        }
        if(cand.speciesID == selectionManager.currentSelection.candidate.speciesID) {
            z -= 1.71f;
            data.color.w = 0;
        }
        else {
            z -= 0.1f;
            data.color.w = 0.637f; // fog, placement
        }
        if (cand.candidateID == selectionManager.currentSelection.candidate.candidateID) {
            data.isSelected = 1;
            z = -2.91f;
            data.color.w = 0;
            //data.color = Vector4.one;
        }
        else {
            //data.color.w = 0; // fog, placement
        }
        if(isResourceMode) {
            ///hue *= 0.5f;
            //data.color *= 0.275f;//.w = 1f;
            data.color.w = 1f; // fog, placement
            z = 1f;
        }

        var coordinates = AnchorBottomLeft(x, y);
        data.worldPos = new Vector3(coordinates.x, coordinates.y, z);   
        
        data.speciesID = cand.speciesID;

        data.color.w = 0; // fog, placement
        creatureLines[index] = data;
        // Mouse hover highlight                 
        //if ((coordinates - cursorCoords).magnitude < 0.05f) { 
        //    data.color = Color.white;
        //}

    }
    
    private void CreateSpeciesIcon(SpeciesGenomePool pool) 
    {
        AgentGenome templateGenome = masterGenomePool.completeSpeciesPoolsList[pool.speciesID].foundingCandidate.candidateGenome; 
        Color color = templateGenome.bodyGenome.appearanceGenome.primaryColor;
        
        var icon = Instantiate(prefabSpeciesIcon).GetComponent<SpeciesIconUI>();
        icon.Initialize(speciesIcons.Count - 1, masterGenomePool.completeSpeciesPoolsList[pool.speciesID], anchorGO.transform, color);
        speciesIcons.Add(icon);
        Debug.Log("NEW SPECIES ICON INSTANTIATED : " + pool.speciesID);
    }

    public void SortSpeciesIconList() {
        /*if (speciesIconsSorted == null) {
            speciesIconsSorted = new List<SpeciesIconUI>();
        }
        speciesIconsSorted.Clear();
        
        for(int i = 0; i<speciesIcons.Count; i++) {
            speciesIconsSorted.Add(speciesIcons[i]);
        }
        
        for(int i = 0; i < speciesIcons.Count - 1; i++) {
            //SpeciesIconUI swapIcon = new SpeciesIconUI(); 
            //iconA = speciesIcons[i];
            SpeciesIconUI icon = speciesIcons[i];
            SpeciesIconUI iconNext = speciesIcons[i+1];
            if(icon.linkedPool.avgLifespan < iconNext.linkedPool.avgLifespan) {
                //swapIcon = icon;
                speciesIconsSorted[i] = iconNext;
                speciesIconsSorted[i+1] = icon;
            }
            
        }

        speciesIcons.Clear();// = speciesIconsSorted;
        for(int i = 0; i<speciesIconsSorted.Count; i++) {
            speciesIcons.Add(speciesIconsSorted[i]);
        }
        */
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
    
    public void ClickSpeciesIcon(SpeciesIconUI iconUI) 
    {
        if (iconUI.speciesID == historySelectedSpeciesID) 
        {
            //Debug.Log("ClickSpeciesIcon(SpeciesIconUI iconUI) " + curPanelMode);
            if (isPopulationMode) 
            {
                // Acting as a "BACK" button!
                curPanelMode = HistoryPanelMode.EntireSimulation;
            }
            else if (isActiveSpeciesMode || isEntireSimulationMode) 
            {
                // Zoom into sel species pop
                curPanelMode = HistoryPanelMode.SelSpeciesPopulation;
            }
        }

        selectionManager.SetSelectedFromSpeciesUI(iconUI.speciesID);
        uiManagerRef.speciesOverviewUI.RefreshGenomeButtons();
        uiManagerRef.ExitTooltipObject();

    }
    
    public void ClickButtonToggleGraphMode() {
        isGraphMode = !isGraphMode; // deprecate!
        if (isActiveSpeciesMode) {
            curPanelMode = HistoryPanelMode.EntireSimulation;
        }
        else {
            if (isEntireSimulationMode) {
                curPanelMode = HistoryPanelMode.ActiveSpecies;
            }
        }        
    }
    public void ClickButtonToggleResourceMode() {        
        isResourceMode = !isResourceMode;
    }
    
    public void ClickButtonToggleExtinct() { }
    
    public void ClickButtonBack() {
        if (curPanelMode == HistoryPanelMode.SelSpeciesPopulation) {
            curPanelMode = HistoryPanelMode.ActiveSpecies;
        } 
        else if (curPanelMode == HistoryPanelMode.ActiveSpecies) {
            curPanelMode = HistoryPanelMode.EntireSimulation;
        }
        else if (curPanelMode == HistoryPanelMode.SelCreatureTimeline) {
            curPanelMode = HistoryPanelMode.SelSpeciesPopulation;
        }        
    }   
    
    public void Tick() 
    {
        if(isNudgeOn) {
            nudgeCounter++;
            if(nudgeCounter >= 420) {
                CloseNudgeMessage();                
            }
        }

        var mouseInOpenCloseArea = Screen.height - Input.mousePosition.y < 64 && Input.mousePosition.x < 64;
        openCloseButton.SetMouseEnter(mouseInOpenCloseArea);

        mouseWithinPanelBounds = Screen.height - Input.mousePosition.y < panelSizePixels && Input.mousePosition.x < panelSizePixels;
        //mousePosPanelCoords = Vector2.zero;
        uiManagerRef.SetCursorInSpeciesHistoryPanel(mouseWithinPanelBounds);
        
        if(mouseWithinPanelBounds) {
            mousePosPanelCoords.x = Input.mousePosition.x;
            mousePosPanelCoords.y = Input.mousePosition.y - (Screen.height - panelSizePixels);
        }
        tooltipOpenCloseButton.tooltipString = isPanelOpen ? "Hide Timeline Panel" : "Open Timeline Panel";

        buttonToggleExtinct.gameObject.SetActive(false);
        //float targetStartTimeStep = 0f;
        tempPanelSpeciesPop.SetActive(false);
        tempPanelGraph.SetActive(false);
        buttonBack.gameObject.SetActive(false);
        buttonSelCreatureEventsLink.gameObject.SetActive(false);
        tempPanelLifeEvents.gameObject.SetActive(false);

        SpeciesGenomePool selPool = simManager.masterGenomePool.completeSpeciesPoolsList[selectionManager.currentSelection.candidate.speciesID];
        
        switch (curPanelMode)
        {
            case HistoryPanelMode.EntireSimulation:
                buttonToggleGraphMode.gameObject.SetActive(true);                
                UpdateSpeciesIconsLineageMode();
                //targetStartTimeStep = simManager.masterGenomePool.completeSpeciesPoolsList[simManager.masterGenomePool.currentlyActiveSpeciesIDList[0]].timeStepCreated;
                tempPanelGraph.SetActive(true);
                textPanelStateDebug.text = uiManagerRef.clockPanelUI.cursorTimeStep +
                                            ", SP# " + currentSelection.candidate.speciesID +
                                            "\nC# " + currentSelection.candidate.candidateID +
                                            "\nminX: " + graphBoundsMinX +
                                            "\nmaxX: " + graphBoundsMaxX +
                                            "\nminY: " + graphBoundsMinY +
                                            "\nmaxY: " + graphBoundsMaxY +
                                            //"\nmusicCounter: " + AudioManager.instance.frameCounter + " / " + AudioManager.instance.GetFramesPerTrack() +
                                            "\nnow playing: " + AudioManager.instance.curTrackPlaying.name;
                 
                break;
            case HistoryPanelMode.ActiveSpecies:
                buttonToggleGraphMode.gameObject.SetActive(true);                
                UpdateSpeciesIconsLineageMode();
                tempPanelGraph.SetActive(true);
                textPanelStateDebug.text = curPanelMode +
                                            ", SP# " + currentSelection.candidate.speciesID +
                                            "\nC# " + currentSelection.candidate.candidateID +
                                            "\nminX: " + graphBoundsMinX +
                                            "\nmaxX: " + graphBoundsMaxX +
                                            "\nminY: " + graphBoundsMinY +
                                            "\nmaxY: " + graphBoundsMaxY +
                                            //"\nmusicCounter: " + AudioManager.instance.frameCounter + " / " + AudioManager.instance.GetFramesPerTrack() +
                                            "\nnow playing: " + AudioManager.instance.curTrackPlaying.name;
                break;
            case HistoryPanelMode.SelSpeciesPopulation:
                tempPanelSpeciesPop.SetActive(true);
                
                UpdateSpeciesIconsSinglePop();

                buttonToggleGraphMode.gameObject.SetActive(false);
                buttonSelCreatureEventsLink.gameObject.SetActive(false);
                buttonSelCreatureEventsLink.gameObject.transform.localPosition = new Vector3(400f, 180f, 0f); // sel genome button
                textPanelStateDebug.text = curPanelMode + "\n" +
                                            ", S# " + currentSelection.candidate.speciesID +                                            
                                            "\nminX: " + graphBoundsMinX +
                                            "\nmaxX: " + graphBoundsMaxX +
                                            "\nminY: " + graphBoundsMinY +
                                            "\nmaxY: " + graphBoundsMaxY +
                                            //"\nmusicCounter: " + AudioManager.instance.frameCounter + " / " + AudioManager.instance.GetFramesPerTrack() +
                                            "\nnow playing: " + AudioManager.instance.curTrackPlaying.name;
                
                break;
            case HistoryPanelMode.SelCreatureTimeline:
                tempPanelSpeciesPop.SetActive(false);
                UpdateSpeciesIconsCreatureEvents();
                //targetStartTimeStep = currentSelection.candidate.performanceData.timeStepHatched;
                buttonToggleGraphMode.gameObject.SetActive(false);
                buttonBack.gameObject.SetActive(true);
                tempPanelLifeEvents.gameObject.SetActive(true);
                buttonSelCreatureEventsLink.gameObject.SetActive(false);
                break;
        }

        if(isResourceMode) {
            //***EAC update this to be based on cursor timeline position (specific date)
            ResourceDataPoint animalData = simManager.simResourceManager.GetResourceDataAtTime(simManager.simResourceManager.simResourcesArray[6], uiManagerRef.clockPanelUI.cursorTimeStep);
            ResourceDataPoint microbeData = simManager.simResourceManager.GetResourceDataAtTime(simManager.simResourceManager.simResourcesArray[5], uiManagerRef.clockPanelUI.cursorTimeStep);
            ResourceDataPoint plantData = simManager.simResourceManager.GetResourceDataAtTime(simManager.simResourceManager.simResourcesArray[4], uiManagerRef.clockPanelUI.cursorTimeStep);
            ResourceDataPoint wasteData = simManager.simResourceManager.GetResourceDataAtTime(simManager.simResourceManager.simResourcesArray[3], uiManagerRef.clockPanelUI.cursorTimeStep);
            ResourceDataPoint decomposerData = simManager.simResourceManager.GetResourceDataAtTime(simManager.simResourceManager.simResourcesArray[2], uiManagerRef.clockPanelUI.cursorTimeStep);
            ResourceDataPoint algaeData = simManager.simResourceManager.GetResourceDataAtTime(simManager.simResourceManager.simResourcesArray[1], uiManagerRef.clockPanelUI.cursorTimeStep);
            ResourceDataPoint nutrientData = simManager.simResourceManager.GetResourceDataAtTime(simManager.simResourceManager.simResourcesArray[0], uiManagerRef.clockPanelUI.cursorTimeStep);
            if (animalData != null) {
                textPanelStateDebug.text = animalData.timestep + "RESOURCES @t" + uiManagerRef.clockPanelUI.cursorTimeStep + ":\n" +
                                            "Animals: " + animalData.value +
                                            "\nMicrobes: " + microbeData.value +
                                            "\nPlants: " + plantData.value +
                                            "\nAlgae: " + algaeData.value +
                                            "\nDecomposers: " + decomposerData.value +
                                            "\nWaste: " + wasteData.value +
                                            "\nNutrients: " + nutrientData.value;
            }
            else {
                textPanelStateDebug.text = "RESOURCES @t:\n";
            }
            
        }
        
        ClearDeadSpeciesIcons();

        foreach (var icon in speciesIcons) {
            bool isSelected = icon.speciesID == historySelectedSpeciesID;
            if (isSelected) icon.transform.SetAsLastSibling();
            icon.UpdateSpeciesIconDisplay(panelSizePixels, isSelected);
        }

        UpdateCreatureEventIcons(currentSelection.candidate);

        float lerpSpeed = 0.073f;
        graphBoundsMinX = Mathf.Lerp(graphBoundsMinX, targetGraphBoundsMinX, lerpSpeed);
        graphBoundsMaxX = Mathf.Lerp(graphBoundsMaxX, targetGraphBoundsMaxX, lerpSpeed);
        graphBoundsMinY = Mathf.Lerp(graphBoundsMinY, targetGraphBoundsMinY, lerpSpeed);
        graphBoundsMaxY = Mathf.Lerp(graphBoundsMaxY, targetGraphBoundsMaxY, lerpSpeed);
    }

    public void AddNewSpeciesToPanel(SpeciesGenomePool pool) {
        //Debug.Log("AddNewSpeciesToPanelUI: " + pool.speciesID);
        CreateSpeciesIcon(pool);
        SortSpeciesIconList();
    }
    
    private void UpdateSpeciesIconsLineageMode() {
        int numExtantSpecies = simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count;
        for (int s = 0; s < speciesIcons.Count; s++) {
            float x;
            float y = 0;
            SpeciesGenomePool pool = speciesIcons[s].linkedPool;
            float totalTicksAlive = 0;// 
            if (pool.speciesDataPointsList.Count > 0) {
                // based on lifespan rank -- but not evenly spaced, need ranked index list
                totalTicksAlive = pool.speciesDataPointsList[pool.speciesDataPointsList.Count - 1].lifespan;
            }
            if (pool.isExtinct) {
                x = ((float)pool.timeStepExtinct - graphBoundsMinX) / (Mathf.Max(1f, (float)simManager.simAgeTimeSteps) - graphBoundsMinX);
                //y = 1f - (float)s / Mathf.Max(simManager.masterGenomePool.completeSpeciesPoolsList.Count - 1, 1f);
            }
            else {
                x = 1.04f;// 0.995f + (s % 4) * 0.005f;
                //y = 1f - (float)s / Mathf.Max(numExtantSpecies - 1, 1f); // evenly spaced
                if(selectionManager.currentSelection.candidate.speciesID == pool.speciesID) {
                    x = 1.08f;
                }
            }

            y = Mathf.Clamp01((totalTicksAlive - this.graphBoundsMinY) / (this.graphBoundsMaxY - this.graphBoundsMinY));

            speciesIcons[s].SetTargetCoords(AnchorBottomLeft(x, y));
        }
    }
        
    /// Simple list, evenly spaced 
    private void UpdateSpeciesIconsDefault() {
        for (int i = 0; i < speciesIcons.Count; i++) {      
            float x = displayWidth + marginLeft;
            float y = (float)i / Mathf.Max(speciesIcons.Count - 1, 1f);
            y = y * displayHeight + marginBottom;
            speciesIcons[i].SetTargetCoords(new Vector2(x, y));
        }
    }
    
    private void UpdateSpeciesIconsSinglePop() 
    {
        PositionSpeciesIcons();
        if(simManager.simAgeTimeSteps % 3 == 0)
            PositionCreatureIcons();
    }
    
    void PositionSpeciesIcons()
    {
        int prevSpeciesIndex = historySelectedSpeciesID - 1;
        if (prevSpeciesIndex < 0) prevSpeciesIndex = masterGenomePool.completeSpeciesPoolsList.Count - 1;
        
        int nextSpeciesIndex = historySelectedSpeciesID + 1;
        if (nextSpeciesIndex >= masterGenomePool.completeSpeciesPoolsList.Count) nextSpeciesIndex = 0;    
    
        foreach (var icon in speciesIcons) 
        {            
            var coordinates = GetCoordinatesBySpeciesID(icon.linkedPool.speciesID, nextSpeciesIndex, prevSpeciesIndex);
            coordinates = AnchorBottomLeft(coordinates);
            icon.SetTargetCoords(coordinates);
        }        
    }
    
    Vector2 AnchorBottomLeft(Vector2 original) { 
        return AnchorBottomLeft(original.x, original.y); 
    }
    
    public Vector2 AnchorBottomLeft(float x, float y)
    {
        x = x * displayWidth + marginLeft;
        y = y * displayHeight + marginBottom;
        return new Vector2(x, y);
    }
    
    Vector2 GetCoordinatesBySpeciesID(int id, int nextIndex, int priorIndex)
    {
        // Cycle next species
        if (id == nextIndex)
            return new Vector2(0.05f, 0f);
        // Selected
        if (id == historySelectedSpeciesID) 
            return new Vector2(0.05f, 0.5f);
        // Cycle previous species
        if (id == priorIndex) 
            return new Vector2(0.05f, 1f);
            
        // Default
        return new Vector2(0.2f, -0.2f);
    }
    
    void PositionCreatureIcons()
    {
        SpeciesGenomePool pool = simManager.masterGenomePool.completeSpeciesPoolsList[currentSelection.candidate.speciesID];
        int numAgentsDisplayed = Mathf.Max(pool.GetNumberAgentsEvaluated(), 1); // avoid divide by 0

        for (int line = 0; line < worldTreeNumCreatureLines; line++) 
        {
            var position = Vector2.zero;
            if (line >= uiManagerRef.speciesOverviewUI.candidateGenomeButtons.Count) // pool.candidateGenomesList.Count) 
                continue;

            CandidateAgentData candidate = uiManagerRef.speciesOverviewUI.candidateGenomeButtons[line].candidateRef; // pool.candidateGenomesList[line];
            if(candidate == null) {
                continue;
            }
            
            float x = 1f;
            if (candidate.candidateID == selectionManager.currentSelection.candidate.candidateID) x = 1.1f;
            
            float frac = 0f;
            if (candidate.performanceData.bezierCurve != null) {
                frac = candidate.performanceData.bezierCurve.points[2].y;//EndPoint yPos of curve (float)line / (float)numAgentsDisplayed;
            }
            float y = (frac - graphBoundsMinY) / (graphBoundsMaxY - graphBoundsMinY);// (candidate.performanceData.creatureDataPointsList[candidate.performanceData.creatureDataPointsList.Count - 1].lifespan - graphBoundsMinY) / Mathf.Max(1,(graphBoundsMaxY - graphBoundsMinY));
            
            if (pool.isExtinct || candidate.performanceData.timeStepDied > 1) {
                float timeSinceDeath = candidate.performanceData.timeStepDied - graphBoundsMinX;
                x = timeSinceDeath / (float)(simManager.simAgeTimeSteps - graphBoundsMinX);
            }
            
            
            position = AnchorBottomLeft(x, Mathf.Clamp01(y)) * panelSizePixels;

            //***EAC FIX!
            uiManagerRef.speciesOverviewUI.SetButtonPosition(line, new Vector3(position.x, position.y, 0f));
        }        
    }
    
    /// Defaults: simple list, evenly spaced
    private void UpdateSpeciesIconsCreatureEvents() 
    {
        foreach (var icon in speciesIcons) 
            icon.SetTargetCoords(Vector2.zero);
    }
    
    /// Prevent attempts to access destroyed objects
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
        speciesLineDataCBuffer?.Release();
        creatureLineDataCBuffer?.Release();
        resourceLineDataCBuffer?.Release();
        gridLineDataCBuffer?.Release();
    }
}
