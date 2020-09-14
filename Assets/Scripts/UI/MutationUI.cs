using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MutationUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public bool isUnlocked;
    public bool isOpen;
    public bool isPointerOver;

    public Animator animatorMutationUI;

    public Image imageMutationCurTarget; // in watcher panel
    public Text textMutationTargetLayer;

    public Material mutationThumbnailDecomposersMatCur;
    public Material mutationThumbnailDecomposersMatNew;
    public Material mutationThumbnailDecomposersMatA;
    public Material mutationThumbnailDecomposersMatB;
    public Material mutationThumbnailDecomposersMatC;
    public Material mutationThumbnailDecomposersMatD;

    public GameObject posCurGO;
    public GameObject posNewGO;
    public GameObject posAGO;
    public GameObject posBGO;
    public GameObject posCGO;
    public GameObject posDGO;
    public float renderSpaceMult = 0.05f;
    public float critterSizeMult = 1f;

    public Material mutationVertebrateRenderMat;
    public Image imageMutationVertebrateRender;

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
    public int selectedToolbarMutationID = 0;
    public Button buttonToolbarMutateConfirm;
    public Text textMutationParameters;
    public Text textCost;
    public Text textTopCenter;


	// Use this for initialization
	void Start () {
		
	}

    public void PointerEnter() {
        isPointerOver = true;
        Debug.Log("PointerEnter");
    }
    public void PointerExits() {
        isPointerOver = false;
        Debug.Log("PointerExits");
    }
	
    private void UpdateUI(TrophicLayersManager layerManager) {
        //textToolbarWingPanelName.text = "Mutations:";

        animatorMutationUI.SetBool("_IsOpen", isOpen);

        textMutationPanelOptionA.text = "";// "Tiny";
        textMutationPanelOptionB.text = "";// "Small";
        textMutationPanelOptionC.text = "";// "Large";
        textMutationPanelOptionD.text = "";// "Huge";

        textMutationPanelTitleCur.text = "CURRENT";
        string[] titlesTxt = new string[4];
        titlesTxt[0] = "TINY MUTATION";
        titlesTxt[1] = "SMALL MUTATION";
        titlesTxt[2] = "LARGE MUTATION";
        titlesTxt[3] = "HUGE MUTATION";
        textMutationPanelTitleNew.text = "MUTATION:";// titlesTxt[selectedToolbarMutationID];

        imageMutationPanelThumbnailA.gameObject.SetActive(true);
        imageMutationPanelThumbnailB.gameObject.SetActive(true);
        imageMutationPanelThumbnailC.gameObject.SetActive(true);
        imageMutationPanelThumbnailD.gameObject.SetActive(true);

        buttonToolbarMutateConfirm.gameObject.SetActive(false);
        if(selectedToolbarMutationID >= 0) {
            buttonToolbarMutateConfirm.gameObject.SetActive(true);
            /*
            if(selectedToolbarMutationID == 0) {
                imageMutationPanelHighlightA.color = Color.white;
                imageMutationPanelHighlightB.color = Color.black;
                imageMutationPanelHighlightC.color = Color.black;
                imageMutationPanelHighlightD.color = Color.black;
            }
            else if(selectedToolbarMutationID == 1) {
                imageMutationPanelHighlightA.color = Color.black;
                imageMutationPanelHighlightB.color = Color.white;
                imageMutationPanelHighlightC.color = Color.black;
                imageMutationPanelHighlightD.color = Color.black;
            }
            else if(selectedToolbarMutationID == 2) {
                imageMutationPanelHighlightA.color = Color.black;
                imageMutationPanelHighlightB.color = Color.black;
                imageMutationPanelHighlightC.color = Color.white;
                imageMutationPanelHighlightD.color = Color.black;
            }
            else if(selectedToolbarMutationID == 3) {
                imageMutationPanelHighlightA.color = Color.black;
                imageMutationPanelHighlightB.color = Color.black;
                imageMutationPanelHighlightC.color = Color.black;
                imageMutationPanelHighlightD.color = Color.white;
            }
            */
        }

        TrophicSlot slotRef = uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot;

        textMutationTargetLayer.text = slotRef.speciesName;
        textMutationTargetLayer.color = uiManagerRef.worldSpiritHubUI.curIconColor;
        imageMutationCurTarget.color = uiManagerRef.worldSpiritHubUI.curIconColor;
        imageMutationCurTarget.sprite = uiManagerRef.worldSpiritHubUI.curIconSprite;

        imageMutationVertebrateRender.gameObject.SetActive(false);
        //imageMutationCurTarget.gameObject.SetActive(true);
        imageMutationPanelCurPortrait.gameObject.SetActive(true);
        imageMutationPanelNewPortrait.gameObject.SetActive(true);

        if(slotRef.kingdomID == 0) { // DECOMPOSERS
            UpdateDecomposerUI(layerManager);
            
        }
        else if(slotRef.kingdomID == 1) { // PLANTS
            if(slotRef.tierID == 0) {
                UpdateAlgaeUI(layerManager);
                
            }
            else {  // PLANTS:
                UpdatePlantsUI();
                
            }            
        }
        else if(slotRef.kingdomID == 2) { // ANIMALS
            if(slotRef.tierID == 0) {  // Zooplankton
                UpdateZooplanktonUI();                
            }
            else { // vertebrates
                UpdateVertebratesUI(slotRef);
                imageMutationPanelThumbnailA.gameObject.SetActive(false);
                imageMutationPanelThumbnailB.gameObject.SetActive(false);
                imageMutationPanelThumbnailC.gameObject.SetActive(false);
                imageMutationPanelThumbnailD.gameObject.SetActive(false);
            }
        }
        else if(slotRef.kingdomID == 3) { // Terrain
            UpdateTerrainUI(slotRef);
        }
    }
    private void UpdateDecomposerUI(TrophicLayersManager layerManager) {
        
        Color uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[0].displayColorPri;
        uiColor.a = 1f;
        imageMutationPanelThumbnailA.color = uiColor;
        uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[1].displayColorPri;
        uiColor.a = 1f;
        imageMutationPanelThumbnailB.color = uiColor;
        uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[2].displayColorPri;
        uiColor.a = 1f;
        imageMutationPanelThumbnailC.color = uiColor;
        uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[3].displayColorPri;
        uiColor.a = 1f;
        imageMutationPanelThumbnailD.color = uiColor;

        uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].displayColorPri;
        uiColor.a = 1f;
        imageMutationPanelNewPortrait.color = uiColor;
        textMutationParameters.text = "Metabolic Rate:\nEfficiency:";
        float deltaMetabolism = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].metabolicRate
                        - uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.metabolicRate;
        string strang = GetStringDelta(deltaMetabolism); // strColorStart + (deltaMetabolism * 100f).ToString("F2") + strColorEnd;

        float deltaEfficiency = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].growthEfficiency - 
                        uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.growthEfficiency;
        string strDeltaEfficiency = GetStringDelta(deltaEfficiency);

        
        textMutationPanelCur.text = (uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.metabolicRate * 100f).ToString("F2") +
                                    "\n" + uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.growthEfficiency.ToString("F2");//"44.38";
        textMutationPanelNew.text = (uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].metabolicRate * 100f).ToString("F2") +
                                    strang + "\n" + uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].growthEfficiency.ToString("F2") + strDeltaEfficiency;// "83.46";
        textCost.text = "";
        textTopCenter.text = "";
        uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.displayColorPri;
        uiColor.a = 1f;

        //Cur Mat
        mutationThumbnailDecomposersMatCur.SetVector("_TintPri", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.displayColorPri);
        mutationThumbnailDecomposersMatCur.SetVector("_TintSec", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.displayColorSec);
        mutationThumbnailDecomposersMatCur.SetInt("_PatternRow", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.patternRowID);
        mutationThumbnailDecomposersMatCur.SetInt("_PatternColumn", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.patternColumnID);
        mutationThumbnailDecomposersMatCur.SetFloat("_PatternThreshold", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.patternThreshold);
        // New:
        mutationThumbnailDecomposersMatNew.SetVector("_TintPri", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].displayColorPri);
        mutationThumbnailDecomposersMatNew.SetVector("_TintSec", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].displayColorSec);
        mutationThumbnailDecomposersMatNew.SetInt("_PatternRow", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].patternRowID);
        mutationThumbnailDecomposersMatNew.SetInt("_PatternColumn", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].patternColumnID);
        mutationThumbnailDecomposersMatNew.SetFloat("_PatternThreshold", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].patternThreshold);
        // A;
        mutationThumbnailDecomposersMatA.SetVector("_TintPri", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[0].displayColorPri);
        mutationThumbnailDecomposersMatA.SetVector("_TintSec", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[0].displayColorSec);
        mutationThumbnailDecomposersMatA.SetInt("_PatternRow", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[0].patternRowID);
        mutationThumbnailDecomposersMatA.SetInt("_PatternColumn", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[0].patternColumnID);
        mutationThumbnailDecomposersMatA.SetFloat("_PatternThreshold", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[0].patternThreshold);
        //B:
        mutationThumbnailDecomposersMatB.SetVector("_TintPri", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[1].displayColorPri);
        mutationThumbnailDecomposersMatB.SetVector("_TintSec", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[1].displayColorSec);
        mutationThumbnailDecomposersMatB.SetInt("_PatternRow", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[1].patternRowID);
        mutationThumbnailDecomposersMatB.SetInt("_PatternColumn", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[1].patternColumnID);
        mutationThumbnailDecomposersMatB.SetFloat("_PatternThreshold", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[1].patternThreshold);
        //C;
        mutationThumbnailDecomposersMatC.SetVector("_TintPri", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[2].displayColorPri);
        mutationThumbnailDecomposersMatC.SetVector("_TintSec", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[2].displayColorSec);
        mutationThumbnailDecomposersMatC.SetInt("_PatternRow", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[2].patternRowID);
        mutationThumbnailDecomposersMatC.SetInt("_PatternColumn", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[2].patternColumnID);
        mutationThumbnailDecomposersMatC.SetFloat("_PatternThreshold", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[2].patternThreshold);
        //D:
        mutationThumbnailDecomposersMatD.SetVector("_TintPri", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[3].displayColorPri);
        mutationThumbnailDecomposersMatD.SetVector("_TintSec", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[3].displayColorSec);
        mutationThumbnailDecomposersMatD.SetInt("_PatternRow", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[3].patternRowID);
        mutationThumbnailDecomposersMatD.SetInt("_PatternColumn", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[3].patternColumnID);
        mutationThumbnailDecomposersMatD.SetFloat("_PatternThreshold", uiManagerRef.gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[3].patternThreshold);

    }
    private void UpdateAlgaeUI(TrophicLayersManager layerManager) {
        
        textMutationParameters.text = "Metabolic Rate:\nEfficiency:";
        float deltaMetabolism = uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].metabolicRate -
                        uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.metabolicRate;
        string strDeltaMetabolism = GetStringDelta(deltaMetabolism);
        float deltaEfficiency = uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].growthEfficiency - 
                        uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.growthEfficiency;
        string strDeltaEfficiency = GetStringDelta(deltaEfficiency);
        
        textMutationPanelCur.text = (uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.metabolicRate * 100f).ToString("F2") +
                                    "\n" + uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.growthEfficiency.ToString("F2");//"44.38";
        textMutationPanelNew.text = (uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].metabolicRate * 100f).ToString("F2") +
                                    strDeltaMetabolism + "\n" + 
                                    uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].growthEfficiency.ToString("F2") +
                                    strDeltaEfficiency;// "83.46";
        textCost.text = "";
        textTopCenter.text = "";
        
        //Cur Mat
        mutationThumbnailDecomposersMatCur.SetVector("_TintPri", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.displayColorPri);
        mutationThumbnailDecomposersMatCur.SetVector("_TintSec", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.displayColorSec);
        mutationThumbnailDecomposersMatCur.SetInt("_PatternRow", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.patternRowID);
        mutationThumbnailDecomposersMatCur.SetInt("_PatternColumn", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.patternColumnID);
        mutationThumbnailDecomposersMatCur.SetFloat("_PatternThreshold", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.patternThreshold);
        // New:
        mutationThumbnailDecomposersMatNew.SetVector("_TintPri", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].displayColorPri);
        mutationThumbnailDecomposersMatNew.SetVector("_TintSec", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].displayColorSec);
        mutationThumbnailDecomposersMatNew.SetInt("_PatternRow", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].patternRowID);
        mutationThumbnailDecomposersMatNew.SetInt("_PatternColumn", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].patternColumnID);
        mutationThumbnailDecomposersMatNew.SetFloat("_PatternThreshold", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].patternThreshold);
        // A;
        mutationThumbnailDecomposersMatA.SetVector("_TintPri", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[0].displayColorPri);
        mutationThumbnailDecomposersMatA.SetVector("_TintSec", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[0].displayColorSec);
        mutationThumbnailDecomposersMatA.SetInt("_PatternRow", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[0].patternRowID);
        mutationThumbnailDecomposersMatA.SetInt("_PatternColumn", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[0].patternColumnID);
        mutationThumbnailDecomposersMatA.SetFloat("_PatternThreshold", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[0].patternThreshold);
        //B:
        mutationThumbnailDecomposersMatB.SetVector("_TintPri", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[1].displayColorPri);
        mutationThumbnailDecomposersMatB.SetVector("_TintSec", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[1].displayColorSec);
        mutationThumbnailDecomposersMatB.SetInt("_PatternRow", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[1].patternRowID);
        mutationThumbnailDecomposersMatB.SetInt("_PatternColumn", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[1].patternColumnID);
        mutationThumbnailDecomposersMatB.SetFloat("_PatternThreshold", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[1].patternThreshold);
        //C;
        mutationThumbnailDecomposersMatC.SetVector("_TintPri", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[2].displayColorPri);
        mutationThumbnailDecomposersMatC.SetVector("_TintSec", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[2].displayColorSec);
        mutationThumbnailDecomposersMatC.SetInt("_PatternRow", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[2].patternRowID);
        mutationThumbnailDecomposersMatC.SetInt("_PatternColumn", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[2].patternColumnID);
        mutationThumbnailDecomposersMatC.SetFloat("_PatternThreshold", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[2].patternThreshold);
        //D:
        mutationThumbnailDecomposersMatD.SetVector("_TintPri", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[3].displayColorPri);
        mutationThumbnailDecomposersMatD.SetVector("_TintSec", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[3].displayColorSec);
        mutationThumbnailDecomposersMatD.SetInt("_PatternRow", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[3].patternRowID);
        mutationThumbnailDecomposersMatD.SetInt("_PatternColumn", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[3].patternColumnID);
        mutationThumbnailDecomposersMatD.SetFloat("_PatternThreshold", uiManagerRef.gameManager.simulationManager.vegetationManager.algaeSlotGenomeMutations[3].patternThreshold);

    }
    private string GetStringDelta(float delta) {
        
        string strang = "";
        string strColorStart = "<color=#999999FF> (";
        string strColorEnd = ")</color>";
        if (delta > 0.001f) {
            strColorStart = "<color=#55FF55FF> (+";
        }
        else if(delta < -0.001f) {
            strColorStart = "<color=#FF5555FF> (";
        }

        strang = strColorStart + (delta * 100f).ToString("F2") + strColorEnd;

        return strang;
    }
    private void UpdateTerrainUI(TrophicSlot slotRef) {
        if (slotRef.tierID == 0) { // needed?
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
                
                colorCur = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent.color;
                colorNew = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[selectedIndex].color;
                textMutationPanelCur.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent.elevationChange.ToString();
                textMutationPanelNew.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeMutations[selectedIndex].textDescriptionMutation;
            }
            else if(slotRef.slotID == 1) {
                colorOptionA = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[0].color;
                colorOptionB = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[1].color;
                colorOptionC = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[2].color;
                colorOptionD = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[3].color;
                colorCur = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent.color;
                colorNew = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[selectedIndex].color;
                textMutationPanelCur.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent.elevationChange.ToString();
                textMutationPanelNew.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeMutations[selectedIndex].textDescriptionMutation;
            }
            else if(slotRef.slotID == 2) {
                colorOptionA = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[0].color;
                colorOptionB = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[1].color;
                colorOptionC = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[2].color;
                colorOptionD = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[3].color;
                colorCur = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent.color;
                colorNew = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[selectedIndex].color;
                textMutationPanelCur.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent.elevationChange.ToString();
                textMutationPanelNew.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeMutations[selectedIndex].textDescriptionMutation;
            }
            else {
                colorOptionA = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[0].color;
                colorOptionB = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[1].color;
                colorOptionC = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[2].color;
                colorOptionD = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[3].color;
                colorCur = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent.color;    
                colorNew = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[selectedIndex].color;
                textMutationPanelCur.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent.elevationChange.ToString();
                textMutationPanelNew.text = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeMutations[selectedIndex].textDescriptionMutation;
            }
            // **** v v v Make these into arrays during cleanup
            colorOptionA.a = 1f;
            colorOptionB.a = 1f;
            colorOptionC.a = 1f;
            colorOptionD.a = 1f;
            colorCur.a = 1f;
            imageMutationPanelThumbnailA.color = Color.white;//colorOptionA;
            imageMutationPanelThumbnailB.color = Color.white;//colorOptionB;
            imageMutationPanelThumbnailC.color = Color.white;//colorOptionC;
            imageMutationPanelThumbnailD.color = Color.white;//colorOptionD;

            imageMutationPanelCurPortrait.color = Color.white;//colorCur;

            //Cur Mat
            mutationThumbnailDecomposersMatCur.SetVector("_TintPri", colorCur);
            mutationThumbnailDecomposersMatCur.SetVector("_TintSec", colorCur);
            // New:
            mutationThumbnailDecomposersMatNew.SetVector("_TintPri", colorNew);
            mutationThumbnailDecomposersMatNew.SetVector("_TintSec", colorNew);
        
            mutationThumbnailDecomposersMatA.SetVector("_TintPri", colorOptionA);
            mutationThumbnailDecomposersMatA.SetVector("_TintSec", colorOptionA);
            //B:
            mutationThumbnailDecomposersMatB.SetVector("_TintPri", colorOptionB);
            mutationThumbnailDecomposersMatB.SetVector("_TintSec", colorOptionB);
            //C;
            mutationThumbnailDecomposersMatC.SetVector("_TintPri", colorOptionC);
            mutationThumbnailDecomposersMatC.SetVector("_TintSec", colorOptionC);
            //D:
            mutationThumbnailDecomposersMatD.SetVector("_TintPri", colorOptionD);
            mutationThumbnailDecomposersMatD.SetVector("_TintSec", colorOptionD);
        }
    }
    private void UpdateZooplanktonUI() {

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
        
        uiColor = uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent.representativeData.color;
        uiColor.a = 1f;
        imageMutationPanelCurPortrait.color = uiColor; 

        //swimSpeed01
        textMutationParameters.text = "Swim Speed:\nAging Rate:\nAttract:";

        textMutationPanelCur.text = uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent.swimSpeed01.ToString("F2") +
                                    "\n" + uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent.agingRate01.ToString("F2") +
                                    "\n" + uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent.attractForce01.ToString("F2"); // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();


        float deltaSwimSpeed = uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].swimSpeed01 - 
                        uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent.swimSpeed01;
        string strDeltaSwimSpeed = GetStringDelta(deltaSwimSpeed);

        float deltaAgingRate = uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].agingRate01 - 
                        uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent.agingRate01;
        string strDeltaAgingRate = GetStringDelta(deltaAgingRate);

        float deltaAttract = uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].attractForce01 - 
                        uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeCurrent.attractForce01;
        string strDeltaAttract = GetStringDelta(deltaAttract);


        textMutationPanelNew.text = uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].swimSpeed01.ToString("F2") +
                                    strDeltaSwimSpeed + "\n" +
                                    uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].agingRate01.ToString("F2") +
                                    strDeltaAgingRate + "\n" +
                                    uiManagerRef.gameManager.simulationManager.zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].attractForce01.ToString("F2") +
                                    strDeltaAttract;



    }
    private void UpdatePlantsUI() {
        Color uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[0].displayColorPri; // new Color(hue.x, hue.y, hue.z);
        //uiColor.a = 1f;
        imageMutationPanelThumbnailA.color = uiColor; // UnityEngine.Random.ColorHSV();
        uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[1].displayColorPri;
        imageMutationPanelThumbnailB.color = uiColor;
        uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[2].displayColorPri;
        imageMutationPanelThumbnailC.color = uiColor;
        uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[3].displayColorPri;
        imageMutationPanelThumbnailD.color = uiColor;
          
        uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.displayColorPri;
        imageMutationPanelCurPortrait.color = uiColor; 
        uiColor = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].displayColorPri;        
        imageMutationPanelNewPortrait.color = uiColor;
        

        textMutationParameters.text = "Growth Rate:\nNONE:";

        textMutationPanelCur.text = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.growthRate.ToString("F2") +
                                    "\n" + "ADD HERE";

        float deltaGrowthRate = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].growthRate - 
                        uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent.growthRate;
        string strDeltaGrowthRate = GetStringDelta(deltaGrowthRate);

        textMutationPanelNew.text = uiManagerRef.gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].growthRate.ToString("F2") +
                                    strDeltaGrowthRate;

   
        
    }
    private void UpdateVertebratesUI(TrophicSlot slotRef) {
        imageMutationVertebrateRender.gameObject.SetActive(true);
        //imageMutationCurTarget.gameObject.SetActive(false);
        imageMutationPanelCurPortrait.gameObject.SetActive(false);
        imageMutationPanelNewPortrait.gameObject.SetActive(false);

        int slotID = slotRef.slotID;
        // *** HACK!!!! ***
        slotID = 0;

        int speciesID = slotRef.linkedSpeciesID;
        
        //mutationVertebrateRenderMat.SetTexture("_MainTex", rt);


        /*Vector3 hue0 = uiManagerRef.gameManager.simulationManager.masterGenomePool.vertebrateSlotsGenomesMutationsArray[slotID][0].representativeGenome.bodyGenome.appearanceGenome.huePrimary;
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
    */
    }
    


    public void UpdateMutationPanelUI(TrophicLayersManager layerManager) {
        panelMutationSpirit.SetActive(isOpen);
        if(isOpen) {
            animatorMutationUI.SetBool("_IsSelectedA", false);
            animatorMutationUI.SetBool("_IsSelectedB", false);   
            animatorMutationUI.SetBool("_IsSelectedC", false);
            animatorMutationUI.SetBool("_IsSelectedD", false);

            if(selectedToolbarMutationID == 0) {
                animatorMutationUI.SetBool("_IsSelectedA", true);
            }
            else if(selectedToolbarMutationID == 1) {
                animatorMutationUI.SetBool("_IsSelectedB", true);
            }
            else if(selectedToolbarMutationID == 2) {
                animatorMutationUI.SetBool("_IsSelectedC", true);
            }
            else {
                animatorMutationUI.SetBool("_IsSelectedD", true);
            }

            UpdateUI(layerManager);
        }
    }

    public void OpenMutationPanel() {
        isOpen = true;

        
    }
    
    public void ClickToolButton() {
        Debug.Log("Click mutation toggle button)");
        isOpen = !isOpen;
        if(selectedToolbarMutationID == 0) {
            animatorMutationUI.SetBool("_IsSelectedA", true);
        }
    }
    public void ClickMutationConfirm() {
        animatorMutationUI.SetTrigger("_TriggerMutate");
        

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
                /*
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
                uiManagerRef.gameManager.simulationManager.masterGenomePool.ProcessSlotMutation(slotRef.slotID, selectedToolbarMutationID, slotRef.linkedSpeciesID);

                uiManagerRef.InitToolbarPortraitCritterData(slotRef);
                */
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
        Debug.Log("ClickMutationOption(" + id.ToString() + ")");
        //uiManagerRef.InitToolbarPortraitCritterData(uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot);
        selectedToolbarMutationID = id;

        animatorMutationUI.SetBool("_IsSelectedA", false);
        animatorMutationUI.SetBool("_IsSelectedB", false);   
        animatorMutationUI.SetBool("_IsSelectedC", false);
        animatorMutationUI.SetBool("_IsSelectedD", false);

        if(selectedToolbarMutationID == 0) {
            animatorMutationUI.SetBool("_IsSelectedA", true);
        }
        else if(selectedToolbarMutationID == 1) {
            animatorMutationUI.SetBool("_IsSelectedB", true);
        }
        else if(selectedToolbarMutationID == 2) {
            animatorMutationUI.SetBool("_IsSelectedC", true);
        }
        else {
            animatorMutationUI.SetBool("_IsSelectedD", true);
        }
    }

}
