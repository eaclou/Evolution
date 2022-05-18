using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldTreePanelUI : MonoBehaviour
{
    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    SimulationManager simulationManager => SimulationManager.instance;
    MasterGenomePool masterGenomePool => simulationManager.masterGenomePool;
    UIManager uiManagerRef => UIManager.instance;
    SelectionManager selectionManager => SelectionManager.instance;

    [SerializeField] GameObject panelSpeciesTree;
    public bool isShowingExtinct = false;
    public Image imageSelectedSpeciesBG;    
    public Text textTitle;    

    public GameObject anchorGO;
    public GameObject prefabSpeciesIcon;
    public GameObject prefabCreatureIcon;

    private List<SpeciesIconUI> speciesIconsList;  // keeping track of spawned buttons
    private List<CreatureIconUI> creatureIconsList;

    //private int focusLevel = 0;  // ***TEMP!!!   0==species, 1==creatures, 2==selectedCreature

    public float timelineStartTimeStep = 0f;

    //private int curPanelMode = 0;  // 0 == lineage, 1 == graph
    //public int GetPanelMode() {
    //    return curPanelMode;
    //}
    
    public void Awake() {
        speciesIconsList = new List<SpeciesIconUI>();
    }
    
    public void Set(bool value) {
        panelSpeciesTree.SetActive(value);        
    }

    /*public void SetFocusLevel(int focusLvl) {
        focusLevel = focusLvl;
    }
    public int GetFocusLevel() {
        return focusLevel;
    }
    public void ToggleFocusLevel() {
        if(focusLevel == 0) {
            focusLevel = 1;
        }
        else if(focusLevel == 1) {
            focusLevel = 0;
        }
        else {
            focusLevel = 0;
        }        
    }*/
    
    public void RefreshPanelUI() {
        //UpdateSpeciesIconsTargetCoords();
        //textSelectedSpeciesTitle.text = "Selected Species: #" + uiManagerRef.selectedSpeciesID;

        Vector3 hue = simulationManager.masterGenomePool.completeSpeciesPoolsList[selectionManager.currentSelection.historySelectedSpeciesID].foundingCandidate.primaryHue;
        imageSelectedSpeciesBG.color = new Color(hue.x, hue.y, hue.z);
    }
    
    
    public void ClickButtonToggleExtinct() {
        isShowingExtinct = !isShowingExtinct;
        if(isShowingExtinct) { // was extinct, switch to current:
            if(masterGenomePool.currentlyActiveSpeciesIDList.Count < masterGenomePool.completeSpeciesPoolsList.Count) {
                int defaultSpeciesID = 0;
                for(int i = 0; i < masterGenomePool.completeSpeciesPoolsList.Count; i++) {
                    if(masterGenomePool.completeSpeciesPoolsList[i].isExtinct) {
                        defaultSpeciesID = i;
                        break;
                    }
                }
                //SetSelectedSpeciesUI(defaultSpeciesID);
            }
        }
        else {
            int defaultSpeciesID = masterGenomePool.currentlyActiveSpeciesIDList[0];
            //SetSelectedSpeciesUI(defaultSpeciesID);   
        }  
    }

}
