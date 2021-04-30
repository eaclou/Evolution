using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllSpeciesTreePanelUI : MonoBehaviour
{
    [SerializeField] GameObject panelSpeciesTree;
    public bool isShowingExtinct = false;
    public Text textSelectedSpeciesTitle;
    public Image imageSelectedSpeciesBG;
    public Text textSpeciationTree;    
    public Text textStatsBody;

    public GameObject anchorGO;
    [SerializeField]
    private int panelSidePixelCount = 256;
    public GameObject prefabSpeciesBar;

    private List<SpeciesTreeBarUI> speciesIconsList;  // keeping track of spawned buttons

    SimulationManager simulationManager => SimulationManager.instance;
    MasterGenomePool masterGenomePool => simulationManager.masterGenomePool;
    UIManager uiManagerRef => UIManager.instance;

    public void Awake() {
        speciesIconsList = new List<SpeciesTreeBarUI>();
    }
    

    public void Set(bool value) {
        panelSpeciesTree.SetActive(value);        
    }

    public void UpdateUI() {

        //*** update positions of buttons, etc.
        
        for(int i = 0; i < speciesIconsList.Count; i++) {
            SpeciesTreeBarUI icon = speciesIconsList[i];
            icon.SetTargetPosition(new Vector3(32f * (float)i, (float)panelSidePixelCount -64f * (float)i, 0f));
            icon.UpdateButtonDisplay();
        }

    }

    public void InitializeSpeciesListBars() {
        Debug.Log("InitializeSpeciesListBarsInitializeSpeciesListBarsInitializeSpeciesListBars");
        int numSpecies = masterGenomePool.completeSpeciesPoolsList.Count;

        foreach (Transform child in anchorGO.transform) { // clear all GO's
            Destroy(child.gameObject);
        }

        for (int s = 0; s < numSpecies; s++) {
            int speciesID = s;
            int parentSpeciesID = masterGenomePool.completeSpeciesPoolsList[speciesID].parentSpeciesID;

            AgentGenome templateGenome = masterGenomePool.completeSpeciesPoolsList[speciesID].leaderboardGenomesList[0].candidateGenome; //.bodyGenome.coreGenome.name;
            Color color = new Color(templateGenome.bodyGenome.appearanceGenome.huePrimary.x, templateGenome.bodyGenome.appearanceGenome.huePrimary.y, templateGenome.bodyGenome.appearanceGenome.huePrimary.z);

            GameObject obj = Instantiate(prefabSpeciesBar, new Vector3(0f, 0f, 0f), Quaternion.identity);
            obj.transform.SetParent(anchorGO.transform, false);
            obj.transform.localPosition = Vector3.zero;

            if (uiManagerRef.selectedSpeciesID == speciesID) {
                obj.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
            }
            else {
                obj.transform.localScale = new Vector3(1f, 1f, 1f);
            }
            obj.GetComponent<Image>().color = color;

            string labelText = "";
            labelText += "[" + speciesID.ToString() + "] " + masterGenomePool.completeSpeciesPoolsList[speciesID].foundingCandidate.candidateGenome.bodyGenome.coreGenome.name;

            int savedParentSpeciesID = parentSpeciesID;
            string lineageTxt = "";
            for (int i = 0; i < 4; i++) {

                if (savedParentSpeciesID >= 0) {
                    //get parent pool:
                    SpeciesGenomePool parentPool = masterGenomePool.completeSpeciesPoolsList[savedParentSpeciesID];
                    lineageTxt += " <- " + parentPool.speciesID.ToString();

                    savedParentSpeciesID = parentPool.parentSpeciesID;
                }
                else {
                    //lineageTxt += "*";
                    break;
                }
            }
            labelText += lineageTxt;
            if (speciesID == uiManagerRef.selectedSpeciesID) {
                labelText += " *****";
            }
            obj.GetComponentInChildren<Text>().text = labelText;
            SpeciesTreeBarUI buttonScript = obj.GetComponent<SpeciesTreeBarUI>();
            speciesIconsList.Add(buttonScript);

            buttonScript.Initialize(s, masterGenomePool.completeSpeciesPoolsList[speciesID]);
            
            //buttonScript.UpdateButtonDisplay();
        }
    }

    public void AddNewSpeciesToPanel(SpeciesGenomePool pool) {

    }
    /*
    public void UpdateSpeciesListBarsOLD() {
        // ****************************************************
        // ***** Add tab for Extinct species:
        if(isShowingExtinct) {
            List<int> extinctSpeciesIDList = new List<int>();
            for(int i = 0; i < masterGenomePool.completeSpeciesPoolsList.Count; i++) {
                if(masterGenomePool.completeSpeciesPoolsList[i].isExtinct) {
                    extinctSpeciesIDList.Add(i);
                    //break;
                }
            }

            foreach (Transform child in anchorGO.transform) {
                    GameObject.Destroy(child.gameObject);
            }
            for (int s = 0; s < extinctSpeciesIDList.Count; s++) {
                int speciesID = extinctSpeciesIDList[s];

                SpeciesGenomePool sourcePool = masterGenomePool.completeSpeciesPoolsList[speciesID];
             
                AgentGenome templateGenome = sourcePool.leaderboardGenomesList[0].candidateGenome; //.bodyGenome.coreGenome.name;
                Color color = new Color(templateGenome.bodyGenome.appearanceGenome.huePrimary.x, templateGenome.bodyGenome.appearanceGenome.huePrimary.y, templateGenome.bodyGenome.appearanceGenome.huePrimary.z);

                GameObject obj = Instantiate(prefabSpeciesBar, new Vector3(0, 0, 0), Quaternion.identity);
                obj.transform.SetParent(anchorGO.transform, false);

                if(uiManagerRef.selectedSpeciesID == speciesID) {
                    obj.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
                }
                else {
                    obj.transform.localScale = Vector3.one;
                }
                obj.GetComponent<Image>().color = color;
                if(sourcePool.isFlaggedForExtinction) {
                    obj.GetComponent<Image>().color = color * 0.5f;
                }

                string labelText = "";                
                labelText += "[" + speciesID.ToString() + "] " + sourcePool.foundingCandidate.candidateGenome.bodyGenome.coreGenome.name;
                if(speciesID == uiManagerRef.focusedCandidate.speciesID) {
                    labelText += " ***";
                }
                obj.GetComponentInChildren<Text>().text = labelText;
                SpeciesTreeBarUI buttonScript = obj.GetComponent<SpeciesTreeBarUI>();
                
                buttonScript.Initialize(s, speciesID);
            }
        }
        else {  // EXTANT!!! ***
            int numActiveSpecies = masterGenomePool.currentlyActiveSpeciesIDList.Count;

            foreach (Transform child in anchorGO.transform) {
                Destroy(child.gameObject);
            }
            
            for (int s = 0; s < numActiveSpecies; s++) {
                int speciesID = masterGenomePool.currentlyActiveSpeciesIDList[s];
                int parentSpeciesID = masterGenomePool.completeSpeciesPoolsList[speciesID].parentSpeciesID;

                AgentGenome templateGenome = masterGenomePool.completeSpeciesPoolsList[speciesID].leaderboardGenomesList[0].candidateGenome; //.bodyGenome.coreGenome.name;
                Color color = new Color(templateGenome.bodyGenome.appearanceGenome.huePrimary.x, templateGenome.bodyGenome.appearanceGenome.huePrimary.y, templateGenome.bodyGenome.appearanceGenome.huePrimary.z);

                GameObject obj = Instantiate(prefabSpeciesBar, new Vector3(0, 0, 0), Quaternion.identity);
                obj.transform.SetParent(anchorGO.transform, false);

                if(uiManagerRef.selectedSpeciesID == speciesID) {
                    obj.transform.localScale = new Vector3(1.06f, 1.42f, 1f);
                }
                else {
                    obj.transform.localScale = new Vector3(1f, 1f, 1f);
                }
                obj.GetComponent<Image>().color = color;

                string labelText = "";                
                labelText += "[" + speciesID.ToString() + "] " + masterGenomePool.completeSpeciesPoolsList[speciesID].foundingCandidate.candidateGenome.bodyGenome.coreGenome.name;

                int savedParentSpeciesID = parentSpeciesID;
                string lineageTxt = "";
                for(int i = 0; i < 64; i++) {
                
                    if (savedParentSpeciesID >= 0) {
                        
                        SpeciesGenomePool parentPool = masterGenomePool.completeSpeciesPoolsList[savedParentSpeciesID];
                        lineageTxt += " <- " + parentPool.speciesID.ToString();

                        savedParentSpeciesID = parentPool.parentSpeciesID;
                    }
                    else {
                        
                        break;
                    }
                }
                labelText += lineageTxt;
                if (speciesID == uiManagerRef.focusedCandidate.speciesID) {
                    labelText += " *****";
                }
                obj.GetComponentInChildren<Text>().text = labelText;
                SpeciesTreeBarUI buttonScript = obj.GetComponent<SpeciesTreeBarUI>();
                buttonScript.Initialize(s, speciesID);
            }
        }
    }
    */
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
