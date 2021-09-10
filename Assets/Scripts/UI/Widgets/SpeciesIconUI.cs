using UnityEngine;
using UnityEngine.UI;

public class SpeciesIconUI : MonoBehaviour {
    UIManager uiManager => UIManager.instance;

    //public AllSpeciesTreePanelUI allSpeciesTreePanelUI;
    public int index;
    public int speciesID;

    public SpeciesGenomePool linkedPool;

    public Vector2 targetCoords; // UI canvas
    private Vector2 currentCoords;

    public TooltipUI tooltip;

    public Image imageColor1;
    public Image imageColor2;
    public Image imageColor3;

    public void Initialize(int index, SpeciesGenomePool pool) {        
        this.index = index;
        this.linkedPool = pool;
        this.speciesID = pool.speciesID;

        targetCoords = Vector2.zero;

        //Debug.Log("NEW BUTTON! " + index + ", " + pool.speciesID);
    }

    // Updates focusedCandidate in uiManager 
    public void Clicked() {
        uiManager.historyPanelUI.ClickSpeciesIcon(this);
        //uiManager.SetSelectedSpeciesUI(speciesID);        
    }
    public void SetTargetCoords(Vector2 newCoords) {
        targetCoords = newCoords;
    }
    public void UpdateSpeciesIconDisplay(int panelPixelSize, bool isSelected) {

        string toolString = "Species " + linkedPool.representativeCandidate.name.Substring(0, 1);
        // POSITION
        currentCoords = Vector2.Lerp(currentCoords, targetCoords, 0.67f);

        gameObject.transform.localPosition = new Vector3(currentCoords.x * (float)panelPixelSize, currentCoords.y * (float)panelPixelSize, 0f);

        Color colorPri = new Color(linkedPool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary.x, linkedPool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary.y, linkedPool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary.z);
        Color colorSec = new Color(linkedPool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary.x, linkedPool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary.y, linkedPool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary.z);   
        imageColor1.color = colorPri;
        imageColor2.color = colorSec;
        // APPEARANCE
        if (isSelected) {
            gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            gameObject.GetComponent<Image>().color = Color.white;
        }
        else {
            gameObject.transform.localScale = Vector3.one;
            gameObject.GetComponent<Image>().color = Color.gray * 0.5f;

            
        }  
        
        if(linkedPool.isExtinct) {
            gameObject.GetComponentInChildren<Text>().color = Color.gray * 0.05f;
            gameObject.transform.localScale = Vector3.one * 0.5f;
            imageColor3.color = Color.black;
            toolString += "\n(Extinct)";
        }
        else {
            imageColor3.color = Color.white;
            if (linkedPool.isFlaggedForExtinction) {
                gameObject.GetComponentInChildren<Text>().color = Color.gray;
            }
            else {
                gameObject.GetComponentInChildren<Text>().color = Color.white;
            }
            toolString += "\nAvg Life: " + linkedPool.avgCandidateData.performanceData.totalTicksAlive.ToString("F0");
        }

        tooltip.tooltipString = toolString;
    }
}
