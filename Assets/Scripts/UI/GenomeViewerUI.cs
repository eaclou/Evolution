using System;
using UnityEngine;
using UnityEngine.UI;

public class GenomeViewerUI : MonoBehaviour {
    SimulationManager simulationManager => SimulationManager.instance;
    UIManager uiManager => UIManager.instance;

    public SpeciesOverviewUI speciesOverviewUI;
    public GameObject panelGenomeSensors;
    
    [SerializeField] Tab genomeTab;
    [SerializeField] Tab historyTab;
    [SerializeField] Tab performanceTab;

    public GameObject panelGenomeAbilities;
    public GameObject panelPerformanceBehavior;
    public GameObject panelEaten;
    
    
    public bool isGenomeTabActive = true;
    public bool isPerformanceTabActive = false;
    public bool isHistoryTabActive = false;

    public GameObject imageDeadDim;

    public bool isTooltipHover = true;
    public string tooltipString;

    void Start () {
		isTooltipHover = false;
	}
    
    public void UpdateUI() {
        // * WPP: only usage, may as well set in editor
        // relates to future use items
        panelGenomeAbilities.SetActive(false);
        panelGenomeSensors.SetActive(true);
        panelPerformanceBehavior.SetActive(true);
        panelEaten.SetActive(true);

        // * WPP delegate to components (organize scattered references first)
        genomeTab.SetActive(isGenomeTabActive);
        performanceTab.SetActive(isPerformanceTabActive);
        historyTab.SetActive(isHistoryTabActive);
        imageDeadDim.SetActive(simulationManager.targetAgentIsDead);
    }
    
    #region Button Clicks
    
    public void ClickButtonNext() {
        uiManager.CycleFocusedCandidateGenome();
    }
    
    // * WPP: should be a similar process to ClickButtonNext
    public void ClickButtonPrev() {
        //speciesOverviewUI.CycleHallOfFame();
        speciesOverviewUI.CycleCurrentGenome();
    }
    
    public void ClickButtonGenomeTab() {
        isGenomeTabActive = true;
        isPerformanceTabActive = false;
        isHistoryTabActive = false; 
    }
    
    public void ClickButtonPerformanceTab() {
        isGenomeTabActive = false;
        isPerformanceTabActive = true;
        isHistoryTabActive = false;
    }
    
    public void ClickButtonHistoryTab() {
        isGenomeTabActive = false;
        isPerformanceTabActive = false;
        isHistoryTabActive = true;
    }
    
    #endregion

    
    public void EnterTooltipObject(GenomeButtonTooltipSource tip) {
        isTooltipHover = true;
        tooltipString = tip.tooltipString;
    }
    
    public void ExitTooltipObject() {
        isTooltipHover = false;
    }
    
    [Serializable]
    public class Tab
    {
        [SerializeField] GameObject panel;
        [SerializeField] Image image;
        
        public void SetActive(bool value)
        {
            panel.SetActive(value);
            image.color = value ? Color.white : Color.gray;
        }
    }
}
