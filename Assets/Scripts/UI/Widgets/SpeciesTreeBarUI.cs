using UnityEngine;

public class SpeciesTreeBarUI : MonoBehaviour {

    public UIManager uiManagerRef;
    public int index;
    public int speciesID;


    public void Initialize(UIManager man, int index, int speciesID) {
        uiManagerRef = man;
        this.index = index;
        this.speciesID = speciesID;
    }

    public void ClickedThisButton() {
        uiManagerRef.globalResourcesUI.SetSelectedSpeciesUI(speciesID);  // updates focusedCandidate in uiManager        
    }
}
