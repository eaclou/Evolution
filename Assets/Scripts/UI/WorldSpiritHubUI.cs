using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpiritHubUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public bool isOpen;
        
    public int selectedToolbarOtherLayer = 0;
    public Button buttonToolbarOther0;  // Minerals
    public Button buttonToolbarOther1;  // Water
    public Button buttonToolbarOther2;  // Air

    public int selectedToolbarTerrainLayer = 0;
    public Button buttonToolbarTerrain0;
    public Button buttonToolbarTerrain1;
    public Button buttonToolbarTerrain2;
    public Button buttonToolbarTerrain3;
    //
    public Button buttonToolbarRemoveDecomposer;
    public Button buttonToolbarDecomposers;
    //
    public Button buttonToolbarRemovePlant;
    public Button buttonToolbarAlgae;
    public Button buttonToolbarPlant1;
    public Button buttonToolbarPlant2;
    //
    public Button buttonToolbarRemoveAnimal;
    public Button buttonToolbarZooplankton;
    public Button buttonToolbarAnimal1;
    public Button buttonToolbarAnimal2;
    public Button buttonToolbarAnimal3;
    public Button buttonToolbarAnimal4;
    
	// Use this for initialization
	void Start () {
        
	}
	
	
        

        Color iconColor = Color.white;
        
        bool isSelectedDecomposers = false;     
        bool isSelectedAlgae = false;  
        bool isSelectedPlants = false;  
        bool isSelectedZooplankton = false;  
        bool isSelectedVertebrate0 = false;  
        bool isSelectedVertebrate1 = false;  
        bool isSelectedVertebrate2 = false;  
        bool isSelectedVertebrate3 = false;  
        bool isSelectedMinerals = false;  
        bool isSelectedWater = false;  
        bool isSelectedAir = false;  
        bool isSelectedTerrain0 = false;  
        bool isSelectedTerrain1 = false;  
        bool isSelectedTerrain2 = false;  
        bool isSelectedTerrain3 = false;  
        if(layerManager.isSelectedTrophicSlot) {
            if(layerManager.selectedTrophicSlotRef.kingdomID == 0) {
                isSelectedDecomposers = true; 
                iconColor = colorDecomposersLayer;
                imageToolbarSpeciesPortraitRender.sprite = spriteSpiritDecomposerIcon;
            }
            else if(layerManager.selectedTrophicSlotRef.kingdomID == 1) {
                if(layerManager.selectedTrophicSlotRef.tierID == 0) {
                    isSelectedAlgae = true;
                    iconColor = colorAlgaeLayer;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritAlgaeIcon;
                }
                else {
                    isSelectedPlants = true;  
                    iconColor = colorPlantsLayer;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritPlantIcon;
                }                                   
            }
            else if(layerManager.selectedTrophicSlotRef.kingdomID == 2) {
                if(layerManager.selectedTrophicSlotRef.tierID == 0) {
                    isSelectedZooplankton = true;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritZooplanktonIcon;
                }
                else {
                    iconColor = colorVertebratesLayer;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritVertebrateIcon;

                    if(layerManager.selectedTrophicSlotRef.slotID == 0) {                        
                        isSelectedVertebrate0 = true;  
                    }
                    else if(layerManager.selectedTrophicSlotRef.slotID == 1) {
                        isSelectedVertebrate1 = true; 
                    }
                    else if(layerManager.selectedTrophicSlotRef.slotID == 2) {
                        isSelectedVertebrate2 = true; 
                    }
                    else {
                        isSelectedVertebrate3 = true; 
                    }
                    
                }
                                   
            }
            else if(layerManager.selectedTrophicSlotRef.kingdomID == 3) {
                iconColor = colorTerrainLayer;
                if(layerManager.selectedTrophicSlotRef.slotID == 0) {
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritWorldIcon;
                    isSelectedTerrain0 = true;
                }
                else if(layerManager.selectedTrophicSlotRef.slotID == 1) {
                    isSelectedTerrain1 = true;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritStoneIcon;
                }
                else if(layerManager.selectedTrophicSlotRef.slotID == 2) {
                    isSelectedTerrain2 = true;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritPebblesIcon;
                }
                else if(layerManager.selectedTrophicSlotRef.slotID == 3) {
                    isSelectedTerrain3 = true;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritSandIcon;
                }
                                   
            }
            else if(layerManager.selectedTrophicSlotRef.kingdomID == 4) {
                if(layerManager.selectedTrophicSlotRef.slotID == 0) {
                    isSelectedMinerals = true;
                    iconColor = colorMineralLayer;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritMineralsIcon;
                }
                else if(layerManager.selectedTrophicSlotRef.slotID == 1) {
                    isSelectedWater = true;
                    iconColor = colorWaterLayer;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritWaterIcon;
                }
                else if(layerManager.selectedTrophicSlotRef.slotID == 2) {
                    isSelectedAir = true;
                    iconColor = colorAirLayer;
                    imageToolbarSpeciesPortraitRender.sprite = spriteSpiritAirIcon;
                }
            }
        }
        else {

        }
        SetToolbarButtonStateUI(ref buttonToolbarDecomposers, layerManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status, isSelectedDecomposers);  
        
        SetToolbarButtonStateUI(ref buttonToolbarAlgae, layerManager.kingdomPlants.trophicTiersList[0].trophicSlots[0].status, isSelectedAlgae);
        SetToolbarButtonStateUI(ref buttonToolbarPlant1, layerManager.kingdomPlants.trophicTiersList[1].trophicSlots[0].status, isSelectedPlants);
         
        SetToolbarButtonStateUI(ref buttonToolbarZooplankton, layerManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0].status, isSelectedZooplankton);        
        SetToolbarButtonStateUI(ref buttonToolbarAnimal1, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0].status, isSelectedVertebrate0);
        SetToolbarButtonStateUI(ref buttonToolbarAnimal2, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[1].status, isSelectedVertebrate1);
        SetToolbarButtonStateUI(ref buttonToolbarAnimal3, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[2].status, isSelectedVertebrate2);
        SetToolbarButtonStateUI(ref buttonToolbarAnimal4, layerManager.kingdomAnimals.trophicTiersList[1].trophicSlots[3].status, isSelectedVertebrate3);
           
        SetToolbarButtonStateUI(ref buttonToolbarTerrain0, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[0].status, isSelectedTerrain0);
        SetToolbarButtonStateUI(ref buttonToolbarTerrain1, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[1].status, isSelectedTerrain1);
        SetToolbarButtonStateUI(ref buttonToolbarTerrain2, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[2].status, isSelectedTerrain2);
        SetToolbarButtonStateUI(ref buttonToolbarTerrain3, layerManager.kingdomTerrain.trophicTiersList[0].trophicSlots[3].status, isSelectedTerrain3);

        SetToolbarButtonStateUI(ref buttonToolbarOther0, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[0].status, isSelectedMinerals);
        SetToolbarButtonStateUI(ref buttonToolbarOther1, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[1].status, isSelectedWater);
        SetToolbarButtonStateUI(ref buttonToolbarOther2, layerManager.kingdomOther.trophicTiersList[0].trophicSlots[2].status, isSelectedAir);
    



private void UpdateSpiritBrushDescriptionsUI() {
        TrophicLayersManager layerManager = gameManager.simulationManager.trophicLayersManager;  

        int linkedSpiritIndex = 0;

        if (layerManager.selectedTrophicSlotRef.kingdomID == 0) {
            linkedSpiritIndex = 7;            
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 1) {
            if(layerManager.selectedTrophicSlotRef.tierID == 0) {
                linkedSpiritIndex = 8;
            }
            else {
                linkedSpiritIndex = 9;
            }            
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 2) {
            if(layerManager.selectedTrophicSlotRef.tierID == 0) {
                linkedSpiritIndex = 10;
            }
            else {
                linkedSpiritIndex = 11;
            }
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 3) {
            if(layerManager.selectedTrophicSlotRef.slotID == 0) {  // world/bedrock
                linkedSpiritIndex = 0;
            }
            else if(layerManager.selectedTrophicSlotRef.slotID == 1) {
                linkedSpiritIndex = 1;
            }
            else if(layerManager.selectedTrophicSlotRef.slotID == 2) {
                linkedSpiritIndex = 2;
            }
            else {
                linkedSpiritIndex = 3;
            }
        }
        else {  // 4 == OTHER
            if(layerManager.selectedTrophicSlotRef.slotID == 0) {  // minerals
                linkedSpiritIndex = 4;
            }
            else if(layerManager.selectedTrophicSlotRef.slotID == 1) {  // water
                linkedSpiritIndex = 5;
            }
            else {  // air
                linkedSpiritIndex = 6;
            }
        }

        int spiritBrushIndex = 0;
        if(curActiveTool == ToolType.Add) {
            spiritBrushIndex = 1;
        }
        else if (curActiveTool == ToolType.Stir) {
            spiritBrushIndex = 2; 
        }

        string strSpiritBrushDescription = "";
        string[] linkedSpiritNamesArray = new string[12]; 
        linkedSpiritNamesArray[0] = "World";
        linkedSpiritNamesArray[1] = "Stone";
        linkedSpiritNamesArray[2] = "Pebbles";
        linkedSpiritNamesArray[3] = "Sand";
        linkedSpiritNamesArray[4] = "Minerals";
        linkedSpiritNamesArray[5] = "Water";
        linkedSpiritNamesArray[6] = "Air";
        linkedSpiritNamesArray[7] = "Decomposers";
        linkedSpiritNamesArray[8] = "Algae";
        linkedSpiritNamesArray[9] = "Plants";
        linkedSpiritNamesArray[10] = "Zooplankton";
        linkedSpiritNamesArray[11] = "Vertebrates";
        
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

        string startTxt = "Left-Click:\n";
        string midTxt = "\n\nRight-Click:\n";
        string[][] strBrushEffectsArray = new string[5][];
        for(int s = 0; s < 5; s++) {
            strBrushEffectsArray[s] = new string[12];            
        }
        // KNOWLEDGE BRUSH:
        strBrushEffectsArray[0][0] = startTxt + "Knowledge Spirit --> World Spirit" + midTxt + "Knowledge Spirit --> World Spirit";
        strBrushEffectsArray[0][1] = startTxt + "Knowledge Spirit --> Stone Spirit" + midTxt + "Knowledge Spirit --> Stone Spirit";
        strBrushEffectsArray[0][2] = startTxt + "Knowledge Spirit --> Pebbles Spirit" + midTxt + "Knowledge Spirit --> Pebbles Spirit";
        strBrushEffectsArray[0][3] = startTxt + "Knowledge Spirit --> Sand Spirit" + midTxt + "Knowledge Spirit --> Sand Spirit";
        strBrushEffectsArray[0][4] = startTxt + "Knowledge Spirit --> Minerals Spirit" + midTxt + "Knowledge Spirit --> Minerals Spirit";
        strBrushEffectsArray[0][5] = startTxt + "Knowledge Spirit --> Water Spirit" + midTxt + "Knowledge Spirit --> Water Spirit";
        strBrushEffectsArray[0][6] = startTxt + "Knowledge Spirit --> Air Spirit" + midTxt + "Knowledge Spirit --> Air Spirit";
        strBrushEffectsArray[0][7] = startTxt + "Knowledge Spirit --> Decomposer Spirit" + midTxt + "Knowledge Spirit --> Decomposer Spirit";
        strBrushEffectsArray[0][8] = startTxt + "Knowledge Spirit --> Algae Spirit" + midTxt + "Knowledge Spirit --> Algae Spirit";
        strBrushEffectsArray[0][9] = startTxt + "Knowledge Spirit --> Plant Spirit" + midTxt + "Knowledge Spirit --> Plant Spirit";
        strBrushEffectsArray[0][10] = startTxt + "Knowledge Spirit --> Zooplankton Spirit" + midTxt + "Knowledge Spirit --> Zooplankton Spirit";
        strBrushEffectsArray[0][11] = startTxt + "Knowledge Spirit --> Vertebrate Spirit" + midTxt + "Knowledge Spirit --> Vertebrate Spirit";
        // CREATION BRUSH:
        strBrushEffectsArray[1][0] = startTxt + "Creates World" + midTxt + "None";
        strBrushEffectsArray[1][1] = startTxt + "Raises stone from deep below" + midTxt + "Destroys stone, deeping the Pond";
        strBrushEffectsArray[1][2] = startTxt + "Creates mounds of pebbles" + midTxt + "Removes pebbles from the area";
        strBrushEffectsArray[1][3] = startTxt + "Blankets the terrain with sand" + midTxt + "Removes sand from the area";
        strBrushEffectsArray[1][4] = startTxt + "Creates nutrient-rich minerals in the ground" + midTxt + "Saps nutrients out of the environment";
        strBrushEffectsArray[1][5] = startTxt + "Raises the water level" + midTxt + "Lowers water level";
        strBrushEffectsArray[1][6] = startTxt + "Increases wind strength" + midTxt + "Decreases wind strength";
        strBrushEffectsArray[1][7] = startTxt + "Creates Decomposers" + midTxt + "Kills decomposers in the area";
        strBrushEffectsArray[1][8] = startTxt + "Creates a bloom of Algae" + midTxt + "Kills algae in the area";
        strBrushEffectsArray[1][9] = startTxt + "Creates floating plant seedlings" + midTxt + "Kills plants in the area";
        strBrushEffectsArray[1][10] = startTxt + "Creates simple tiny creatures" + midTxt + "Kills nearby zooplankton";
        strBrushEffectsArray[1][11] = startTxt + "Hatches Vertebrates" + midTxt + "Kills Animals";

        // STIR BRUSH:
        strBrushEffectsArray[2][0] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][1] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][2] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][3] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][4] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][5] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][6] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][7] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][8] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][9] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][10] = startTxt + "Drags water along with itself while moving" + midTxt + "None";
        strBrushEffectsArray[2][11] = startTxt + "Drags water along with itself while moving" + midTxt + "None";

        // WATCHER BRUSH: // RESOURCES
        strBrushEffectsArray[3][0] = startTxt + "Senses nearby spirits" + midTxt + "None";
        strBrushEffectsArray[3][1] = startTxt + "See properties of the ground" + midTxt + "None";
        strBrushEffectsArray[3][2] = startTxt + "See properties of the ground" + midTxt + "None";
        strBrushEffectsArray[3][3] = startTxt + "See properties of the ground" + midTxt + "None";
        strBrushEffectsArray[3][4] = startTxt + "See properties of the ground" + midTxt + "None";
        strBrushEffectsArray[3][5] = startTxt + "Info" + midTxt + "None";
        strBrushEffectsArray[3][6] = startTxt + "Info" + midTxt + "None";
        strBrushEffectsArray[3][7] = startTxt + "Info" + midTxt + "None";
        strBrushEffectsArray[3][8] = startTxt + "Info" + midTxt + "None";
        // WATCHER BRUSH: // separate brush???
        strBrushEffectsArray[3][9] = startTxt + "Follows the nearest Plant" + midTxt + "Stops following";
        strBrushEffectsArray[3][10] = startTxt + "Follows the nearest Zooplankton" + midTxt + "Stops following";
        strBrushEffectsArray[3][11] = startTxt + "Follows the nearest Vertebrate" + midTxt + "Stops following";

        // MUTATION BRUSH:
        strBrushEffectsArray[4][0] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][1] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][2] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][3] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][4] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][5] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][6] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][7] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][8] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][9] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][10] = startTxt + "Mutates more" + midTxt + "Mutates less";
        strBrushEffectsArray[4][11] = startTxt + "Mutates more" + midTxt + "Mutates less";

        //strBrushEffectsArray[1][0] = startTxt + "Creates " + linkedSpiritNamesArray[linkedSpiritIndex] + midTxt + "Kills " + linkedSpiritNamesArray[linkedSpiritIndex];
        //strBrushEffectsArray[2][0] = startTxt + "Spirit movement drags water along with it." + midTxt + "None";
        //strBrushEffectsArray[3][0] = startTxt + "Follows the nearest " + linkedSpiritNamesArray[linkedSpiritIndex] + midTxt + "Stops following";
        //strBrushEffectsArray[4][0] = startTxt + "Mutates nearby " + linkedSpiritNamesArray[linkedSpiritIndex] + midTxt + "Lowers mutation rates";
        // STONE :
        //strBrushEffectsArray[5] = "Extra.";
        //strSpiritBrushEffects = "Left-Click:\n" + strLeftClickEffect[leftClickDescriptionIndex] + "\n\nRight-Click:\n" + strRightClickEffect[rightClickDescriptionIndex];
        
        textSpiritBrushDescription.text = strSpiritBrushDescriptionArray[spiritBrushIndex];
        textSpiritBrushEffects.text = strBrushEffectsArray[spiritBrushIndex][linkedSpiritIndex];
        textLinkedSpiritDescription.text = strLinkedSpiritDescriptionArray[linkedSpiritIndex];
    }
}
