using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClockUI : MonoBehaviour {
    public UIManager uiManagerRef;
    public bool isUnlocked;
    public bool isOpen;

    public GameObject panelClockMinimized;
    public Image imageClockHandA;
	public Image imageClockHandB;
    public Image imageClockHandC;
    
    // Use this for initialization
	void Start () {
		
	}
	
	public void UpdateClockUI (int numTicks) {
        float angVelA = -2.25f;
        float angVelB = -0.25f;
        float angVelC = -0.002f;
        imageClockHandA.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, (float)numTicks * angVelA);
        imageClockHandB.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, (float)numTicks * angVelB);
        imageClockHandC.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, (float)numTicks * angVelC);
	}
}
