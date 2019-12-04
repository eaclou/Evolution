using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WatcherUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public Image imageWatcherButtonMIP;  // superscript button over watcher toolbar Button
    public Image imageWatcherCurTarget; // in watcher panel
    public Text textTargetLayer;
    public Button buttonWatcherLock;
    public Button buttonHighlightingToggle;
    public Button buttonFollowingToggle;
    public bool isHighlight;
    public bool isFollow;
    public TrophicSlot targetSlotRef;
    public bool isSelected;

	// Use this for initialization
	void Start () {
        isHighlight = false;
	}
	
	

    public void ClickButtonHighlightingToggle() {
        isHighlight = !isHighlight;
        if(isHighlight) {
            uiManagerRef.curActiveTool = UIManager.ToolType.None;
            //Set ToolType to .None
        }
    }
    public void ClickButtonFollowingToggle() {
        isFollow = !isFollow;

        if(isFollow) {
            uiManagerRef.curActiveTool = UIManager.ToolType.None;
            //Set ToolType to .None
            //uiManagerRef.StartFollowingAgent();
            //uiManagerRef.StartFollowingPlantParticle();
            //uiManagerRef.StartFollowingAnimalParticle();
        }
        else {
            uiManagerRef.StopFollowingAgent();
            uiManagerRef.StopFollowingPlantParticle();
            uiManagerRef.StopFollowingAnimalParticle();
        }
    }
}
