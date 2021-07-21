using System;
using UnityEngine;
using UnityEngine.UI;

public class GenomeViewerUI : MonoBehaviour {
    UIManager uiManager => UIManager.instance;

    public GameObject panelGenomeSensors;    
    public GameObject panelPerformanceBehavior;
    public GameObject panelEaten;
    
        
    public void UpdateUI() {
        // * WPP: only usage, may as well set in editor
        // relates to future use items
        //panelGenomeAbilities.SetActive(false);
        panelGenomeSensors.SetActive(true);
        panelPerformanceBehavior.SetActive(true);
        panelEaten.SetActive(true);

        // * WPP delegate to components (organize scattered references first)
        //genomeTab.SetActive(isGenomeTabActive);
        //performanceTab.SetActive(isPerformanceTabActive);
        //historyTab.SetActive(isHistoryTabActive);
        //imageDeadDim.SetActive(simulationManager.targetAgentIsDead);
    }
    
    #region Button Clicks
    
    public void ClickButtonNext() {
        uiManager.CycleFocusedCandidateGenome();
    }
    
    // * WPP: should be a similar process to ClickButtonNext
    public void ClickButtonPrev() {
        //speciesOverviewUI.CycleHallOfFame();
        //speciesOverviewUI.CycleCurrentGenome();
    }
        
    
    #endregion
    //moved to ObserverModeUI:
    /*public void EnterTooltipObject(GenomeButtonTooltipSource tip) {
        isTooltipHover = true;
        tooltipString = tip.tooltipString;
    }
    
    public void ExitTooltipObject() {
        isTooltipHover = false;
    }*/
    
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
