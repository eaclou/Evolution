using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalResourcesUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public bool isUnlocked;
    public bool isOpen;
    public Text textGlobalMass;

    public GameObject panelGlobalResourcesMain;

    public Text textSelectedSpeciesTitle;
    public Text textSpeciationTree;
    public int selectedSpeciesIndex = 1;
    public Text textStatsBody;
  
    public Text textMeterOxygen;
    public Text textMeterNutrients;
    public Text textMeterDetritus;
    public Text textMeterDecomposers;
    public Text textMeterAlgae;
    public Text textMeterPlants;
    public Text textMeterZooplankton;
    public Text textMeterAnimals;

    public Material knowledgeGraphOxygenMat;
    public Material knowledgeGraphNutrientsMat;
    public Material knowledgeGraphDetritusMat;
    public Material knowledgeGraphDecomposersMat;
    public Material knowledgeGraphAlgaeMat;
    public Material knowledgeGraphPlantsMat;
    public Material knowledgeGraphZooplanktonMat;
    public Material knowledgeGraphVertebratesMat;

    public Texture2D statsSpeciesColorKey;
    private int maxDisplaySpecies = 32;
    public Texture2D[] statsTreeOfLifeSpeciesTexArray;
    public float[] maxValuesStatArray;
    public float[] minValuesStatArray;

    public float curSpeciesStatValue;
    public string curSpeciesStatName;
    public int selectedSpeciesStatsIndex;

    public Material speciesGraphMat;
    public Image speciesGraphImage;
    public Text textGraphStatType;

    private Texture2D brainGenomeTex;
    public Material brainGenomeMat;

	// Use this for initialization
	void Start () {
        brainGenomeTex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        brainGenomeTex.filterMode = FilterMode.Point;
        brainGenomeTex.wrapMode = TextureWrapMode.Clamp;
        brainGenomeMat.SetTexture("_MainTex", brainGenomeTex);
        //CreateBrainGenomeTexture(null);

        // Best place for these???
        if (statsSpeciesColorKey == null) {
            statsSpeciesColorKey = new Texture2D(maxDisplaySpecies, 1, TextureFormat.ARGB32, false);
            statsSpeciesColorKey.filterMode = FilterMode.Point;
            statsSpeciesColorKey.wrapMode = TextureWrapMode.Clamp;
        }
        
        statsTreeOfLifeSpeciesTexArray = new Texture2D[16]; // start with 16 choosable stats

        for (int i = 0; i < statsTreeOfLifeSpeciesTexArray.Length; i++) {
            Texture2D statsTexture = new Texture2D(maxDisplaySpecies, 1, TextureFormat.RGBAFloat, false);
            statsTexture.filterMode = FilterMode.Bilinear;
            statsTexture.wrapMode = TextureWrapMode.Clamp;

            statsTreeOfLifeSpeciesTexArray[i] = statsTexture;
        }
                
        
        maxValuesStatArray = new float[16];
        minValuesStatArray = new float[16];
        for (int i = 0; i < maxValuesStatArray.Length; i++) {
            maxValuesStatArray[i] = 0.000001f;
            minValuesStatArray[i] = 1000000f;
        }        
        
	}

    public void CreateBrainGenomeTexture(AgentGenome genome) {

        
        int curLinearIndex = 0;
        
        //brainGenomeTex:

        if (genome.brainGenome.linkList != null) {
            
            for(int i = 0; i < brainGenomeTex.width * brainGenomeTex.height; i++) {

                int xIndex = i % brainGenomeTex.width;
                int yIndex = Mathf.FloorToInt(i / brainGenomeTex.width);
                              
                Color testColor;

                if (genome.brainGenome.linkList.Count > i) {

                    float weightVal = genome.brainGenome.linkList[i].weight;
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
                    testColor = Color.gray; // CLEAR
                    curLinearIndex = i;
                    //break;
                }
                
                brainGenomeTex.SetPixel(xIndex, yIndex, testColor);
            }
        }
        else {

        }
        
        // Body Genome
        //int xI = curLinearIndex % brainGenomeTex.width;
        //int yI = Mathf.FloorToInt(curLinearIndex / brainGenomeTex.width);
        
        brainGenomeTex.Apply();
    }
    
    public void UpdateSpeciesTreeDataTextures(int year) {  // refactor using year?
        //Debug.Log("WOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO " + year.ToString());
        int numActiveSpecies = uiManagerRef.gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList.Count;
        int numTotalSpecies = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count;

        //int maxDisplaySpecies = 32;
        int[] displaySpeciesIndicesArray;
        displaySpeciesIndicesArray = new int[maxDisplaySpecies];


        TheRenderKing.TreeOfLifeSpeciesKeyData[] speciesKeyDataArray = new TheRenderKing.TreeOfLifeSpeciesKeyData[32];

         // Get Active ones first:
        for(int i = 0; i < uiManagerRef.gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList.Count; i++) {
            SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i]];
            SpeciesGenomePool parentPool;
            Vector3 parentHue = Vector3.one;
            if(pool.parentSpeciesID != -1) {
                parentPool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[pool.parentSpeciesID];
                parentHue = parentPool.representativeGenome.bodyGenome.appearanceGenome.huePrimary;
            }
            Vector3 huePrimary = pool.representativeGenome.bodyGenome.appearanceGenome.huePrimary;
            Vector3 hueSecondary = pool.representativeGenome.bodyGenome.appearanceGenome.hueSecondary;
            
            statsSpeciesColorKey.SetPixel(i, 1, new Color(huePrimary.x, huePrimary.y, huePrimary.z));            
            //Debug.Log("(" + i.ToString() + ", " + gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i].ToString());
            displaySpeciesIndicesArray[i] = uiManagerRef.gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i];

            TheRenderKing.TreeOfLifeSpeciesKeyData keyData = new TheRenderKing.TreeOfLifeSpeciesKeyData();
            keyData.timeCreated = pool.timeStepCreated;  // Use TimeSteps instead of Years???
            keyData.timeExtinct = pool.timeStepExtinct;
            keyData.huePrimary = huePrimary;
            keyData.hueSecondary = hueSecondary;
            keyData.parentHue = parentHue;
            keyData.isExtinct = pool.isExtinct ? 1f : 0f;
            keyData.isOn = 1f;
            //int selectedID = treeOfLifeManager.selectedID;
            
            keyData.isSelected = (selectedSpeciesIndex == uiManagerRef.gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i]) ? 1f : 0f;

            speciesKeyDataArray[i] = keyData;
        }
        
        // Then fill with most recently extinct:
        for(int i = (numTotalSpecies - 1); i > Mathf.Clamp((numTotalSpecies - maxDisplaySpecies), 0, numTotalSpecies); i--) {
            SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[i];
            SpeciesGenomePool parentPool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[pool.parentSpeciesID];

            Vector3 huePrimary = pool.representativeGenome.bodyGenome.appearanceGenome.huePrimary;
            Vector3 hueSecondary = pool.representativeGenome.bodyGenome.appearanceGenome.hueSecondary;
            Vector3 parentHue = parentPool.representativeGenome.bodyGenome.appearanceGenome.huePrimary;
            if(uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[i].isExtinct) {
                huePrimary = Vector3.Lerp(huePrimary, Vector3.one * 0.2f, 0.75f);
            }
            statsSpeciesColorKey.SetPixel(i, 1, new Color(huePrimary.x, huePrimary.y, huePrimary.z));
            
            displaySpeciesIndicesArray[i] = i;

            TheRenderKing.TreeOfLifeSpeciesKeyData keyData = new TheRenderKing.TreeOfLifeSpeciesKeyData();
            keyData.timeCreated = pool.timeStepCreated;  // Use TimeSteps instead of Years???
            keyData.timeExtinct = pool.timeStepExtinct;
            keyData.huePrimary = huePrimary;
            keyData.hueSecondary = hueSecondary;
            keyData.parentHue = parentHue;
            keyData.isExtinct = pool.isExtinct ? 1f : 0f;
            keyData.isOn = 1f;
            if(i >= uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count) {
                keyData.isOn = 0f;
            }
            if(pool.yearCreated == -1) {
                keyData.isOn = 0f;
            }
            if(i == 0) {
                keyData.isOn = 0f;
            }
            
            keyData.isSelected = selectedSpeciesIndex == i ? 1f : 0f;

            speciesKeyDataArray[i] = keyData;
        }
        statsSpeciesColorKey.Apply();

        //uiManagerRef.gameManager.simulationManager.theRenderKing.treeOfLifeSpeciesDataKeyCBuffer.SetData(speciesKeyDataArray);

        // ========== data: =========== //
        int years = Mathf.Min(2048, year);  // cap textures at 2k for now?
        years = Mathf.Max(1, years);
        // check for resize before doing it?
        for(int i = 0; i < statsTreeOfLifeSpeciesTexArray.Length; i++) {
            statsTreeOfLifeSpeciesTexArray[i].Resize(years, maxDisplaySpecies);
        }
        
        for(int i = 0; i < maxValuesStatArray.Length; i++) {
            maxValuesStatArray[i] = 0.0000001f;
            minValuesStatArray[i] = 1000000f;
        }
        // for each year & each species, create 2D texture with fitness scores:
        for(int s = 0; s < maxDisplaySpecies; s++) {            
            
            if(displaySpeciesIndicesArray[s] < uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count) {

                SpeciesGenomePool speciesPool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[displaySpeciesIndicesArray[s]];
                if(speciesPool == null) {
                    Debug.LogError("well, shit");
                }
                for(int t = 0; t < years; t++) {
                
                    //int index = t - speciesPool.yearCreated;
                    
                    for(int a = 0; a < statsTreeOfLifeSpeciesTexArray.Length; a++) {
                        float valStat = 0f;
                        //= speciesPool
                        // I know there's a better way to do this:
                        if(a == 0) {
                            valStat = speciesPool.avgLifespanPerYearList[t];
                        }
                        else if(a == 1) {
                            valStat = speciesPool.avgConsumptionDecayPerYearList[t];
                        }
                        else if(a == 2) {
                            valStat = speciesPool.avgConsumptionPlantPerYearList[t];
                        }
                        else if(a == 3) {
                            valStat = speciesPool.avgConsumptionMeatPerYearList[t];
                        }
                        else if(a == 4) {
                            valStat = speciesPool.avgBodySizePerYearList[t];
                        }
                        else if(a == 5) {
                            valStat = speciesPool.avgSpecAttackPerYearList[t];
                        }
                        else if(a == 6) {
                            valStat = speciesPool.avgSpecDefendPerYearList[t];
                        }
                        else if(a == 7) {
                            valStat = speciesPool.avgSpecSpeedPerYearList[t];
                        }
                        else if(a == 8) {
                            valStat = speciesPool.avgSpecUtilityPerYearList[t];
                        }
                        else if(a == 9) {
                            valStat = speciesPool.avgFoodSpecDecayPerYearList[t];
                        }
                        else if(a == 10) {
                            valStat = speciesPool.avgFoodSpecPlantPerYearList[t];
                        }
                        else if(a == 11) {
                            valStat = speciesPool.avgFoodSpecMeatPerYearList[t];
                        }
                        else if(a == 12) {
                            valStat = speciesPool.avgNumNeuronsPerYearList[t];
                        }
                        else if(a == 13) {
                            valStat = speciesPool.avgNumAxonsPerYearList[t];
                        }
                        else if(a == 14) {
                            valStat = speciesPool.avgDamageDealtPerYearList[t];
                        }
                        else if(a == 15) {
                            valStat = speciesPool.avgDamageTakenPerYearList[t];
                        }

                        
                        minValuesStatArray[a] = Mathf.Min(minValuesStatArray[a], valStat);                        
                        maxValuesStatArray[a] = Mathf.Max(maxValuesStatArray[a], valStat);
                                                
                        statsTreeOfLifeSpeciesTexArray[a].SetPixel(t, s, new Color(valStat, valStat, valStat, 1f));
                    }                    
                }                
            }            
        }
        
        for (int b = 0; b < statsTreeOfLifeSpeciesTexArray.Length; b++) {
            statsTreeOfLifeSpeciesTexArray[b].Apply();
        }
        //int selectedSpeciesID = treeOfLifeManager.selectedID;
        

        RefreshGraphMaterial();

        
        //speciesGraphMat.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[selectedSpeciesStatsIndex]);
        //speciesGraphMat.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
        //speciesGraphMat.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[selectedSpeciesStatsIndex].width);
        //speciesGraphMat.SetInt("_SelectedSpeciesID", selectedSpeciesIndex);
        /*
        uniform float _MinValue;
			uniform float _MaxValue;

			uniform int _SelectedSpeciesID;
			uniform int _NumDisplayed;
			uniform float _NumEntries;
    */
    }
    

    public void ClickCycleSpecies() {
        selectedSpeciesIndex++;

        if(selectedSpeciesIndex >= uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count) {
            selectedSpeciesIndex = 0;
        }

        SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex];

        UpdateSpeciesTreeDataTextures(uiManagerRef.gameManager.simulationManager.curSimYear);        

        CreateBrainGenomeTexture(pool.representativeGenome);
        uiManagerRef.gameManager.simulationManager.theRenderKing.InitializeCreaturePortraitGenomes(pool.representativeGenome);
    }

    public void ClickCycleCandidate() {
        SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex];
        if(pool.candidateGenomesList.Count > 0) {
            int randIndex = UnityEngine.Random.Range(0, pool.candidateGenomesList.Count - 1);
            AgentGenome genome = pool.candidateGenomesList[randIndex].candidateGenome;
            CreateBrainGenomeTexture(genome);
            uiManagerRef.gameManager.simulationManager.theRenderKing.InitializeCreaturePortraitGenomes(genome);
        }
        
        
    }

    public void ClickCycleGraphStatsIndex() {
        selectedSpeciesStatsIndex++;

        if(selectedSpeciesStatsIndex >= statsTreeOfLifeSpeciesTexArray.Length) {
            selectedSpeciesStatsIndex = 0;
        }

        //
        UpdateSpeciesTreeDataTextures(uiManagerRef.gameManager.simulationManager.curSimYear);

        //RefreshGraphMaterial();
    }

    private void RefreshGraphMaterial() {

        speciesGraphMat.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[selectedSpeciesStatsIndex]);
        speciesGraphMat.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
        speciesGraphMat.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[selectedSpeciesStatsIndex].width);
        speciesGraphMat.SetInt("_SelectedSpeciesID", selectedSpeciesIndex);
        speciesGraphMat.SetFloat("_MaximumValue", maxValuesStatArray[selectedSpeciesStatsIndex]);
        speciesGraphMat.SetFloat("_MinimumValue", minValuesStatArray[selectedSpeciesStatsIndex]);

        speciesGraphImage.material = speciesGraphMat;
        speciesGraphImage.gameObject.SetActive(false);
        speciesGraphImage.gameObject.SetActive(true);
    }
    

    public void ClickToolButton() {
        Debug.Log("Click mutation toggle button)");
        isOpen = !isOpen;
    }

    private void UpdateUI() {
        

        textGlobalMass.text = "Global Biomass: " + uiManagerRef.gameManager.simulationManager.simResourceManager.curTotalMass.ToString("F0");
        SimResourceManager resourcesRef = uiManagerRef.gameManager.simulationManager.simResourceManager;
        //textMeterOxygen.text = resourcesRef.curGlobalOxygen.ToString("F0");
        textMeterNutrients.text = resourcesRef.curGlobalNutrients.ToString("F0");
        textMeterDetritus.text = resourcesRef.curGlobalDetritus.ToString("F0");
        textMeterDecomposers.text = resourcesRef.curGlobalDecomposers.ToString("F0");
        textMeterAlgae.text = (resourcesRef.curGlobalAlgaeReservoir).ToString("F0");
        textMeterPlants.text = (resourcesRef.curGlobalPlantParticles).ToString("F0");
        textMeterZooplankton.text = (resourcesRef.curGlobalAnimalParticles).ToString("F0");
        textMeterAnimals.text = (resourcesRef.curGlobalAgentBiomass).ToString("F2");

        textSelectedSpeciesTitle.text = "Selected Species:\n" + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].representativeGenome.bodyGenome.coreGenome.name;

        string speciesDebugStr = "";
        int numActiveSpecies = uiManagerRef.gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList.Count;
        speciesDebugStr += numActiveSpecies.ToString() + " Active Species:\n";
        for (int s = 0; s < numActiveSpecies; s++) {
            int speciesID = uiManagerRef.gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[s];
            int parentSpeciesID = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].parentSpeciesID;
            int numCandidates = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].candidateGenomesList.Count;
            int numLeaders = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].leaderboardGenomesList.Count;
            int numBorn = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].numAgentsEvaluated;
            int speciesPopSize = 0;
            float avgFitness = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgLifespan;
            for (int a = 0; a < uiManagerRef.gameManager.simulationManager._NumAgents; a++) {
                if (uiManagerRef.gameManager.simulationManager.agentsArray[a].speciesIndex == speciesID) {
                    speciesPopSize++;
                }
            }
            if(uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].isFlaggedForExtinction) {
                speciesDebugStr += "xxx ";
            }
            speciesDebugStr += "Species[" + speciesID.ToString() + "] p(" + parentSpeciesID.ToString() + "), size: " + speciesPopSize.ToString() + ", #cands: " + numCandidates.ToString() + ", numEvals: " + numBorn.ToString() + 
                            ",   avgFitness: " + avgFitness.ToString("F2") + 
                            ",   avgConsumption: (" + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgConsumptionDecay.ToString("F4") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgConsumptionPlant.ToString("F4") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgConsumptionMeat.ToString("F4") +
                            "),   avgBodySize: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgBodySize.ToString("F3") +
                            ",   avgTalentSpec: (" + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecAttack.ToString("F2") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecDefend.ToString("F2") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecSpeed.ToString("F2") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecUtility.ToString("F2") +
                            "),   avgDiet: (" + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodSpecDecay.ToString("F2") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodSpecPlant.ToString("F2") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodSpecMeat.ToString("F2") +
                            "),   avgNumNeurons: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgNumNeurons.ToString("F1") +
                            ",   avgNumAxons: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgNumAxons.ToString("F1") +
                            ", avgFitness: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFitnessScore.ToString("F2") +
                            ", avgExp: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgExperience.ToString() + "\n\n";
        }


        string speciesTreeString = "SPECIES LIST:\n";
        for (int s = 0; s < numActiveSpecies; s++) {
            int speciesID = uiManagerRef.gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[s];
            int parentSpeciesID = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].parentSpeciesID;
            if(selectedSpeciesIndex == speciesID) {
                speciesTreeString += "**SELECTED** ";
            }
            if(uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].isFlaggedForExtinction) {
                speciesTreeString += "(extinct) ";
            }
            speciesTreeString += "Species[" + speciesID.ToString() + "] " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].representativeGenome.bodyGenome.coreGenome.name + " p(" + parentSpeciesID.ToString() + ")\n";
        }
        
        textSpeciationTree.text = speciesTreeString; // speciesDebugStr;

        //Debug.Break(); 

        SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex];
        if(pool != null) {
            string speciesStatsString = "SPECIES[" + selectedSpeciesIndex.ToString() + "] " + pool.representativeGenome.bodyGenome.coreGenome.name + " STATS:\n\n";

            int parentSpeciesID = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].parentSpeciesID;
            int numCandidates = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].candidateGenomesList.Count;
            int numLeaders = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].leaderboardGenomesList.Count;
            int numBorn = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].numAgentsEvaluated;
            int speciesPopSize = 0;
            float avgFitness = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].avgLifespan;
            for (int a = 0; a < uiManagerRef.gameManager.simulationManager._NumAgents; a++) {
                if (uiManagerRef.gameManager.simulationManager.agentsArray[a].speciesIndex == selectedSpeciesIndex) {
                    speciesPopSize++;
                }
            }

            
            string debugTxtGlobalSim = "";
            debugTxtGlobalSim += "\n\nNumCreaturesBorn: " + uiManagerRef.gameManager.simulationManager.numAgentsBorn.ToString() + ", numDied: " + uiManagerRef.gameManager.simulationManager.numAgentsDied.ToString() + ", ~Gen: " + ((float)uiManagerRef.gameManager.simulationManager.numAgentsBorn / (float)uiManagerRef.gameManager.simulationManager._NumAgents).ToString();
            debugTxtGlobalSim += "\nSimulation Age: " + uiManagerRef.gameManager.simulationManager.simAgeTimeSteps.ToString();
            debugTxtGlobalSim += "\nYear " + uiManagerRef.gameManager.simulationManager.curSimYear.ToString() + "\n\n";


            speciesStatsString += "Species[" + selectedSpeciesIndex.ToString() + "] p(" + parentSpeciesID.ToString() + "), size: " + speciesPopSize.ToString() + ", #cands: " + numCandidates.ToString() + ", numEvals: " + numBorn.ToString() +
                            "\navgFitness: " + avgFitness.ToString("F2") +
                            "\navgConsumption: (C: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].avgConsumptionDecay.ToString("F4") +
                            ", P: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].avgConsumptionPlant.ToString("F4") + ", M: " +
                            uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].avgConsumptionMeat.ToString("F4") + ")";

                            //"\navgTalentSpec: (" + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].avgSpecAttack.ToString("F2") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].avgSpecDefend.ToString("F2") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].avgSpecSpeed.ToString("F2") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].avgSpecUtility.ToString("F2") +
                            //")\navgDiet: (" + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].avgFoodSpecDecay.ToString("F2") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].avgFoodSpecPlant.ToString("F2") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].avgFoodSpecMeat.ToString("F2") +
                            //"\navgFitness: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].avgFitnessScore.ToString("F2");
        

            string debugTxtAgent = "\n\n";
            debugTxtAgent += "Base Size: " + pool.representativeGenome.bodyGenome.coreGenome.creatureBaseLength.ToString("F2") + ",  Aspect: " + pool.representativeGenome.bodyGenome.coreGenome.creatureAspectRatio.ToString("F2") + "\n";
            debugTxtAgent += "AvgBodySize: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].avgBodySize.ToString("F3") + "\n";
                            
            debugTxtAgent += "Fullsize Dimensions: ( " + pool.representativeGenome.bodyGenome.GetFullsizeBoundingBox().x.ToString("F2") + ", " + pool.representativeGenome.bodyGenome.GetFullsizeBoundingBox().y.ToString("F2") + ", " + pool.representativeGenome.bodyGenome.GetFullsizeBoundingBox().z.ToString("F2") + " )\n";
            debugTxtAgent += "BONUS - Damage: " + pool.representativeGenome.bodyGenome.coreGenome.talentSpecializationAttack.ToString("F2") + ", Speed: " + pool.representativeGenome.bodyGenome.coreGenome.talentSpecializationSpeed.ToString("F2") + ", Health: " + pool.representativeGenome.bodyGenome.coreGenome.talentSpecializationDefense.ToString("F2") + ", Energy: " + pool.representativeGenome.bodyGenome.coreGenome.talentSpecializationUtility.ToString("F2") + "\n";
            debugTxtAgent += "DIET - Decay: " + pool.representativeGenome.bodyGenome.coreGenome.dietSpecializationDecay.ToString("F2") + ", Plant: " + pool.representativeGenome.bodyGenome.coreGenome.dietSpecializationPlant.ToString("F2") + ", Meat: " + pool.representativeGenome.bodyGenome.coreGenome.dietSpecializationMeat.ToString("F2") + "\n";

            debugTxtAgent += "avgNumNeurons: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].avgNumNeurons.ToString("F1");
            debugTxtAgent += "\navgNumAxons: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex].avgNumAxons.ToString("F1");
                
            string sensorString = "\nSENSORS_ETC:\n";
            AgentGenome genome = pool.representativeGenome;
        
            string mouthA = "Mouth Type: ";
            if (genome.bodyGenome.coreGenome.isPassive) {
                mouthA += "Passive";
            }
            else {
                mouthA += "Active?";
            }
            sensorString += mouthA + "\n";

            sensorString += "PriCol: " + genome.bodyGenome.appearanceGenome.huePrimary.ToString();
            sensorString += ", SecColor: " + genome.bodyGenome.appearanceGenome.hueSecondary.ToString() + "\n";
            // proportions:
            sensorString += "BODY Proportions: [" + genome.bodyGenome.coreGenome.mouthLength.ToString();
            sensorString += ", " + genome.bodyGenome.coreGenome.headLength.ToString();
            sensorString += ", " + genome.bodyGenome.coreGenome.bodyLength.ToString();
            sensorString += ", " + genome.bodyGenome.coreGenome.tailLength.ToString() + "]\n";        
            // eyes
            sensorString += "EYE: SocRad: " + genome.bodyGenome.coreGenome.socketRadius.ToString();
            sensorString += ", SocHght: " + genome.bodyGenome.coreGenome.socketHeight.ToString() + "\n";
            // tail
            sensorString += "TAIL: FinLength: " + genome.bodyGenome.coreGenome.tailFinBaseLength.ToString();
            sensorString += ", FinSpread: " + genome.bodyGenome.coreGenome.tailFinSpreadAngle.ToString();
            // sensors & shit:
                        
            sensorString += "\nSENSORS:\nComms: " + (genome.bodyGenome.communicationGenome.useComms ? "off" : "ON!");
            sensorString += "\nWater: " + (genome.bodyGenome.environmentalGenome.useWaterStats ? "off" : "ON!");
            sensorString += "\nWallsCard: " + (genome.bodyGenome.environmentalGenome.useCardinals ? "off" : "ON!");
            sensorString += "\nWallsDiag: " + (genome.bodyGenome.environmentalGenome.useDiagonals ? "off" : "ON!");
            sensorString += "\nNutrients: " + (genome.bodyGenome.foodGenome.useNutrients ? "off" : "ON!");
            sensorString += "\nFoodPos: " + (genome.bodyGenome.foodGenome.usePos ? "off" : "ON!");
            sensorString += "\nFoodVel: " + (genome.bodyGenome.foodGenome.useVel ? "off" : "ON!");
            sensorString += "\nFoodDir: " + (genome.bodyGenome.foodGenome.useDir ? "off" : "ON!");
            sensorString += "\nFoodInfo: " + (genome.bodyGenome.foodGenome.useStats ? "off" : "ON!");
            sensorString += "\nEggsack: " + (genome.bodyGenome.foodGenome.useEggs ? "off" : "ON!");
            sensorString += "\nCorpse: " + (genome.bodyGenome.foodGenome.useCorpse ? "off" : "ON!");
            sensorString += "\nFriendPos: " + (genome.bodyGenome.friendGenome.usePos ? "off" : "ON!");
            sensorString += "\nFriendVel: " + (genome.bodyGenome.friendGenome.useVel ? "off" : "ON!");
            sensorString += "\nFriendDir: " + (genome.bodyGenome.friendGenome.useDir ? "off" : "ON!");
            sensorString += "\nThreatPos: " + (genome.bodyGenome.threatGenome.usePos ? "off" : "ON!");
            sensorString += "\nThreatVel: " + (genome.bodyGenome.threatGenome.useVel ? "off" : "ON!");
            sensorString += "\nThreatDir: " + (genome.bodyGenome.threatGenome.useDir ? "off" : "ON!");
            sensorString += "\nThreatInfo: " + (genome.bodyGenome.threatGenome.useStats ? "off" : "ON!");
        
        

            textStatsBody.text = debugTxtGlobalSim + speciesStatsString + debugTxtAgent + sensorString;
        }

        SpeciesGenomePool selectedPool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex];
        
        if(selectedSpeciesStatsIndex == 0) {
            curSpeciesStatValue = selectedPool.avgLifespan;
            curSpeciesStatName = "Avg Lifespan";
        }
        else if(selectedSpeciesStatsIndex == 1) {
            curSpeciesStatValue = selectedPool.avgConsumptionDecay;
            curSpeciesStatName = "Avg Carrion Eaten";
        }
        else if(selectedSpeciesStatsIndex == 2) {
            curSpeciesStatValue = selectedPool.avgConsumptionPlant;
            curSpeciesStatName = "Avg Plants Eaten";
        }
        else if(selectedSpeciesStatsIndex == 3) {
            curSpeciesStatValue = selectedPool.avgConsumptionMeat;
            curSpeciesStatName = "Avg Meat Eaten";
        }
        else if(selectedSpeciesStatsIndex == 4) {
            curSpeciesStatValue = selectedPool.avgBodySize;
            curSpeciesStatName = "Avg Body Weight";
        }
        else if(selectedSpeciesStatsIndex == 5) {
            curSpeciesStatValue = selectedPool.avgSpecAttack;
            curSpeciesStatName = "Specialization: Attack";
        }
        else if(selectedSpeciesStatsIndex == 6) {
            curSpeciesStatValue = selectedPool.avgSpecDefend;
            curSpeciesStatName = "Specialization: Defense";
        }
        else if(selectedSpeciesStatsIndex == 7) {
            curSpeciesStatValue = selectedPool.avgSpecSpeed;
            curSpeciesStatName = "Specialization: Speed";
        }
        else if(selectedSpeciesStatsIndex == 8) {
            curSpeciesStatValue = selectedPool.avgSpecUtility;
            curSpeciesStatName = "Specialization: Utility";
        }
        else if(selectedSpeciesStatsIndex == 9) {
            curSpeciesStatValue = selectedPool.avgFoodSpecDecay;
            curSpeciesStatName = "Digestion Efficiency: Carrion";
        }
        else if(selectedSpeciesStatsIndex == 10) {
            curSpeciesStatValue = selectedPool.avgFoodSpecPlant;
            curSpeciesStatName = "Digestion Efficiency: Plants";
        }
        else if(selectedSpeciesStatsIndex == 11) {
            curSpeciesStatValue = selectedPool.avgFoodSpecMeat;
            curSpeciesStatName = "Digestion Efficiency: Meat";
        }
        else if(selectedSpeciesStatsIndex == 12) {
            curSpeciesStatValue = selectedPool.avgNumNeurons;
            curSpeciesStatName = "Total Neurons in Brain";
        }
        else if(selectedSpeciesStatsIndex == 13) {
            curSpeciesStatValue = selectedPool.avgNumAxons;
            curSpeciesStatName = "Total Axons in Brain";
        }        
        else if(selectedSpeciesStatsIndex == 14) {
            curSpeciesStatValue = selectedPool.avgDamageDealt;
            curSpeciesStatName = "Damage Dealt";
        }
        else if(selectedSpeciesStatsIndex == 15) {
            curSpeciesStatValue = selectedPool.avgDamageTaken;
            curSpeciesStatName = "Damage Taken";
        }

        textGraphStatType.text = "[" + selectedSpeciesStatsIndex.ToString() + "]" + curSpeciesStatName + ":  " + curSpeciesStatValue.ToString() + ", M: " + maxValuesStatArray[selectedSpeciesStatsIndex].ToString() + ", m: " + minValuesStatArray[selectedSpeciesStatsIndex].ToString();
        RefreshGraphMaterial();

        
    }

    public void UpdateGlobalResourcesPanelUpdate() {
        
        this.isOpen = uiManagerRef.knowledgeUI.isOpen;
        

        panelGlobalResourcesMain.SetActive(isOpen);
        if(isOpen) {
            UpdateUI();
        }
    }   
}
