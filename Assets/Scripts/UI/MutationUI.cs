using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MutationUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public bool isOpen;

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
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ClickMutationConfirm() {
        TrophicSlot slotRef = gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef;
        if(slotRef.kingdomID == 0) {  // Decomposers
            gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID];
            //gameManager.simulationManager.vegetationManager.WorldLayerDecomposerGenomeStuff(ref decomposerSlotGenomeCurrent, 0f);
            gameManager.simulationManager.vegetationManager.GenerateWorldLayerDecomposersGenomeMutationOptions();
        }
        else if(slotRef.kingdomID == 1) {  // Plants
            if (slotRef.tierID == 0) {
                gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID];
                gameManager.simulationManager.vegetationManager.GenerateWorldLayerAlgaeGridGenomeMutationOptions();
            }
            else {
                // OLD //gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID];
                gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.plantRepData = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].plantRepData;
                gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.textDescriptionMutation = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation;
                gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.growthRate = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].growthRate;
                gameManager.simulationManager.vegetationManager.ProcessPlantSlotMutation();
                gameManager.simulationManager.vegetationManager.GenerateWorldLayerPlantParticleGenomeMutationOptions();
                 
            }
            
            //gameManager.simulationManager.vegetationManager.ProcessSlotMutation();
            //algaeRepData = gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.algaeRepData;
        }
        else if(slotRef.kingdomID == 2) {  // Animals
            if(slotRef.tierID == 0) { // zooplankton
                gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID];
                gameManager.simulationManager.zooplanktonManager.GenerateWorldLayerZooplanktonGenomeMutationOptions();
                gameManager.simulationManager.zooplanktonManager.ProcessSlotMutation();
            }
            else { // vertebrates
                // *** REFERENCE ISSUE!!!!!
                AgentGenome parentGenome = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotRef.slotID][selectedToolbarMutationID].representativeGenome;
                //generate "mutated" genome copy with 0 mutationSize ??? workaround:::::  ***********
                gameManager.simulationManager.settingsManager.mutationSettingsVertebrates.mutationStrengthSlot = 0f;

                //vertebrateSlotsGenomesCurrentArray[slotID].representativeGenome  // **** Use this genome as basis?
                AgentGenome mutatedGenome = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[0].Mutate(parentGenome, true, true);  // does speciesPoolIndex matter?

                gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].representativeGenome = mutatedGenome;
                gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].name = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotRef.slotID][selectedToolbarMutationID].name;
                //gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotRef.slotID][selectedToolbarMutationID];
                //gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].representativeGenome.bodyGenome.appearanceGenome.huePrimary = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotRef.slotID][selectedToolbarMutationID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;

                Debug.Log("CONFIR<M  " + gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].representativeGenome.bodyGenome.appearanceGenome.huePrimary.ToString());
                //gameManager.simulationManager.masterGenomePool.
                //gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[slotRef.linkedSpeciesID].representativeGenome = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotRef.slotID].representativeGenome;

                gameManager.simulationManager.masterGenomePool.GenerateWorldLayerVertebrateGenomeMutationOptions(slotRef.slotID, slotRef.linkedSpeciesID);
                //gameManager.simulationManager.masterGenomePool.ProcessSlotMutation(slotRef.slotID, selectedToolbarMutationID, slotRef.linkedSpeciesID);
                //InitToolbarPortraitCritterData(slotRef);
            }
        }
        else if(slotRef.kingdomID == 3) { // Terrain
            if(slotRef.slotID == 0) {
                //gameManager.theRenderKing.baronVonTerrain.ApplyMutation(id);
                gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[selectedToolbarMutationID];
            }
            else if(slotRef.slotID == 1) {
                //gameManager.theRenderKing.baronVonTerrain.ApplyMutation(id);
                gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[selectedToolbarMutationID];
            }
            else if(slotRef.slotID == 2) {
                gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[selectedToolbarMutationID];
            }
            else if(slotRef.slotID == 3) {
                gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[selectedToolbarMutationID];
            }

            // if terrain:
            gameManager.theRenderKing.baronVonTerrain.GenerateTerrainSlotGenomeMutationOptions(slotRef.slotID);
            //gameManager.theRenderKing.ClickTestTerrainUpdateMaps(false, 0.05f); // refresh color
        }
        Debug.Log("MUTATION!!! kingdom(" + slotRef.kingdomID.ToString() + ")");
        //selectedToolbarMutationID = 0; // Reset?? figure out what you want to do here
    }
    public void ClickMutationOption(int id) { // **** Need better smarter way to detect selected slot and point to corresponding data
        //Debug.Log("ClickMutationOption(" + id.ToString() + ")");

        selectedToolbarMutationID = id;
                
    }
    private void UpdateToolbarMutationPanel(TrophicLayersManager layerManager) {
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


        if(layerManager.selectedTrophicSlotRef.kingdomID == 0) { // DECOMPOSERS
            // Look up decomposer variants and populate UI elements from them:
            //textMutationPanelOptionA.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[0].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionB.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[1].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionC.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[2].textDescriptionMutation; // "Major Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionD.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[3].textDescriptionMutation; // "Major Decomposers Mutation!";
            Color uiColor = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[0].displayColor;
            uiColor.a = 1f;
            imageMutationPanelThumbnailA.color = uiColor; // UnityEngine.Random.ColorHSV();
            uiColor = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[1].displayColor;
            uiColor.a = 1f;
            imageMutationPanelThumbnailB.color = uiColor;
            uiColor = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[2].displayColor;
            uiColor.a = 1f;
            imageMutationPanelThumbnailC.color = uiColor;
            uiColor = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[3].displayColor;
            uiColor.a = 1f;
            imageMutationPanelThumbnailD.color = uiColor;

            uiColor = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].displayColor;
            uiColor.a = 1f;
            imageMutationPanelNewPortrait.color = uiColor;
            textMutationPanelCur.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.textDescriptionMutation; // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();
            textMutationPanelNew.text = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation; // "placeholder";
            uiColor = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.displayColor;
            uiColor.a = 1f;
            imageMutationPanelCurPortrait.color = uiColor; 
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 1) { // PLANTS

            // Algae Particles:
            //gameManager.simulationManager.vegetationManager.

            //textMutationPanelOptionA.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[0].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionB.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[1].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionC.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[2].textDescriptionMutation; // "Major Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].color.a * 100f).ToString("F0") + "%";
            //textMutationPanelOptionD.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[3].textDescriptionMutation; // "Major Decomposers Mutation!";
            if(layerManager.selectedTrophicSlotRef.tierID == 0) {
                Color uiColor = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[0].displayColor; // new Color(hue.x, hue.y, hue.z);
                //uiColor.a = 1f;
                imageMutationPanelThumbnailA.color = uiColor; // UnityEngine.Random.ColorHSV();
                uiColor = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[1].displayColor;
                imageMutationPanelThumbnailB.color = uiColor;
                uiColor = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[2].displayColor;
                imageMutationPanelThumbnailC.color = uiColor;
                uiColor = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[3].displayColor;
                imageMutationPanelThumbnailD.color = uiColor;
            
                uiColor = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].displayColor;
                //uiColor.a = 1f;
                imageMutationPanelNewPortrait.color = uiColor;
                textMutationPanelCur.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.textDescriptionMutation; // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();
                textMutationPanelNew.text = gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation; // "placeholder";
            
                uiColor = gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.displayColor;
                imageMutationPanelCurPortrait.color = uiColor; 
            }
            else {  // PLANTS:
                Color uiColor = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[0].displayColor; // new Color(hue.x, hue.y, hue.z);
                //uiColor.a = 1f;
                imageMutationPanelThumbnailA.color = uiColor; // UnityEngine.Random.ColorHSV();
                uiColor = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[1].displayColor;
                imageMutationPanelThumbnailB.color = uiColor;
                uiColor = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[2].displayColor;
                imageMutationPanelThumbnailC.color = uiColor;
                uiColor = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[3].displayColor;
                imageMutationPanelThumbnailD.color = uiColor;
            
                uiColor = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].displayColor;
                //uiColor.a = 1f;
                imageMutationPanelNewPortrait.color = uiColor;
                textMutationPanelCur.text = gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.textDescriptionMutation; // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();
                textMutationPanelNew.text = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation; // "placeholder";
            
                uiColor = gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.displayColor;
                imageMutationPanelCurPortrait.color = uiColor; 
            }
            
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 2) { // ANIMALS
            if(layerManager.selectedTrophicSlotRef.tierID == 0) {  // Zooplankton
                //textMutationPanelOptionA.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[0].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].color.a * 100f).ToString("F0") + "%";
                //textMutationPanelOptionB.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[1].textDescriptionMutation; // "Minor Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].color.a * 100f).ToString("F0") + "%";
                //textMutationPanelOptionC.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[2].textDescriptionMutation; // "Major Decomposers Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].color.a * 100f).ToString("F0") + "%";
                //textMutationPanelOptionD.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[3].textDescriptionMutation; // "Major Decomposers Mutation!";
                Color uiColor = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[0].representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelThumbnailA.color = uiColor; // UnityEngine.Random.ColorHSV();
                uiColor = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[1].representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelThumbnailB.color = uiColor;
                uiColor = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[2].representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelThumbnailC.color = uiColor;
                uiColor = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[3].representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelThumbnailD.color = uiColor;

                uiColor = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelNewPortrait.color = uiColor;
                textMutationPanelCur.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent.textDescriptionMutation; // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();
                textMutationPanelNew.text = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation; // "placeholder";
                uiColor = gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent.representativeData.color;
                uiColor.a = 1f;
                imageMutationPanelCurPortrait.color = uiColor; 
            }
            else { // vertebrates
                int slotID = layerManager.selectedTrophicSlotRef.slotID;
                //textMutationPanelOptionA.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][0].textDescriptionMutation; 
                //textMutationPanelOptionB.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][1].textDescriptionMutation; 
                //textMutationPanelOptionC.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][2].textDescriptionMutation; 
                //textMutationPanelOptionD.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][3].textDescriptionMutation;

                int speciesID = layerManager.selectedTrophicSlotRef.linkedSpeciesID;
                Vector3 hue0 = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][0].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                imageMutationPanelThumbnailA.color = new Color(hue0.x, hue0.y, hue0.z); // gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][0].displayColor; // new Color(hue0.x, hue0.y, hue0.z); 
                Vector3 hue1 = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][1].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                imageMutationPanelThumbnailB.color = new Color(hue1.x, hue1.y, hue1.z); // gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][1].displayColor; //
                Vector3 hue2 = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][2].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                imageMutationPanelThumbnailC.color = new Color(hue2.x, hue2.y, hue2.z); // gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][2].displayColor; //
                Vector3 hue3 = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][3].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                imageMutationPanelThumbnailD.color = new Color(hue3.x, hue3.y, hue3.z); // gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][3].displayColor; //
                //Vector3 hue0 = gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;

                Vector3 hue = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[layerManager.selectedTrophicSlotRef.slotID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                Debug.Log("ADSF: " + hue.ToString());
                //Vector3 hueCur = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][0].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                Color thumbCol = new Color(hue.x, hue.y, hue.z); 
                imageMutationPanelCurPortrait.color = thumbCol;
                imageMutationPanelCurPortrait.sprite = null;

                //imageMutationPanelCurPortrait.color = Color.white;
                Vector3 hueNew = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][selectedToolbarMutationID].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
                imageToolbarSpeciesPortraitBorder.color = thumbCol; 
                imageMutationPanelNewPortrait.color = new Color(hueNew.x, hueNew.y, hueNew.z); // uiColor;
                textMutationPanelCur.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesCurrentArray[slotID].name; // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();
                textMutationPanelNew.text = gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][selectedToolbarMutationID].textDescriptionMutation; // "placeholder";
            }
        }
        else if(layerManager.selectedTrophicSlotRef.kingdomID == 3) { // Terrain
            if (layerManager.selectedTrophicSlotRef.tierID == 0) {
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
                if(layerManager.selectedTrophicSlotRef.slotID == 0) {
                    colorOptionA = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].color;
                    colorOptionB = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].color;
                    colorOptionC = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].color;
                    colorOptionD = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[3].color;
                    // *** make these Text objects into an array:
                    //textMutationPanelOptionA.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].textDescriptionMutation; // "Minor Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[0].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionB.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].textDescriptionMutation; // "Minor Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[1].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionC.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].textDescriptionMutation; // "Major Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[2].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionD.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[3].textDescriptionMutation; // "Major Mutation!";
                    colorCur = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent.color;
                    imageMutationPanelNewPortrait.color = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[selectedIndex].color;
                    textMutationPanelCur.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent.elevationChange.ToString();
                    textMutationPanelNew.text = gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[selectedIndex].textDescriptionMutation;
                }
                else if(layerManager.selectedTrophicSlotRef.slotID == 1) {
                    colorOptionA = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[0].color;
                    colorOptionB = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[1].color;
                    colorOptionC = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[2].color;
                    colorOptionD = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[3].color;
                    //textMutationPanelOptionA.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[0].textDescriptionMutation; // "Minor Stones Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[0].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionB.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[1].textDescriptionMutation; // "Stones Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[1].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionC.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[2].textDescriptionMutation; // "Major Stones Mutation!"; //\nShininess: " + (gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[2].color.a * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionD.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[3].textDescriptionMutation; // "Major Stones Mutation!";
                    colorCur = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent.color;
                    imageMutationPanelNewPortrait.color = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[selectedIndex].color;
                    textMutationPanelCur.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent.elevationChange.ToString();
                    textMutationPanelNew.text = gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[selectedIndex].textDescriptionMutation;
                }
                else if(layerManager.selectedTrophicSlotRef.slotID == 2) {
                    colorOptionA = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[0].color;
                    colorOptionB = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[1].color;
                    colorOptionC = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[2].color;
                    colorOptionD = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[3].color;
                    //textMutationPanelOptionA.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[0].textDescriptionMutation; // "Minor Pebbles Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[0].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionB.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[1].textDescriptionMutation; // "Pebbles Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[1].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionC.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[2].textDescriptionMutation; // "Major Pebbles Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[2].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionD.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[3].textDescriptionMutation; // "Major Pebbles Mutation!"; 
                    colorCur = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent.color;
                    imageMutationPanelNewPortrait.color = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[selectedIndex].color;
                    textMutationPanelCur.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent.elevationChange.ToString();
                    textMutationPanelNew.text = gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[selectedIndex].textDescriptionMutation;
                }
                else {
                    colorOptionA = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[0].color;
                    colorOptionB = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[1].color;
                    colorOptionC = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[2].color;
                    colorOptionC = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[3].color;
                    //textMutationPanelOptionA.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[0].textDescriptionMutation; // "Minor Sand Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[0].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionB.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[1].textDescriptionMutation; // "Sand Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[1].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionC.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[2].textDescriptionMutation; // "Major Sand Mutation!"; //\nElevation: " + (gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[2].elevationChange * 100f).ToString("F0") + "%";
                    //textMutationPanelOptionD.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[3].textDescriptionMutation; // "Major Sand Mutation!";
                    colorCur = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent.color;    
                    imageMutationPanelNewPortrait.color = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[selectedIndex].color;
                    textMutationPanelCur.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent.elevationChange.ToString();
                    textMutationPanelNew.text = gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[selectedIndex].textDescriptionMutation;
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
}
