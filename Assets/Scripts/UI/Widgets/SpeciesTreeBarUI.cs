using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeciesTreeBarUI : MonoBehaviour {

    public UIManager uiManagerRef;
    public int index;
    public int speciesID;


    public void Initialize(UIManager man, int index, int speciesID) {
        uiManagerRef = man;
        this.index = index;
        this.speciesID = speciesID;
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ClickedThisButton() {
        //Debug.Log("SADFASDFA");
        uiManagerRef.globalResourcesUI.SetSelectedSpeciesUI(speciesID);  // updates focusedCandidate in uiManager

        
    }
}
