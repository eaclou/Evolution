using UnityEngine;

public class SpeciesTreeBarUI : MonoBehaviour {
    UIManager uiManager => UIManager.instance;

    //public AllSpeciesTreePanelUI allSpeciesTreePanelUI;
    public int index;
    public int speciesID;


    public void Initialize(int index, int speciesID) {        
        this.index = index;
        this.speciesID = speciesID;
    }

    // Updates focusedCandidate in uiManager 
    public void ClickedThisButton() {
        uiManager.SetSelectedSpeciesUI(speciesID);        
    }
}
