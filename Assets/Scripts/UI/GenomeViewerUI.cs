using System;
using UnityEngine;
using UnityEngine.UI;

public class GenomeViewerUI : MonoBehaviour {
    SimulationManager simulationManager => SimulationManager.instance;
    UIManager uiManagerRef => UIManager.instance;

    public Text textFocusedCandidate;
    public Text textGenomeOverviewA;
    public Text textGenomeOverviewB;
    public Text textGenomeOverviewC;

    public GameObject panelGenomeSensors;
    
    [SerializeField] Sensor plantFoodSensor;
    [SerializeField] Sensor eggsFoodSensor;
    [SerializeField] Sensor meatFoodSensor;
    [SerializeField] Sensor corpseFoodSensor;
    [SerializeField] Sensor microbeFoodSensor;
    [SerializeField] Sensor friendSensor;
    [SerializeField] Sensor foeSensor;
    [SerializeField] Sensor wallSensor;
    [SerializeField] Sensor waterSensor;
    [SerializeField] Sensor commSensor;
    
    [SerializeField] Tab genomeTab;
    [SerializeField] Tab historyTab;
    [SerializeField] Tab performanceTab;
    
    // WPP 5/6/21: replaced with Sensor    
    //public Image imageSensorFoodPlant;
    //public Image imageSensorFoodMicrobe;
    //public Image imageSensorFoodEggs;
    //public Image imageSensorFoodMeat;
    //public Image imageSensorFoodCorpse;
    //public Image imageSensorFriend;
    //public Image imageSensorFoe;
    //public Image imageSensorWalls;
    //public Image imageSensorWater;
    //public Image imageSensorComms;
    
    public Image imageSensorInternals;
    public Image imageSensorContact;

    public Image imagePortraitTitleBG;

    public GameObject panelGenomeAbilities;
    
    // WPP: variables not used
    //public Image imageAbilityFeed;
    //public Image imageAbilityAttack;
    //public Image imageAbilityDefend;
    //public Image imageAbilityDash;
    //public Image imageAbilityRest;

    //public GameObject panelGenomeSpecializations;
    //public GameObject panelGenomeDigestion;
    
    //public bool isPerformancePanelON;
    //public bool isSpecializationPanelOn;    
    //public bool isBrainPanelOn;
    //public Toggle toggleAutofollow;

    public Image imageSpecAttack;
    public Image imageSpecDefense;
    public Image imageSpecSpeed;
    public Image imageSpecEnergy;
    public Text textSpecAttack;
    public Text textSpecDefense;
    public Text textSpecSpeed;
    public Text textSpecEnergy;

    public Image imageDigestPlant;
    public Image imageDigestMeat;
    public Image imageDigestDecay;    
    public Text textDigestPlant;
    public Text textDigestMeat;
    public Text textDigestDecay;

    public GameObject panelPerformanceBehavior;
    public Image imageBehaviorFeed;
    public Image imageBehaviorAttack;
    public Image imageBehaviorDefend;
    public Image imageBehaviorDash;
    public Image imageBehaviorRest;
    public Text textBehaviorFeed;
    public Text textBehaviorAttack;
    public Text textBehaviorDefend;
    public Text textBehaviorDash;
    public Text textBehaviorRest;

    public GameObject panelEaten;
    public Image imageEatenPlants;
    public Image imageEatenMicrobes;
    public Image imageEatenAnimals;
    public Image imageEatenEggs;
    public Image imageEatenCorpse;
    public Text textEatenPlants;
    public Text textEatenMicrobes;
    public Text textEatenAnimals;
    public Text textEatenEggs;
    public Text textEatenCorpse;

    // WPP 5/6/21: moved to nested Tab class
    // LOST REFERENCES
    //public GameObject panelGenomeTab;
    //public GameObject panelPerformanceTab;
    //public GameObject panelHistoryTab;    
    
    //public Button buttonGenomeTab;
    //public Button buttonPerformanceTab;
    //public Button buttonHistoryTab;
    // Real-Time panel handled in center bottom with creature portrait and brain info
    
    public bool isGenomeTabActive = true;
    public bool isPerformanceTabActive = false;
    public bool isHistoryTabActive = false;

    public GameObject imageDeadDim;

    public bool isTooltipHover = true;
    public string tooltipString;

    private Texture2D brainGenomeTex; //Barcode
    public Material brainGenomeMat;

    void Start () {
		isTooltipHover = false;
        brainGenomeTex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        brainGenomeTex.filterMode = FilterMode.Point;
        brainGenomeTex.wrapMode = TextureWrapMode.Clamp;
        brainGenomeMat.SetTexture("_MainTex", brainGenomeTex);
	}
	
	#region Cached Agent References
	
    // WPP 5/6/21: added for quick access without GC
    CandidateAgentData candidate;
    CandidateAgentData.PerformanceData performanceData;
    AgentGenome candidateGenome;
    BrainGenome brainGenome;
    BodyGenome bodyGenome;
    CritterModuleCoreGenome coreGenome;
    CritterModuleAppearanceGenome appearanceGenome;
    CritterModuleFoodSensorsGenome food;
    CritterModuleFriendSensorsGenome friend;
    CritterModuleThreatSensorsGenome threat;
    CritterModuleEnvironmentSensorsGenome environment;
    CritterModuleCommunicationGenome communication;
	
    void CacheAgentReferences(CandidateAgentData agent)
    {
        candidate = agent;
        performanceData = agent.performanceData;
        CacheAgentReferences(agent.candidateGenome);
    }
    
    void CacheAgentReferences(AgentGenome agent)
    {
        candidateGenome = agent;
        brainGenome = candidateGenome.brainGenome;
        bodyGenome = candidateGenome.bodyGenome;
        coreGenome = bodyGenome.coreGenome;
        appearanceGenome = bodyGenome.appearanceGenome;
        
        food = bodyGenome.foodGenome;
        friend = bodyGenome.friendGenome;
        threat = bodyGenome.threatGenome;
        environment = bodyGenome.environmentalGenome;
        communication = bodyGenome.communicationGenome;        
    }
    
    #endregion
	
	// WPP 5/6/21: applied early exit, broke out SetTitleString function
    public void UpdateUI(SpeciesGenomePool pool, CandidateAgentData _candidate) {
        if (_candidate == null || _candidate.candidateGenome == null)
            return;
            
        CacheAgentReferences(_candidate);
            
        SetTitleString();

        Vector3 hue = Vector3.one * 0.75f;
        Vector3 hueB = Vector3.one * 0.25f;
        /*
        //CandidateAgentData avgCandidate;
        if(pool.avgCandidateDataYearList.Count > 1) {
            int yearIndex = Mathf.RoundToInt(Mathf.Lerp(0f, (float)pool.avgCandidateDataYearList.Count - 1f, uiManagerRef.speciesOverviewUI.sliderLineageGenomes.value));
            candidate = pool.avgCandidateDataYearList[yearIndex];
        }
        else {
            candidate = pool.avgCandidateData;
        }
        */
        
        hue = appearanceGenome.huePrimary;
        hueB = appearanceGenome.hueSecondary;
        
        //Vector3 hue = pool.avgCandidateData.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
        imagePortraitTitleBG.color = new Color(hue.x, hue.y, hue.z);
        textFocusedCandidate.color = new Color(hueB.x, hueB.y, hueB.z);
        //+ ", " + candidate.numCompletedEvaluations.ToString() + ", " + candidate.speciesID.ToString() + ", " + candidate.isBeingEvaluated.ToString() + ", " + candidate.candidateGenome.bodyGenome.coreGenome.name;

        panelPerformanceBehavior.SetActive(true);
        panelEaten.SetActive(true);
        //panelGenomeSpecializations.SetActive(false);
        //panelGenomeDigestion.SetActive(false);
        panelGenomeAbilities.SetActive(false);
        panelGenomeSensors.SetActive(true);

        // WPP 5/6/21: delegated to Tab nested class
        genomeTab.SetActive(isGenomeTabActive);
        performanceTab.SetActive(isPerformanceTabActive);
        historyTab.SetActive(isHistoryTabActive);
        //panelGenomeTab.SetActive(isGenomeTabActive);
        //buttonGenomeTab.GetComponentInChildren<Image>().color = isGenomeTabActive ? Color.white : Color.gray;
        //panelPerformanceTab.SetActive(isPerformanceTabActive);
        //buttonPerformanceTab.GetComponentInChildren<Image>().color = isPerformanceTabActive ? Color.white : Color.gray;
        //panelHistoryTab.SetActive(isHistoryTabActive);
        //buttonHistoryTab.GetComponentInChildren<Image>().color = isHistoryTabActive ? Color.white : Color.gray;
        
        //panel
        imageDeadDim.gameObject.SetActive(false);
        float lifespan = performanceData.totalTicksAlive;
        textGenomeOverviewA.text = "Lifespan: " + (lifespan * 0.1f).ToString("F0") + ", Gen: " + candidateGenome.generationCount;

        if(candidate.isBeingEvaluated) {
            lifespan = simulationManager.agentInFocus.ageCounter;
            textGenomeOverviewA.text = "Age: " + (lifespan * 0.1f).ToString("F0") + ", Gen: " + candidateGenome.generationCount;
        }
        if(simulationManager.agentInFocusIsDead) {
            imageDeadDim.gameObject.SetActive(true);
        }

        textGenomeOverviewB.text = "Size: " + (100f * coreGenome.creatureBaseLength).ToString("F0") + ", Aspect 1:" + (1f / coreGenome.creatureAspectRatio).ToString("F0");
        textGenomeOverviewC.text = "Brain Size: " + brainGenome.bodyNeuronList.Count + "--" + brainGenome.linkList.Count;
                  
        UpdateDigestSpecUI(coreGenome);
        UpdateSpecializationsUI(coreGenome);

        UpdatePerformanceBehaviors(pool, performanceData); // ******
        UpdateSensorsUI();      
    }
    
    // * WPP: move to separate component
    void SetTitleString()
    {
        string titleString = "<size=18>Critter</size> " + candidate.candidateID + "<size=18>";
        
        if(candidate.isBeingEvaluated) 
        {
            titleString += "\n(following)";
        }
        else 
        {
            if(candidate.numCompletedEvaluations > 0) 
            {
                titleString += "\nFossil";
            }
            else 
            {
                titleString += "\nUnborn";
            }
        }
        
        titleString += "</size>";
        textFocusedCandidate.text = titleString;
    }
    
    public void CreateBrainGenomeTexture(AgentGenome _candidate)
    {
        CacheAgentReferences(_candidate);
        CreateBrainGenomeTexture();
    }
    
    public void CreateBrainGenomeTexture() {
        int width = 256;
        brainGenomeTex.Resize(width, 1); // pool.leaderboardGenomesList.Count);

        for(int x = 0; x < width; x++) {
            int xIndex = x;// i % brainGenomeTex.width;
            int yIndex = 0; // i; // Mathf.FloorToInt(i / brainGenomeTex.width);
                              
            Color testColor;

            if (brainGenome.linkList.Count > x) 
            {
                float weightVal = brainGenome.linkList[x].weight;
                testColor = new Color(weightVal * 0.5f + 0.5f, weightVal * 0.5f + 0.5f, weightVal * 0.5f + 0.5f);
                
                if(weightVal < -0.25f) 
                {
                    testColor = Color.Lerp(testColor, Color.black, 0.15f);
                }
                else if(weightVal > 0.25f) 
                {
                    testColor = Color.Lerp(testColor, Color.white, 0.15f);
                }
                else 
                {
                    testColor = Color.Lerp(testColor, Color.gray, 0.15f);
                }
            }
            else 
            {
                testColor = Color.black; // CLEAR
            }
                
            brainGenomeTex.SetPixel(xIndex, yIndex, testColor);
        }
        
        brainGenomeTex.Apply();
    }    
    
    // WPP 5/6/21: Replaced repeating logic with Sensor nested class
    // * WPP: Move to separate component
    private void UpdateSensorsUI() { 
        if (bodyGenome.foodGenome == null) return;
        
        plantFoodSensor.SetSensorEnabled(food.useNutrients || food.useStats);
        microbeFoodSensor.SetSensorEnabled(food.usePos);
        eggsFoodSensor.SetSensorEnabled(food.useEggs);
        meatFoodSensor.SetSensorEnabled(food.useVel);
        corpseFoodSensor.SetSensorEnabled(food.useCorpse);
        friendSensor.SetSensorEnabled(friend.usePos || friend.useVel || friend.useDir);
        foeSensor.SetSensorEnabled(threat.usePos || threat.useVel || threat.useDir);
        waterSensor.SetSensorEnabled(environment.useWaterStats);
        wallSensor.SetSensorEnabled(environment.useCardinals || environment.useDiagonals);
        commSensor.SetSensorEnabled(communication.useComms);
        
        /*
        imageSensorComms.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.communicationGenome.useComms;
        imageSensorWater.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.environmentalGenome.useWaterStats;
        imageSensorWalls.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.environmentalGenome.useCardinals || genome.bodyGenome.environmentalGenome.useDiagonals;
        imageSensorFoodPlant.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = foodGenome.useNutrients || foodGenome.useStats;
        imageSensorFoodMicrobe.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.foodGenome.usePos;  // ********* THESE ARE WRONG ^ ^ ^
        imageSensorFoodMeat.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.foodGenome.useVel;
        imageSensorFoodEggs.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.foodGenome.useEggs;
        imageSensorFoodCorpse.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.foodGenome.useCorpse;
        imageSensorFriend.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.friendGenome.usePos || genome.bodyGenome.friendGenome.useVel || genome.bodyGenome.friendGenome.useDir;
        imageSensorFoe.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.threatGenome.usePos || genome.bodyGenome.threatGenome.useVel || genome.bodyGenome.threatGenome.useDir;
        imageSensorInternals.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = true;
        imageSensorContact.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = true;

        if (genome.bodyGenome.communicationGenome.useComms) {
            imageSensorComms.color = Color.white;
        }
        else {
            imageSensorComms.color = Color.gray * 0.75f;
        }
        if (genome.bodyGenome.environmentalGenome.useWaterStats) {
            imageSensorWater.color = Color.white;
        }
        else {
            imageSensorWater.color = Color.gray * 0.75f;
        }
        if (genome.bodyGenome.environmentalGenome.useCardinals || genome.bodyGenome.environmentalGenome.useDiagonals) {
            imageSensorWalls.color = Color.white;
        }
        else {
            imageSensorWalls.color = Color.gray * 0.75f;
        }
                        
        if (foodGenome.useNutrients || foodGenome.useStats) {
            imageSensorFoodPlant.color = Color.white;
        }
        else {
            imageSensorFoodPlant.color = Color.gray * 0.75f;
        }

        if (foodGenome.usePos) {
            imageSensorFoodMicrobe.color = Color.white;
        }
        else {
            imageSensorFoodMicrobe.color = Color.gray * 0.75f;
        }

        if (foodGenome.useVel) {
            imageSensorFoodMeat.color = Color.white;
        }
        else {
            imageSensorFoodMeat.color = Color.gray * 0.75f;
        }

        if (foodGenome.useEggs) {
            imageSensorFoodEggs.color = Color.white;
        }
        else {
            imageSensorFoodEggs.color = Color.gray * 0.75f;
        }

        if (foodGenome.useCorpse) {
            imageSensorFoodCorpse.color = Color.white;
        }
        else {
            imageSensorFoodCorpse.color = Color.gray * 0.75f;
        }

        if (genome.bodyGenome.friendGenome.usePos || genome.bodyGenome.friendGenome.useVel || genome.bodyGenome.friendGenome.useDir) {
            imageSensorFriend.color = Color.white;
        }
        else {
            imageSensorFriend.color = Color.gray * 0.75f;
        }
        if (genome.bodyGenome.threatGenome.usePos || genome.bodyGenome.threatGenome.useVel || genome.bodyGenome.threatGenome.useDir) {
            imageSensorFoe.color = Color.white;
        }
        else {
            imageSensorFoe.color = Color.gray * 0.75f;
        }
        */

        imageSensorInternals.color = Color.white;        
        imageSensorContact.color = Color.white;         
    }
    
    // WPP 5/6/21: Only pass coreGenome since it is the only part being used
    // delegated common calculations to static methods
    // * WPP: Move to separate component
    private void UpdateDigestSpecUI(CritterModuleCoreGenome coreGenome) {
        PondHelpers.SetPercentText(textDigestPlant, coreGenome.dietSpecializationPlant);
        PondHelpers.SetPercentText(textDigestMeat, coreGenome.dietSpecializationMeat);
        PondHelpers.SetPercentText(textDigestDecay, coreGenome.dietSpecializationDecay);

        PondHelpers.SetLocalYScale(imageDigestPlant, coreGenome.dietSpecializationPlant);
        PondHelpers.SetLocalYScale(imageDigestMeat, coreGenome.dietSpecializationMeat);
        PondHelpers.SetLocalYScale(imageDigestDecay, coreGenome.dietSpecializationDecay);
    }
    
    // * WPP: move to separate component
    private void UpdateSpecializationsUI(CritterModuleCoreGenome coreGenome) {
        PondHelpers.SetPercentText(textSpecAttack, coreGenome.talentSpecializationAttack);
        PondHelpers.SetPercentText(textSpecDefense, coreGenome.talentSpecializationDefense);
        PondHelpers.SetPercentText(textSpecSpeed, coreGenome.talentSpecializationSpeed);
        PondHelpers.SetPercentText(textSpecEnergy, coreGenome.talentSpecializationUtility);

        PondHelpers.SetLocalYScale(imageSpecAttack, coreGenome.talentSpecializationAttack);
        PondHelpers.SetLocalYScale(imageSpecDefense, coreGenome.talentSpecializationDefense);
        PondHelpers.SetLocalYScale(imageSpecSpeed, coreGenome.talentSpecializationSpeed);
        PondHelpers.SetLocalYScale(imageSpecEnergy, coreGenome.talentSpecializationUtility);
    }
	
	// * WPP: move to separate component
    public void UpdatePerformanceBehaviors(SpeciesGenomePool pool, CandidateAgentData.PerformanceData performanceData) { //CandidateAgentData candidate) {
        //if(candidate.performanceData != null) {
        textBehaviorAttack.text = performanceData.totalTimesAttacked.ToString("F0");
        textBehaviorDefend.text = performanceData.totalTimesDefended.ToString("F0");
        textBehaviorDash.text = performanceData.totalTimesDashed.ToString("F0");
        textBehaviorRest.text = (performanceData.totalTicksRested * 0.01f).ToString("F0");
        textBehaviorFeed.text = performanceData.totalTimesPregnant.ToString("F0");
                
        PondHelpers.SetLocalYScale(imageBehaviorAttack, performanceData.attackActionPercent);
        PondHelpers.SetLocalYScale(imageBehaviorDefend, performanceData.defendActionPercent);
        PondHelpers.SetLocalYScale(imageBehaviorDash, performanceData.dashActionPercent);
        PondHelpers.SetLocalYScale(imageBehaviorRest, Mathf.Clamp01(performanceData.totalTicksRested / 600f));
        PondHelpers.SetLocalYScale(imageBehaviorFeed, Mathf.Clamp01(performanceData.totalTimesPregnant / 4f));
        
        textEatenPlants.text = performanceData.totalFoodEatenPlant.ToString("F2");
        textEatenMicrobes.text = performanceData.totalFoodEatenZoop.ToString("F2");
        textEatenAnimals.text = performanceData.totalFoodEatenCreature.ToString("F2");
        textEatenEggs.text = performanceData.totalFoodEatenEgg.ToString("F2");
        textEatenCorpse.text = performanceData.totalFoodEatenCorpse.ToString("F2");

        // WPP: applied getters for percent eaten of each type
        PondHelpers.SetLocalYScale(imageEatenPlants, performanceData.percentPlantEaten);
        PondHelpers.SetLocalYScale(imageEatenMicrobes, performanceData.percentZooplantonEaten);
        PondHelpers.SetLocalYScale(imageEatenAnimals, performanceData.percentCreatureEaten);
        PondHelpers.SetLocalYScale(imageEatenEggs, performanceData.percentEggEaten);
        PondHelpers.SetLocalYScale(imageEatenCorpse, performanceData.percentCorpseEaten);
        //textBehaviorAttack.text = candidate.evaluationScoresList[0].ToString();
        
        /*
        textBehaviorAttack.text = pool.avgPerformanceData.totalTimesAttacked.ToString("F0");
        textBehaviorDefend.text = pool.avgPerformanceData.totalTimesDefended.ToString("F0");
        textBehaviorDash.text = pool.avgPerformanceData.totalTimesDashed.ToString("F0");
        textBehaviorRest.text = (pool.avgPerformanceData.totalTicksRested * 0.01f).ToString("F0");
        textBehaviorFeed.text = pool.avgPerformanceData.totalTimesPregnant.ToString("F0");
        
        float totalTimesActed = pool.avgPerformanceData.totalTimesAttacked + pool.avgPerformanceData.totalTimesDefended + pool.avgPerformanceData.totalTimesDashed + 0.001f; // <-- prevent divide by 0
        imageBehaviorAttack.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalTimesAttacked / totalTimesActed, 1f);
        imageBehaviorDefend.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalTimesDefended / totalTimesActed, 1f);
        imageBehaviorDash.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalTimesDashed / totalTimesActed, 1f);
        imageBehaviorRest.transform.localScale = new Vector3(1f, Mathf.Clamp01(pool.avgPerformanceData.totalTicksRested / 600f), 1f);
        imageBehaviorFeed.transform.localScale = new Vector3(1f, Mathf.Clamp01(pool.avgPerformanceData.totalTimesPregnant / 4f), 1f);
        
        textEatenPlants.text = pool.avgPerformanceData.totalFoodEatenPlant.ToString("F2");
        textEatenMicrobes.text = pool.avgPerformanceData.totalFoodEatenZoop.ToString("F2");
        textEatenAnimals.text = pool.avgPerformanceData.totalFoodEatenCreature.ToString("F2");
        textEatenEggs.text = pool.avgPerformanceData.totalFoodEatenEgg.ToString("F2");
        textEatenCorpse.text = pool.avgPerformanceData.totalFoodEatenCorpse.ToString("F2");

        float totalEaten = pool.avgPerformanceData.totalFoodEatenPlant + pool.avgPerformanceData.totalFoodEatenZoop + pool.avgPerformanceData.totalFoodEatenCreature + pool.avgPerformanceData.totalFoodEatenEgg + pool.avgPerformanceData.totalFoodEatenCorpse + 0.001f;
        imageEatenPlants.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalFoodEatenPlant / totalEaten, 1f);
        imageEatenMicrobes.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalFoodEatenZoop / totalEaten, 1f);
        imageEatenAnimals.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalFoodEatenCreature / totalEaten, 1f);
        imageEatenEggs.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalFoodEatenEgg / totalEaten, 1f);
        imageEatenCorpse.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalFoodEatenCorpse / totalEaten, 1f);
    */
    }
    
    #region Button Clicks
    
    // WPP 5/7/21: moved to UIManager, replaced logic with static
    public void ClickButtonNext() {
        /*
        int curSpeciesID = uiManagerRef.selectedSpeciesID;

        if(curSpeciesID >= simulationManager.speciesPoolCount - 1) {
            curSpeciesID = 0;
        }
        else {
            curSpeciesID += 1;
        }
        */
        
        //uiManagerRef.globalResourcesUI.SetSelectedSpeciesUI(curSpeciesID);  // *** These should be combined??
        uiManagerRef.CycleFocusedCandidateGenome();
    }
    
    public void ClickButtonPrev() {
        //uiManagerRef.speciesOverviewUI.CycleHallOfFame();
        uiManagerRef.speciesOverviewUI.CycleCurrentGenome();
    }
    
    public void ClickButtonGenomeTab() {
        isGenomeTabActive = true;
        isPerformanceTabActive = false;
        isHistoryTabActive = false; 
    }
    
    public void ClickButtonPerformanceTab() {
        isGenomeTabActive = false;
        isPerformanceTabActive = true;
        isHistoryTabActive = false;
    }
    
    public void ClickButtonHistoryTab() {
        isGenomeTabActive = false;
        isPerformanceTabActive = false;
        isHistoryTabActive = true;
    }
    
    // WPP: variables set but not used
    /*
    public void ClickButtonPerformance() {
        ClearPanelBools();
        isPerformancePanelON = true;
    }

    public void ClickButtonSpecializations() {
        ClearPanelBools();
        isSpecializationPanelOn = true;
    }
    
    public void ClickButtonBrain() {
        ClearPanelBools();
        isBrainPanelOn = true;
    }
    
    private void ClearPanelBools() {
        isPerformancePanelON = false;
        isSpecializationPanelOn = false;
        isBrainPanelOn = false;
    }
    */
    
    #endregion
    
    public void EnterTooltipObject(GenomeButtonTooltipSource tip) {
        isTooltipHover = true;
        tooltipString = tip.tooltipString;
    }
    
    public void ExitTooltipObject() {
        isTooltipHover = false;
    }
    
    [Serializable] 
    public class Sensor
    {
        [SerializeField] Image image;
        [SerializeField] GenomeButtonTooltipSource tooltip;
        
        public void SetSensorEnabled(bool value) 
        { 
            tooltip.isSensorEnabled = value;
            
            // * Expose values in central location (lookup?)
            image.color = value ? Color.white : Color.gray * 0.75f;     
        }
    }
    
    [Serializable]
    public class Tab
    {
        [SerializeField] GameObject panel;
        [SerializeField] Image image;
        
        public void SetActive(bool value)
        {
            if (!panel || !image)
            {
                Debug.LogError("Missing references on Tab element");
                return;
            }
        
            panel.SetActive(value);
            image.color = value ? Color.white : Color.gray;
        }
    }
}
