using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KnowledgeUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public bool isUnlocked;
    public bool isOpen;
    public Image imageKnowledgeButtonMIP;  // superscript button over watcher toolbar Button
    public Image imageKnowledgeCurTarget; // in watcher panel
    public Text textTargetLayer;
    //public Button buttonKnowledgeLock;
    public Image imageColorBar;

    public Animator animatorKnowledgeUI;

    public Image imageKnowledgeSpeciesStatsGraph;

    public Image knowledgeGraphVertebrateLifespan;
    public Image knowledgeGraphVertebratePopulation;
    public Image knowledgeGraphVertebrateFoodEaten;
    public Image knowledgeGraphVertebrateGenome;
    public Text textKnowledgeGraphLifespan;
    public Text textKnowledgeGraphPopulation;
    public Text textKnowledgeGraphFoodEaten;
    public Text textKnowledgeGraphGenome;
    public Material knowledgeGraphVertebrateLifespanMat0;
    public Material knowledgeGraphVertebratePopulationMat0;
    public Material knowledgeGraphVertebrateFoodEatenMat0;
    public Material knowledgeGraphVertebrateGenomeMat0;
    public Material knowledgeGraphVertebrateLifespanMat1;
    public Material knowledgeGraphVertebratePopulationMat1;
    public Material knowledgeGraphVertebrateFoodEatenMat1;
    public Material knowledgeGraphVertebrateGenomeMat1;
    public Material knowledgeGraphVertebrateLifespanMat2;
    public Material knowledgeGraphVertebratePopulationMat2;
    public Material knowledgeGraphVertebrateFoodEatenMat2;
    public Material knowledgeGraphVertebrateGenomeMat2;
    public Material knowledgeGraphVertebrateLifespanMat3;
    public Material knowledgeGraphVertebratePopulationMat3;
    public Material knowledgeGraphVertebrateFoodEatenMat3;
    public Material knowledgeGraphVertebrateGenomeMat3;

    public Image imageKnowledgeMapTextureViewer;
    public Material uiKnowledgeMapViewerMat;

    public GameObject panelKnowledgeSpiritBase;
    //public GameObject panelKnowledgeInfoWorld;
    public GameObject panelKnowledgeInfoDecomposers;
    public GameObject panelKnowledgeInfoAlgae;
    public GameObject panelKnowledgeInfoPlants;
    public GameObject panelKnowledgeInfoZooplankton;
    public GameObject panelKnowledgeInfoVertebrates;

    public Text textKnowledgeSpeciesSummary;


    // Use this for initialization
    void Start() {
        //isUnlocked = true;
        //isOpen = true;
    }

    // Update is called once per frame
    void Update() {

    }

    public void ClickToolButton() {
        isOpen = !isOpen;

        if(isOpen) {
            animatorKnowledgeUI.SetBool("_IsOpen", true);
        }
        else {
            animatorKnowledgeUI.SetBool("_IsOpen", false);
        }
    }

    public void OpenKnowledgePanel() {
        isOpen = true;
        animatorKnowledgeUI.SetBool("_IsOpen", true);
    }

    private void UpdateUI(TrophicLayersManager layerManager) {
        //TrophicSlot slotRef = uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef;

        //panelKnowledgeInfoWorld.SetActive(false);
        panelKnowledgeInfoDecomposers.SetActive(false);
        panelKnowledgeInfoAlgae.SetActive(false);
        panelKnowledgeInfoPlants.SetActive(false);
        panelKnowledgeInfoZooplankton.SetActive(false);
        panelKnowledgeInfoVertebrates.SetActive(false);
        
        textKnowledgeSpeciesSummary.gameObject.SetActive(true);
        string summaryText = GetSpeciesDescriptionString(uiManagerRef.gameManager.simulationManager);
        textKnowledgeSpeciesSummary.text = summaryText;

        TrophicSlot slotRef = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot;

        textTargetLayer.text = slotRef.speciesName;
        textTargetLayer.color = uiManagerRef.worldSpiritHubUI.curIconColor;
        imageColorBar.color = uiManagerRef.worldSpiritHubUI.curIconColor;
        imageKnowledgeCurTarget.color = uiManagerRef.worldSpiritHubUI.curIconColor;
        imageKnowledgeCurTarget.sprite = uiManagerRef.worldSpiritHubUI.curIconSprite;

        imageKnowledgeMapTextureViewer.gameObject.SetActive(false);
        
        uiKnowledgeMapViewerMat.SetTexture("_AltitudeTex", uiManagerRef.gameManager.theRenderKing.baronVonTerrain.terrainHeightDataRT);
        uiKnowledgeMapViewerMat.SetTexture("_ResourceGridTex", uiManagerRef.gameManager.simulationManager.vegetationManager.resourceGridRT1);
        uiKnowledgeMapViewerMat.SetFloat("_WaterLevel", uiManagerRef.gameManager.theRenderKing.baronVonWater._GlobalWaterLevel);

        //knowledgeLockedTrophicSlotRef ---> worldSpiritSelectedSlotRef
        if (slotRef.kingdomID == 0) {
            // DECOMPOSERS
            panelKnowledgeInfoDecomposers.SetActive(true);

            imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
            uiKnowledgeMapViewerMat.SetTexture("_MainTex", uiManagerRef.gameManager.simulationManager.vegetationManager.resourceGridRT1);
            uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
            uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 2f);
            uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
            uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 2);
            uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
            uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1.05f);
            
        }
        else if (slotRef.kingdomID == 1) {
            if (slotRef.tierID == 0) {  // ALGAE
                panelKnowledgeInfoAlgae.SetActive(true);
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", uiManagerRef.gameManager.simulationManager.vegetationManager.resourceGridRT1);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 2f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 3);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1.05f); 
            }
            else {  // PLANT PARTICLES
                panelKnowledgeInfoPlants.SetActive(true);

                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", uiManagerRef.gameManager.simulationManager.environmentFluidManager._SourceColorRT);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 0);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 0f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f); 
            }
        }
        else if (slotRef.kingdomID == 2) {
            if (slotRef.tierID == 0) { // ZOOPLANKTON
                panelKnowledgeInfoZooplankton.SetActive(true);

                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", uiManagerRef.gameManager.simulationManager.environmentFluidManager._SourceColorRT);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 0);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 0f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f); 
            }
            else {  // VERTEBRATES
                panelKnowledgeInfoVertebrates.SetActive(true);

                
                
                float lifespan = 0f;
                float population = 0f;
                float foodEaten = 0f;
                float genome = 0f;

                if (slotRef.slotID == 0) {
                    lifespan = uiManagerRef.gameManager.simulationManager.graphDataVertebrateLifespan0.curVal;
                    population = uiManagerRef.gameManager.simulationManager.graphDataVertebratePopulation0.curVal;
                    foodEaten = uiManagerRef.gameManager.simulationManager.graphDataVertebrateFoodEaten0.curVal;
                    genome = uiManagerRef.gameManager.simulationManager.graphDataVertebrateGenome0.curVal;

                    knowledgeGraphVertebrateLifespan.material = knowledgeGraphVertebrateLifespanMat0;
                    knowledgeGraphVertebratePopulation.material = knowledgeGraphVertebratePopulationMat0;
                    knowledgeGraphVertebrateFoodEaten.material = knowledgeGraphVertebrateFoodEatenMat0;
                    knowledgeGraphVertebrateGenome.material = knowledgeGraphVertebrateGenomeMat0;
                }
                else if (slotRef.slotID == 1) {
                    lifespan = uiManagerRef.gameManager.simulationManager.graphDataVertebrateLifespan1.curVal;
                    population = uiManagerRef.gameManager.simulationManager.graphDataVertebratePopulation1.curVal;
                    foodEaten = uiManagerRef.gameManager.simulationManager.graphDataVertebrateFoodEaten1.curVal;
                    genome = uiManagerRef.gameManager.simulationManager.graphDataVertebrateGenome1.curVal;

                    knowledgeGraphVertebrateLifespan.material = knowledgeGraphVertebrateLifespanMat1;
                    knowledgeGraphVertebratePopulation.material = knowledgeGraphVertebratePopulationMat1;
                    knowledgeGraphVertebrateFoodEaten.material = knowledgeGraphVertebrateFoodEatenMat1;
                    knowledgeGraphVertebrateGenome.material = knowledgeGraphVertebrateGenomeMat1;
                }
                else if (slotRef.slotID == 2) {
                    lifespan = uiManagerRef.gameManager.simulationManager.graphDataVertebrateLifespan2.curVal;
                    population = uiManagerRef.gameManager.simulationManager.graphDataVertebratePopulation2.curVal;
                    foodEaten = uiManagerRef.gameManager.simulationManager.graphDataVertebrateFoodEaten2.curVal;
                    genome = uiManagerRef.gameManager.simulationManager.graphDataVertebrateGenome2.curVal;

                    knowledgeGraphVertebrateLifespan.material = knowledgeGraphVertebrateLifespanMat2;
                    knowledgeGraphVertebratePopulation.material = knowledgeGraphVertebratePopulationMat2;
                    knowledgeGraphVertebrateFoodEaten.material = knowledgeGraphVertebrateFoodEatenMat2;
                    knowledgeGraphVertebrateGenome.material = knowledgeGraphVertebrateGenomeMat2;
                }
                else {
                    lifespan = uiManagerRef.gameManager.simulationManager.graphDataVertebrateLifespan3.curVal;
                    population = uiManagerRef.gameManager.simulationManager.graphDataVertebratePopulation3.curVal;
                    foodEaten = uiManagerRef.gameManager.simulationManager.graphDataVertebrateFoodEaten3.curVal;
                    genome = uiManagerRef.gameManager.simulationManager.graphDataVertebrateGenome3.curVal;

                    knowledgeGraphVertebrateLifespan.material = knowledgeGraphVertebrateLifespanMat3;
                    knowledgeGraphVertebratePopulation.material = knowledgeGraphVertebratePopulationMat3;
                    knowledgeGraphVertebrateFoodEaten.material = knowledgeGraphVertebrateFoodEatenMat3;
                    knowledgeGraphVertebrateGenome.material = knowledgeGraphVertebrateGenomeMat3;
                }

                textKnowledgeGraphLifespan.text = lifespan.ToString("F0");
                textKnowledgeGraphPopulation.text = population.ToString("F0");
                textKnowledgeGraphFoodEaten.text = foodEaten.ToString("F3");
                textKnowledgeGraphGenome.text = genome.ToString("F1");  // avg Body size

                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", uiManagerRef.gameManager.simulationManager.environmentFluidManager._SourceColorRT);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 3);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f); 
            }

        }
        else if (slotRef.kingdomID == 3) {
            //panelKnowledgeInfoWorld.SetActive(true);
            if (slotRef.slotID == 0) {
                // WORLD 
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", uiManagerRef.gameManager.theRenderKing.baronVonTerrain.terrainHeightDataRT);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 0.7f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 0);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f);  
            }
            else if (slotRef.slotID == 1) {
                // STONE
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", uiManagerRef.gameManager.theRenderKing.baronVonTerrain.terrainHeightDataRT);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 0);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f);
            }
            else if (slotRef.slotID == 2) {
                // PEBBLES
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", uiManagerRef.gameManager.theRenderKing.baronVonTerrain.terrainHeightDataRT);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 1);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f);
            }
            else {
                // SAND
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", uiManagerRef.gameManager.theRenderKing.baronVonTerrain.terrainHeightDataRT);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 2);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f);
            }
        }
        else if (slotRef.kingdomID == 4) {
            //panelKnowledgeInfoWorld.SetActive(true);
            
            if (slotRef.slotID == 0) {
                // Minerals
                panelKnowledgeInfoAlgae.SetActive(true);
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", uiManagerRef.gameManager.simulationManager.vegetationManager.resourceGridRT1);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 0);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f); 
            }
            else if (slotRef.slotID == 1) {
                // Water
                //Debug.Log("sadfasdfadsfas");
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", uiManagerRef.gameManager.theRenderKing.baronVonWater.waterSurfaceDataRT0);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 1f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 0);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f);  
            
            }
            else if (slotRef.slotID == 2) {
                // AIR
                imageKnowledgeMapTextureViewer.gameObject.SetActive(true);
                uiKnowledgeMapViewerMat.SetTexture("_MainTex", uiManagerRef.gameManager.theRenderKing.baronVonWater.waterSurfaceDataRT0);
                uiKnowledgeMapViewerMat.SetVector("_Zoom", Vector4.one);
                uiKnowledgeMapViewerMat.SetFloat("_Amplitude", 0.5f);
                uiKnowledgeMapViewerMat.SetVector("_ChannelMask", Vector4.one); // _ChannelMask);
                uiKnowledgeMapViewerMat.SetInt("_ChannelSoloIndex", 0);
                uiKnowledgeMapViewerMat.SetFloat("_IsChannelSolo", 1f);
                uiKnowledgeMapViewerMat.SetFloat("_Gamma", 1f);  
            }
        }
    }

    public void UpdateKnowledgePanelUI(TrophicLayersManager layerManager) {


        panelKnowledgeSpiritBase.SetActive(true); // && uiManagerRef.worldSpiritHubUI.isOpen);
        if(isOpen) {
            UpdateUI(layerManager);
        }        
    }

    /*
    
        string[] strSpiritBrushDescriptionArray = new string[6]; // = "Decomposers break down the old so that new life can grow.";
        strSpiritBrushDescriptionArray[0] = "Provides information about the world and its contents, and chronicles events through time";
        strSpiritBrushDescriptionArray[1] = "This spirit possesses limited control of life & existence itself";
        strSpiritBrushDescriptionArray[2] = "A mysterious Kelpie able to control the flow of water";
        strSpiritBrushDescriptionArray[3] = "A Watcher Spirit can track an organism's journey through space and time";
        strSpiritBrushDescriptionArray[4] = "Mutate...       blah blah";
        strSpiritBrushDescriptionArray[5] = "Extra.";

        string[] strLinkedSpiritDescriptionArray = new string[12]; // = "Decomposers break down the old so that new life can grow.";
        strLinkedSpiritDescriptionArray[0] = "The World Spirit provides the spark for a new universe";
        strLinkedSpiritDescriptionArray[1] = "Stone Spirits are some of the oldest known";
        strLinkedSpiritDescriptionArray[2] = "Pebble Spirits are usually found in rivers and streams";
        strLinkedSpiritDescriptionArray[3] = "Sand Spirits";
        strLinkedSpiritDescriptionArray[4] = "Mineral Spirits infuse nutrients into the earth.";
        strLinkedSpiritDescriptionArray[5] = "Water Spirits";
        strLinkedSpiritDescriptionArray[6] = "Air Spirits";
        strLinkedSpiritDescriptionArray[7] = "Decomposers break down the old so that new life can grow.";
        strLinkedSpiritDescriptionArray[8] = "Algae needs light and nutrients to grow.";
        strLinkedSpiritDescriptionArray[9] = "Floating Plants that are a foodsource for Vertebrates";
        strLinkedSpiritDescriptionArray[10] = "Tiny Organisms that feed on Algae";
        strLinkedSpiritDescriptionArray[11] = "Animals that can feed on Plants, Zooplankton, or even other Vertebrates.";

    */
    public string GetSpeciesDescriptionString(SimulationManager simManager) {
        string str = "";

        TrophicSlot slot = simManager.uiManager.worldSpiritHubUI.selectedWorldSpiritSlot;

        if(slot.kingdomID == 0) {
            str = "Bacterial and Fungal organisms that recycle vital nutrients."; //   \n\nUses: <b><color=#A97860FF>Waste</color></b>, <b><color=#8EDEEEFF>Oxygen</color></b>\n\nProduces: <b><color=#FBC653FF>Nutrients</color></b>";

            str += "\n\n";
            str += "<size=13><b>Total Biomass: " + simManager.simResourceManager.curGlobalDecomposers.ToString("F1") + "</b></size>\n\n";

            str += "<color=#FBC653FF>Nutrient Production: <b>" + simManager.simResourceManager.nutrientsProducedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";
            str += "<color=#8EDEEEFF>Oxygen Usage: <b>" + simManager.simResourceManager.oxygenUsedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";
            str += "<color=#A97860FF>Waste Processed: <b>" + simManager.simResourceManager.detritusRemovedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";

        }
        else if(slot.kingdomID == 1) {
            if (slot.tierID == 0) {  // ALGAE GRID
                str = "Microscopic Plants that form the foundation of the ecosystem along with the Decomposers."; //   \n\nUses: <b><color=#FBC653FF>Nutrients</color></b>\n\nProduces: <b><color=#8EDEEEFF>Oxygen</color></b>";

                str += "\n\n";
                str += "<size=13><b>Total Biomass: " + simManager.simResourceManager.curGlobalAlgaeReservoir.ToString("F1") + "</b></size>\n\n";

                str += "<color=#8EDEEEFF>Oxygen Production: <b>" + simManager.simResourceManager.oxygenProducedByAlgaeReservoirLastFrame.ToString("F3") + "</b></color>\n";
                str += "<color=#FBC653FF>Nutrient Usage: <b>" + simManager.simResourceManager.nutrientsUsedByAlgaeReservoirLastFrame.ToString("F3") + "</b></color>\n";
                str += "<color=#A97860FF>Waste Generated: <b>" + simManager.simResourceManager.wasteProducedByAlgaeReservoirLastFrame.ToString("F3") + "</b></color>\n";

            }
            else { //BIG PLANTS
                str = "Larger Plants."; //   \n\nUses: <b><color=#FBC653FF>Nutrients</color></b>\n\nProduces: <b><color=#8EDEEEFF>Oxygen</color></b>";
                str += "\n\nWelcome to the Big Leagues, chloroplasts";
                str += "\n\n";
                str += "<size=13><b>Total Biomass: " + simManager.simResourceManager.curGlobalPlantParticles.ToString("F1") + "</b></size>\n\n";

                str += "<color=#8EDEEEFF>Oxygen Production: <b>" + simManager.simResourceManager.oxygenProducedByPlantParticlesLastFrame.ToString("F3") + "</b></color>\n";
                str += "<color=#FBC653FF>Nutrient Usage: <b>" + simManager.simResourceManager.nutrientsUsedByPlantParticlesLastFrame.ToString("F3") + "</b></color>\n";
                str += "<color=#A97860FF>Waste Generated: <b>" + simManager.simResourceManager.wasteProducedByPlantParticlesLastFrame.ToString("F3") + "</b></color>\n";

            }
            
        }
        else if(slot.kingdomID == 2) {
            if(slot.tierID == 0) {
                str = "Tiny Animals that feed on Algae."; //   \n\nUses: <b><color=#8EDEEEFF>Oxygen</color></b>\n\nProduces: <b><color=#A97860FF>Waste</color></b>";
                str += "\n\n";
                str += "<size=13><b>Total Biomass: " + simManager.simResourceManager.curGlobalAnimalParticles.ToString("F1") + "</b></size>\n\n";
                str += "<color=#8EDEEEFF>Oxygen Usage: <b>" + simManager.simResourceManager.oxygenUsedByAnimalParticlesLastFrame.ToString("F3") + "</b></color>\n";                
                str += "<color=#A97860FF>Waste Generated: <b>" + simManager.simResourceManager.wasteProducedByAnimalParticlesLastFrame.ToString("F3") + "</b></color>\n";

            }
            else {
                str = "Simple Animal that feeds on Plants and Zooplankton.";  //    \n\nUses: <b><color=#8EDEEEFF>Oxygen</color></b>\n\nProduces: <b><color=#A97860FF>Waste</color></b>";
                str += "\n\n";
                /*float speciesMass = 0f;
                if (slot.slotID == 0) {
                    speciesMass = simManager.simResourceManager.curGlobalAgentBiomass0;
                }
                else if(slot.slotID == 1) {
                    speciesMass = simManager.simResourceManager.curGlobalAgentBiomass1;
                }
                else if(slot.slotID == 2) {
                    speciesMass = simManager.simResourceManager.curGlobalAgentBiomass2;
                }
                else {
                    speciesMass = simManager.simResourceManager.curGlobalAgentBiomass3;
                }*/
                str += "<size=13><b>Total Biomass: " + simManager.simResourceManager.curGlobalAgentBiomass.ToString("F1") + "</b></size>\n\n";// simManager.simResourceManager.curGlobalAgentBiomass.ToString("F1") + "</b></size>\n\n";

                SpeciesGenomePool GenomePool = simManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot.linkedSpeciesID];
                str += "<color=#8EDEEEFF>Oxygen Usage: <b>" + simManager.simResourceManager.oxygenUsedByAgentsLastFrame.ToString("F3") + "</b></color>\n";
                str += "<color=#A97860FF>Waste Generated: <b>" + simManager.simResourceManager.wasteProducedByAgentsLastFrame.ToString("F3") + "</b></color>\n";
                //str += "<color=#8BD06AFF>Avg Food Eaten: <b>" + (GenomePool.avgConsumptionPlant + GenomePool.avgConsumptionMeat).ToString("F3") + "</b></color>\n";
                //str += "\n\n\nAvg Lifespan: <b>" + (GenomePool.avgLifespan / 1500f).ToString("F1") + " Years</b>\n\n";

                GenomePool.representativeGenome.bodyGenome.CalculateFullsizeBoundingBox();
                str += "Avg Body Size: <b>" + ((GenomePool.representativeGenome.bodyGenome.fullsizeBoundingBox.x + GenomePool.representativeGenome.bodyGenome.fullsizeBoundingBox.y) * 0.5f * GenomePool.representativeGenome.bodyGenome.fullsizeBoundingBox.z).ToString("F2") + "</b>\n";
                str += "Avg Brain Size: <b>" + ((GenomePool.avgNumNeurons + GenomePool.avgNumAxons) * 1f).ToString("F0") + "</b>\n";
                
                str += "\nFOOD EATEN:\nPlants: <b>" + ((GenomePool.avgConsumptionPlant) * 1f).ToString("F3") + "</b> [" + (GenomePool.avgFoodSpecPlant).ToString() + "]\n";
                str += "Meat: <b>" + ((GenomePool.avgConsumptionMeat) * 1f).ToString("F3") + "</b> [" + (GenomePool.avgFoodSpecMeat).ToString() + "]\n";

                str += "\nSPECIALIZATIONS:\nAttack: <b>" + ((GenomePool.avgSpecAttack) * 1f).ToString("F2") + "</b>\n";
                str += "Defend: <b>" + ((GenomePool.avgSpecDefend) * 1f).ToString("F2") + "</b>\n";
                str += "Speed: <b>" + ((GenomePool.avgSpecSpeed) * 1f).ToString("F2") + "</b>\n";
                str += "Utility: <b>" + ((GenomePool.avgSpecUtility) * 1f).ToString("F2") + "</b>\n";
            }            
        }
        else if(slot.kingdomID == 3) {
            if(slot.slotID == 0) {
                str = "World";
                str += "\n\nWorld Size: X square meters";
            }
            else if(slot.slotID == 1) {
                str = "Stone";
                str += "\n\nTotal Stone: Y lbs";
            }
            else if(slot.slotID == 2) {
                str = "Pebbles";
            }
            else {
                str = "Fine Sand";
            }
        }
        else {
            if(slot.slotID == 0) {
                str = "Minerals";
            }
            else if(slot.slotID == 1) {
                str = "Water";
            }
            else if(slot.slotID == 2) {
                str = "Air";
            }
        }


        return str;
    }
    /*
    private void UpdateUI() {        
        imageKnowledgeSpeciesStatsGraph.gameObject.SetActive(false);

        TrophicSlot slot = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot; ; // slot uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef;

        string descriptionText = "";

        if (slot.kingdomID == 0) {
            descriptionText += "<size=13><b>Total Biomass: " + uiManagerRef.gameManager.simulationManager.simResourceManager.curGlobalDecomposers.ToString("F1") + "</b></size>\n\n";

            descriptionText += "<color=#FBC653FF>Nutrient Production: <b>" + uiManagerRef.gameManager.simulationManager.simResourceManager.nutrientsProducedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#8EDEEEFF>Oxygen Usage: <b>" + uiManagerRef.gameManager.simulationManager.simResourceManager.oxygenUsedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#A97860FF>Waste Processed: <b>" + uiManagerRef.gameManager.simulationManager.simResourceManager.detritusRemovedByDecomposersLastFrame.ToString("F3") + "</b></color>\n";
            
        }
        else if (slot.kingdomID == 1) {
            descriptionText += "<size=13><b>Total Biomass: " + uiManagerRef.gameManager.simulationManager.simResourceManager.curGlobalPlantParticles.ToString("F1") + "</b></size>\n\n";

            descriptionText += "<color=#8EDEEEFF>Oxygen Production: <b>" + uiManagerRef.gameManager.simulationManager.simResourceManager.oxygenProducedByPlantParticlesLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#FBC653FF>Nutrient Usage: <b>" + uiManagerRef.gameManager.simulationManager.simResourceManager.nutrientsUsedByPlantParticlesLastFrame.ToString("F3") + "</b></color>\n";
            descriptionText += "<color=#A97860FF>Waste Generated: <b>" + uiManagerRef.gameManager.simulationManager.simResourceManager.wasteProducedByPlantParticlesLastFrame.ToString("F3") + "</b></color>\n";

        }
        else if (slot.kingdomID == 2) {
            if (slot.tierID == 0) {  // ZOOPLANKTON
                descriptionText += "<size=13><b>Total Biomass: " + uiManagerRef.gameManager.simulationManager.simResourceManager.curGlobalAnimalParticles.ToString("F1") + "</b></size>\n\n";

                descriptionText += "<color=#8EDEEEFF>Oxygen Usage: <b>" + uiManagerRef.gameManager.simulationManager.simResourceManager.oxygenUsedByAnimalParticlesLastFrame.ToString("F3") + "</b></color>\n";
                //descriptionText += "<color=#FBC653FF>Nutrient Usage: <b>" + gameManager.simulationManager.simResourceManager.nutrientsUsedByAlgaeParticlesLastFrame.ToString("F3") + "</b></color>\n";
                descriptionText += "<color=#A97860FF>Waste Generated: <b>" + uiManagerRef.gameManager.simulationManager.simResourceManager.wasteProducedByAnimalParticlesLastFrame.ToString("F3") + "</b></color>\n";

            }
            else {  // AGENTS
                imageKnowledgeSpeciesStatsGraph.gameObject.SetActive(true);

                SpeciesGenomePool selectedPool = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot.linkedSpeciesID];

                descriptionText += "<size=13><b>Total Biomass: " + uiManagerRef.gameManager.simulationManager.simResourceManager.curGlobalAgentBiomass.ToString("F1") + "</b></size>\n\n";

                descriptionText += "<color=#8EDEEEFF>Oxygen Usage: <b>" + uiManagerRef.gameManager.simulationManager.simResourceManager.oxygenUsedByAgentsLastFrame.ToString("F3") + "</b></color>\n";
                descriptionText += "<color=#A97860FF>Waste Generated: <b>" + uiManagerRef.gameManager.simulationManager.simResourceManager.wasteProducedByAgentsLastFrame.ToString("F3") + "</b></color>\n";

                descriptionText += "<color=#8BD06AFF>Avg Food Consumed: <b>" + (selectedPool.avgConsumptionPlant + selectedPool.avgConsumptionMeat).ToString("F3") + "</b></color>\n";
                //descriptionText += "Avg Meat Consumed: <b>" + selectedPool.avgConsumptionMeat.ToString("F3") + "</b>\n\n";  
                //descriptionText += "Descended From Species: <b>" + selectedPool.parentSpeciesID.ToString() + "</b>\n";
                //descriptionText += "Year Evolved: <b>" + selectedPool.yearCreated.ToString() + "</b>\n\n";
                descriptionText += "\n\n\nAvg Lifespan: <b>" + (selectedPool.avgLifespan / 1500f).ToString("F1") + " Years</b>\n\n";

                selectedPool.representativeGenome.bodyGenome.CalculateFullsizeBoundingBox();
                descriptionText += "Avg Body Size: <b>" + ((selectedPool.representativeGenome.bodyGenome.fullsizeBoundingBox.x + selectedPool.representativeGenome.bodyGenome.fullsizeBoundingBox.y) * 0.5f * selectedPool.representativeGenome.bodyGenome.fullsizeBoundingBox.z).ToString("F2") + "</b>\n";
                //+ selectedPool.avgBodySize.ToString("F2") + "</b>\n";
                descriptionText += "Avg Brain Size: <b>" + ((selectedPool.avgNumNeurons + selectedPool.avgNumAxons) * 0.1f).ToString("F1") + "</b>\n";
                //descriptionText += "Avg Axon Count: <b>" + selectedPool.avgNumAxons.ToString("F0") + "</b>\n\n";
                
                // TEMP UNLOCK TEXT:
                // Slot 4/4:
                
            }
        }
        else if (slot.kingdomID == 3) {  // TERRAIN
            if(slot.slotID == 0) {
                //world
            }
            else if(slot.slotID == 1) {
                //stone
            }
            else if(slot.slotID == 2) {
                //pebbles
            }
            else {
                //sand
            }
        }
        else {  // OTHER
            if(slot.slotID == 0) {
                //minerals
            }
            else if(slot.slotID == 1) {
                //water
            }
            else {
                //atmosphere
            }
        }


        //imageColorBar
        //imageKnowledgeCurTarget

        //textKnowledgeSpeciesSummary.text = descriptionText;
    }
    */
}