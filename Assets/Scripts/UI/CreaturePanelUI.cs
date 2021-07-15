using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CreaturePanelUI : MonoBehaviour
{
    [SerializeField]
    GameObject panelPortrait;
    [SerializeField]
    GameObject panelGenome;
    [SerializeField]
    GameObject panelBrain;
    [SerializeField]
    GameObject panelPaperDoll;

    private PanelMode curPanelMode;
    public enum PanelMode {
        Portrait,
        Genome,
        Brain
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    public void Tick() {
        if(curPanelMode == PanelMode.Portrait) {
            panelPortrait.SetActive(true);

            panelGenome.SetActive(false);
            panelBrain.SetActive(false);
        }
        else if(curPanelMode == PanelMode.Genome) {
            panelGenome.SetActive(true);

            panelPortrait.SetActive(false);
            panelBrain.SetActive(false);
        }
        else if(curPanelMode == PanelMode.Brain) {
            panelBrain.SetActive(true);

            panelGenome.SetActive(false);
            panelPortrait.SetActive(false);
        }
    }

    public void SetPanelMode(int modeID) {
        curPanelMode = (PanelMode)modeID;
    }
}
