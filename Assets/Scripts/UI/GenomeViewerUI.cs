﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class GenomeViewerUI : MonoBehaviour {
    SimulationManager simulationManager => SimulationManager.instance;
    UIManager uiManager => UIManager.instance;

    public SpeciesOverviewUI speciesOverviewUI;
    
    // WPP 5/10/21: delegated to panels
    public CreaturePortraitPanel portrait;
    //public Text textFocusedCandidate;
    public GenomeOverviewPanel genomeOverview;
    //public Text textGenomeOverviewA;
    //public Text textGenomeOverviewB;
    //public Text textGenomeOverviewC;

    public GameObject panelGenomeSensors;
    
    [SerializeField] Tab genomeTab;
    [SerializeField] Tab historyTab;
    [SerializeField] Tab performanceTab;
    
    // WPP 5/9/21: delegated  
    [SerializeField] SensorsPanel sensorsPanel;  
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
    //public Image imageSensorInternals;
    //public Image imageSensorContact;

    //public Image imagePortraitTitleBG;

    public GameObject panelGenomeAbilities;
    
    // WPP 5/9/21: not used -> delete?
    public Image imageAbilityFeed;
    public Image imageAbilityAttack;
    public Image imageAbilityDefend;
    public Image imageAbilityDash;
    public Image imageAbilityRest;
    public GameObject panelGenomeSpecializations;
    public GameObject panelGenomeDigestion;
    
    public bool isPerformancePanelON;
    public bool isSpecializationPanelOn;    
    public bool isBrainPanelOn;
    public Toggle toggleAutofollow;

    // WPP: 5/9/21: delegated
    public SpecializationPanel specializationPanel;
    //public Image imageSpecAttack;
    //public Image imageSpecDefense;
    //public Image imageSpecSpeed;
    //public Image imageSpecEnergy;
    //public Text textSpecAttack;
    //public Text textSpecDefense;
    //public Text textSpecSpeed;
    //public Text textSpecEnergy;

    // WPP 5/9/21: delegated
    [SerializeField] DigestionPanel digestionPanel;
    //public Image imageDigestPlant;
    //public Image imageDigestMeat;
    //public Image imageDigestDecay;    
    //public Text textDigestPlant;
    //public Text textDigestMeat;
    //public Text textDigestDecay;

    public GameObject panelPerformanceBehavior;
    
    // WPP 5/9/21: delegated to BehaviorPanel
    [SerializeField] BehaviorPanel behaviorPanel;
    //public Image imageBehaviorFeed;
    //public Image imageBehaviorAttack;
    //public Image imageBehaviorDefend;
    //public Image imageBehaviorDash;
    //public Image imageBehaviorRest;
    //public Text textBehaviorFeed;
    //public Text textBehaviorAttack;
    //public Text textBehaviorDefend;
    //public Text textBehaviorDash;
    //public Text textBehaviorRest;

    public GameObject panelEaten;
    
    // WPP 5/9/21: delegated
    [SerializeField] FoodEatenPanel foodEatenPanel;
    //public Image imageEatenPlants;
    //public Image imageEatenMicrobes;
    //public Image imageEatenAnimals;
    //public Image imageEatenEggs;
    //public Image imageEatenCorpse;
    //public Text textEatenPlants;
    //public Text textEatenMicrobes;
    //public Text textEatenAnimals;
    //public Text textEatenEggs;
    //public Text textEatenCorpse;

    // WPP 5/6/21: moved to nested Tab class
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

    // WPP: delegated to BrainGenomeImage
    //private Texture2D brainGenomeTex; // Barcode
    //public Material brainGenomeMat;

    void Start () {
		isTooltipHover = false;
        //brainGenomeTex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        //brainGenomeTex.filterMode = FilterMode.Point;
        //brainGenomeTex.wrapMode = TextureWrapMode.Clamp;
        //brainGenomeMat.SetTexture("_MainTex", brainGenomeTex);
	}
	
    // WPP: quick access without GC    
    AgentGenome genome;
    BodyGenome body;
    PerformanceData performance;
    CritterModuleCoreGenome core;
	
	// WPP 5/6/21: applied early exit, broke out SetTitleString function
    public void UpdateUI(SpeciesGenomePool pool, CandidateAgentData candidate) {
        if (candidate == null || candidate.candidateGenome == null)
            return;
            
        performance = candidate.performanceData;
        genome = candidate.candidateGenome;
        body = genome.bodyGenome;
        core = body.coreGenome;
        
        // WPP: hue assigned then reassigned?
        //Vector3 hue = Vector3.one * 0.75f;
        //Vector3 hueB = Vector3.one * 0.25f;
        //hue = appearance.huePrimary;
        //hueB = appearance.hueSecondary;
            
        portrait.SetTitleText(candidate);

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
        
        //Vector3 hue = pool.avgCandidateData.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
        //imagePortraitTitleBG.color = new Color(hue.x, hue.y, hue.z);
        //textFocusedCandidate.color = new Color(hueB.x, hueB.y, hueB.z);

        panelPerformanceBehavior.SetActive(true);
        panelEaten.SetActive(true);
        //panelGenomeSpecializations.SetActive(false);
        //panelGenomeDigestion.SetActive(false);
        
        // * WPP: only usage, may as well set in editor
        panelGenomeAbilities.SetActive(false);
        panelGenomeSensors.SetActive(true);

        // WPP 5/6/21: delegated to Tab nested class
        // * delegate to components (organize scattered references first)
        genomeTab.SetActive(isGenomeTabActive);
        performanceTab.SetActive(isPerformanceTabActive);
        historyTab.SetActive(isHistoryTabActive);
        //panelGenomeTab.SetActive(isGenomeTabActive);
        //buttonGenomeTab.GetComponentInChildren<Image>().color = isGenomeTabActive ? Color.white : Color.gray;
        //panelPerformanceTab.SetActive(isPerformanceTabActive);
        //buttonPerformanceTab.GetComponentInChildren<Image>().color = isPerformanceTabActive ? Color.white : Color.gray;
        //panelHistoryTab.SetActive(isHistoryTabActive);
        //buttonHistoryTab.GetComponentInChildren<Image>().color = isHistoryTabActive ? Color.white : Color.gray;
        
        // WPP 5/10/21: conditional implied in value (+ value accessed via getter logic)
        imageDeadDim.SetActive(simulationManager.targetAgentIsDead);
        //imageDeadDim.gameObject.SetActive(false);
        
        genomeOverview.Refresh(candidate);
        
        // WPP 5/10/21: delegated to GenomeOverviewPanel
        /*
        float lifespan = performance.totalTicksAlive;
        
        textGenomeOverviewA.text = "Lifespan: " + (lifespan * 0.1f).ToString("F0") + ", Gen: " + genome.generationCount;

        if(candidate.isBeingEvaluated) {
            // [WPP: peek reference to target agent for defect note]
            lifespan = simulationManager.targetAgentAge;
            textGenomeOverviewA.text = "Age: " + (lifespan * 0.1f).ToString("F0") + ", Gen: " + genome.generationCount;
        }
        //if (simulationManager.targetAgentIsDead) {   
        //if(simulationManager.agentsArray[cameraManager.targetAgentIndex].curLifeStage == Agent.AgentLifeStage.Dead) {
        //    imageDeadDim.gameObject.SetActive(true);
        //}

        textGenomeOverviewB.text = "Size: " + (100f * core.creatureBaseLength).ToString("F0") + ", Aspect 1:" + (1f / core.creatureAspectRatio).ToString("F0");
        textGenomeOverviewC.text = "Brain Size: " + brain.bodyNeuronList.Count + "--" + brain.linkList.Count;
        */  
                  
        // WPP 5/9/21: delegated
        //UpdateSpecializationsUI(core);
        //UpdateDigestSpecUI(core);
        //UpdatePerformanceBehaviors(performance);
        //UpdateSensorsUI(genome); 
 
        digestionPanel.Refresh(core);
        specializationPanel.Refresh(core);
        behaviorPanel.Refresh(performance);
        foodEatenPanel.Refresh(performance);        
        sensorsPanel.Refresh(genome);     
    }
    
    #region Delegated Away
    
    // WPP 5/10/21: moved to CreaturePortraitPanel
    /*
    void SetTitleString(CandidateAgentData candidate)
    {
        string titleString = "<size=18>Critter</size> " + candidate.candidateID + "<size=18>";
        if(candidate.isBeingEvaluated) {
            titleString += "\n(following)";
        }
        else {
            if(candidate.numCompletedEvaluations > 0) {
                titleString += "\nFossil";
            }
            else {
                titleString += "\nUnborn";
            }
        }
        titleString += "</size>";
        textFocusedCandidate.text = titleString;
    }
    */
    
    // WPP 5/10/21: delegated to BrainGenomeImage
    /*public void CreateBrainGenomeTexture(AgentGenome genome) {
        int width = 256;
        brainGenomeTex.Resize(width, 1); // pool.leaderboardGenomesList.Count);

        for(int x = 0; x < width; x++) {
            int xIndex = x;// i % brainGenomeTex.width;
            int yIndex = 0; // i; // Mathf.FloorToInt(i / brainGenomeTex.width);
                              
            Color testColor;

            if (genome.brainGenome.linkList.Count > x) {
                float weightVal = genome.brainGenome.linkList[x].weight;
                testColor = new Color(weightVal * 0.5f + 0.5f, weightVal * 0.5f + 0.5f, weightVal * 0.5f + 0.5f);
                if(weightVal < -0.25f) {
                    testColor = Color.Lerp(testColor, Color.black, 0.15f);
                }
                else if(weightVal > 0.25f) {
                    testColor = Color.Lerp(testColor, Color.white, 0.15f);
                }
                else {
                    testColor = Color.Lerp(testColor, Color.gray, 0.15f);
                }
            }
            else {
                testColor = Color.black; // CLEAR
            }
                
            brainGenomeTex.SetPixel(xIndex, yIndex, testColor);
        }
        
        brainGenomeTex.Apply();
    }*/
            
    // WPP 5/6/21: Replaced repeating logic with Sensor nested class
    // 5/9/21: delegated to SensorsPanel
    //private void UpdateSensorsUI(AgentGenome genome) {
    //    if (genome.bodyGenome.foodGenome == null) return;
                
        /*plantFoodSensor.SetSensorEnabled(food.useNutrients || food.useStats);
        microbeFoodSensor.SetSensorEnabled(food.usePos);
        eggsFoodSensor.SetSensorEnabled(food.useEggs);
        meatFoodSensor.SetSensorEnabled(food.useVel);
        corpseFoodSensor.SetSensorEnabled(food.useCorpse);
        friendSensor.SetSensorEnabled(friend.usePos || friend.useVel || friend.useDir);
        foeSensor.SetSensorEnabled(threat.usePos || threat.useVel || threat.useDir);
        waterSensor.SetSensorEnabled(environment.useWaterStats);
        wallSensor.SetSensorEnabled(environment.useCardinals || environment.useDiagonals);
        commSensor.SetSensorEnabled(communication.useComms);*/
        
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

        //imageSensorInternals.color = Color.white;        
        //imageSensorContact.color = Color.white;         
    //}
    
    // WPP 5/9/21: delegated to DigestionPanel
    /*
    private void UpdateDigestSpecUI(CritterModuleCoreGenome coreGenome) {
        textDigestPlant.text = (coreGenome.dietSpecializationPlant * 100f).ToString("F0");
        textDigestMeat.text = (coreGenome.dietSpecializationMeat * 100f).ToString("F0");
        textDigestDecay.text = (coreGenome.dietSpecializationDecay * 100f).ToString("F0");

        imageDigestPlant.transform.localScale = new Vector3(1f, coreGenome.dietSpecializationPlant, 1f);
        imageDigestMeat.transform.localScale = new Vector3(1f, coreGenome.dietSpecializationMeat, 1f);
        imageDigestDecay.transform.localScale = new Vector3(1f, coreGenome.dietSpecializationDecay, 1f);
    }*/
    
    // WPP 5/9/21: delegated to SpecializationPanel
    /*
    private void UpdateSpecializationsUI(CritterModuleCoreGenome coreGenome) {
        textSpecAttack.text = (coreGenome.talentSpecializationAttack * 100f).ToString("F0");
        textSpecDefense.text = (coreGenome.talentSpecializationDefense * 100f).ToString("F0");
        textSpecSpeed.text = (coreGenome.talentSpecializationSpeed * 100f).ToString("F0");
        textSpecEnergy.text = (coreGenome.talentSpecializationUtility * 100f).ToString("F0");

        imageSpecAttack.transform.localScale = new Vector3(1f, coreGenome.talentSpecializationAttack, 1f);
        imageSpecDefense.transform.localScale = new Vector3(1f, coreGenome.talentSpecializationDefense, 1f);
        imageSpecSpeed.transform.localScale = new Vector3(1f, coreGenome.talentSpecializationSpeed, 1f);
        imageSpecEnergy.transform.localScale = new Vector3(1f, coreGenome.talentSpecializationUtility, 1f);
    }
    */
	
	// WPP 5/9/21: delegated calculations to PerformanceData
	// + delegated to BehaviorPanel, FoodEatenPanel
	/*
    public void UpdatePerformanceBehaviors(PerformanceData data) {
        //if(candidate.performanceData != null) {
        
        textBehaviorAttack.text = data.totalTimesAttacked.ToString("F0");
        textBehaviorDefend.text = data.totalTimesDefended.ToString("F0");
        textBehaviorDash.text = data.totalTimesDashed.ToString("F0");
        textBehaviorRest.text = (data.totalTicksRested * 0.01f).ToString("F0");
        textBehaviorFeed.text = data.totalTimesPregnant.ToString("F0");
                
        imageBehaviorAttack.transform.localScale = new Vector3(1f, data.attackActionPercent, 1f);
        imageBehaviorDefend.transform.localScale = new Vector3(1f, data.defendActionPercent, 1f);
        imageBehaviorDash.transform.localScale = new Vector3(1f, data.dashActionPercent, 1f);
        imageBehaviorRest.transform.localScale = new Vector3(1f, Mathf.Clamp01(data.totalTicksRested / 600f), 1f);
        imageBehaviorFeed.transform.localScale = new Vector3(1f, Mathf.Clamp01(data.totalTimesPregnant / 4f), 1f);
        
        textEatenPlants.text = data.totalFoodEatenPlant.ToString("F2");
        textEatenMicrobes.text = data.totalFoodEatenZoop.ToString("F2");
        textEatenAnimals.text = data.totalFoodEatenCreature.ToString("F2");
        textEatenEggs.text = data.totalFoodEatenEgg.ToString("F2");
        textEatenCorpse.text = data.totalFoodEatenCorpse.ToString("F2");

        imageEatenPlants.transform.localScale = new Vector3(1f, data.plantEatenPercent, 1f);
        imageEatenMicrobes.transform.localScale = new Vector3(1f, data.zooplanktonEatenPercent, 1f);
        imageEatenAnimals.transform.localScale = new Vector3(1f, data.creatureEatenPercent, 1f);
        imageEatenEggs.transform.localScale = new Vector3(1f, data.eggEatenPercent, 1f);
        imageEatenCorpse.transform.localScale = new Vector3(1f, data.corpseEatenPercent, 1f);
        //textBehaviorAttack.text = candidate.evaluationScoresList[0].ToString();
        
        // Previously commented-out
        //textBehaviorAttack.text = pool.avgPerformanceData.totalTimesAttacked.ToString("F0");
        //textBehaviorDefend.text = pool.avgPerformanceData.totalTimesDefended.ToString("F0");
        //textBehaviorDash.text = pool.avgPerformanceData.totalTimesDashed.ToString("F0");
        //textBehaviorRest.text = (pool.avgPerformanceData.totalTicksRested * 0.01f).ToString("F0");
        //textBehaviorFeed.text = pool.avgPerformanceData.totalTimesPregnant.ToString("F0");
        
        //float totalTimesActed = pool.avgPerformanceData.totalTimesAttacked + pool.avgPerformanceData.totalTimesDefended + pool.avgPerformanceData.totalTimesDashed + 0.001f; // <-- prevent divide by 0
        //imageBehaviorAttack.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalTimesAttacked / totalTimesActed, 1f);
        //imageBehaviorDefend.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalTimesDefended / totalTimesActed, 1f);
        //imageBehaviorDash.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalTimesDashed / totalTimesActed, 1f);
        //imageBehaviorRest.transform.localScale = new Vector3(1f, Mathf.Clamp01(pool.avgPerformanceData.totalTicksRested / 600f), 1f);
        //imageBehaviorFeed.transform.localScale = new Vector3(1f, Mathf.Clamp01(pool.avgPerformanceData.totalTimesPregnant / 4f), 1f);
        
        //textEatenPlants.text = pool.avgPerformanceData.totalFoodEatenPlant.ToString("F2");
        //textEatenMicrobes.text = pool.avgPerformanceData.totalFoodEatenZoop.ToString("F2");
        //textEatenAnimals.text = pool.avgPerformanceData.totalFoodEatenCreature.ToString("F2");
        //textEatenEggs.text = pool.avgPerformanceData.totalFoodEatenEgg.ToString("F2");
        //textEatenCorpse.text = pool.avgPerformanceData.totalFoodEatenCorpse.ToString("F2");

        //float totalEaten = pool.avgPerformanceData.totalFoodEatenPlant + pool.avgPerformanceData.totalFoodEatenZoop + pool.avgPerformanceData.totalFoodEatenCreature + pool.avgPerformanceData.totalFoodEatenEgg + pool.avgPerformanceData.totalFoodEatenCorpse + 0.001f;
        //imageEatenPlants.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalFoodEatenPlant / totalEaten, 1f);
        //imageEatenMicrobes.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalFoodEatenZoop / totalEaten, 1f);
        //imageEatenAnimals.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalFoodEatenCreature / totalEaten, 1f);
        //imageEatenEggs.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalFoodEatenEgg / totalEaten, 1f);
        //imageEatenCorpse.transform.localScale = new Vector3(1f, pool.avgPerformanceData.totalFoodEatenCorpse / totalEaten, 1f);
    }
    */
    
    #endregion
    
    #region Button Clicks
    
    // WPP: logic moved to UIManager
    public void ClickButtonNext() {
        uiManager.CycleFocusedCandidateGenome();
        /*int curSpeciesID = uiManagerRef.selectedSpeciesID;

        if(curSpeciesID >= simulationManager.masterGenomePool.completeSpeciesPoolsList.Count - 1) {
            curSpeciesID = 0;
        }
        else {
            curSpeciesID += 1;
        }
        //uiManagerRef.globalResourcesUI.SetSelectedSpeciesUI(curSpeciesID);  // *** These should be combined??
        uiManagerRef.SetFocusedCandidateGenome(simulationManager.masterGenomePool.completeSpeciesPoolsList[curSpeciesID].representativeCandidate);*/
    }
    
    // * WPP: should be a similar process to ClickButtonNext
    public void ClickButtonPrev() {
        //speciesOverviewUI.CycleHallOfFame();
        speciesOverviewUI.CycleCurrentGenome();
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
    
    #endregion
    
    private void ClearPanelBools() {
        isPerformancePanelON = false;
        isSpecializationPanelOn = false;
        //isHistoryPanelOn = false;
        isBrainPanelOn = false;
        //isFrontPageOn = false;
    }
    
    public void EnterTooltipObject(GenomeButtonTooltipSource tip) {
        isTooltipHover = true;
        tooltipString = tip.tooltipString;
    }
    
    public void ExitTooltipObject() {
        isTooltipHover = false;
    }
    
    [Serializable]
    public class Tab
    {
        [SerializeField] GameObject panel;
        [SerializeField] Image image;
        
        public void SetActive(bool value)
        {
            panel.SetActive(value);
            image.color = value ? Color.white : Color.gray;
        }
    }
}
