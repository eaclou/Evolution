using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryPanelUI : MonoBehaviour
{
    public Transform anchor;
    public GameObject prefabSpeciesIcon;
    //public GameObject prefabCreatureIcon;
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
    
    public static int panelSizePixels => 360;
    
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
    
    //***EAC deprecate! -> not used, safe to delete
    public struct TreeOfLifeEventLineData { 
        public int timeStepActivated;
        /// minor = 0, major = 0.5, extreme = 1.0
        public float eventCategory;  
        public float isActive;
    }
    
    public bool isAllSpeciesMode => curPanelMode == HistoryPanelMode.AllSpecies;
    bool isPopulationMode => curPanelMode == HistoryPanelMode.SpeciesPopulation;
    bool isTimelineMode => curPanelMode == HistoryPanelMode.CreatureTimeline;
    //public bool isActiveSpeciesMode => curPanelMode == HistoryPanelMode.ActiveSpecies;
    
    public bool isGraphMode;

    private bool isPanelOpen = false;

    [SerializeField]
    Animator historyPanelAnimator;
    [SerializeField]
    Button buttonOpenClose;

    // How to sync rendered geo with UI buttons???

    /*void Start() {
        uiManagerRef.OnAgentSelected += RefreshFocusedAgent;
    }
    
    void OnDestroy() {
        if (UIManager.exists) uiManagerRef.OnAgentSelected -= RefreshFocusedAgent;
    }*/
    
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
        icon.OnCreate(anchor);
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
            eventCoords.y = (1f - ( (float)candidate.candidateEventDataList[i].type / 12f)) * displayHeight + marginBottom;
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
        worldTreeLineDataCBuffer = new ComputeBuffer(worldTreeBufferCount, sizeof(float) * 7);
        
        
        // Create creature lines
        for(int line = 0; line < worldTreeNumCreatureLines; line++) {
            for (int i = 0; i < worldTreeNumPointsPerLine; i++) {
                CreateCreatureLine(line, i, cursorCoords, worldTreeLines);
            }
        }

        // Create species lines (on top)
        for(int line = 0; line < worldTreeNumSpeciesLines; line++) {
            for (int i = 0; i < worldTreeNumPointsPerLine; i++) {
                CreateSpeciesLine(line, i, cursorCoords, worldTreeLines);
            }
        }
        
        
        worldTreeLineDataCBuffer.SetData(worldTreeLines);
    }
    
    void CreateSpeciesLine(int line, int point, Vector2 cursorCoords, WorldTreeLineData[] worldTreeLines)
    {
        int index = line * worldTreeNumPointsPerLine + point;

        if (line >= simManager.masterGenomePool.completeSpeciesPoolsList.Count) 
            return;
        
        SpeciesGenomePool pool = simManager.masterGenomePool.completeSpeciesPoolsList[line];
        WorldTreeLineData data = new WorldTreeLineData();

        if (isAllSpeciesMode)
        {
            if (isGraphMode) 
            {
                int graphDataYearIndexStart = 0;
                int graphDataYearIndexEnd = 0;
            
                if (pool.avgCandidateDataYearList.Count == 0) 
                    return;
            
                float xCoord = (float)(point % worldTreeNumPointsPerLine) / (float)worldTreeNumPointsPerLine;
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
                float xEnd01 = pool.isExtinct ? 
                    (float)(pool.timeStepExtinct - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart) : 1f;

                if (pool.speciesID == selectionManager.currentSelection.historySelectedSpeciesID) {
                    hue = Vector3.one;
                    zCoord = -0.1f;
                }
            
                if (xStart01 > xCoord || xEnd01 < xCoord) {
                    hue = Vector3.zero;
                    alpha = 0f;
                }

                xCoord = xCoord * displayWidth + marginLeft;  // rescaling --> make this more robust
                yCoord = yCoord * displayHeight + marginBottom;

                if((new Vector2(xCoord, yCoord) - cursorCoords).magnitude < 0.05f) {
                    //data.color = Color.white;
                    hue = Vector3.Lerp(hue, Vector3.one, 0.8f);
                    //alpha = 1f;
                }

                data.worldPos = new Vector3(xCoord, yCoord, zCoord);
                data.color = new Color(hue.x, hue.y, hue.z, alpha);// Color.HSVToRGB(lerp, 1f - lerp, 1f); // Color.Lerp(Color.white, Color.black, lineID * 0.11215f);
            }
            else {
                // LINEAGE:
                float xCoord = (float)point / (float)worldTreeNumPointsPerLine;
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
                if(pool.speciesID == selectionManager.currentSelection.historySelectedSpeciesID) {
                    hue = Vector3.one;
                    zCoord = -0.1f;
                }                        
                data.color = new Color(hue.x, hue.y, hue.z, alpha); // Color.HSVToRGB(lerp, 1f - lerp, 1f); // Color.Lerp(Color.white, Color.black, lineID * 0.11215f);

                xCoord = xCoord * displayWidth + marginLeft;  // rescaling --> make this more robust
                yCoord = yCoord * displayHeight + marginBottom;
                if((new Vector2(xCoord, yCoord) - cursorCoords).magnitude < 0.05f) {
                    data.color = Color.white;
                }
                data.worldPos = new Vector3(xCoord, yCoord, zCoord);
            }
        }
        
        if (curPanelMode == HistoryPanelMode.SpeciesPopulation || curPanelMode == HistoryPanelMode.CreatureTimeline) {
            data.worldPos = Vector3.zero;
            data.color = new Color(0f, 0f, 0f, 0f);
        }
        
        worldTreeLines[index] = data;
    }
    
    void CreateCreatureLine(int line, int point, Vector2 cursorCoords, WorldTreeLineData[] worldTreeLines)
    {
        int index = (line + worldTreeNumSpeciesLines) * worldTreeNumPointsPerLine + point;
        SpeciesGenomePool pool = simManager.masterGenomePool.completeSpeciesPoolsList[selectionManager.currentSelection.historySelectedSpeciesID];

        if (line >= pool.candidateGenomesList.Count) 
            return;
        
        WorldTreeLineData data = new WorldTreeLineData();
        CandidateAgentData cand = pool.candidateGenomesList[line];

        float xCoord = (float)point / (float)worldTreeNumPointsPerLine;

        int numAgentsDisplayed = Mathf.Max(pool.GetNumberAgentsEvaluated(), 1); // Prevent divide by 0
        float yCoord = 1f - (float)line / (float)numAgentsDisplayed;
        
        int timeStepStart = Mathf.RoundToInt(timelineStartTimeStep);
        float xStart = (float)(pool.candidateGenomesList[line].performanceData.timeStepHatched - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
        float xEnd = 1f;
        if(pool.isExtinct || cand.performanceData.timeStepDied > 1) {
            xEnd = (float)(cand.performanceData.timeStepDied - timeStepStart) / (float)(simManager.simAgeTimeSteps - timeStepStart);
        }
        
        bool inXBounds = xStart <= xCoord && xEnd >= xCoord;
        Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary * 1.5f;
        
        data.color = GetCreatureLineColor(hue, cand, inXBounds); 
        // new Color(hue.x, hue.y, hue.z);// Color.HSVToRGB(lerp, 1f - lerp, 1f); // Color.Lerp(Color.white, Color.black, lineID * 0.11215f);
        
        xCoord = xCoord * displayWidth + marginLeft;  // rescaling --> make this more robust
        yCoord = yCoord * displayHeight + marginBottom;
                        
        data.worldPos = new Vector3(xCoord, yCoord, 0f);   
         
        // Mouse hover highlight                 
        if ((new Vector2(xCoord, yCoord) - cursorCoords).magnitude < 0.05f) { 
            data.color = Color.white;
        }

        if (isTimelineMode) {//if (isPopulationMode || isTimelineMode) {
            data.worldPos = Vector3.zero;
            data.color = new Color(0f, 0f, 0f, 0f);
        }
        
        worldTreeLines[index] = data;
    }
    
    Color GetCreatureLineColor(Vector3 hue, CandidateAgentData candidate, bool inXBounds)
    {
        Color color = new Color(hue.x, hue.y, hue.z);

        //if (xStart > xCoord || xEnd < xCoord) {
        if (!inXBounds) {
            //hue = Vector3.zero;
            color.a = 0f;
        }
        else if (candidate.candidateID == selectionManager.currentSelection.candidate.candidateID) {
            //hue = Vector3.one;
            color.r = 1f;
            color.g = 1f;
            color.b = 1f;
            color.a = 1f;
        }
        if (!candidate.isBeingEvaluated && candidate.numCompletedEvaluations == 0) {
            //hue = Vector3.zero;
            color.a = 0f;
        }   
        if (candidate.performanceData.timeStepHatched <= 1) {
            //hue = Vector3.zero;
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
        AgentGenome templateGenome = masterGenomePool.completeSpeciesPoolsList[pool.speciesID].leaderboardGenomesList[0].candidateGenome; 
        Color color = new Color(templateGenome.bodyGenome.appearanceGenome.huePrimary.x, templateGenome.bodyGenome.appearanceGenome.huePrimary.y, templateGenome.bodyGenome.appearanceGenome.huePrimary.z);
        
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
    
    public void ClickSpeciesIcon(SpeciesIconUI iconUI) 
    {
        if(iconUI.speciesID == selectionManager.currentSelection.historySelectedSpeciesID) 
        {
            Debug.Log("ClickSpeciesIcon(SpeciesIconUI iconUI) " + curPanelMode);
            if (isPopulationMode) 
            {
                // Acting as a "BACK" button!
                curPanelMode = HistoryPanelMode.AllSpecies;
            }
            else if (isAllSpeciesMode) 
            {
                // Zoom into sel species pop
                curPanelMode = HistoryPanelMode.SpeciesPopulation;
                //int indexLast = Mathf.Max(0, iconUI.linkedPool.candidateGenomesList.Count - 1);
                //if (iconUI.linkedPool.candidateGenomesList.Count == 0) return;
                
                //uiManagerRef.selectionManager.SetFocusedCandidateGenome(iconUI.linkedPool.candidateGenomesList[0]);
                //uiManagerRef.speciesOverviewUI.buttons[0].ClickedThisButton();
            }
        }
        else 
        {
            selectionManager.SetSelectedFromSpeciesUI(iconUI.speciesID);
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
        if(curPanelMode == HistoryPanelMode.SpeciesPopulation) {
            curPanelMode = HistoryPanelMode.AllSpecies;
        } 
    }
    
    public void ClickButtonModeCycle() {
        curPanelMode++;
        if((int)curPanelMode >= 4) {
            curPanelMode = 0;
        }
    }

    public void OpenClose() {
        isPanelOpen = !isPanelOpen;
        if(isPanelOpen) {
            buttonOpenClose.GetComponentInChildren<Text>().text = ">";
        }
        else {
            buttonOpenClose.GetComponentInChildren<Text>().text = "<";
        }
        historyPanelAnimator.SetBool("_IsPanelOpen", isPanelOpen);
    }
    public void MouseEnterOpenCloseButtonArea() {
        
        Animator OpenCloseButtonAnimator = buttonOpenClose.GetComponent<Animator>();
        OpenCloseButtonAnimator.SetBool("ON", true);
    }
    public void MouseExitOpenCloseButtonArea() {
        Animator OpenCloseButtonAnimator = buttonOpenClose.GetComponent<Animator>();
        OpenCloseButtonAnimator.SetBool("ON", false);
    }    
    
    public void ClickedSelectedCreatureEvents() {
        curPanelMode = HistoryPanelMode.CreatureTimeline;
    }
    
    public void Tick() 
    {
        if(Screen.height - Input.mousePosition.y < 64 && Input.mousePosition.x < 64) {            
            MouseEnterOpenCloseButtonArea();            
        }
        else {
            MouseExitOpenCloseButtonArea();
        }

        textPanelStateDebug.text = "MODE: " + selectionManager.currentSelection.historySelectedSpeciesID + selectionManager.currentSelection.candidate.speciesID;
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
                //buttonToggleExtinct.gameObject.SetActive(true);
                if (isGraphMode) {
                    UpdateSpeciesIconsGraphMode();
                    targetStartTimeStep = simManager.masterGenomePool.completeSpeciesPoolsList[simManager.masterGenomePool.currentlyActiveSpeciesIDList[0]].timeStepCreated;
                    tempPanelGraph.SetActive(true);
                }
                else {
                    UpdateSpeciesIconsLineageMode();
                }
                break;
            case HistoryPanelMode.SpeciesPopulation:
                tempPanelSpeciesPop.SetActive(true);
                UpdateSpeciesIconsSinglePop();
                targetStartTimeStep = simManager.masterGenomePool.completeSpeciesPoolsList[selectionManager.currentSelection.historySelectedSpeciesID].candidateGenomesList[0].performanceData.timeStepHatched; //***EAC better less naive way to calculate this
                //buttonToggleExtinct.gameObject.SetActive(false);
                buttonToggleGraphMode.gameObject.SetActive(false);
                buttonSelCreatureEventsLink.gameObject.SetActive(true);
                break;
            case HistoryPanelMode.CreatureTimeline:
                tempPanelSpeciesPop.SetActive(false);
                UpdateSpeciesIconsCreatureEvents();
                targetStartTimeStep = selectionManager.currentSelection.candidate.performanceData.timeStepHatched;
                //buttonToggleExtinct.gameObject.SetActive(false);
                buttonToggleGraphMode.gameObject.SetActive(false);
                buttonBack.gameObject.SetActive(true);
                tempPanelLifeEvents.gameObject.SetActive(true);
                buttonSelCreatureEventsLink.gameObject.SetActive(false);
                break;
        }

        ClearDeadSpeciesIcons();

        foreach (var icon in speciesIcons) {
            bool isSelected = icon.speciesID == selectionManager.currentSelection.historySelectedSpeciesID;
            if (isSelected) icon.transform.SetAsLastSibling();
            icon.UpdateSpeciesIconDisplay(panelSizePixels, isSelected);
        }

        UpdateCreatureEventIcons(selectionManager.currentSelection.candidate);
        
        timelineStartTimeStep = Mathf.Lerp(timelineStartTimeStep, targetStartTimeStep, 0.15f);
    }

    public void AddNewSpeciesToPanel(SpeciesGenomePool pool) {
        //Debug.Log("AddNewSpeciesToPanelUI: " + pool.speciesID);
        CreateSpeciesIcon(pool);
    }
    
    private void UpdateSpeciesIconsLineageMode() {        
        for (int s = 0; s < speciesIcons.Count; s++) {            
            float yCoord = 1f - (float)s / Mathf.Max(speciesIcons.Count - 1, 1f);  
            
            float xCoord = speciesIcons[s].linkedPool.isExtinct ? 
                (float)speciesIcons[s].linkedPool.timeStepExtinct / Mathf.Max(1f, (float)simManager.simAgeTimeSteps) : 1f;

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
        
        var bestScore = GetMostTicksAlive();
        if (bestScore == 0f) bestScore = 1f;
        SortSpeciesIcons(bestScore);
    }
    
    float GetMostTicksAlive()
    {
        float result = 0f;
        
        foreach (var icon in speciesIcons) 
        {
            var pool = icon.linkedPool;
            var ticksAlive = (float)pool.avgCandidateDataYearList[pool.avgCandidateDataYearList.Count - 1].performanceData.totalTicksAlive;

            if (ticksAlive > result)
                result = ticksAlive;
        }
        
        return result;        
    }
    
    void SortSpeciesIcons(float bestScore)
    {
        foreach (var icon in speciesIcons) 
        {
            SpeciesGenomePool pool = icon.linkedPool;
            float valStat = pool.avgCandidateDataYearList[pool.avgCandidateDataYearList.Count - 1].performanceData.totalTicksAlive;
            float xCoord = pool.isExtinct ? (float)pool.timeStepExtinct / Mathf.Max(1f, (float)simManager.simAgeTimeSteps) : 1f;
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

            int prevSpeciesIndex = selectionManager.currentSelection.historySelectedSpeciesID - 1;
            if (prevSpeciesIndex < 0) prevSpeciesIndex = masterGenomePool.completeSpeciesPoolsList.Count - 1;
            int nextSpeciesIndex = selectionManager.currentSelection.historySelectedSpeciesID + 1;
            if (nextSpeciesIndex >= masterGenomePool.completeSpeciesPoolsList.Count) nextSpeciesIndex = 0;

            // CYCLE PREV SPECIES
            if (icon.linkedPool.speciesID == prevSpeciesIndex) {  
                xCoord = 0f;
                yCoord = 1f;
            }
            // SELECTED
            if (icon.linkedPool.speciesID == selectionManager.currentSelection.historySelectedSpeciesID) {   
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
        SpeciesGenomePool pool = simManager.masterGenomePool.completeSpeciesPoolsList[selectionManager.currentSelection.historySelectedSpeciesID];
        int numAgentsDisplayed = Mathf.Max(pool.GetNumberAgentsEvaluated(), 1); // avoid divide by 0
        
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
            //float xCoord = 0f;            
            //float yCoord = 0f;   
            //icon.SetTargetCoords(new Vector2(xCoord, yCoord));
            icon.SetTargetCoords(Vector2.zero);
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
