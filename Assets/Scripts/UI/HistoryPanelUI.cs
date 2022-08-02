using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryPanelUI : MonoBehaviour
{
    public GameObject anchorGO;
    public GameObject prefabSpeciesIcon;
    //public GameObject prefabCreatureIcon;
    public GameObject prefabCreatureEventIconUI;

    public Button buttonToggleExtinct;
    public Button buttonToggleGraphMode;
    public Button buttonSelCreatureEventsLink;
    public Button buttonBack;
    
    /// Keeps track of spawned buttons
    private List<SpeciesIconUI> speciesIcons = new List<SpeciesIconUI>();  
    //private List<SpeciesIconUI> speciesIconsSorted = new List<SpeciesIconUI>(); 
    private List<CreatureEventIconUI> creatureEventIcons = new List<CreatureEventIconUI>();
    //private List<CreatureIconUI> creatureIconsList;

    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    SimulationManager simManager => SimulationManager.instance;
    MasterGenomePool masterGenomePool => simManager.masterGenomePool;
    UIManager uiManagerRef => UIManager.instance;
    SelectionManager selectionManager => SelectionManager.instance;
    SelectionData currentSelection => selectionManager.currentSelection;
    int historySelectedSpeciesID => currentSelection.historySelectedSpeciesID;

    public float timelineStartTimeStep = 0f;
    
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

    private float displayWidth => 1f - marginLeft - marginRight;
    private float displayHeight => 1f - marginTop - marginMiddle - marginBottom - clockHeight;

    public enum HistoryPanelMode {
        AllSpecies,
        SpeciesPopulation,
        CreatureTimeline
    }
    
    private HistoryPanelMode curPanelMode = HistoryPanelMode.SpeciesPopulation;

    public void SetCurPanelMode(HistoryPanelMode mode) {
        curPanelMode = mode;
    }
        
    public struct WorldTreeLineData { //species + maybe creatures?
        public Vector3 worldPos;
        public Vector4 color;
        public int speciesID;
        public int candidateID;
        public int isAlive; // wasteful, bools dont work
        public int isSelected; // wasteful, bools dont work
    }
    public struct ResourceLineData {
        public Vector3 worldPos; // timestep. value. depth
        public Vector4 color;
        //public int speciesID;
        //public int candidateID;
        //public int isAlive; // wasteful, bools dont work
        //public int isSelected; // wasteful, bools dont work
    }
    public struct GridLineData {
        public Vector3 worldPos;
        public Vector4 color;
        public float scale; // ?? what else?
    }
    
    public ComputeBuffer worldTreeLineDataCBuffer; // species+creature lines
    public ComputeBuffer resourceLineDataCBuffer;
    public ComputeBuffer gridLineDataCBuffer;
    private int worldTreeNumPointsPerLine = 128;    
    private int worldTreeNumSpeciesLines = 32;
    private int worldTreeNumCreatureLines = 32;
    private int worldTreeNumPointsPerCreatureLine = 32;
    private int worldTreeNumGridLines = 128;
    private int worldTreeNumPointsPerGridLine = 4;
    private int worldTreeNumResourceLines = 8;
    private int worldTreeNumPointsPerResourceLine = 128;
    private int worldTreeBufferCount => worldTreeNumPointsPerLine * (worldTreeNumSpeciesLines + worldTreeNumCreatureLines);
    private int gridLineBufferCount => worldTreeNumGridLines * worldTreeNumPointsPerGridLine;
    private int resourceLineBufferCount => worldTreeNumResourceLines * worldTreeNumPointsPerResourceLine;
    private int sizeOfWorldLineDataStruct => sizeof(float) * 7 + sizeof(int) * 4;
    private int sizeOfResourceLineDataStruct => sizeof(float) * 7;
    private int sizeOfGridLineDataStruct => sizeof(float) * 8;
    public bool isAllSpeciesMode => curPanelMode == HistoryPanelMode.AllSpecies;
    bool isPopulationMode => curPanelMode == HistoryPanelMode.SpeciesPopulation;
    bool isTimelineMode => curPanelMode == HistoryPanelMode.CreatureTimeline;
    
    public bool isGraphMode;

    bool isPanelOpen => openCloseButton.isOpen;

    [SerializeField]
    OpenCloseButton openCloseButton;
    [SerializeField]
    TooltipUI tooltipOpenCloseButton;

    private bool isNudgeOn = false;
    [ReadOnly]
    public int nudgeCounter;

    public float minScoreValue = 500f;
    public float maxScoreValue = 1000f;
    private float minTimelineTargetValue;// => 
    private float maxTimelineTargetValue;
    public float minTimelineValue;// => Mathf.Max(0f, simManager.simAgeTimeSteps - (maxScoreValue - minScoreValue) * 31.4f);
    public float maxTimelineValue;// => simManager.simAgeTimeSteps;
    
    public void Start() {
        openCloseButton.SetHighlight(true);
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
            if (curPanelMode != HistoryPanelMode.CreatureTimeline) {
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
                case HistoryPanelMode.AllSpecies: SetCurPanelMode(HistoryPanelMode.SpeciesPopulation); break;
                case HistoryPanelMode.SpeciesPopulation: SetCurPanelMode(HistoryPanelMode.CreatureTimeline); break;
            }
        }
        // different species
        else 
        {
            switch (curPanelMode)
            {
                case HistoryPanelMode.AllSpecies: SetCurPanelMode(HistoryPanelMode.SpeciesPopulation); break;
                case HistoryPanelMode.SpeciesPopulation: break;
                case HistoryPanelMode.CreatureTimeline: SetCurPanelMode(HistoryPanelMode.SpeciesPopulation); break;
            }
        }
    }
    
    // * Rename to suggest this is a per-frame update, break out the initialization logic
    // * APPLICATION BOTTLENECK: focus optimization efforts here!
        // Since this is for visuals, could call on a fixed interval, e.g. 1/5 second
    public void InitializeRenderBuffers() 
    {
        // * For testing
        var cursorCoords = theCursorCzar.GetScaledCursorCoords(360f, -720f);

        // Initialize world tree line data
        var worldTreeLines = new WorldTreeLineData[worldTreeBufferCount];
        worldTreeLineDataCBuffer?.Release();
        worldTreeLineDataCBuffer = new ComputeBuffer(worldTreeBufferCount, sizeOfWorldLineDataStruct);
        
        // Create species lines
        for (int line = 0; line < worldTreeNumSpeciesLines; line++) {
            for (int i = 0; i < worldTreeNumPointsPerLine; i++) {
                CreateSpeciesLine(line, i, cursorCoords, worldTreeLines);
            }
        }
        
        // Create creature lines
        for (int line = 0; line < worldTreeNumCreatureLines; line++) {
            for (int i = 0; i < worldTreeNumPointsPerLine; i++) {
                CreateCreatureLine(line, i, cursorCoords, worldTreeLines);
            }
        }
        var gridLines = new GridLineData[gridLineBufferCount];
        gridLineDataCBuffer?.Release();
        gridLineDataCBuffer = new ComputeBuffer(gridLineBufferCount, sizeOfGridLineDataStruct);
        // Create Gridlines
        for (int line = 0; line < worldTreeNumGridLines; line++) {
            for (int i = 0; i < worldTreeNumPointsPerGridLine; i++) {
                CreateGridLine(line, i, cursorCoords, gridLines);
            }
        }
        // Create world resource stat lines
        var resourceLines = new ResourceLineData[resourceLineBufferCount];
        resourceLineDataCBuffer?.Release();
        resourceLineDataCBuffer = new ComputeBuffer(resourceLineBufferCount, sizeOfResourceLineDataStruct);
        
        for (int i = 0; i < worldTreeNumResourceLines; i++) {
            if(i >= simManager.simResourceManager.simResourcesArray.Length) {
                continue;
            }
            for(int j = 0; j < worldTreeNumPointsPerResourceLine; j++) {
                CreateResourceLine(i, j, cursorCoords, resourceLines, simManager.simResourceManager.simResourcesArray[i]);
            }
        }

        gridLineDataCBuffer.SetData(gridLines);
        resourceLineDataCBuffer.SetData(resourceLines);
        worldTreeLineDataCBuffer.SetData(worldTreeLines);

        uiManagerRef.clockPanelUI.UpdateResourceStats();
    }
    void CreateGridLine(int line, int pIndex, Vector2 cursorCoords, GridLineData[] gridLines) {
         int index = (line) * worldTreeNumPointsPerGridLine + pIndex;
        
        if (line >= worldTreeNumGridLines) 
            return;
        //current bounds of graph: minValue/MaxValue, minTimestep/curTime
        //figure out what kind of line this needs to be based on line index
        GridLineData data = new GridLineData();        
        float frac = (float)pIndex / (float)worldTreeNumPointsPerGridLine;
        float x;
        float y;
        if(line % 2 == 0) { //horizontal
            x = frac * simManager.simAgeTimeSteps * 1.5f;
            y = line * 200f;
        }
        else { // vertical
            x = line * 4000f;
            y = frac * maxScoreValue * 1.5f;
        }
        
        data.color = new Vector4(0.35f, 0.35f, 0.35f, 1f);
        
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
        if (isPopulationMode || isTimelineMode) {
            //data.worldPos = Vector3.zero;
            data.color = resource.GetColor() * 0.55f; // new Color(0f, 0f, 0f, 0f);
        }
        
        if (pIndex == 0) data.color.w = 0f;
        var coordinates = AnchorBottomLeft(x, y);
        data.worldPos = new Vector3(coordinates.x, coordinates.y, 1f);   
        
        resourceLines[index] = data;
    }
    void CreateSpeciesLine(int line, int point, Vector2 cursorCoords, WorldTreeLineData[] worldTreeLines)
    {
        int index = line * worldTreeNumPointsPerLine + point;

        if (line >= simManager.masterGenomePool.completeSpeciesPoolsList.Count) 
            return;
        
        SpeciesGenomePool pool = simManager.masterGenomePool.completeSpeciesPoolsList[line];
        WorldTreeLineData data = new WorldTreeLineData();
                    
        if (pool.speciesDataPointsList.Count == 0 || point >= pool.speciesDataPointsList.Count) 
            return;

        float x = pool.speciesDataPointsList[point].timestep; // (float)pool.speciesDataPointsList[point].timestep / (float)simManager.simAgeTimeSteps;
        //float val = (float)(pool.speciesDataPointsList[point].lifespan - minScoreValue) / (maxScoreValue - minScoreValue);
        float y = pool.speciesDataPointsList[point].lifespan; // Mathf.Lerp(valStart, valEnd, frac); // Mathf.Sin(xCoord / orbitalPeriod * (simManager.simAgeTimeSteps) * animTimeScale) * 0.075f * (float)lineID + 0.5f;
        float z = 0f;
            
        Vector3 hue = pool.foundingCandidate.primaryHue;
        hue.x = Mathf.Clamp01(hue.x + 0.35f);
        hue.y = Mathf.Clamp01(hue.y + 0.35f);
        hue.z = Mathf.Clamp01(hue.z + 0.35f);
        float alpha = 1f;
        int timeStepStart = Mathf.RoundToInt(timelineStartTimeStep);
         
        data.isSelected = 0;
        if (pool.speciesID == historySelectedSpeciesID) {
            //hue = Vector3.one;
            //z = -0.1f;
            data.isSelected = 1;
        }
        data.isAlive = 1;
        if (pool.isExtinct) {
            data.isAlive = 0;
        }
        else {
            //if(pool.isFlaggedForExtinction) {
                //data.isEndangered = 1;
            //}
        }
        if (point == 0) alpha = 0f;
        //if (!isGraphMode) { // || isTimelineMode) { //if (isPopulationMode || isTimelineMode) {
            //data.worldPos = Vector3.zero;
           // data.color *= 0.55f; // new Color(0f, 0f, 0f, 0f);
        //}
        var coordinates = AnchorBottomLeft(x, y); // new Vector2(x, y); // 

        data.worldPos = new Vector3(coordinates.x, coordinates.y, z);
        data.color = new Color(hue.x, hue.y, hue.z, alpha);// Color.HSVToRGB(lerp, 1f - lerp, 1f); // Color.Lerp(Color.white, Color.black, lineID * 0.11215f);
        
        worldTreeLines[index] = data;
        /*else 
        {
            // LINEAGE:
            float x = (float)point / (float)worldTreeNumPointsPerLine;
            float y = 1f - ((float)pool.speciesID / (float)Mathf.Max(simManager.masterGenomePool.completeSpeciesPoolsList.Count - 1, 1)); // Mathf.Sin(xCoord / orbitalPeriod * (simManager.simAgeTimeSteps) * animTimeScale) * 0.075f * (float)lineID + 0.5f;
            float z = 0f;
                                
            //Vector3 hue = pool.foundingCandidate.primaryHue;
            Vector3 hue = pool.foundingCandidate.primaryHue;
            hue.x = Mathf.Clamp01(hue.x + 0.45f);
            hue.y = Mathf.Clamp01(hue.y + 0.45f);
            hue.z = Mathf.Clamp01(hue.z + 0.45f);
            float alpha = 1f;
            int timeStepStart = Mathf.RoundToInt(timelineStartTimeStep);

            float xStart01 = (float)(pool.timeStepCreated - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
            float xEnd01 = 1f;
            if (pool.isExtinct) {
                xEnd01 = (float)(pool.timeStepExtinct - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
            }
                        
            if (pool.speciesID == historySelectedSpeciesID) {
                hue = Vector3.one;
                z = -0.1f;
            }                        
            // Color.HSVToRGB(lerp, 1f - lerp, 1f); // Color.Lerp(Color.white, Color.black, lineID * 0.11215f);
            
            var coordinates = AnchorBottomLeft(x, y);
            if ((coordinates - cursorCoords).magnitude < 0.05f) {
                hue = Vector3.one;
                alpha = 1f;
            }
            if(curPanelMode != HistoryPanelMode.AllSpecies) {
                alpha = 0f;
            }
            data.color = new Color(hue.x, hue.y, hue.z, alpha); 
            data.worldPos = new Vector3(coordinates.x, coordinates.y, z);
        }
        */
        
    }
    
    void CreateCreatureLine(int line, int point, Vector2 cursorCoords, WorldTreeLineData[] worldTreeLines)
    {
        int index = (line + worldTreeNumSpeciesLines) * worldTreeNumPointsPerLine + point;
        SpeciesGenomePool pool = simManager.masterGenomePool.completeSpeciesPoolsList[historySelectedSpeciesID];

        if (line >= pool.candidateGenomesList.Count) 
            return;
        
        WorldTreeLineData data = new WorldTreeLineData();
        CandidateAgentData cand = pool.candidateGenomesList[line];

        float startTimestep = cand.performanceData.timeStepHatched;
        float endTimeStep = cand.performanceData.timeStepDied;
        if (cand.isBeingEvaluated && cand.numCompletedEvaluations == 0) {
            endTimeStep = simManager.simAgeTimeSteps;
        }
        float timeRange = endTimeStep - startTimestep;
        float frac = ((float)point / (float)worldTreeNumPointsPerLine);
        float x = Mathf.Lerp(startTimestep, endTimeStep, frac);

        int numAgentsDisplayed = Mathf.Max(pool.GetNumberAgentsEvaluated(), 1); // Prevent divide by 0
        float fraction = 1f - (float)line / numAgentsDisplayed;
        float y = fraction * (maxScoreValue - minScoreValue) + minScoreValue;
        
        
        Vector3 hue = pool.foundingCandidate.primaryHue;
        hue.x = Mathf.Clamp01(hue.x + 0.45f);
        hue.y = Mathf.Clamp01(hue.y + 0.45f);
        hue.z = Mathf.Clamp01(hue.z + 0.45f);
        
        data.color = new Color(hue.x, hue.y, hue.z);// GetCreatureLineColor(hue, cand, inXBounds); 
        // new Color(hue.x, hue.y, hue.z);// Color.HSVToRGB(lerp, 1f - lerp, 1f); // Color.Lerp(Color.white, Color.black, lineID * 0.11215f);
        
        var coordinates = AnchorBottomLeft(x, y);
        data.worldPos = new Vector3(coordinates.x, coordinates.y, 1f);   
        
        if (!cand.isBeingEvaluated && cand.numCompletedEvaluations == 0 ||
            cand.performanceData.timeStepHatched <= 1 ||
            point == 0) {
            
            data.color = new Color(0f, 0f, 0f, 0f);
        }
        if (cand.candidateID == selectionManager.currentSelection.candidate.candidateID) {
            data.isSelected = 1;
        }
        worldTreeLines[index] = data;
        // Mouse hover highlight                 
        //if ((coordinates - cursorCoords).magnitude < 0.05f) { 
        //    data.color = Color.white;
        //}

        
        
        
    }
    
    Color GetCreatureLineColor(Vector3 hue, CandidateAgentData candidate, bool inXBounds)
    {
        Color color = new Color(hue.x, hue.y, hue.z);

        if (!inXBounds) {
            color.a = 0f;
        }
        else if (candidate.candidateID == currentSelection.candidate.candidateID) {
            color.r = 1f;
            color.g = 1f;
            color.b = 1f;
            color.a = 1f;
        }
        if (!candidate.isBeingEvaluated && candidate.numCompletedEvaluations == 0 ||
            candidate.performanceData.timeStepHatched <= 1 ||
            curPanelMode == HistoryPanelMode.AllSpecies) {
            color.a = 0f;
        }
        if (candidate.performanceData.totalTicksAlive >= 1) {
            color.r *= 0.35f;     
            color.g *= 0.35f; 
            color.b *= 0.35f; 
        }

        return color;
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
                curPanelMode = HistoryPanelMode.AllSpecies;
            }
            else if (isAllSpeciesMode) 
            {
                // Zoom into sel species pop
                curPanelMode = HistoryPanelMode.SpeciesPopulation;
            }
        }

        selectionManager.SetSelectedFromSpeciesUI(iconUI.speciesID);
        uiManagerRef.speciesOverviewUI.RebuildGenomeButtons();
        uiManagerRef.ExitTooltipObject();

    }
    
    public void ClickButtonToggleGraphMode() {
        isGraphMode = !isGraphMode;


    }
    
    public void ClickButtonToggleExtinct() { }
    
    public void ClickButtonBack() {
        if (curPanelMode == HistoryPanelMode.SpeciesPopulation) {
            curPanelMode = HistoryPanelMode.AllSpecies;
        } 
        if (curPanelMode == HistoryPanelMode.CreatureTimeline) {
            curPanelMode = HistoryPanelMode.SpeciesPopulation;
        }        
    }
    
    public void ClickButtonModeCycle() {
        curPanelMode++;
        if ((int)curPanelMode >= 4) {
            curPanelMode = 0;
        }
    }
    
    public void ClickedSelectedCreatureEvents() {
        curPanelMode = HistoryPanelMode.CreatureTimeline;
    }
    
    public void Tick() 
    {
        if(isNudgeOn) {
            nudgeCounter++;
            if(nudgeCounter >= 520) {
                CloseNudgeMessage();                
            }
        }

        var mouseInOpenCloseArea = Screen.height - Input.mousePosition.y < 64 && Input.mousePosition.x < 64;
        openCloseButton.SetMouseEnter(mouseInOpenCloseArea);
        tooltipOpenCloseButton.tooltipString = isPanelOpen ? "Hide Timeline Panel" : "Open Timeline Panel";

        textPanelStateDebug.text = "min: (" + minTimelineValue.ToString("F0") + ", " + minScoreValue.ToString("F0") + "), max: (" + maxTimelineValue.ToString("F0") + ", " + maxScoreValue.ToString("F0") + ")";
        buttonToggleExtinct.gameObject.SetActive(false);
        float targetStartTimeStep = 0f;
        tempPanelSpeciesPop.SetActive(false);
        tempPanelGraph.SetActive(false);
        buttonBack.gameObject.SetActive(false);
        buttonSelCreatureEventsLink.gameObject.SetActive(false);
        tempPanelLifeEvents.gameObject.SetActive(false);


        
        switch (curPanelMode)
        {
            case HistoryPanelMode.AllSpecies:
                buttonToggleGraphMode.gameObject.SetActive(true);
                //graphBoundsY = minScoreValue;
                //graphBoundsYEnd = maxScoreValue;
                UpdateSpeciesIconsLineageMode();
                
                
                //UpdateSpeciesIconsGraphMode();
                targetStartTimeStep = simManager.masterGenomePool.completeSpeciesPoolsList[simManager.masterGenomePool.currentlyActiveSpeciesIDList[0]].timeStepCreated;
                tempPanelGraph.SetActive(true);
                
                break;
            case HistoryPanelMode.SpeciesPopulation:
                tempPanelSpeciesPop.SetActive(true);
                targetStartTimeStep = simManager.masterGenomePool.completeSpeciesPoolsList[historySelectedSpeciesID].candidateGenomesList[0].performanceData.timeStepHatched; //***EAC better less naive way to calculate this
                //graphBoundsY = simManager.masterGenomePool.completeSpeciesPoolsList[historySelectedSpeciesID].candidateGenomesList[0].performanceData.totalTicksAlive - 5;
                //graphBoundsYEnd = simManager.masterGenomePool.completeSpeciesPoolsList[historySelectedSpeciesID].candidateGenomesList[0].performanceData.totalTicksAlive + 5;
                
                UpdateSpeciesIconsSinglePop();

                buttonToggleGraphMode.gameObject.SetActive(false);
                buttonSelCreatureEventsLink.gameObject.SetActive(true);
                buttonSelCreatureEventsLink.gameObject.transform.localPosition = new Vector3(360f, 180f, 0f);
                break;
            case HistoryPanelMode.CreatureTimeline:
                tempPanelSpeciesPop.SetActive(false);
                UpdateSpeciesIconsCreatureEvents();
                targetStartTimeStep = currentSelection.candidate.performanceData.timeStepHatched;
                buttonToggleGraphMode.gameObject.SetActive(false);
                buttonBack.gameObject.SetActive(true);
                tempPanelLifeEvents.gameObject.SetActive(true);
                buttonSelCreatureEventsLink.gameObject.SetActive(false);
                break;
        }
        
        ClearDeadSpeciesIcons();

        foreach (var icon in speciesIcons) {
            bool isSelected = icon.speciesID == historySelectedSpeciesID;
            if (isSelected) icon.transform.SetAsLastSibling();
            icon.UpdateSpeciesIconDisplay(panelSizePixels, isSelected);
        }

        UpdateCreatureEventIcons(currentSelection.candidate);
        
        timelineStartTimeStep = Mathf.Lerp(timelineStartTimeStep, targetStartTimeStep, 0.15f);
        //graphBoundsX = timelineStartTimeStep;
        //graphBoundsXEnd = simManager.simAgeTimeSteps;
                
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
                x = ((float)pool.timeStepExtinct - minTimelineValue) / (Mathf.Max(1f, (float)simManager.simAgeTimeSteps) - minTimelineValue);
                //y = 1f - (float)s / Mathf.Max(simManager.masterGenomePool.completeSpeciesPoolsList.Count - 1, 1f);
            }
            else {
                x = 1.04f;// 0.995f + (s % 4) * 0.005f;
                //y = 1f - (float)s / Mathf.Max(numExtantSpecies - 1, 1f); // evenly spaced
                if(selectionManager.currentSelection.candidate.speciesID == pool.speciesID) {
                    x = 1.08f;
                }
            }

            //float test = y;
            //test = test / maxScoreValue;
            y = Mathf.Clamp01((totalTicksAlive - this.minScoreValue) / (this.maxScoreValue - this.minScoreValue));

            speciesIcons[s].SetTargetCoords(AnchorBottomLeft(x, y));
        }
    }
    
    private void UpdateSpeciesIconsGraphMode() {
        if (speciesIcons[0].linkedPool.speciesDataPointsList.Count < 1) {
            UpdateSpeciesIconsDefault();
            return;
        }

        var bestScore = uiManagerRef.speciesGraphPanelUI.maxValuesStatArray[0]; // GetMostTicksAlive();
        if (bestScore == 0f) bestScore = 1f;
        //SortSpeciesIcons(bestScore);
    }
    
    
    void SortSpeciesIcons(float bestScore)
    {
        foreach (var icon in speciesIcons) 
        {
            SpeciesGenomePool pool = icon.linkedPool;
            float totalTicksAlive = 0f;
            if(pool.speciesDataPointsList.Count > 0) {
                totalTicksAlive = pool.speciesDataPointsList[pool.speciesDataPointsList.Count - 1].lifespan;
            }
            float x = pool.isExtinct ? (float)pool.timeStepExtinct / Mathf.Max(1f, (float)simManager.simAgeTimeSteps) : 1f;
            float y = Mathf.Clamp01(totalTicksAlive / bestScore);
            icon.SetTargetCoords(AnchorBottomLeft(x, y));
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
            //icon.SetTargetCoords(new Vector2(x, y));
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
            return Vector2.zero;
        // Selected
        if (id == historySelectedSpeciesID) 
            return new Vector2(0, 0.5f);
        // Cycle previous species
        if (id == priorIndex) 
            return new Vector2(0f, 1f);
            
        // Default
        return new Vector2(-.2f, .2f);
    }
    
    void PositionCreatureIcons()
    {
        SpeciesGenomePool pool = simManager.masterGenomePool.completeSpeciesPoolsList[historySelectedSpeciesID];
        int numAgentsDisplayed = Mathf.Max(pool.GetNumberAgentsEvaluated(), 1); // avoid divide by 0
        Vector3 hue = pool.foundingCandidate.primaryHue * 2f;
        int timeStepStart = Mathf.RoundToInt(timelineStartTimeStep);

        for (int line = 0; line < worldTreeNumCreatureLines; line++) 
        {
            if (line >= pool.candidateGenomesList.Count) 
                continue;
                            
            CandidateAgentData candidate = pool.candidateGenomesList[line];

            float x = 1f;                
            float y = 1f - (float)line / (float)numAgentsDisplayed;
            
            if (pool.isExtinct || candidate.performanceData.timeStepDied > 1) {
                float timeSinceDeath = candidate.performanceData.timeStepDied - timeStepStart;
                x = timeSinceDeath / (float)(simManager.simAgeTimeSteps - timeStepStart);
            }
            
            var position = AnchorBottomLeft(x, y) * panelSizePixels;

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
        worldTreeLineDataCBuffer?.Release();
        resourceLineDataCBuffer?.Release();
        gridLineDataCBuffer?.Release();
    }
}
