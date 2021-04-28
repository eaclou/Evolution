using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimEventComponent : MonoBehaviour {
    UIManager uiManagerRef => UIManager.instance;

    public int index = -1;
    public Text textEventName;
    public Text textEventCost;
    public Image imageBG;
    public bool isSelected = false;

	public void UpdateSimEventPanel(SimEventData data, int slotIndex) {
        index = slotIndex;

        textEventName.text = data.name;
        textEventCost.text = "$" + data.cost.ToString();

        /*// set background color?
        Color bgColor = uiManager.buttonEventMinorColor;
        if(data.category == SimEventData.SimEventCategories.Major) {
            bgColor = uiManager.buttonEventMajorColor;
        }
        if(data.category == SimEventData.SimEventCategories.Extreme) {
            bgColor = uiManager.buttonEventExtremeColor;
        }*/

        if(isSelected) {
            //bgColor *= 2f;
            //imageBG.color = new Color(0.6f, 0.6f, 0.6f);
        }
        //else {
        //    imageBG.color = new Color(0.3f, 0.3f, 0.3f);
        //}
        //imageBG.color = bgColor;
    }

    public void ClickedOnThisEvent() {
        //uiManagerRef.ClickedOnEvent(this);
        
    }
}
