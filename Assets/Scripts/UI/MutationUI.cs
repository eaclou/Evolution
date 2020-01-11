using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MutationUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public bool isUnlocked;
    public bool isOpen;

    public Image imageMutationCurTarget; // in watcher panel
    public Text textMutationTargetLayer;

    // Mutation Panel elements:
    public GameObject panelMutationSpirit;
    public Image imageMutationPanelThumbnailA;
    public Image imageMutationPanelThumbnailB;
    public Image imageMutationPanelThumbnailC;
    public Image imageMutationPanelThumbnailD;
    public Text textMutationPanelOptionA;
    public Text textMutationPanelOptionB;
    public Text textMutationPanelOptionC;
    public Text textMutationPanelOptionD;
    public Image imageMutationPanelHighlightA;  // changes upon selection
    public Image imageMutationPanelHighlightB;
    public Image imageMutationPanelHighlightC;
    public Image imageMutationPanelHighlightD;
    public Button buttonMutationReroll;
    public Image imageMutationPanelCurPortrait;
    public Image imageMutationPanelNewPortrait;
    public Text textMutationPanelCur;
    public Text textMutationPanelNew;
    public Text textMutationPanelTitleCur;
    public Text textMutationPanelTitleNew;
    public GameObject panelNewMutationPreview;
    private int selectedToolbarMutationID = 0;
    public Button buttonToolbarMutateConfirm;


	// Use this for initialization
	void Start () {
		
	}
	
    private void UpdateUI(TrophicLayersManager layerManager) {
        //textToolbarWingPanelName.text = "Mutations:";
        
        textMutationPanelOptionA.text = "Tiny";
        textMutationPanelOptionB.text = "Small";
        textMutationPanelOptionC.text = "Large";
        textMutationPanelOptionD.text = "Huge";

        textMutationPanelTitleCur.text = "CURRENT";
        string[] titlesTxt = new string[4];
        titlesTxt[0] = "TINY MUTATION";
        titlesTxt[1] = "SMALL MUTATION";
        titlesTxt[2] = "LARGE MUTATION";
        titlesTxt[3] = "HUGE MUTATION";
        textMutationPanelTitleNew.text = titlesTxt[selectedToolbarMutationID];

        buttonToolbarMutateConfirm.gameObject.SetActive(false);
        if(selectedToolbarMutationID >= 0) {
            buttonToolbarMutateConfirm.gameObject.SetActive(true);

            if(selectedToolbarMutationID == 0) {
                imageMutationPanelHighlightA.color = Color.gray;
                imageMutationPanelHighlightB.color = Color.black;
                imageMutationPanelHighlightC.color = Color.black;
                imageMutationPanelHighlightD.color = Color.black;
            }
            else if(selectedToolbarMutationID == 1) {
                imageMutationPanelHighlightA.color = Color.black;
                imageMutationPanelHighlightB.color = Color.gray;
                imageMutationPanelHighlightC.color = Color.black;
                imageMutationPanelHighlightD.color = Color.black;
            }
            else if(selectedToolbarMutationID == 2) {
                imageMutationPanelHighlightA.color = Color.black;
                imageMutationPanelHighlightB.color = Color.black;
                imageMutationPanelHighlightC.color = Color.gray;
                imageMutationPanelHighlightD.color = Color.black;
            }
            else if(selectedToolbarMutationID == 3) {
                imageMutationPanelHighlightA.color = Color.black;
                imageMutationPanelHighlightB.color = Color.black;
                imageMutationPanelHighlightC.color = Color.black;
                imageMutationPanelHighlightD.color = Color.gray;
            }
        }

        TrophicSlot slotRef = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot;

        textMutationTargetLayer.text = slotRef.speciesName;
        textMutationTargetLayer.color = uiManagerRef.worldSpiritHubUI.curIconColor;
        imageMutationCurTarget.color = uiManagerRef.worldSpiritHubUI.curIconColor;
        imageMutationCurTarget.sprite = uiManagerRef.worldSpiritHubUI.curIconSprite;

        if(slotRef.kingdomID == 0) { // DECOMPOSERS
            // Look up decomposer variants and populate UI elements from them:
            //textMutationPanelOptionA.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[0].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionB.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[1].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionC.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[2].textDescriptionMutation; // "Major Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionD.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[3].textDescriptionMutation; // "Major Decomposers Mutation!";
            Color uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[0].displayColor;
            uiColor.a = 1f;
            imageMutationPanelThumbnailA.color = uiColor; // UnityEngine.Random.ColorHSV();
            uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[1].displayColor;
            uiColor.a = 1f;
            imageMutationPanelThumbnailB.color = uiColor;
            uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[2].displayColor;
            uiColor.a = 1f;
            imageMutationPanelThumbnailC.color = uiColor;
            uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[3].displayColor;
            uiColor.a = 1f;
            imageMutationPanelThumbnailD.color = uiColor;

            uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].displayColor;
            uiColor.a = 1f;
            imageMutationPanelNewPortrait.color = uiColor;
            textMutationPanelCur.text = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.textDescriptionMutation; // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();
            textMutationPanelNew.text = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation; // "placeholder";
            uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.displayColor;
            uiColor.a = 1f;
            imageMutationPanelCurPortrait.color = uiColor; 
        }
        else if(slotRef.kingdomID == 1) { // PLANTS

            // Algae Particles:
            //gameManager.simulationManager.vegetationManager.

            //textMutationPanelOptionA.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[0].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionB.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[1].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionC.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[2].textDescriptionMutation; // "Major Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionD.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[3].textDescriptionMutation; // "Major Decomposers Mutation!";
            if(slotRef.tierID == 0) {
                Color uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[0].displayColor; // new Color(hue.x, hue.y, hue.z);
                //uiColor.a = 1f;
                imageMutationPanelThumbnailA.color = uiColor; // UnityEngine.Random.ColorHSV();
                uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[1].displayColor;
                imageMutationPanelThumbnailB.color = uiColor;
                uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[2].displayColor;
                imageMutationPanelThumbnailC.color = uiColor;
                uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[3].displayColor;
                imageMutationPanelThumbnailD.color = uiColor;
            
                uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].displayColor;
                //uiColor.a = 1f;
                imageMutationPanelNewPortrait.color = uiColor;
                textMutationPanelCur.text = uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.textDescriptionMutation; // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();
                textMutationPanelNew.text = uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation; // "placeholder";
            
                uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.displayColor;
                imageMutationPanelCurPortrait.color = uiColor; 
            }
            else {  // PLANTS:
                Color uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[0].displayColor; // new Color(hue.x, hue.y, hue.z);
                //uiColor.a = 1f;
                imageMutationPanelThumbnailA.color = uiColor; // UnityEngine.Random.ColorHSV();
                uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[1].displayColor;
                imageMutationPanelThumbnailB.color = uiColor;
                uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[2].displayColor;
                imageMutationPanelThumbnailC.color = uiColor;
                uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[3].displayColor;
                imageMutationPanelThumbnailD.color = uiColor;
            
                uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].displayColor;
                //uiColor.a = 1f;
                imageMutationPanelNewPortrait.color = uiColor;
                textMutationPanelCur.text = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.textDescriptionMutation; // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();
                textMutationPanelNew.text = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation; // "placeholder";
            
                uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.displayColor;
                imageMutationPanelCurPortrait.color = uiColor; 
            }
            
        }
        else if(slotRef.kingdomID == 2) { // ANIMALS
            if(slotRef.tierID == 0) {  // Zooplankton
                //textMutationPanelOptionA.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[0].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].color.a * 100f).ToString("F0") + "%";
                //textMutationPanelOptionB.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[1].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].color.a * 100f).ToString("F0") + "%";
                //textMutationPanelOptionC.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[2].textDescriptionMutation; // "Major Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].color.a * 100f).ToString("F0") + "%";
                //textMutationPanelOptionD.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[3].textDescriptionMutation; // "Major Decomposers Mutation!";
                Color uiColor = uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[0].representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelThumbnailA.color = uiColor; // UnityEngine.Random.ColorHSV();
                uiColor = uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[1].representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelThumbnailB.color = uiColor;
                uiColor = uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[2].representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelThumbnailC.color = uiColor;
                uiColor = uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[3].representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelThumbnailD.color = uiColor;

                uiColor = uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelNewPortrait.color = uiColor;
                textMutationPanelCur.text = uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent.textDescriptionMutation; // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();
                textMutationPanelNew.text = uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation; // "placeholder";
                uiColor = uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent.representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelCurPortrait.color = uiColor; 
            }
            else { // vertebrates
                int slotID = slotRef.slotID;
                //textMutationPanelOptionA.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][0].textDescriptionMutation; 
                //textMutationPanelOptionB.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][1].textDescriptionMutation; 
                //textMutationPanelOptionC.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][2].textDescriptionMutation; 
                //textMutationPanelOptionD.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][3].textDescriptionMutation;

                int speciesID = slotRef.linkedSpeciesID;
                Vector3 hue0 = uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][0].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                imageMutationPanelThumbnailA.color = new Color(hue0.x, hue0.y, hue0.z); // gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][0].displayColor; // new Color(hue0.x, hue0.y, hue0.z); 
                Vector3 hue1 = uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][1].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                imageMutationPanelThumbnailB.color = new Color(hue1.x, hue1.y, hue1.z); // gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][1].displayColor; //
                Vector3 hue2 = uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][2].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                imageMutationPanelThumbnailC.color = new Color(hue2.x, hue2.y, hue2.z); // gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][2].displayColor; //
                Vector3 hue3 = uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][3].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                imageMutationPanelThumbnailD.color = new Color(hue3.x, hue3.y, hue3.z); // gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][3].displayColor; //
                //Vector3 hue0 = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;

                Vector3 hue = uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                Debug.Log("ADSF: " + hue.ToString());
                //Vector3 hueCur = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][0].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                Color thumbCol = new Color(hue.x, hue.y, hue.z); 
                imageMutationPanelCurPortrait.color = thumbCol;
                imageMutationPanelCurPortrait.sprite = null;

                //imageMutationPanelCurPortrait.color = Color.white;
                Vector3 hueNew = uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][selectedToolbarMutationID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                //imageToolbarSpeciesPortraitBorder.color = thumbCol; 
                imageMutationPanelNewPortrait.color = new Color(hueNew.x, hueNew.y, hueNew.z); // uiColor;
                textMutationPanelCur.text = uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotID].name; // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();
                textMutationPanelNew.text = uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][selectedToolbarMutationID].textDescriptionMutation; // "placeholder";
            }
        }
        else if(slotRef.kingdomID == 3) { // Terrain
            if (slotRef.tierID == 0) {
                Color colorOptionA = Color.white;
                Color colorOptionB = Color.white;
                Color colorOptionC = Color.white;
                Color colorOptionD = Color.white;
                Color colorCur = Color.white;
                Color colorNew = Color.white;
                int selectedIndex = selectedToolbarMutationID;
                if(selectedIndex < 0) {
                    selectedIndex = 0;
                    panelNewMutationPreview.SetActive(false);
                }
                else {
                    panelNewMutationPreview.SetActive(true);
                }
                if(slotRef.slotID == 0) {
                    colorOptionA = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].color;
                    colorOptionB = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].color;
                    colorOptionC = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].color;
                    colorOptionD = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[3].color;
                    // *** make these Text objects into an array:
                    //textMutationPanelOptionA.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].textDescriptionMutation; // "Minor Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionB.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].textDescriptionMutation; // "Minor Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionC.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].textDescriptionMutation; // "Major Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionD.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[3].textDescriptionMutation; // "Major Mutation!";
                    colorCur = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent.color;
                    imageMutationPanelNewPortrait.color = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[selectedIndex].color;
                    textMutationPanelCur.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent.elevationChange.ToString();
                    textMutationPanelNew.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[selectedIndex].textDescriptionMutation;
                }
                else if(slotRef.slotID == 1) {
                    colorOptionA = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[0].color;
                    colorOptionB = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[1].color;
                    colorOptionC = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[2].color;
                    colorOptionD = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[3].color;
                    //textMutationPanelOptionA.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[0].textDescriptionMutation; // "Minor Stones Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[0].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionB.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[1].textDescriptionMutation; // "Stones Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[1].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionC.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[2].textDescriptionMutation; // "Major Stones Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[2].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionD.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[3].textDescriptionMutation; // "Major Stones Mutation!";
                    colorCur = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent.color;
                    imageMutationPanelNewPortrait.color = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[selectedIndex].color;
                    textMutationPanelCur.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent.elevationChange.ToString();
                    textMutationPanelNew.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[selectedIndex].textDescriptionMutation;
                }
                else if(slotRef.slotID == 2) {
                    colorOptionA = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[0].color;
                    colorOptionB = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[1].color;
                    colorOptionC = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[2].color;
                    colorOptionD = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[3].color;
                    //textMutationPanelOptionA.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[0].textDescriptionMutation; // "Minor Pebbles Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[0].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionB.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[1].textDescriptionMutation; // "Pebbles Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[1].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionC.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[2].textDescriptionMutation; // "Major Pebbles Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[2].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionD.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[3].textDescriptionMutation; // "Major Pebbles Mutation!"; 
                    colorCur = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent.color;
                    imageMutationPanelNewPortrait.color = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[selectedIndex].color;
                    textMutationPanelCur.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent.elevationChange.ToString();
                    textMutationPanelNew.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[selectedIndex].textDescriptionMutation;
                }
                else {
                    colorOptionA = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[0].color;
                    colorOptionB = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[1].color;
                    colorOptionC = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[2].color;
                    colorOptionC = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[3].color;
                    //textMutationPanelOptionA.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[0].textDescriptionMutation; // "Minor Sand Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[0].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionB.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[1].textDescriptionMutation; // "Sand Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[1].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionC.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[2].textDescriptionMutation; // "Major Sand Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[2].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionD.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[3].textDescriptionMutation; // "Major Sand Mutation!";
                    colorCur = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent.color;    
                    imageMutationPanelNewPortrait.color = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[selectedIndex].color;
                    textMutationPanelCur.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent.elevationChange.ToString();
                    textMutationPanelNew.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[selectedIndex].textDescriptionMutation;
                }
                // **** v v v Make these into arrays during cleanup
                colorOptionA.a = 1f;
                colorOptionB.a = 1f;
                colorOptionC.a = 1f;
                colorOptionD.a = 1f;
                colorCur.a = 1f;
                imageMutationPanelThumbnailA.color = colorOptionA;
                imageMutationPanelThumbnailA.sprite = null;
                imageMutationPanelThumbnailB.color = colorOptionB;
                imageMutationPanelThumbnailB.sprite = null;
                imageMutationPanelThumbnailC.color = colorOptionC;
                imageMutationPanelThumbnailC.sprite = null;
                imageMutationPanelThumbnailD.color = colorOptionD;
                imageMutationPanelThumbnailD.sprite = null;

                imageMutationPanelCurPortrait.color = colorCur;

            }
        }
    }

    public void UpdateMutationPanelUI(TrophicLayersManager layerManager) {
        panelMutationSpirit.SetActive(isOpen);
        if(isOpen) {
            UpdateUI(layerManager);
        }
    }

    public void OpenMutationPanel() {
        isOpen = true;
    }
    
    public void ClickToolButton() {
        Debug.Log("Click mutation toggle button)");
        isOpen = !isOpen;
    }
    public void ClickMutationConfirm() {
        TrophicSlot slotRef = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot;
        if(slotRef.kingdomID == 0) {  // Decomposers
            uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID];
            //gameManager.simulationManager.vegetationManager.WorldLayerDecomposerGenomeStuff(ref decomposerSlotGenomeCurrent, 0f);
            uiManagerRef.gameManager.simulationManager.vegetationManager.GenerateWorldLayerDecomposersGenomeMutationOptions();
        }
        else if(slotRef.kingdomID == 1) {  // Plants
            if (slotRef.tierID == 0) {
                uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent = uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID];
                uiManagerRef.gameManager.simulationManager.vegetationManager.GenerateWorldLayerAlgaeGridGenomeMutationOptions();
            }
            else {
                // OLD //gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID];
                uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.plantRepData = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].plantRepData;
                uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.textDescriptionMutation = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation;
                uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.growthRate = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].growthRate;
                uiManagerRef.gameManager.simulationManager.vegetationManager.ProcessPlantSlotMutation();
                uiManagerRef.gameManager.simulationManager.vegetationManager.GenerateWorldLayerPlantParticleGenomeMutationOptions();
                 
            }
            
            //gameManager.simulationManager.vegetationManager.ProcessSlotMutation();
            //algaeRepData = gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.algaeRepData;
        }
        else if(slotRef.kingdomID == 2) {  // Animals
            if(slotRef.tierID == 0) { // zooplankton
                uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent = uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID];
                uiManagerRef.gameManager.simulationManager.zooplanktonManager.GenerateWorldLayerZooplanktonGenomeMutationOptions();
                uiManagerRef.gameManager.simulationManager.zooplanktonManager.ProcessSlotMutation();
            }
            else { // vertebrates
                // *** REFERENCE ISSUE!!!!!
                AgentGenome parentGenome = uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotRef.slotID][selectedToolbarMutationID].representativeGenome;
                //generate "mutated" genome copy with 0 mutationSize ??? workaround:::::  ***********
                uiManagerRef.gameManager.simulationManager.settingsManager.mutationSettingsVertebrates.mutationStrengthSlot = 0f;

                //vertebrateSlotsGenomesCurrentArray[slotID].representativeGenome  // **** Use this genome as basis?
                AgentGenome mutatedGenome = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[0].Mutate(parentGenome, true, true);  // does speciesPoolIndex matter?

                uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].representativeGenome = mutatedGenome;
                uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].name = uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotRef.slotID][selectedToolbarMutationID].name;
                //gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotRef.slotID][selectedToolbarMutationID];
                //gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].representativeGenome.bodyGenome.appearanceGenome.huePrimary = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotRef.slotID][selectedToolbarMutationID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;

                Debug.Log("CONFIR<M  " + uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].representativeGenome.bodyGenome.appearanceGenome.huePrimary.ToString());
                //gameManager.simulationManager.masterGenomePool.
                //gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[slotRef.linkedSpeciesID].representativeGenome = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].representativeGenome;

                uiManagerRef.gameManager.simulationManager.masterGenomePool.GenerateWorldLayerVertebrateGenomeMutationOptions(slotRef.slotID, slotRef.linkedSpeciesID);
                //gameManager.simulationManager.masterGenomePool.ProcessSlotMutation(slotRef.slotID, selectedToolbarMutationID, slotRef.linkedSpeciesID);
                //InitToolbarPortraitCritterData(slotRef);
            }
        }
        else if(slotRef.kingdomID == 3) { // Terrain
            if(slotRef.slotID == 0) {
                uiManagerRef.gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[selectedToolbarMutationID];
            }
            else if(slotRef.slotID == 1) {
                uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[selectedToolbarMutationID];
            }
            else if(slotRef.slotID == 2) {
                uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[selectedToolbarMutationID];
            }
            else if(slotRef.slotID == 3) { // world?
                uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[selectedToolbarMutationID];
            }

            // if terrain:
            uiManagerRef.gameManager.theRenderKing.baronVonTerrain.GenerateTerrainSlotGenomeMutationOptions(slotRef.slotID);
            //gameManager.theRenderKing.ClickTestTerrainUpdateMaps(false, 0.05f); // refresh color
        }
        Debug.Log("MUTATION!!! kingdom(" + slotRef.kingdomID.ToString() + ")");
        //selectedToolbarMutationID = 0; // Reset?? figure out what you want to do here
    }
    public void ClickMutationOption(int id) { // **** Need better smarter way to detect selected slot and point to corresponding data
        //Debug.Log("ClickMutationOption(" + id.ToString() + ")");

        selectedToolbarMutationID = id;
                
    }

}
