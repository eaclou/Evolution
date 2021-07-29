using System;
using UnityEngine;
using UnityEngine.UI;

public class GenomeViewerUI : MonoBehaviour {
    UIManager uiManager => UIManager.instance;

    public GameObject panelGenomeSensors;    
    public GameObject panelPerformanceBehavior;
    public GameObject panelEaten;
    
        
    public void UpdateUI() {
        
        panelGenomeSensors.SetActive(true);
        panelPerformanceBehavior.SetActive(true);
        panelEaten.SetActive(true);

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
