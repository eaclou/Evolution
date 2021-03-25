using UnityEngine;

public class SpeciesTreeBarUI : MonoBehaviour {

    public GlobalResourcesUI globalResourcesUI;
    public int index;
    public int speciesID;


    public void Initialize(GlobalResourcesUI globalResourcesUI, int index, int speciesID) {
        this.globalResourcesUI = globalResourcesUI;
        this.index = index;
        this.speciesID = speciesID;
    }

    // Updates focusedCandidate in uiManager 
    public void ClickedThisButton() {
        globalResourcesUI.SetSelectedSpeciesUI(speciesID);        
    }
}
