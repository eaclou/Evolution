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

    GameManager gameManager => GameManager.instance;
    SimulationManager simulationManager => gameManager.simulationManager;
    VegetationManager vegetationManager => simulationManager.vegetationManager;
    ZooplanktonManager zooplanktonManager => simulationManager.zooplanktonManager;
    TheRenderKing theRenderKing => gameManager.theRenderKing;
    BaronVonTerrain baronVonTerrain => theRenderKing.baronVonTerrain;

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
        
        Color uiColor = vegetationManager.decomposerSlotGenomeMutations[0].displayColorPri;
        uiColor.a = 1f;
        imageMutationPanelThumbnailA.color = uiColor;
        uiColor = vegetationManager.decomposerSlotGenomeMutations[1].displayColorPri;
        uiColor.a = 1f;
        imageMutationPanelThumbnailB.color = uiColor;
        uiColor = vegetationManager.decomposerSlotGenomeMutations[2].displayColorPri;
        uiColor.a = 1f;
        imageMutationPanelThumbnailC.color = uiColor;
        uiColor = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[3].displayColorPri;
        uiColor.a = 1f;
        imageMutationPanelThumbnailD.color = uiColor;

        uiColor = gameManager.simulationManager.vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].displayColorPri;
        uiColor.a = 1f;
        imageMutationPanelNewPortrait.color = uiColor;
        textMutationParameters.text = "Metabolic Rate:\nEfficiency:";
        float deltaMetabolism = vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].metabolicRate
                        - vegetationManager.decomposerSlotGenomeCurrent.metabolicRate;
        string strang = GetStringDelta(deltaMetabolism); // strColorStart + (deltaMetabolism * 100f).ToString("F2") + strColorEnd;

        float deltaEfficiency = vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].growthEfficiency - 
                        vegetationManager.decomposerSlotGenomeCurrent.growthEfficiency;
        string strDeltaEfficiency = GetStringDelta(deltaEfficiency);

        
        textMutationPanelCur.text = (vegetationManager.decomposerSlotGenomeCurrent.metabolicRate * 100f).ToString("F2") +
                                    "\n" + vegetationManager.decomposerSlotGenomeCurrent.growthEfficiency.ToString("F2");//"44.38";
        textMutationPanelNew.text = (vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].metabolicRate * 100f).ToString("F2") +
                                    strang + "\n" + vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].growthEfficiency.ToString("F2") + strDeltaEfficiency;// "83.46";
        textCost.text = "";
        textTopCenter.text = "";
        uiColor = vegetationManager.decomposerSlotGenomeCurrent.displayColorPri;
        uiColor.a = 1f;

        //Cur Mat
        mutationThumbnailDecomposersMatCur.SetVector("_TintPri", vegetationManager.decomposerSlotGenomeCurrent.displayColorPri);
        mutationThumbnailDecomposersMatCur.SetVector("_TintSec", vegetationManager.decomposerSlotGenomeCurrent.displayColorSec);
        mutationThumbnailDecomposersMatCur.SetInt("_PatternRow", vegetationManager.decomposerSlotGenomeCurrent.patternRowID);
        mutationThumbnailDecomposersMatCur.SetInt("_PatternColumn", vegetationManager.decomposerSlotGenomeCurrent.patternColumnID);
        mutationThumbnailDecomposersMatCur.SetFloat("_PatternThreshold", vegetationManager.decomposerSlotGenomeCurrent.patternThreshold);
        // New:
        mutationThumbnailDecomposersMatNew.SetVector("_TintPri", vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].displayColorPri);
        mutationThumbnailDecomposersMatNew.SetVector("_TintSec", vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].displayColorSec);
        mutationThumbnailDecomposersMatNew.SetInt("_PatternRow", vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].patternRowID);
        mutationThumbnailDecomposersMatNew.SetInt("_PatternColumn", vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].patternColumnID);
        mutationThumbnailDecomposersMatNew.SetFloat("_PatternThreshold", vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID].patternThreshold);
        // A;
        mutationThumbnailDecomposersMatA.SetVector("_TintPri", vegetationManager.decomposerSlotGenomeMutations[0].displayColorPri);
        mutationThumbnailDecomposersMatA.SetVector("_TintSec", vegetationManager.decomposerSlotGenomeMutations[0].displayColorSec);
        mutationThumbnailDecomposersMatA.SetInt("_PatternRow", vegetationManager.decomposerSlotGenomeMutations[0].patternRowID);
        mutationThumbnailDecomposersMatA.SetInt("_PatternColumn", vegetationManager.decomposerSlotGenomeMutations[0].patternColumnID);
        mutationThumbnailDecomposersMatA.SetFloat("_PatternThreshold", vegetationManager.decomposerSlotGenomeMutations[0].patternThreshold);
        //B:
        mutationThumbnailDecomposersMatB.SetVector("_TintPri", vegetationManager.decomposerSlotGenomeMutations[1].displayColorPri);
        mutationThumbnailDecomposersMatB.SetVector("_TintSec", vegetationManager.decomposerSlotGenomeMutations[1].displayColorSec);
        mutationThumbnailDecomposersMatB.SetInt("_PatternRow", vegetationManager.decomposerSlotGenomeMutations[1].patternRowID);
        mutationThumbnailDecomposersMatB.SetInt("_PatternColumn", vegetationManager.decomposerSlotGenomeMutations[1].patternColumnID);
        mutationThumbnailDecomposersMatB.SetFloat("_PatternThreshold", vegetationManager.decomposerSlotGenomeMutations[1].patternThreshold);
        //C;
        mutationThumbnailDecomposersMatC.SetVector("_TintPri", vegetationManager.decomposerSlotGenomeMutations[2].displayColorPri);
        mutationThumbnailDecomposersMatC.SetVector("_TintSec", vegetationManager.decomposerSlotGenomeMutations[2].displayColorSec);
        mutationThumbnailDecomposersMatC.SetInt("_PatternRow", vegetationManager.decomposerSlotGenomeMutations[2].patternRowID);
        mutationThumbnailDecomposersMatC.SetInt("_PatternColumn", vegetationManager.decomposerSlotGenomeMutations[2].patternColumnID);
        mutationThumbnailDecomposersMatC.SetFloat("_PatternThreshold", vegetationManager.decomposerSlotGenomeMutations[2].patternThreshold);
        //D:
        mutationThumbnailDecomposersMatD.SetVector("_TintPri", vegetationManager.decomposerSlotGenomeMutations[3].displayColorPri);
        mutationThumbnailDecomposersMatD.SetVector("_TintSec", vegetationManager.decomposerSlotGenomeMutations[3].displayColorSec);
        mutationThumbnailDecomposersMatD.SetInt("_PatternRow", vegetationManager.decomposerSlotGenomeMutations[3].patternRowID);
        mutationThumbnailDecomposersMatD.SetInt("_PatternColumn", vegetationManager.decomposerSlotGenomeMutations[3].patternColumnID);
        mutationThumbnailDecomposersMatD.SetFloat("_PatternThreshold", vegetationManager.decomposerSlotGenomeMutations[3].patternThreshold);

    }
    private void UpdateAlgaeUI(TrophicLayersManager layerManager) {
        
        textMutationParameters.text = "Metabolic Rate:\nEfficiency:";
        float deltaMetabolism = vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].metabolicRate -
                        vegetationManager.algaeSlotGenomeCurrent.metabolicRate;
        string strDeltaMetabolism = GetStringDelta(deltaMetabolism);
        float deltaEfficiency = vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].growthEfficiency - 
                        vegetationManager.algaeSlotGenomeCurrent.growthEfficiency;
        string strDeltaEfficiency = GetStringDelta(deltaEfficiency);
        
        textMutationPanelCur.text = (vegetationManager.algaeSlotGenomeCurrent.metabolicRate * 100f).ToString("F2") +
                                    "\n" + vegetationManager.algaeSlotGenomeCurrent.growthEfficiency.ToString("F2");//"44.38";
        textMutationPanelNew.text = (vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].metabolicRate * 100f).ToString("F2") +
                                    strDeltaMetabolism + "\n" + 
                                    vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].growthEfficiency.ToString("F2") +
                                    strDeltaEfficiency;// "83.46";
        textCost.text = "";
        textTopCenter.text = "";
        
        //Cur Mat
        mutationThumbnailDecomposersMatCur.SetVector("_TintPri", vegetationManager.algaeSlotGenomeCurrent.displayColorPri);
        mutationThumbnailDecomposersMatCur.SetVector("_TintSec", vegetationManager.algaeSlotGenomeCurrent.displayColorSec);
        mutationThumbnailDecomposersMatCur.SetInt("_PatternRow", vegetationManager.algaeSlotGenomeCurrent.patternRowID);
        mutationThumbnailDecomposersMatCur.SetInt("_PatternColumn", vegetationManager.algaeSlotGenomeCurrent.patternColumnID);
        mutationThumbnailDecomposersMatCur.SetFloat("_PatternThreshold", vegetationManager.algaeSlotGenomeCurrent.patternThreshold);
        // New:
        mutationThumbnailDecomposersMatNew.SetVector("_TintPri", vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].displayColorPri);
        mutationThumbnailDecomposersMatNew.SetVector("_TintSec", vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].displayColorSec);
        mutationThumbnailDecomposersMatNew.SetInt("_PatternRow", vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].patternRowID);
        mutationThumbnailDecomposersMatNew.SetInt("_PatternColumn", vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].patternColumnID);
        mutationThumbnailDecomposersMatNew.SetFloat("_PatternThreshold", vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID].patternThreshold);
        // A;
        mutationThumbnailDecomposersMatA.SetVector("_TintPri", vegetationManager.algaeSlotGenomeMutations[0].displayColorPri);
        mutationThumbnailDecomposersMatA.SetVector("_TintSec", vegetationManager.algaeSlotGenomeMutations[0].displayColorSec);
        mutationThumbnailDecomposersMatA.SetInt("_PatternRow", vegetationManager.algaeSlotGenomeMutations[0].patternRowID);
        mutationThumbnailDecomposersMatA.SetInt("_PatternColumn", vegetationManager.algaeSlotGenomeMutations[0].patternColumnID);
        mutationThumbnailDecomposersMatA.SetFloat("_PatternThreshold", vegetationManager.algaeSlotGenomeMutations[0].patternThreshold);
        //B:
        mutationThumbnailDecomposersMatB.SetVector("_TintPri", vegetationManager.algaeSlotGenomeMutations[1].displayColorPri);
        mutationThumbnailDecomposersMatB.SetVector("_TintSec", vegetationManager.algaeSlotGenomeMutations[1].displayColorSec);
        mutationThumbnailDecomposersMatB.SetInt("_PatternRow", vegetationManager.algaeSlotGenomeMutations[1].patternRowID);
        mutationThumbnailDecomposersMatB.SetInt("_PatternColumn", vegetationManager.algaeSlotGenomeMutations[1].patternColumnID);
        mutationThumbnailDecomposersMatB.SetFloat("_PatternThreshold", vegetationManager.algaeSlotGenomeMutations[1].patternThreshold);
        //C;
        mutationThumbnailDecomposersMatC.SetVector("_TintPri", vegetationManager.algaeSlotGenomeMutations[2].displayColorPri);
        mutationThumbnailDecomposersMatC.SetVector("_TintSec", vegetationManager.algaeSlotGenomeMutations[2].displayColorSec);
        mutationThumbnailDecomposersMatC.SetInt("_PatternRow", vegetationManager.algaeSlotGenomeMutations[2].patternRowID);
        mutationThumbnailDecomposersMatC.SetInt("_PatternColumn", vegetationManager.algaeSlotGenomeMutations[2].patternColumnID);
        mutationThumbnailDecomposersMatC.SetFloat("_PatternThreshold", vegetationManager.algaeSlotGenomeMutations[2].patternThreshold);
        //D:
        mutationThumbnailDecomposersMatD.SetVector("_TintPri", vegetationManager.algaeSlotGenomeMutations[3].displayColorPri);
        mutationThumbnailDecomposersMatD.SetVector("_TintSec", vegetationManager.algaeSlotGenomeMutations[3].displayColorSec);
        mutationThumbnailDecomposersMatD.SetInt("_PatternRow", vegetationManager.algaeSlotGenomeMutations[3].patternRowID);
        mutationThumbnailDecomposersMatD.SetInt("_PatternColumn", vegetationManager.algaeSlotGenomeMutations[3].patternColumnID);
        mutationThumbnailDecomposersMatD.SetFloat("_PatternThreshold", vegetationManager.algaeSlotGenomeMutations[3].patternThreshold);

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
                colorOptionA = baronVonTerrain.bedrockSlotGenomeMutations[0].color;
                colorOptionB = baronVonTerrain.bedrockSlotGenomeMutations[1].color;
                colorOptionC = baronVonTerrain.bedrockSlotGenomeMutations[2].color;
                colorOptionD = baronVonTerrain.bedrockSlotGenomeMutations[3].color;
                // *** make these Text objects into an array:
                
                colorCur = baronVonTerrain.bedrockSlotGenomeCurrent.color;
                colorNew = baronVonTerrain.bedrockSlotGenomeMutations[selectedIndex].color;
                textMutationPanelCur.text = baronVonTerrain.bedrockSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.bedrockSlotGenomeCurrent.elevationChange.ToString();
                textMutationPanelNew.text = baronVonTerrain.bedrockSlotGenomeMutations[selectedIndex].textDescriptionMutation;
            }
            else if(slotRef.slotID == 1) {
                colorOptionA = baronVonTerrain.stoneSlotGenomeMutations[0].color;
                colorOptionB = baronVonTerrain.stoneSlotGenomeMutations[1].color;
                colorOptionC = baronVonTerrain.stoneSlotGenomeMutations[2].color;
                colorOptionD = baronVonTerrain.stoneSlotGenomeMutations[3].color;
                colorCur = baronVonTerrain.stoneSlotGenomeCurrent.color;
                colorNew = baronVonTerrain.stoneSlotGenomeMutations[selectedIndex].color;
                textMutationPanelCur.text = baronVonTerrain.stoneSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.stoneSlotGenomeCurrent.elevationChange.ToString();
                textMutationPanelNew.text = baronVonTerrain.stoneSlotGenomeMutations[selectedIndex].textDescriptionMutation;
            }
            else if(slotRef.slotID == 2) {
                colorOptionA = baronVonTerrain.pebblesSlotGenomeMutations[0].color;
                colorOptionB = baronVonTerrain.pebblesSlotGenomeMutations[1].color;
                colorOptionC = baronVonTerrain.pebblesSlotGenomeMutations[2].color;
                colorOptionD = baronVonTerrain.pebblesSlotGenomeMutations[3].color;
                colorCur = baronVonTerrain.pebblesSlotGenomeCurrent.color;
                colorNew = baronVonTerrain.pebblesSlotGenomeMutations[selectedIndex].color;
                textMutationPanelCur.text = baronVonTerrain.pebblesSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.pebblesSlotGenomeCurrent.elevationChange.ToString();
                textMutationPanelNew.text = baronVonTerrain.pebblesSlotGenomeMutations[selectedIndex].textDescriptionMutation;
            }
            else {
                colorOptionA = baronVonTerrain.sandSlotGenomeMutations[0].color;
                colorOptionB = baronVonTerrain.sandSlotGenomeMutations[1].color;
                colorOptionC = baronVonTerrain.sandSlotGenomeMutations[2].color;
                colorOptionD = baronVonTerrain.sandSlotGenomeMutations[3].color;
                colorCur = baronVonTerrain.sandSlotGenomeCurrent.color;    
                colorNew = baronVonTerrain.sandSlotGenomeMutations[selectedIndex].color;
                textMutationPanelCur.text = baronVonTerrain.sandSlotGenomeCurrent.name; // "Properties: " + gameManager.theRenderKing.baronVonTerrain.sandSlotGenomeCurrent.elevationChange.ToString();
                textMutationPanelNew.text = baronVonTerrain.sandSlotGenomeMutations[selectedIndex].textDescriptionMutation;
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

        Color uiColor = zooplanktonManager.zooplanktonSlotGenomeMutations[0].representativeData.color;
        uiColor.a = 1f;
        imageMutationPanelThumbnailA.color = uiColor; // UnityEngine.Random.ColorHSV();
        uiColor = zooplanktonManager.zooplanktonSlotGenomeMutations[1].representativeData.color;
        uiColor.a = 1f;
        imageMutationPanelThumbnailB.color = uiColor;
        uiColor = zooplanktonManager.zooplanktonSlotGenomeMutations[2].representativeData.color;
        uiColor.a = 1f;
        imageMutationPanelThumbnailC.color = uiColor;
        uiColor = zooplanktonManager.zooplanktonSlotGenomeMutations[3].representativeData.color;
        uiColor.a = 1f;
        imageMutationPanelThumbnailD.color = uiColor;
        uiColor = zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].representativeData.color;
        uiColor.a = 1f;
        imageMutationPanelNewPortrait.color = uiColor;
        
        uiColor = zooplanktonManager.zooplanktonSlotGenomeCurrent.representativeData.color;
        uiColor.a = 1f;
        imageMutationPanelCurPortrait.color = uiColor; 

        //swimSpeed01
        textMutationParameters.text = "Swim Speed:\nAging Rate:\nAttract:";

        textMutationPanelCur.text = zooplanktonManager.zooplanktonSlotGenomeCurrent.swimSpeed01.ToString("F2") +
                                    "\n" + zooplanktonManager.zooplanktonSlotGenomeCurrent.agingRate01.ToString("F2") +
                                    "\n" + zooplanktonManager.zooplanktonSlotGenomeCurrent.attractForce01.ToString("F2"); // "Reaction Rate: " + gameManager.simulationManager.vegetationManager.decomposerSlotGenomeCurrent.reactionRate.ToString();


        float deltaSwimSpeed = zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].swimSpeed01 - 
                        zooplanktonManager.zooplanktonSlotGenomeCurrent.swimSpeed01;
        string strDeltaSwimSpeed = GetStringDelta(deltaSwimSpeed);

        float deltaAgingRate = zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].agingRate01 - 
                        zooplanktonManager.zooplanktonSlotGenomeCurrent.agingRate01;
        string strDeltaAgingRate = GetStringDelta(deltaAgingRate);

        float deltaAttract = zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].attractForce01 - 
                        zooplanktonManager.zooplanktonSlotGenomeCurrent.attractForce01;
        string strDeltaAttract = GetStringDelta(deltaAttract);


        textMutationPanelNew.text = zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].swimSpeed01.ToString("F2") +
                                    strDeltaSwimSpeed + "\n" +
                                    zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].agingRate01.ToString("F2") +
                                    strDeltaAgingRate + "\n" +
                                    zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID].attractForce01.ToString("F2") +
                                    strDeltaAttract;



    }
    private void UpdatePlantsUI() {
        Color uiColor = vegetationManager.plantSlotGenomeMutations[0].displayColorPri; // new Color(hue.x, hue.y, hue.z);
        //uiColor.a = 1f;
        imageMutationPanelThumbnailA.color = uiColor; // UnityEngine.Random.ColorHSV();
        uiColor = vegetationManager.plantSlotGenomeMutations[1].displayColorPri;
        imageMutationPanelThumbnailB.color = uiColor;
        uiColor = vegetationManager.plantSlotGenomeMutations[2].displayColorPri;
        imageMutationPanelThumbnailC.color = uiColor;
        uiColor = vegetationManager.plantSlotGenomeMutations[3].displayColorPri;
        imageMutationPanelThumbnailD.color = uiColor;
          
        uiColor = vegetationManager.plantSlotGenomeCurrent.displayColorPri;
        imageMutationPanelCurPortrait.color = uiColor; 
        uiColor = vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].displayColorPri;        
        imageMutationPanelNewPortrait.color = uiColor;
        

        textMutationParameters.text = "Growth Rate:\nNONE:";

        textMutationPanelCur.text = vegetationManager.plantSlotGenomeCurrent.growthRate.ToString("F2") +
                                    "\n" + "ADD HERE";

        float deltaGrowthRate = vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].growthRate - 
                        vegetationManager.plantSlotGenomeCurrent.growthRate;
        string strDeltaGrowthRate = GetStringDelta(deltaGrowthRate);

        textMutationPanelNew.text = vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].growthRate.ToString("F2") +
                                    strDeltaGrowthRate;

   
        
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
            vegetationManager.decomposerSlotGenomeCurrent = vegetationManager.decomposerSlotGenomeMutations[selectedToolbarMutationID];
            //gameManager.simulationManager.vegetationManager.WorldLayerDecomposerGenomeStuff(ref decomposerSlotGenomeCurrent, 0f);
            vegetationManager.GenerateWorldLayerDecomposersGenomeMutationOptions();
        }
        else if(slotRef.kingdomID == 1) {  // Plants
            if (slotRef.tierID == 0) {
                vegetationManager.algaeSlotGenomeCurrent = simulationManager.vegetationManager.algaeSlotGenomeMutations[selectedToolbarMutationID];
                vegetationManager.GenerateWorldLayerAlgaeGridGenomeMutationOptions();
            }
            else {
                // OLD //gameManager.simulationManager.vegetationManager.plantSlotGenomeCurrent = gameManager.simulationManager.vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID];
                vegetationManager.plantSlotGenomeCurrent.plantRepData = vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].plantRepData;
                vegetationManager.plantSlotGenomeCurrent.textDescriptionMutation = vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].textDescriptionMutation;
                vegetationManager.plantSlotGenomeCurrent.growthRate = vegetationManager.plantSlotGenomeMutations[selectedToolbarMutationID].growthRate;
                vegetationManager.ProcessPlantSlotMutation();
                vegetationManager.GenerateWorldLayerPlantParticleGenomeMutationOptions();
            }
            
            //gameManager.simulationManager.vegetationManager.ProcessSlotMutation();
            //algaeRepData = gameManager.simulationManager.vegetationManager.algaeSlotGenomeCurrent.algaeRepData;
        }
        else if(slotRef.kingdomID == 2) {  // Animals
            if(slotRef.tierID == 0) { // zooplankton
                zooplanktonManager.zooplanktonSlotGenomeCurrent = zooplanktonManager.zooplanktonSlotGenomeMutations[selectedToolbarMutationID];
                zooplanktonManager.GenerateWorldLayerZooplanktonGenomeMutationOptions();
                zooplanktonManager.ProcessSlotMutation();
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
                baronVonTerrain.bedrockSlotGenomeCurrent = baronVonTerrain.bedrockSlotGenomeMutations[selectedToolbarMutationID];
            }
            else if(slotRef.slotID == 1) {
                baronVonTerrain.stoneSlotGenomeCurrent = baronVonTerrain.stoneSlotGenomeMutations[selectedToolbarMutationID];
            }
            else if(slotRef.slotID == 2) {
                baronVonTerrain.pebblesSlotGenomeCurrent = baronVonTerrain.pebblesSlotGenomeMutations[selectedToolbarMutationID];
            }
            else if(slotRef.slotID == 3) { // world?
                baronVonTerrain.sandSlotGenomeCurrent = baronVonTerrain.sandSlotGenomeMutations[selectedToolbarMutationID];
            }

            // if terrain:
            baronVonTerrain.GenerateTerrainSlotGenomeMutationOptions(slotRef.slotID);
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
