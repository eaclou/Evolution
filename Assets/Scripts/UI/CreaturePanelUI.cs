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
    Image imageAppearanceIcon;
    [SerializeField]
    Image imageGenomeIcon;
    [SerializeField]
    Image imageBrainIcon;

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

        Color onColor = Color.gray;
        Color offColor = Color.white;

        if (curPanelMode == CreaturePanelMode.Portrait) {
            panelPortrait.SetActive(true);
            imageAppearanceIcon.color = onColor;

            panelGenome.SetActive(false);
            panelBrain.SetActive(false);
            imageGenomeIcon.color = offColor;
            imageBrainIcon.color = offColor;
        }
        else if(curPanelMode == CreaturePanelMode.Genome) {
            panelGenome.SetActive(true);
            imageGenomeIcon.color = onColor;

            panelPortrait.SetActive(false);
            panelBrain.SetActive(false);
            imageAppearanceIcon.color = offColor;
            imageBrainIcon.color = offColor;
        }
        else if(curPanelMode == CreaturePanelMode.Brain) {
            panelBrain.SetActive(true);
            imageBrainIcon.color = onColor;

            panelGenome.SetActive(false);
            panelPortrait.SetActive(false);
            imageGenomeIcon.color = offColor;
            imageAppearanceIcon.color = offColor;
        }
    }

    public void SetPanelMode(int modeID) {
        curPanelMode = (CreaturePanelMode)modeID;
    }
}
