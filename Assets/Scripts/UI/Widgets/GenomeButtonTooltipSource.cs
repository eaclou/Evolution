using UnityEngine;

// * WPP: no functionality, delete script if obsolete
public class GenomeButtonTooltipSource : MonoBehaviour {
    UIManager ui => UIManager.instance;
    GenomeViewerUI genomeViewer => ui.genomeViewerUI;

    public string tooltipString;
    public bool isSensorEnabled;

    public void OnHoverStart() {
        //genomeViewer.EnterTooltipObject(this);
    }
    
    public void OnHoverExit() {
        //genomeViewer.ExitTooltipObject();
    }
}
