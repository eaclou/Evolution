using UnityEngine;

public class SpeciesTreeBarUI : MonoBehaviour {
    UIManager uiManager => UIManager.instance;

    //public AllSpeciesTreePanelUI allSpeciesTreePanelUI;
    public int index;
    public int speciesID;

    public SpeciesGenomePool linkedPool;

    public Vector3 targetPosition; // UI canvas

    public void Initialize(int index, SpeciesGenomePool pool) {        
        this.index = index;
        this.linkedPool = pool;
        this.speciesID = pool.speciesID;

        targetPosition = Vector3.zero;
    }

    // Updates focusedCandidate in uiManager 
    public void ClickedThisButton() {
        uiManager.SetSelectedSpeciesUI(speciesID);        
    }
    public void SetTargetPosition(Vector3 newPos) {
        targetPosition = newPos;
    }
    public void UpdateButtonDisplay() {
        gameObject.transform.localPosition = targetPosition;
    }
}
