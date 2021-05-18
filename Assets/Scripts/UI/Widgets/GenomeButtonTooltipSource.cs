using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenomeButtonTooltipSource : MonoBehaviour {

    
    public string tooltipString;
    //public GenomeViewerUI genomeViewerUIRef;
    public bool isSensorEnabled;

    public void OnHoverStart() {
        UIManager.instance.genomeViewerUI.EnterTooltipObject(this);
        
    }
    public void OnHoverExit() {
        UIManager.instance.genomeViewerUI.EnterTooltipObject(this);
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
