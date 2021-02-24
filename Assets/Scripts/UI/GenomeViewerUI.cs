using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenomeViewerUI : MonoBehaviour {
    public UIManager uiManagerRef;

    public Text textFocusedCandidate;
    public Text textGenomeOverviewA;
    public Text textGenomeOverviewB;
    public Text textGenomeOverviewC;

    public GameObject panelGenomeSensors;
    public Image imageSensorFoodPlant;
    public Image imageSensorFoodMicrobe;
    public Image imageSensorFoodEggs;
    public Image imageSensorFoodMeat;
    public Image imageSensorFoodCorpse;
    public Image imageSensorFriend;
    public Image imageSensorFoe;
    public Image imageSensorWalls;
    public Image imageSensorWater;
    public Image imageSensorInternals;
    public Image imageSensorContact;
    public Image imageSensorComms;

    public Image imagePortraitTitleBG;

    public GameObject panelGenomeAbilities;
    public Image imageAbilityFeed;
    public Image imageAbilityAttack;
    public Image imageAbilityDefend;
    public Image imageAbilityDash;
    public Image imageAbilityRest;

    public GameObject panelGenomeSpecializations;
    public Image imageSpecAttack;
    public Image imageSpecDefense;
    public Image imageSpecSpeed;
    public Image imageSpecEnergy;
    public Text textSpecAttack;
    public Text textSpecDefense;
    public Text textSpecSpeed;
    public Text textSpecEnergy;

    public GameObject panelGenomeDigestion;
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

    public GameObject panelGenomeTab;
    public GameObject panelPerformanceTab;
    public GameObject panelHistoryTab;    
    
    public Button buttonGenomeTab;
    public Button buttonPerformanceTab;
    public Button buttonHistoryTab;
    // Real-Time panel handled in center bottom with creature portrait and brain info
    
    public bool isGenomeTabActive = true;
    public bool isPerformanceTabActive = false;
    public bool isHistoryTabActive = false;

    public GameObject imageDeadDim;
    public Toggle toggleAutofollow;

    public bool isTooltipHover = true;
    public string tooltipString;

    public bool isPerformancePanelON;
    public bool isSpecializationPanelOn;
    //public bool isHistoryPanelOn;
    public bool isBrainPanelOn;



    public void UpdateUI(SpeciesGenomePool pool, CandidateAgentData candidate) {
        if(candidate != null) {

            string titleString = "<size=18>Critter</size> " + candidate.candidateID.ToString() + "<size=18>";
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
            if(candidate != null) {
                hue = candidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
                hueB = candidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary;

            }
            /*else {
                Debug.LogError("candData");
            }
            */
            //Vector3 hue = pool.avgCandidateData.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
            imagePortraitTitleBG.color = new Color(hue.x, hue.y, hue.z);
            textFocusedCandidate.color = new Color(hueB.x, hueB.y, hueB.z);
            //+ ", " + candidate.numCompletedEvaluations.ToString() + ", " + candidate.speciesID.ToString() + ", " + candidate.isBeingEvaluated.ToString() + ", " + candidate.candidateGenome.bodyGenome.coreGenome.name;

            if(toggleAutofollow.isOn) {  // **********

            }

            panelPerformanceBehavior.SetActive(true);
            panelEaten.SetActive(true);
            //panelGenomeSpecializations.SetActive(false);
            //panelGenomeDigestion.SetActive(false);
            panelGenomeAbilities.SetActive(false);
            panelGenomeSensors.SetActive(true);

            panelGenomeTab.SetActive(isGenomeTabActive);
            buttonGenomeTab.GetComponentInChildren<Image>().color = isGenomeTabActive ? Color.white : Color.gray;
            panelPerformanceTab.SetActive(isPerformanceTabActive);
            buttonPerformanceTab.GetComponentInChildren<Image>().color = isPerformanceTabActive ? Color.white : Color.gray;
            panelHistoryTab.SetActive(isHistoryTabActive);
            buttonHistoryTab.GetComponentInChildren<Image>().color = isHistoryTabActive ? Color.white : Color.gray;
            
            //panel
            imageDeadDim.gameObject.SetActive(false);
            float lifespan = (float)candidate.performanceData.totalTicksAlive;
            textGenomeOverviewA.text = "Lifespan: " + (lifespan * 0.1f).ToString("F0") + ", Gen: " + candidate.candidateGenome.generationCount.ToString();

            if(candidate.isBeingEvaluated) {
                
                lifespan = uiManagerRef.gameManager.simulationManager.agentsArray[uiManagerRef.cameraManager.targetAgentIndex].ageCounter;
                textGenomeOverviewA.text = "Age: " + (lifespan * 0.1f).ToString("F0") + ", Gen: " + candidate.candidateGenome.generationCount.ToString();
            }
            if(uiManagerRef.gameManager.simulationManager.agentsArray[uiManagerRef.cameraManager.targetAgentIndex].curLifeStage == Agent.AgentLifeStage.Dead) {
                imageDeadDim.gameObject.SetActive(true);
            }

            
            textGenomeOverviewB.text = "Size: " + (100f * candidate.candidateGenome.bodyGenome.coreGenome.creatureBaseLength).ToString("F0") + ", Aspect 1:" + (1f / candidate.candidateGenome.bodyGenome.coreGenome.creatureAspectRatio).ToString("F0");
            textGenomeOverviewC.text = "Brain Size: " + candidate.candidateGenome.brainGenome.bodyNeuronList.Count.ToString() + "--" + candidate.candidateGenome.brainGenome.linkList.Count.ToString();
            
            //if(isFrontPageOn) {
              
            UpdateDigestSpecUI(pool, candidate.candidateGenome);
            UpdateSpecializationsUI(pool, candidate.candidateGenome);

            UpdatePerformanceBehaviors(pool, candidate); // ******
            UpdateSensorsUI(pool, candidate.candidateGenome);
                //UpdateAbilitiesUI(pool, candidate.candidateGenome);

                //panelGenomeAbilities.SetActive(true);
                //panelGenomeSensors.SetActive(true);

                //isHistoryPanelOn = false;  // ** unneeded
            //}
            //else {
                //isHistoryPanelOn = true;
            //}
        }        
    }
    private void UpdateSensorsUI(SpeciesGenomePool pool, AgentGenome genome) {
        imageSensorComms.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.communicationGenome.useComms;
        imageSensorWater.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.environmentalGenome.useWaterStats;
        imageSensorWalls.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = (genome.bodyGenome.environmentalGenome.useCardinals || genome.bodyGenome.environmentalGenome.useDiagonals);
        imageSensorFoodPlant.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.foodGenome.useNutrients || genome.bodyGenome.foodGenome.useStats;
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
        if (genome.bodyGenome.foodGenome.useNutrients || genome.bodyGenome.foodGenome.useStats) {
            imageSensorFoodPlant.color = Color.white;
        }
        else {
            imageSensorFoodPlant.color = Color.gray * 0.75f;
        }

        if (genome.bodyGenome.foodGenome.usePos) {
            imageSensorFoodMicrobe.color = Color.white;
        }
        else {
            imageSensorFoodMicrobe.color = Color.gray * 0.75f;
        }

        if (genome.bodyGenome.foodGenome.useVel) {
            imageSensorFoodMeat.color = Color.white;
        }
        else {
            imageSensorFoodMeat.color = Color.gray * 0.75f;
        }

        if (genome.bodyGenome.foodGenome.useEggs) {
            imageSensorFoodEggs.color = Color.white;
        }
        else {
            imageSensorFoodEggs.color = Color.gray * 0.75f;
        }

        if (genome.bodyGenome.foodGenome.useCorpse) {
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

        imageSensorInternals.color = Color.white;        
        imageSensorContact.color = Color.white;        
        
    }
    private void UpdateAbilitiesUI(SpeciesGenomePool pool, AgentGenome genome) {

    }
    private void UpdateDigestSpecUI(SpeciesGenomePool pool, AgentGenome genome) {
        textDigestPlant.text = (genome.bodyGenome.coreGenome.dietSpecializationPlant * 100f).ToString("F0");
        textDigestMeat.text = (genome.bodyGenome.coreGenome.dietSpecializationMeat * 100f).ToString("F0");
        textDigestDecay.text = (genome.bodyGenome.coreGenome.dietSpecializationDecay * 100f).ToString("F0");

        imageDigestPlant.transform.localScale = new Vector3(1f, genome.bodyGenome.coreGenome.dietSpecializationPlant, 1f);
        imageDigestMeat.transform.localScale = new Vector3(1f, genome.bodyGenome.coreGenome.dietSpecializationMeat, 1f);
        imageDigestDecay.transform.localScale = new Vector3(1f, genome.bodyGenome.coreGenome.dietSpecializationDecay, 1f);
        
    }
    private void UpdateSpecializationsUI(SpeciesGenomePool pool, AgentGenome genome) {
        textSpecAttack.text = (genome.bodyGenome.coreGenome.talentSpecializationAttack * 100f).ToString("F0");
        textSpecDefense.text = (genome.bodyGenome.coreGenome.talentSpecializationDefense * 100f).ToString("F0");
        textSpecSpeed.text = (genome.bodyGenome.coreGenome.talentSpecializationSpeed * 100f).ToString("F0");
        textSpecEnergy.text = (genome.bodyGenome.coreGenome.talentSpecializationUtility * 100f).ToString("F0");

        imageSpecAttack.transform.localScale = new Vector3(1f, genome.bodyGenome.coreGenome.talentSpecializationAttack, 1f);
        imageSpecDefense.transform.localScale = new Vector3(1f, genome.bodyGenome.coreGenome.talentSpecializationDefense, 1f);
        imageSpecSpeed.transform.localScale = new Vector3(1f, genome.bodyGenome.coreGenome.talentSpecializationSpeed, 1f);
        imageSpecEnergy.transform.localScale = new Vector3(1f, genome.bodyGenome.coreGenome.talentSpecializationUtility, 1f);
    }
	
    public void UpdatePerformanceBehaviors(SpeciesGenomePool pool, CandidateAgentData candidate) {
        //if(candidate.performanceData != null) {
        textBehaviorAttack.text = candidate.performanceData.totalTimesAttacked.ToString("F0");
        textBehaviorDefend.text = candidate.performanceData.totalTimesDefended.ToString("F0");
        textBehaviorDash.text = candidate.performanceData.totalTimesDashed.ToString("F0");
        textBehaviorRest.text = (candidate.performanceData.totalTicksRested * 0.01f).ToString("F0");
        textBehaviorFeed.text = candidate.performanceData.totalTimesPregnant.ToString("F0");
        
        float totalTimesActed = candidate.performanceData.totalTimesAttacked + candidate.performanceData.totalTimesDefended + candidate.performanceData.totalTimesDashed + 0.001f; // <-- prevent divide by 0
        imageBehaviorAttack.transform.localScale = new Vector3(1f, candidate.performanceData.totalTimesAttacked / totalTimesActed, 1f);
        imageBehaviorDefend.transform.localScale = new Vector3(1f, candidate.performanceData.totalTimesDefended / totalTimesActed, 1f);
        imageBehaviorDash.transform.localScale = new Vector3(1f, candidate.performanceData.totalTimesDashed / totalTimesActed, 1f);
        imageBehaviorRest.transform.localScale = new Vector3(1f, Mathf.Clamp01(candidate.performanceData.totalTicksRested / 600f), 1f);
        imageBehaviorFeed.transform.localScale = new Vector3(1f, Mathf.Clamp01(candidate.performanceData.totalTimesPregnant / 4f), 1f);
        
        textEatenPlants.text = candidate.performanceData.totalFoodEatenPlant.ToString("F2");
        textEatenMicrobes.text = candidate.performanceData.totalFoodEatenZoop.ToString("F2");
        textEatenAnimals.text = candidate.performanceData.totalFoodEatenCreature.ToString("F2");
        textEatenEggs.text = candidate.performanceData.totalFoodEatenEgg.ToString("F2");
        textEatenCorpse.text = candidate.performanceData.totalFoodEatenCorpse.ToString("F2");

        float totalEaten = candidate.performanceData.totalFoodEatenPlant + candidate.performanceData.totalFoodEatenZoop + candidate.performanceData.totalFoodEatenCreature + candidate.performanceData.totalFoodEatenEgg + candidate.performanceData.totalFoodEatenCorpse + 0.001f;
        imageEatenPlants.transform.localScale = new Vector3(1f, candidate.performanceData.totalFoodEatenPlant / totalEaten, 1f);
        imageEatenMicrobes.transform.localScale = new Vector3(1f, candidate.performanceData.totalFoodEatenZoop / totalEaten, 1f);
        imageEatenAnimals.transform.localScale = new Vector3(1f, candidate.performanceData.totalFoodEatenCreature / totalEaten, 1f);
        imageEatenEggs.transform.localScale = new Vector3(1f, candidate.performanceData.totalFoodEatenEgg / totalEaten, 1f);
        imageEatenCorpse.transform.localScale = new Vector3(1f, candidate.performanceData.totalFoodEatenCorpse / totalEaten, 1f);

        //}
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
    
    public void ClickButtonNext() {
        int curSpeciesID = uiManagerRef.selectedSpeciesID;

        if(curSpeciesID >= uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count - 1) {
            curSpeciesID = 0;
        }
        else {
            curSpeciesID += 1;
        }
        uiManagerRef.globalResourcesUI.SetSelectedSpeciesUI(curSpeciesID);  // *** These should be combined??
        uiManagerRef.SetFocusedCandidateGenome(uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[curSpeciesID].representativeCandidate);
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
    // Use this for initialization
	void Start () {
		isTooltipHover = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
