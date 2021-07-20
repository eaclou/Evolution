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

    [SerializeField]
    Text textPanelStateDebug;

    private CreaturePanelMode curPanelMode;
    public enum CreaturePanelMode {
        Portrait,
        Genome,
        Brain
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    public void Tick() {
        textPanelStateDebug.text = "MODE: " + curPanelMode;

        if (curPanelMode == CreaturePanelMode.Portrait) {
            panelPortrait.SetActive(true);

            panelGenome.SetActive(false);
            panelBrain.SetActive(false);
        }
        else if(curPanelMode == CreaturePanelMode.Genome) {
            panelGenome.SetActive(true);

            panelPortrait.SetActive(false);
            panelBrain.SetActive(false);
        }
        else if(curPanelMode == CreaturePanelMode.Brain) {
            panelBrain.SetActive(true);

            panelGenome.SetActive(false);
            panelPortrait.SetActive(false);
        }
    }

    public void SetPanelMode(int modeID) {
        curPanelMode = (CreaturePanelMode)modeID;
    }
}
