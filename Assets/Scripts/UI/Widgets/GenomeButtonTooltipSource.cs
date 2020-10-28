using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenomeButtonTooltipSource : MonoBehaviour {

    
    public string tooltipString;
    public GenomeViewerUI genomeViewerUIRef;
    public bool isSensorEnabled;

    public void OnHoverStart() {
        genomeViewerUIRef.EnterTooltipObject(this);
        
    }
    public void OnHoverExit() {
        genomeViewerUIRef.ExitTooltipObject();
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
