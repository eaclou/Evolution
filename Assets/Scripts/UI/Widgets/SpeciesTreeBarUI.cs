using UnityEngine;

public class SpeciesTreeBarUI : MonoBehaviour {
    UIManager uiManager => UIManager.instance;

    //public AllSpeciesTreePanelUI allSpeciesTreePanelUI;
    public int index;
    public int speciesID;

    public SpeciesGenomePool linkedPool;

    public Vector2 targetCoords; // UI canvas

    public void Initialize(int index, SpeciesGenomePool pool) {        
        this.index = index;
        this.linkedPool = pool;
        this.speciesID = pool.speciesID;

        targetCoords = Vector2.zero;

        Debug.Log("NEW BUTTON! " + index + ", " + pool.speciesID);
    }

    // Updates focusedCandidate in uiManager 
    public void ClickedThisButton() {
        uiManager.SetSelectedSpeciesUI(speciesID);        
    }
    public void SetTargetCoords(Vector2 newCoords) {
        targetCoords = newCoords;
    }
    public void UpdateButtonDisplay(int panelPixelSize, bool isSelected) {
        gameObject.transform.localPosition = targetCoords * (float)panelPixelSize;

        if(isSelected) {
            gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
        }
        else {
            gameObject.transform.localScale = Vector3.one;
        }            
    }
}
