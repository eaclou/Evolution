using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimEventComponent : MonoBehaviour {

    public int index = -1;
    public Text textEventName;
    public Text textEventCost;
    public Image imageBG;
    public bool isSelected = false;

    public UIManager uiManagerRef;

	public void UpdateSimEventPanel(UIManager uiManager, SimEventData data, int slotIndex) {
        uiManagerRef = uiManager;
        index = slotIndex;

        textEventName.text = data.name;
        textEventCost.text = data.cost.ToString();

        // set background color?
        if(isSelected) {
            imageBG.color = new Color(0.6f, 0.6f, 0.6f);
        }
        else {
            imageBG.color = new Color(0.3f, 0.3f, 0.3f);
        }
    }

    public void ClickedOnThisEvent() {
        uiManagerRef.ClickedOnEvent(this);
        //imageBG.color = new Color(0.6f, 0.6f, 0.6f);
        //Debug.Log("ClickedOnThisEvent()");
    }
}
