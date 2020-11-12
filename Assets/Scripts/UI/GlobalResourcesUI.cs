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
    public Image imageSelectedSpeciesBG;
    public Text textSpeciationTree;    
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
    private int maxDisplaySpecies = 32;  // **** WILL BECOME A PROBLEM!!!! ****
    public Texture2D[] statsTreeOfLifeSpeciesTexArray;
    public float[] maxValuesStatArray;
    public float[] minValuesStatArray;

    public float curSpeciesStatValue;
    public string curSpeciesStatName;
    public int selectedSpeciesStatsIndex;

    public Material speciesGraphMatLeft;
    public Material speciesGraphMatCenter;
    public Material speciesGraphMatRight;

    public Text textGraphCategory;
    //public GameObject graphPanelGO;
    public Text textGraphStatsLeft;
    public Text textGraphStatsCenter;
    public Text textGraphStatsRight;
    //public Image imageCreaturePortrait;

    private Texture2D brainGenomeTex;
    public Material brainGenomeMat;
    private Texture2D speciesPoolGenomeTex;

    public GameObject treeAnchorUI;
    public GameObject prefabSpeciesBar;

    //public int selectedSpeciesIndex = 1;
    //public AgentGenome focusedAgentGenome;    
    //private int agentSelectType = 2;
    //public int agentIndex;

    
    public GraphCategory selectedGraphCategory;
    public enum GraphCategory {
        Life,
        Body,
        Talents,
        Eaten,
        DigestSpec
    }
	// Use this for initialization
	void Start () {
        brainGenomeTex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        brainGenomeTex.filterMode = FilterMode.Point;
        brainGenomeTex.wrapMode = TextureWrapMode.Clamp;
        brainGenomeMat.SetTexture("_MainTex", brainGenomeTex);
        //CreateBrainGenomeTexture(null);

        
        speciesPoolGenomeTex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        speciesPoolGenomeTex.filterMode = FilterMode.Point;
        speciesPoolGenomeTex.wrapMode = TextureWrapMode.Clamp;
        brainGenomeMat.SetTexture("_MainTex", speciesPoolGenomeTex);

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

    public void ClickButtonGraphType(int buttonID) {
        selectedGraphCategory = (GraphCategory)buttonID;
        
        //selectedGraphCategory = graphCat;
        //selectedSpeciesStatsIndex = i;
        //if(selectedSpeciesStatsIndex >= statsTreeOfLifeSpeciesTexArray.Length) {
        //    selectedSpeciesStatsIndex = 0;
        //}
        //
        //SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex];
        //CreateSpeciesLeaderboardGenomeTexture(pool);
        //CreateBrainGenomeTexture(focusedAgentGenome);
        //uiManagerRef.gameManager.simulationManager.theRenderKing.InitializeCreaturePortraitGenomes(focusedAgentGenome);
        UpdateSpeciesTreeDataTextures(uiManagerRef.gameManager.simulationManager.curSimYear);
        
    }
    public void CreateSpeciesLeaderboardGenomeTexture() {
        int width = 128;
        speciesPoolGenomeTex.Resize(width, 1); // pool.leaderboardGenomesList.Count);
        SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID];
        //for(int i = 0; i < pool.leaderboardGenomesList.Count; i++) {
        AgentGenome genome = uiManagerRef.focusedCandidate.candidateGenome;
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
                testColor = Color.gray; // CLEAR
                    
                //break;
            }
                
            speciesPoolGenomeTex.SetPixel(xIndex, yIndex, testColor);
        }
       
        //}
          
            
        
        // Body Genome
        //int xI = curLinearIndex % speciesPoolGenomeTex.width;
        //int yI = Mathf.FloorToInt(curLinearIndex / speciesPoolGenomeTex.width);
        
        speciesPoolGenomeTex.Apply();
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
                parentHue = parentPool.representativeCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
            }
            Vector3 huePrimary = pool.representativeCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
            Vector3 hueSecondary = pool.representativeCandidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary;
            
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
            
            keyData.isSelected = (uiManagerRef.selectedSpeciesID == uiManagerRef.gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i]) ? 1f : 0f;

            speciesKeyDataArray[i] = keyData;
        }
        
        // Then fill with most recently extinct:
        for(int i = (numTotalSpecies - 1); i > Mathf.Clamp((numTotalSpecies - maxDisplaySpecies), 0, numTotalSpecies); i--) {
            SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[i];

            SpeciesGenomePool parentPool;
            if (pool.parentSpeciesID == -1) {
                parentPool = pool; // whoa man...
            }
            else {
                parentPool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[pool.parentSpeciesID];
            }            

            Vector3 huePrimary = pool.representativeCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
            Vector3 hueSecondary = pool.representativeCandidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary;
            Vector3 parentHue = parentPool.representativeCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
            if(uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[i].isExtinct) {
                huePrimary = Vector3.zero;
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
            
            keyData.isSelected = uiManagerRef.selectedSpeciesID == i ? 1f : 0f;

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
                            valStat = speciesPool.avgFoodEatenEggPerYearList[t];
                        }
                        else if(a == 2) {
                            valStat = speciesPool.avgFoodEatenPlantPerYearList[t];
                        }
                        else if(a == 3) {
                            valStat = speciesPool.avgFoodEatenZoopPerYearList[t];// + speciesPool.avgFoodEatenCreaturePerYearList[t] + speciesPool.avgFoodEatenEggPerYearList[t];
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
    /*
    public void ClickCycleSpecies() {
        Debug.Log("CycleSpecies: " + selectedSpeciesIndex.ToString());

        selectedSpeciesIndex++;

        if(selectedSpeciesIndex >= uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count) {
            selectedSpeciesIndex = 0;
        }        
        
        SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex];
        if(pool.isExtinct) {
            for(int i = selectedSpeciesIndex; i < uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList.Count; i++) {
                if(uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[i].isExtinct) {

                }
                else {
                    selectedSpeciesIndex = i;
                    break;
                }
            }
        }
        SetSelectedSpeciesUI(selectedSpeciesIndex);

        uiManagerRef.cameraManager.isFollowingAgent = false;
        uiManagerRef.watcherUI.StopFollowingAgent();     
    }
    
    */
    /*
    public void ClickCycleCandidate() {
        SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex];
        CreateSpeciesLeaderboardGenomeTexture(pool);
        if(pool.candidateGenomesList.Count > 0) {
            CreateBrainGenomeTexture(focusedAgentGenome);
            uiManagerRef.gameManager.simulationManager.theRenderKing.InitializeCreaturePortraitGenomes(focusedAgentGenome);
        }       
        
    }
    */
    
    
    /*public void ClickToggleAgentSelectType() {
        agentIndex = 0;
        agentSelectType = (agentSelectType + 1) % 3;

        //UpdateFocusedAgentGenome();

        SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[selectedSpeciesIndex];
        CreateSpeciesLeaderboardGenomeTexture(pool);        
        CreateBrainGenomeTexture(focusedAgentGenome);
        uiManagerRef.gameManager.simulationManager.theRenderKing.InitializeCreaturePortraitGenomes(focusedAgentGenome);
        UpdateSpeciesTreeDataTextures(uiManagerRef.gameManager.simulationManager.curSimYear);
    }*/
    /*
    public void ClickCycleAgentIndex() {
        if(agentSelectType == 0) {

        }
        else {
            agentIndex++;  
            
            UpdateFocusedAgentGenome();

            CreateBrainGenomeTexture(focusedAgentGenome);
            uiManagerRef.gameManager.simulationManager.theRenderKing.InitializeCreaturePortraitGenomes(focusedAgentGenome);
            UpdateSpeciesTreeDataTextures(uiManagerRef.gameManager.simulationManager.curSimYear);
        }        
    }
    */
    private void RefreshGraphMaterial() {
        SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID];

        switch(selectedGraphCategory) {
            case GraphCategory.Life:
                speciesGraphMatLeft.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[0]);
                speciesGraphMatLeft.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatLeft.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[0].width);
                speciesGraphMatLeft.SetInt("_SelectedSpeciesID", uiManagerRef.selectedSpeciesID);
                speciesGraphMatLeft.SetFloat("_MaximumValue", maxValuesStatArray[0]);
                speciesGraphMatLeft.SetFloat("_MinimumValue", minValuesStatArray[0]);

                speciesGraphMatCenter.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[14]);
                speciesGraphMatCenter.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatCenter.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[14].width);
                speciesGraphMatCenter.SetInt("_SelectedSpeciesID", uiManagerRef.selectedSpeciesID);
                speciesGraphMatCenter.SetFloat("_MaximumValue", maxValuesStatArray[14]);
                speciesGraphMatCenter.SetFloat("_MinimumValue", minValuesStatArray[14]);

                speciesGraphMatRight.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[15]);
                speciesGraphMatRight.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatRight.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[15].width);
                speciesGraphMatRight.SetInt("_SelectedSpeciesID", uiManagerRef.selectedSpeciesID);
                speciesGraphMatRight.SetFloat("_MaximumValue", maxValuesStatArray[15]);
                speciesGraphMatRight.SetFloat("_MinimumValue", minValuesStatArray[15]);

                //curSpeciesStatValue = pool.avgDamageDealt;
            //curSpeciesStatName = "Damage Dealt";
                textGraphStatsLeft.text = "LIFESPAN\n" + pool.avgLifespan.ToString(); // + ", M: " + maxValuesStatArray[selectedSpeciesStatsIndex].ToString() + ", m: " + minValuesStatArray[selectedSpeciesStatsIndex].ToString();
                textGraphStatsCenter.text = "DMG DEALT\n" + pool.avgDamageDealt.ToString();
                textGraphStatsRight.text = "DMG TAKEN\n" + pool.avgDamageTaken.ToString();

                textGraphCategory.text = "HEALTH";
                break;
            case GraphCategory.Body:
                speciesGraphMatLeft.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[4]);
                speciesGraphMatLeft.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatLeft.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[4].width);
                speciesGraphMatLeft.SetInt("_SelectedSpeciesID", uiManagerRef.selectedSpeciesID);
                speciesGraphMatLeft.SetFloat("_MaximumValue", maxValuesStatArray[4]);
                speciesGraphMatLeft.SetFloat("_MinimumValue", minValuesStatArray[4]);
                
                speciesGraphMatCenter.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[12]);
                speciesGraphMatCenter.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatCenter.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[12].width);
                speciesGraphMatCenter.SetInt("_SelectedSpeciesID", uiManagerRef.selectedSpeciesID);
                speciesGraphMatCenter.SetFloat("_MaximumValue", maxValuesStatArray[12]);
                speciesGraphMatCenter.SetFloat("_MinimumValue", minValuesStatArray[12]);

                speciesGraphMatRight.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[13]);
                speciesGraphMatRight.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatRight.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[13].width);
                speciesGraphMatRight.SetInt("_SelectedSpeciesID", uiManagerRef.selectedSpeciesID);
                speciesGraphMatRight.SetFloat("_MaximumValue", maxValuesStatArray[13]);
                speciesGraphMatRight.SetFloat("_MinimumValue", minValuesStatArray[13]);

                textGraphStatsLeft.text = "BODYSIZE\n" + pool.avgBodySize.ToString();
                textGraphStatsCenter.text = "NEURONS\n" + pool.avgNumNeurons.ToString();
                textGraphStatsRight.text = "AXONS\n" + pool.avgNumAxons.ToString();

                textGraphCategory.text = "BODY & BRAIN";
                break;
            case GraphCategory.Talents:
                speciesGraphMatLeft.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[5]);
                speciesGraphMatLeft.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatLeft.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[5].width);
                speciesGraphMatLeft.SetInt("_SelectedSpeciesID", uiManagerRef.selectedSpeciesID);
                speciesGraphMatLeft.SetFloat("_MaximumValue", maxValuesStatArray[5]);
                speciesGraphMatLeft.SetFloat("_MinimumValue", minValuesStatArray[5]);
                
                speciesGraphMatCenter.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[6]);
                speciesGraphMatCenter.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatCenter.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[6].width);
                speciesGraphMatCenter.SetInt("_SelectedSpeciesID", uiManagerRef.selectedSpeciesID);
                speciesGraphMatCenter.SetFloat("_MaximumValue", maxValuesStatArray[6]);
                speciesGraphMatCenter.SetFloat("_MinimumValue", minValuesStatArray[6]);

                speciesGraphMatRight.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[7]);
                speciesGraphMatRight.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatRight.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[7].width);
                speciesGraphMatRight.SetInt("_SelectedSpeciesID", uiManagerRef.selectedSpeciesID);
                speciesGraphMatRight.SetFloat("_MaximumValue", maxValuesStatArray[7]);
                speciesGraphMatRight.SetFloat("_MinimumValue", minValuesStatArray[7]);

                textGraphStatsLeft.text = "ATTACK\n" + pool.avgSpecAttack.ToString();
                textGraphStatsCenter.text = "DEFENSE\n" + pool.avgSpecDefend.ToString();
                textGraphStatsRight.text = "SPEED\n" + pool.avgSpecSpeed.ToString();

                textGraphCategory.text = "SPECIALIZATIONS";
                break;
            case GraphCategory.Eaten:
                speciesGraphMatLeft.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[1]);
                speciesGraphMatLeft.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatLeft.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[1].width);
                speciesGraphMatLeft.SetInt("_SelectedSpeciesID", uiManagerRef.selectedSpeciesID);
                speciesGraphMatLeft.SetFloat("_MaximumValue", maxValuesStatArray[1]);
                speciesGraphMatLeft.SetFloat("_MinimumValue", minValuesStatArray[1]);

                speciesGraphMatCenter.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[2]);
                speciesGraphMatCenter.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatCenter.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[2].width);
                speciesGraphMatCenter.SetInt("_SelectedSpeciesID", uiManagerRef.selectedSpeciesID);
                speciesGraphMatCenter.SetFloat("_MaximumValue", maxValuesStatArray[2]);
                speciesGraphMatCenter.SetFloat("_MinimumValue", minValuesStatArray[2]);

                speciesGraphMatRight.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[3]);
                speciesGraphMatRight.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatRight.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[3].width);
                speciesGraphMatRight.SetInt("_SelectedSpeciesID", uiManagerRef.selectedSpeciesID);
                speciesGraphMatRight.SetFloat("_MaximumValue", maxValuesStatArray[3]);
                speciesGraphMatRight.SetFloat("_MinimumValue", minValuesStatArray[3]);


                textGraphStatsLeft.text ="EGGS\n" + pool.avgFoodEatenEgg.ToString();
                textGraphStatsCenter.text = "PLANTS\n" + pool.avgFoodEatenPlant.ToString();
                textGraphStatsRight.text = "MICROBES\n" + pool.avgFoodEatenZoop.ToString();

                textGraphCategory.text = "FOOD CONSUMED";
                break;
            case GraphCategory.DigestSpec:
                speciesGraphMatLeft.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[10]);
                speciesGraphMatLeft.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatLeft.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[10].width);
                speciesGraphMatLeft.SetInt("_SelectedSpeciesID", uiManagerRef.selectedSpeciesID);
                speciesGraphMatLeft.SetFloat("_MaximumValue", maxValuesStatArray[10]);
                speciesGraphMatLeft.SetFloat("_MinimumValue", minValuesStatArray[10]);
                
                speciesGraphMatCenter.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[11]);
                speciesGraphMatCenter.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatCenter.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[11].width);
                speciesGraphMatCenter.SetInt("_SelectedSpeciesID", uiManagerRef.selectedSpeciesID);
                speciesGraphMatCenter.SetFloat("_MaximumValue", maxValuesStatArray[11]);
                speciesGraphMatCenter.SetFloat("_MinimumValue", minValuesStatArray[11]);

                speciesGraphMatRight.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[9]);
                speciesGraphMatRight.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatRight.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[9].width);
                speciesGraphMatRight.SetInt("_SelectedSpeciesID", uiManagerRef.selectedSpeciesID);
                speciesGraphMatRight.SetFloat("_MaximumValue", maxValuesStatArray[9]);
                speciesGraphMatRight.SetFloat("_MinimumValue", minValuesStatArray[9]);

                textGraphStatsLeft.text = "HERBIVORE\n" + pool.avgFoodSpecPlant.ToString();
                textGraphStatsCenter.text = "CARNIVORE\n" + pool.avgFoodSpecMeat.ToString();
                textGraphStatsRight.text = "SCAVENGER\n" + pool.avgFoodSpecDecay.ToString();

                textGraphCategory.text = "DIGESTION BONUSES";
                break;
            default:
                break;
        }

        

        //speciesGraphImage.material = speciesGraphMat;
        //speciesGraphImage.gameObject.SetActive(false);
        //speciesGraphImage.gameObject.SetActive(true);

        //graphPanelGO.gameObject.SetActive(false);
        //graphPanelGO.gameObject.SetActive(true);
    }
    
    public void SetSelectedSpeciesUI(int id) {
        
        SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[id];
        uiManagerRef.selectedSpeciesID = id;

        uiManagerRef.SetFocusedCandidateGenome(pool.candidateGenomesList[0]);

        UpdateSpeciesListBars();
     // attach to this parent object
    }

    public void ClickToolButton() {
        Debug.Log("Click mutation toggle button)");
        isOpen = !isOpen;
    }

    public void UpdateSpeciesListBars() {
        int numActiveSpecies = uiManagerRef.gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList.Count;
        
        foreach (Transform child in treeAnchorUI.transform) {
                GameObject.Destroy(child.gameObject);
        }
        for (int s = 0; s < numActiveSpecies; s++) {
            int speciesID = uiManagerRef.gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[s];
            int parentSpeciesID = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].parentSpeciesID;

            AgentGenome templateGenome = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].leaderboardGenomesList[0].candidateGenome; //.bodyGenome.coreGenome.name;
            Color color = new Color(templateGenome.bodyGenome.appearanceGenome.huePrimary.x, templateGenome.bodyGenome.appearanceGenome.huePrimary.y, templateGenome.bodyGenome.appearanceGenome.huePrimary.z);

            GameObject obj = Instantiate(prefabSpeciesBar, new Vector3(0, 0, 0), Quaternion.identity);
            obj.transform.SetParent(treeAnchorUI.transform, false);
            if(uiManagerRef.selectedSpeciesID == speciesID) {
                obj.transform.localScale = Vector3.one * 1.18f;
            }
            obj.GetComponent<Image>().color = color;
            obj.GetComponentInChildren<Text>().text = "[" + speciesID.ToString() + "] " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].foundingCandidate.candidateGenome.bodyGenome.coreGenome.name;
            SpeciesTreeBarUI buttonScript = obj.GetComponent<SpeciesTreeBarUI>();
            buttonScript.Initialize(uiManagerRef, s, speciesID);


        }
    }

    private void UpdateUI() {

        SpeciesGenomePool pool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID];
        
        
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

        textSelectedSpeciesTitle.text = "SPECIES #" + uiManagerRef.selectedSpeciesID.ToString() + ":  " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID].foundingCandidate.candidateGenome.bodyGenome.coreGenome.name;
        
        Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
        imageSelectedSpeciesBG.color = new Color(hue.x, hue.y, hue.z);
        textSelectedSpeciesTitle.color = Color.white; // new Color(hue.x, hue.y, hue.z);

/*
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
                            ",   avgConsumption: (" + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodEatenCorpse.ToString("F4") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodEatenPlant.ToString("F4") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodEatenZoop.ToString("F4") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodEatenEgg.ToString("F4") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodEatenCreature.ToString("F4") +
                            "),   avgBodySize: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgBodySize.ToString("F3") +
                            ",   avgTalentSpec: (" + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecAttack.ToString("F2") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecDefend.ToString("F2") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecSpeed.ToString("F2") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecUtility.ToString("F2") +
                            "),   avgDiet: (" + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodSpecDecay.ToString("F2") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodSpecPlant.ToString("F2") + ", " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodSpecMeat.ToString("F2") +
                            "),   avgNumNeurons: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgNumNeurons.ToString("F1") +
                            ",   avgNumAxons: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgNumAxons.ToString("F1") +
                            ", avgTimesAttacked: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgTimesAttacked.ToString("F2") +
                            ", avgTimesDefended: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgTimesDefended.ToString() +
                            ", avgTimesDashed: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgTimesDashed.ToString() +
                            ", avgTimeResting: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgTimeRested.ToString() +
                            ", avgTimesPregnant: " + uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgTimesPregnant.ToString() + "\n\n";
        }
        */

        

        if(uiManagerRef.gameManager.simulationManager.simAgeTimeSteps % 277 == 275) {
            UpdateSpeciesListBars();
        }
        
        
        //textSpeciationTree.text = speciesTreeString; // speciesDebugStr;

        //Debug.Break(); 
                
        if(pool != null) {
            /*
            string speciesStatsString = "SPECIES[" + uiManagerRef.selectedSpeciesID.ToString() + "] " + pool.representativeCandidate.candidateGenome.bodyGenome.coreGenome.name + " STATS:\n\n";

            int parentSpeciesID = pool.parentSpeciesID;
            int numCandidates = pool.candidateGenomesList.Count;
            int numLeaders = pool.leaderboardGenomesList.Count;
            int numBorn = pool.numAgentsEvaluated;
            int speciesPopSize = 0;
            float avgFitness = pool.avgLifespan;
            for (int a = 0; a < uiManagerRef.gameManager.simulationManager._NumAgents; a++) {
                if (uiManagerRef.gameManager.simulationManager.agentsArray[a].speciesIndex == uiManagerRef.selectedSpeciesID) {
                    speciesPopSize++;
                }
            }


            string debugTxtGlobalSim = "Species[" + pool.speciesID.ToString() + "] pop: " + speciesPopSize.ToString() + ", #cands: " + numCandidates.ToString() + ", #leader: " + numLeaders.ToString() + ", #numEval'd: " + numBorn.ToString() + "\n"; // + "] SELECTED AGENT[" + agentIndex.ToString() + "] ";
            
            
            speciesStatsString += "Species[" + selectedSpeciesIndex.ToString() + "] p(" + parentSpeciesID.ToString() + "), size: " + speciesPopSize.ToString() + ", #cands: " + numCandidates.ToString() + ", numEvals: " + numBorn.ToString() +
                            "\navgFitness: " + avgFitness.ToString("F2") +
                            "\navgFoodEaten:\nCorpses: " + pool.avgFoodEatenCorpse.ToString("F4") +
                            "\nPlants: " + pool.avgFoodEatenPlant.ToString("F4") + 
                            "\nZoop: " + pool.avgFoodEatenZoop.ToString("F4") + 
                            "\nEggs: " + pool.avgFoodEatenEgg.ToString("F4") +
                            "\nCritters: " + pool.avgFoodEatenCreature.ToString("F4") +
                            "\nAvgTimesAttacked: " + pool.avgTimesAttacked.ToString("F4") +
                            "\nAvgTimesDefended: " + pool.avgTimesDefended.ToString("F4") +
                            "\nAvgTimesDashed: " + pool.avgTimesDashed.ToString("F4") +
                            "\nAvgTimeResting: " + pool.avgTimeRested.ToString("F4") +
                            "\nAvgTimesPregnant: " + pool.avgTimesPregnant.ToString("F4");
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
                
            string sensorString = "\n\nSENSORS_ETC:\n";
                    
            string mouthA = "Mouth Type: ";
            if (focusedAgentGenome.bodyGenome.coreGenome.isPassive) {
                mouthA += "Passive";
            }
            else {
                mouthA += "Active?";
            }
            sensorString += mouthA + "\n";

            sensorString += "PriCol: " + focusedAgentGenome.bodyGenome.appearanceGenome.huePrimary.ToString();
            sensorString += ", SecColor: " + focusedAgentGenome.bodyGenome.appearanceGenome.hueSecondary.ToString() + "\n";
            // proportions:
            sensorString += "BODY Proportions: [" + focusedAgentGenome.bodyGenome.coreGenome.mouthLength.ToString();
            sensorString += ", " + focusedAgentGenome.bodyGenome.coreGenome.headLength.ToString();
            sensorString += ", " + focusedAgentGenome.bodyGenome.coreGenome.bodyLength.ToString();
            sensorString += ", " + focusedAgentGenome.bodyGenome.coreGenome.tailLength.ToString() + "]\n";        
            // eyes
            sensorString += "EYE: SocRad: " + focusedAgentGenome.bodyGenome.coreGenome.socketRadius.ToString();
            sensorString += ", SocHght: " + focusedAgentGenome.bodyGenome.coreGenome.socketHeight.ToString() + "\n";
            // tail
            sensorString += "TAIL: FinLength: " + focusedAgentGenome.bodyGenome.coreGenome.tailFinBaseLength.ToString();
            sensorString += ", FinSpread: " + focusedAgentGenome.bodyGenome.coreGenome.tailFinSpreadAngle.ToString();
            // sensors & shit:
                        
            sensorString += "\nSENSORS:\nComms: " + (focusedAgentGenome.bodyGenome.communicationGenome.useComms ? "ON!" : "off");
            sensorString += "\nWater: " + (focusedAgentGenome.bodyGenome.environmentalGenome.useWaterStats ? "ON!" : "off");
            sensorString += "\nWallsCard: " + (focusedAgentGenome.bodyGenome.environmentalGenome.useCardinals ? "ON!" : "off");
            sensorString += "\nWallsDiag: " + (focusedAgentGenome.bodyGenome.environmentalGenome.useDiagonals ? "ON!" : "off");
            sensorString += "\nNutrients: " + (focusedAgentGenome.bodyGenome.foodGenome.useNutrients ? "ON!" : "off");
            sensorString += "\nFoodPos: " + (focusedAgentGenome.bodyGenome.foodGenome.usePos ? "ON!" : "off");
            sensorString += "\nFoodVel: " + (focusedAgentGenome.bodyGenome.foodGenome.useVel ? "ON!" : "off");
            sensorString += "\nFoodDir: " + (focusedAgentGenome.bodyGenome.foodGenome.useDir ? "ON!" : "off");
            sensorString += "\nFoodInfo: " + (focusedAgentGenome.bodyGenome.foodGenome.useStats ? "ON!" : "off");
            sensorString += "\nEggsack: " + (focusedAgentGenome.bodyGenome.foodGenome.useEggs ? "ON!" : "off");
            sensorString += "\nCorpse: " + (focusedAgentGenome.bodyGenome.foodGenome.useCorpse ? "ON!" : "off");
            sensorString += "\nFriendPos: " + (focusedAgentGenome.bodyGenome.friendGenome.usePos ? "ON!" : "off");
            sensorString += "\nFriendVel: " + (focusedAgentGenome.bodyGenome.friendGenome.useVel ? "ON!" : "off");
            sensorString += "\nFriendDir: " + (focusedAgentGenome.bodyGenome.friendGenome.useDir ? "ON!" : "off");
            sensorString += "\nThreatPos: " + (focusedAgentGenome.bodyGenome.threatGenome.usePos ? "ON!" : "off");
            sensorString += "\nThreatVel: " + (focusedAgentGenome.bodyGenome.threatGenome.useVel ? "ON!" : "off");
            sensorString += "\nThreatDir: " + (focusedAgentGenome.bodyGenome.threatGenome.useDir ? "ON!" : "off");
            sensorString += "\nThreatInfo: " + (focusedAgentGenome.bodyGenome.threatGenome.useStats ? "ON!" : "off");
        */


            //textStatsBody.text = debugTxtGlobalSim; // + speciesStatsString; // + debugTxtAgent + sensorString;
        }

       /*
        if(selectedSpeciesStatsIndex == 0) {
            curSpeciesStatValue = pool.avgLifespan;
            curSpeciesStatName = "Avg Lifespan";
        }
        else if(selectedSpeciesStatsIndex == 1) {
            curSpeciesStatValue = pool.avgFoodEatenEgg;
            curSpeciesStatName = "Avg Eggs Eaten";
        }
        else if(selectedSpeciesStatsIndex == 2) {
            curSpeciesStatValue = pool.avgFoodEatenPlant;
            curSpeciesStatName = "Avg Plants Eaten";
        }
        else if(selectedSpeciesStatsIndex == 3) {
            curSpeciesStatValue = pool.avgFoodEatenZoop;
            curSpeciesStatName = "Avg Zoops Eaten";
        }
        else if(selectedSpeciesStatsIndex == 4) {
            curSpeciesStatValue = pool.avgBodySize;
            curSpeciesStatName = "Avg Body Weight";
        }
        else if(selectedSpeciesStatsIndex == 5) {
            curSpeciesStatValue = pool.avgSpecAttack;
            curSpeciesStatName = "Specialization: Attack";
        }
        else if(selectedSpeciesStatsIndex == 6) {
            curSpeciesStatValue = pool.avgSpecDefend;
            curSpeciesStatName = "Specialization: Defense";
        }
        else if(selectedSpeciesStatsIndex == 7) {
            curSpeciesStatValue = pool.avgSpecSpeed;
            curSpeciesStatName = "Specialization: Speed";
        }
        else if(selectedSpeciesStatsIndex == 8) {
            curSpeciesStatValue = pool.avgSpecUtility;
            curSpeciesStatName = "Specialization: Utility";
        }
        else if(selectedSpeciesStatsIndex == 9) {
            curSpeciesStatValue = pool.avgFoodSpecDecay;
            curSpeciesStatName = "Digestion Efficiency: Carrion";
        }
        else if(selectedSpeciesStatsIndex == 10) {
            curSpeciesStatValue = pool.avgFoodSpecPlant;
            curSpeciesStatName = "Digestion Efficiency: Plants";
        }
        else if(selectedSpeciesStatsIndex == 11) {
            curSpeciesStatValue = pool.avgFoodSpecMeat;
            curSpeciesStatName = "Digestion Efficiency: Meat";
        }
        else if(selectedSpeciesStatsIndex == 12) {
            curSpeciesStatValue = pool.avgNumNeurons;
            curSpeciesStatName = "Total Neurons in Brain";
        }
        else if(selectedSpeciesStatsIndex == 13) {
            curSpeciesStatValue = pool.avgNumAxons;
            curSpeciesStatName = "Total Axons in Brain";
        }        
        else if(selectedSpeciesStatsIndex == 14) {
            curSpeciesStatValue = pool.avgDamageDealt;
            curSpeciesStatName = "Damage Dealt";
        }
        else if(selectedSpeciesStatsIndex == 15) {
            curSpeciesStatValue = pool.avgDamageTaken;
            curSpeciesStatName = "Damage Taken";
        }
        */
        
        RefreshGraphMaterial();

        
    }

    public void UpdateGlobalResourcesPanelUpdate() {

        this.isOpen = true; // uiManagerRef.knowledgeUI.isOpen;
        

        panelGlobalResourcesMain.SetActive(isOpen);
        //if(isOpen) {
        UpdateUI();
        //}
    }   
}
