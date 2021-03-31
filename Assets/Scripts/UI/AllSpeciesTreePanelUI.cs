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

    public GameObject treeAnchorUI;
    public GameObject prefabSpeciesBar;

    SimulationManager simulationManager => SimulationManager.instance;
    MasterGenomePool masterGenomePool => simulationManager.masterGenomePool;
    UIManager uiManagerRef => simulationManager.uiManager;

    public void Set(bool value) {
        panelSpeciesTree.SetActive(value);        
    }

    public void UpdateUI() {
        
        textSelectedSpeciesTitle.text = "SPECIES #" + uiManagerRef.selectedSpeciesID.ToString() + ":  " + masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID].foundingCandidate.candidateGenome.bodyGenome.coreGenome.name;
        SpeciesGenomePool pool = masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID];
        Vector3 hue = pool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
        imageSelectedSpeciesBG.color = new Color(hue.x, hue.y, hue.z);
        textSelectedSpeciesTitle.color = Color.white; // new Color(hue.x, hue.y, hue.z);

        if(simulationManager.simAgeTimeSteps % 177 == 275) {
            UpdateSpeciesListBars();
        }
    }
    
    public void UpdateSpeciesListBars() {
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

            foreach (Transform child in treeAnchorUI.transform) {
                    GameObject.Destroy(child.gameObject);
            }
            for (int s = 0; s < extinctSpeciesIDList.Count; s++) {
                int speciesID = extinctSpeciesIDList[s];

                SpeciesGenomePool sourcePool = masterGenomePool.completeSpeciesPoolsList[speciesID];
                //int parentSpeciesID = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].parentSpeciesID;

                AgentGenome templateGenome = sourcePool.leaderboardGenomesList[0].candidateGenome; //.bodyGenome.coreGenome.name;
                Color color = new Color(templateGenome.bodyGenome.appearanceGenome.huePrimary.x, templateGenome.bodyGenome.appearanceGenome.huePrimary.y, templateGenome.bodyGenome.appearanceGenome.huePrimary.z);

                GameObject obj = Instantiate(prefabSpeciesBar, new Vector3(0, 0, 0), Quaternion.identity);
                obj.transform.SetParent(treeAnchorUI.transform, false);

                //int stepCreated = sourcePool.timeStepCreated;
                //int stepExtinct = sourcePool.timeStepExtinct;

                //float barStart01 = (float)stepCreated / (float)stepExtinct; // **** Only shrink sub Image<> !!! not whole button!!!
                //float barEnd01 = 1f;
                if(uiManagerRef.selectedSpeciesID == speciesID) {
                    obj.transform.localScale = new Vector3(1.05f, 1.35f, 1f);
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

            foreach (Transform child in treeAnchorUI.transform) {
                Destroy(child.gameObject);
            }
            /*List<int> speciesTreeIDList = new List<int>();
            int topID = uiManagerRef.gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[0];
            speciesTreeIDList.Add(topID);
            int[] speciesTreeIDArray = new int[numActiveSpecies];
            speciesTreeIDArray[0] = uiManagerRef.gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[0];
            // create ordered list of species by hierarchy:
            for (int s = 0; s < numActiveSpecies; s++) {
                //check list for parent
                int speciesID = uiManagerRef.gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[s];
                int parentSpeciesID = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].parentSpeciesID;

            }
            //int pointerID = 0;
            */
            for (int s = 0; s < numActiveSpecies; s++) {
                int speciesID = masterGenomePool.currentlyActiveSpeciesIDList[s];
                int parentSpeciesID = masterGenomePool.completeSpeciesPoolsList[speciesID].parentSpeciesID;

                AgentGenome templateGenome = masterGenomePool.completeSpeciesPoolsList[speciesID].leaderboardGenomesList[0].candidateGenome; //.bodyGenome.coreGenome.name;
                Color color = new Color(templateGenome.bodyGenome.appearanceGenome.huePrimary.x, templateGenome.bodyGenome.appearanceGenome.huePrimary.y, templateGenome.bodyGenome.appearanceGenome.huePrimary.z);

                GameObject obj = Instantiate(prefabSpeciesBar, new Vector3(0, 0, 0), Quaternion.identity);
                obj.transform.SetParent(treeAnchorUI.transform, false);

                //int stepCreated = uiManagerRef.gameManager.simulationManager.masterGenomePool.completeSpeciesPoolsList[speciesID].timeStepCreated;
                //int stepExtinct = uiManagerRef.gameManager.simulationManager.simAgeTimeSteps;

                //float barStart01 = (float)stepCreated / (float)stepExtinct;
                //float barEnd01 = 1f;
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
                if (speciesID == uiManagerRef.focusedCandidate.speciesID) {
                    labelText += " *****";
                }
                obj.GetComponentInChildren<Text>().text = labelText;
                SpeciesTreeBarUI buttonScript = obj.GetComponent<SpeciesTreeBarUI>();
                buttonScript.Initialize(s, speciesID);
            }
        }
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
