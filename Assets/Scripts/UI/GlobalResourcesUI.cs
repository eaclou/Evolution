using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalResourcesUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public bool isUnlocked;
    public bool isOpen;
    public Text textGlobalMass;

    public GameObject panelGlobalResourcesMain;
      
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
        

    SimulationManager simulationManager => SimulationManager.instance;
    MasterGenomePool masterGenomePool => simulationManager.masterGenomePool;
        
    private Texture2D brainGenomeTex; //brainActivityPanel
    public Material brainGenomeMat;

    void Start() {
        brainGenomeTex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        brainGenomeTex.filterMode = FilterMode.Point;
        brainGenomeTex.wrapMode = TextureWrapMode.Clamp;
        brainGenomeMat.SetTexture("_MainTex", brainGenomeTex);
      
                 
	}
	    
    
    public void CreateBrainGenomeTexture(AgentGenome genome) {

        int width = 256;
        brainGenomeTex.Resize(width, 1); // pool.leaderboardGenomesList.Count);

        //AgentGenome genome = uiManagerRef.focusedCandidate.candidateGenome;
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
                    
                //break;
            }
                
            brainGenomeTex.SetPixel(xIndex, yIndex, testColor);
        }
        /*
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
                    //curLinearIndex = i;
                    //break;
                }
                
                brainGenomeTex.SetPixel(xIndex, yIndex, testColor);
            }
        }
        else {

        }
        */
        // Body Genome
        //int xI = curLinearIndex % brainGenomeTex.width;
        //int yI = Mathf.FloorToInt(curLinearIndex / brainGenomeTex.width);
        
        brainGenomeTex.Apply();
    }    
    
    public void ClickToolButton() {
        Debug.Log("Click globalResourcesUI toggle button)");
        isOpen = !isOpen;
    }

    private void UpdateUI() {
        SpeciesGenomePool pool = masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID];
        
        textGlobalMass.text = "Global Biomass: " + simulationManager.simResourceManager.curTotalMass.ToString("F0");
        SimResourceManager resourcesRef = simulationManager.simResourceManager;
        //textMeterOxygen.text = resourcesRef.curGlobalOxygen.ToString("F0");
        textMeterNutrients.text = resourcesRef.curGlobalNutrients.ToString("F0");
        textMeterDetritus.text = resourcesRef.curGlobalDetritus.ToString("F0");
        textMeterDecomposers.text = resourcesRef.curGlobalDecomposers.ToString("F0");
        textMeterAlgae.text = (resourcesRef.curGlobalAlgaeReservoir).ToString("F0");
        textMeterPlants.text = (resourcesRef.curGlobalPlantParticles).ToString("F0");
        textMeterZooplankton.text = (resourcesRef.curGlobalAnimalParticles).ToString("F0");
        textMeterAnimals.text = (resourcesRef.curGlobalAgentBiomass).ToString("F2");

    }

    public void UpdateGlobalResourcesPanelUpdate() {

        isOpen = true;
        panelGlobalResourcesMain.SetActive(isOpen);
        UpdateUI();
    }   
}
