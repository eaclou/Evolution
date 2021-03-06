﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryPanelUI : MonoBehaviour
{
    public GameObject anchorGO;
    public GameObject prefabSpeciesIcon;
    public GameObject prefabCreatureIcon;

    private List<SpeciesIconUI> speciesIconsList;  // keeping track of spawned buttons
    private List<CreatureIconUI> creatureIconsList;

    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    SimulationManager simulationManager => SimulationManager.instance;
    MasterGenomePool masterGenomePool => simulationManager.masterGenomePool;
    UIManager uiManagerRef => UIManager.instance;

    public float timelineStartTimeStep = 0f;

    [SerializeField]
    Text textPanelStateDebug;

    private int focusLevel = 0; // REMOVE!
    public int GetFocusLevel() {
        return focusLevel;
    }
    private int curIntMode = 0;  // 0 == lineage, 1 == graph // also REMOVE!
    public int GetIntMode() {
        return curIntMode;
    }

    private HistoryPanelMode curPanelMode;
    public enum HistoryPanelMode {
        AllSpecies,
        ActiveSpecies,
        SpeciesPopulation,
        CreatureTimeline
    }

    // How to sync rendered geo with UI buttons???
    
    public void Awake() {
        speciesIconsList = new List<SpeciesIconUI>();
    }
    public void Tick() {
        textPanelStateDebug.text = "MODE: " + curPanelMode;

        
        if(curPanelMode == HistoryPanelMode.AllSpecies) {
            UpdateSpeciesIconsDefault();
        }
        else if(curPanelMode == HistoryPanelMode.ActiveSpecies) {
            UpdateSpeciesIconsGraphMode();
        }
        else if(curPanelMode == HistoryPanelMode.SpeciesPopulation) {

        }
        else if(curPanelMode == HistoryPanelMode.CreatureTimeline) {

        }
        

        for(int i = 0; i < speciesIconsList.Count; i++) {
            SpeciesIconUI icon = speciesIconsList[i];
            
            bool isSelected = false;
            if (icon.speciesID == uiManagerRef.selectedSpeciesID) {
                isSelected = true;
                icon.gameObject.transform.SetAsLastSibling();
            }
            icon.UpdateIconDisplay(360, isSelected);
        }

        float targetStartTimeStep = 0f;
        /*if(focusLevel == 0) {

        }
        else {
            if(simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID].candidateGenomesList.Count > 0) {
                targetStartTimeStep = simulationManager.masterGenomePool.completeSpeciesPoolsList[uiManagerRef.selectedSpeciesID].candidateGenomesList[0].performanceData.timeStepHatched; //***EAC better less naive way to calculate this
            
            }            
        }*/
        timelineStartTimeStep = Mathf.Lerp(timelineStartTimeStep, targetStartTimeStep, 0.125f);
    }

    private void CreateSpeciesIcon(SpeciesGenomePool pool) {
        
            AgentGenome templateGenome = masterGenomePool.completeSpeciesPoolsList[pool.speciesID].leaderboardGenomesList[0].candidateGenome; //.bodyGenome.coreGenome.name;
            Color color = new Color(templateGenome.bodyGenome.appearanceGenome.huePrimary.x, templateGenome.bodyGenome.appearanceGenome.huePrimary.y, templateGenome.bodyGenome.appearanceGenome.huePrimary.z);

            GameObject obj = Instantiate(prefabSpeciesIcon, new Vector3(0f, 0f, 0f), Quaternion.identity);
            obj.transform.SetParent(anchorGO.transform, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            obj.GetComponent<Image>().color = color;
          
            string labelText = "";
            labelText += "[" + pool.speciesID.ToString() + "]";// " + masterGenomePool.completeSpeciesPoolsList[pool.speciesID].foundingCandidate.candidateGenome.bodyGenome.coreGenome.name;

            obj.GetComponentInChildren<Text>().text = labelText;
            SpeciesIconUI iconScript = obj.GetComponent<SpeciesIconUI>();
            speciesIconsList.Add(iconScript);

            iconScript.Initialize(speciesIconsList.Count - 1, masterGenomePool.completeSpeciesPoolsList[pool.speciesID]);
 
    }
    public void InitializeSpeciesIcons() {
        Debug.Log("InitializeSpeciesListBarsInitializeSpeciesListBarsInitializeSpeciesListBars");
        int numSpecies = masterGenomePool.completeSpeciesPoolsList.Count;

        foreach (Transform child in anchorGO.transform) { // clear all GO's
            if(child.GetComponent<SpeciesIconUI>()) {
                Destroy(child.gameObject);
            }
            else {

            }
            
        }

        for (int s = 0; s < numSpecies; s++) {
            int speciesID = s;
            int parentSpeciesID = masterGenomePool.completeSpeciesPoolsList[speciesID].parentSpeciesID;

            CreateSpeciesIcon(masterGenomePool.completeSpeciesPoolsList[speciesID]);
            
        }
    }

    public void AddNewSpeciesToPanel(SpeciesGenomePool pool) {
        Debug.Log("AddNewSpeciesToPanelUI: " + pool.speciesID);

        CreateSpeciesIcon(pool);
    }
    
    private void UpdateSpeciesIconsLineageMode() {        
        for (int s = 0; s < speciesIconsList.Count; s++) {            
            float yCoord = 1f - (float)s / Mathf.Max(speciesIconsList.Count - 1, 1f);  
            float xCoord = 1f;
            if (speciesIconsList[s].linkedPool.isExtinct) {
                xCoord = (float)speciesIconsList[s].linkedPool.timeStepExtinct / Mathf.Max(1f, (float)simulationManager.simAgeTimeSteps);
            }

            float indent = 0.05f;
            if(focusLevel == 0) {

            }
            else {
                xCoord = 0f;
                if(speciesIconsList[s].linkedPool.speciesID == uiManagerRef.selectedSpeciesID) {
                    indent = 0.1f;
                }
            }

            xCoord = xCoord * 0.8f + indent;
            yCoord = yCoord * 0.67f + 0.1f;
            speciesIconsList[s].SetTargetCoords(new Vector2(xCoord, yCoord));
        }
    }
    private void UpdateSpeciesIconsGraphMode() {
        if (speciesIconsList[0].linkedPool.avgCandidateDataYearList.Count < 1) {
            UpdateSpeciesIconsDefault();
            return;
        }
        float bestScore = 0f;
        for (int s = 0; s < speciesIconsList.Count; s++) {
            SpeciesGenomePool pool = speciesIconsList[s].linkedPool;
            float valStat = (float)pool.avgCandidateDataYearList[pool.avgCandidateDataYearList.Count - 1].performanceData.totalTicksAlive;

            if(valStat > bestScore) {
                bestScore = valStat;
            }
        }

        // SORT
        for (int s = 0; s < speciesIconsList.Count; s++) {
            SpeciesGenomePool pool = speciesIconsList[s].linkedPool;
            float valStat = (float)pool.avgCandidateDataYearList[pool.avgCandidateDataYearList.Count - 1].performanceData.totalTicksAlive;

            float xCoord = 1f;
            if (pool.isExtinct) {
                xCoord = (float)pool.timeStepExtinct / Mathf.Max(1f, (float)simulationManager.simAgeTimeSteps);
            }
            if(focusLevel == 0) {

            }
            else {
                xCoord = 0f;
            }
            if(bestScore == 0f) {
                bestScore = 1f;
            }
            float yCoord = Mathf.Clamp01(valStat / bestScore);
            xCoord = xCoord * 0.8f + 0.1f;
            yCoord = yCoord * 0.67f + 0.1f;
            speciesIconsList[s].SetTargetCoords(new Vector2(xCoord, yCoord));
            
        }
    }
    private void UpdateSpeciesIconsDefault() {
        for (int s = 0; s < speciesIconsList.Count; s++) {        // simple list, evenly spaced    
            float xCoord = 1f;
            if(focusLevel == 0) {

            }
            else {
                xCoord = 0f;
            }
            float yCoord = (float)s / Mathf.Max(speciesIconsList.Count - 1, 1f);      
            xCoord = xCoord * 0.8f + 0.1f;
            yCoord = yCoord * 0.67f + 0.1f;
            speciesIconsList[s].SetTargetCoords(new Vector2(xCoord, yCoord));
        }
    }
}
