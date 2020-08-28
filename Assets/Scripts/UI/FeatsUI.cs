using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FeatsUI : MonoBehaviour {

    public UIManager uiManagerRef;
    public bool isUnlocked;
    public bool isOpen;

    public GameObject panelFeats;

    public Text textMostRecent;
    public Text textLog;


    public void UpdateFeatsPanelUI(List<Feat> featsList) {
        if(isOpen) {

            if(featsList.Count > 0) {
                textMostRecent.text = featsList[0].name + "\n" + featsList[0].description;

                string strLog = ""; // + featsList[0].description;//
                
                for(int i = 0; i < featsList.Count; i++) {
                    strLog += featsList[i].name + "\n[" + featsList[i].eventFrame.ToString() + "] " + featsList[i].description;

                    strLog += "\n\n";
                }
                textLog.text = strLog;

            }
            else {
                textMostRecent.text = "is this working?\n";

                textLog.text = "textlog\n";//
            }
            

            panelFeats.SetActive(true);
        }
        else {
            panelFeats.SetActive(false);
        }
        

    }

    public void ClickOpenClose() {
        isOpen = !isOpen;
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
