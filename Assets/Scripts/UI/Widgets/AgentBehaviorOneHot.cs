using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgentBehaviorOneHot : MonoBehaviour {

    public GameObject behaviorBarRest;
    public GameObject behaviorBarDash;
    public GameObject behaviorBarGuard;
    public GameObject behaviorBarBite;
    public GameObject behaviorBarAttack;
    public GameObject behaviorBarOther;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateBars(float rest, float dash, float guard, float bite, float attack, float other) {

        behaviorBarRest.transform.localScale = new Vector3(rest, 1f,  1f);
        UpdateBarColor(behaviorBarRest.GetComponent<Image>(), rest);
        
        behaviorBarDash.transform.localScale = new Vector3(dash, 1f, 1f);
        UpdateBarColor(behaviorBarDash.GetComponent<Image>(), dash);

        behaviorBarGuard.transform.localScale = new Vector3(guard, 1f,  1f);
        UpdateBarColor(behaviorBarGuard.GetComponent<Image>(), guard);

        behaviorBarBite.transform.localScale = new Vector3(bite, 1f, 1f);
        UpdateBarColor(behaviorBarBite.GetComponent<Image>(), bite);

        behaviorBarAttack.transform.localScale = new Vector3(attack, 1f, 1f);
        UpdateBarColor(behaviorBarAttack.GetComponent<Image>(), attack);

        behaviorBarOther.transform.localScale = new Vector3(other, 1f, 1f);
        UpdateBarColor(behaviorBarOther.GetComponent<Image>(), other);

    }

    private void UpdateBarColor(Image image, float val) {
        if(val < -0.25f) {
            image.color = Color.red;
        }
        else if(val > 0.25f) {
            image.color = Color.green;
        }
        else {
            image.color = Color.gray;
        }
        
    }
}
