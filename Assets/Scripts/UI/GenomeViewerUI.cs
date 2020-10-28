using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenomeViewerUI : MonoBehaviour {
    public UIManager uiManagerRef;

    public Text textFocusedGenome;

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

    public bool isTooltipHover = true;
    public string tooltipString;


    public void UpdateUI(SpeciesGenomePool pool, AgentGenome genome) {
        if(genome != null) {
            UpdateSensorsUI(pool, genome);
            UpdateAbilitiesUI(pool, genome);
            UpdateDigestSpecUI(pool, genome);
            UpdateSpecializationsUI(pool, genome);
            //textFocusedGenome.text = uiManagerRef.globalResourcesUI.focusedAgentGenome.bodyGenome.coreGenome.name;

            UpdatePerformanceBehaviors(pool, null); // ******
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
        //textBehaviorAttack.text = candidate.evaluationScoresList[0].ToString();
        textBehaviorAttack.text = pool.avgTimesAttacked.ToString("F0");
        textBehaviorDefend.text = pool.avgTimesDefended.ToString("F0");
        textBehaviorDash.text = pool.avgTimesDashed.ToString("F0");
        textBehaviorRest.text = (pool.avgTimeRested * 0.01f).ToString("F0");
        textBehaviorFeed.text = pool.avgTimesPregnant.ToString("F0");

        float totalTimesActed = pool.avgTimesAttacked + pool.avgTimesDefended + pool.avgTimesDashed + 0.001f; // <-- prevent divide by 0
        imageBehaviorAttack.transform.localScale = new Vector3(1f, pool.avgTimesAttacked / totalTimesActed, 1f);
        imageBehaviorDefend.transform.localScale = new Vector3(1f, pool.avgTimesDefended / totalTimesActed, 1f);
        imageBehaviorDash.transform.localScale = new Vector3(1f, pool.avgTimesDashed / totalTimesActed, 1f);
        imageBehaviorRest.transform.localScale = new Vector3(1f, Mathf.Clamp01(pool.avgTimeRested / 1000f), 1f);
        imageBehaviorFeed.transform.localScale = new Vector3(1f, Mathf.Clamp01(pool.avgTimesPregnant / 6f), 1f);


        textEatenPlants.text = pool.avgFoodEatenPlant.ToString("F2");
        textEatenMicrobes.text = pool.avgFoodEatenZoop.ToString("F2");
        textEatenAnimals.text = pool.avgFoodEatenCreature.ToString("F2");
        textEatenEggs.text = pool.avgFoodEatenEgg.ToString("F2");
        textEatenCorpse.text = pool.avgFoodEatenCorpse.ToString("F2");

        float totalEaten = pool.avgFoodEatenPlant + pool.avgFoodEatenZoop + pool.avgFoodEatenCreature + pool.avgFoodEatenEgg + pool.avgFoodEatenCorpse + 0.001f;
        imageEatenPlants.transform.localScale = new Vector3(1f, pool.avgFoodEatenPlant / totalEaten, 1f);
        imageEatenMicrobes.transform.localScale = new Vector3(1f, pool.avgFoodEatenZoop / totalEaten, 1f);
        imageEatenAnimals.transform.localScale = new Vector3(1f, pool.avgFoodEatenCreature / totalEaten, 1f);
        imageEatenEggs.transform.localScale = new Vector3(1f, pool.avgFoodEatenEgg / totalEaten, 1f);
        imageEatenCorpse.transform.localScale = new Vector3(1f, pool.avgFoodEatenCorpse / totalEaten, 1f);
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
