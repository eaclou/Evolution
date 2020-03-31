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

    public Text textRest;
    public Text textDash;
    public Text textGuard;
    public Text textBite;
    public Text textAttack;
    public Text textOther;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateBars(float rest, float dash, float guard, float bite, float attack, float other, bool isCooldown) {

        float highestPriority = Mathf.Max(rest, Mathf.Max(dash, Mathf.Max(guard, Mathf.Max(bite, attack))));

        bool isActive = false;
        if(rest >= highestPriority && !isCooldown) {
            textRest.color = Color.white;
            isActive = true;
        }
        else {
            textRest.color = Color.gray;
        }
        behaviorBarRest.transform.localScale = new Vector3(rest, 1f,  1f);
        UpdateBarColor(behaviorBarRest.GetComponent<Image>(), rest, isActive);
        
        isActive = false;
        if(dash >= highestPriority && !isCooldown) {
            textDash.color = Color.white;
            isActive = true;
        }
        else {
            textDash.color = Color.gray;
        }
        behaviorBarDash.transform.localScale = new Vector3(dash, 1f, 1f);
        UpdateBarColor(behaviorBarDash.GetComponent<Image>(), dash, isActive);

        isActive = false;
        if(guard >= highestPriority && !isCooldown) {
            textGuard.color = Color.white;
            isActive = true;
        }
        else {
            textGuard.color = Color.gray;
        }
        behaviorBarGuard.transform.localScale = new Vector3(guard, 1f,  1f);
        UpdateBarColor(behaviorBarGuard.GetComponent<Image>(), guard, isActive);

        isActive = false;
        if(bite >= highestPriority && !isCooldown) {
            textBite.color = Color.white;
            isActive = true;
        }
        else {
            textBite.color = Color.gray;
        }
        behaviorBarBite.transform.localScale = new Vector3(bite, 1f, 1f);
        UpdateBarColor(behaviorBarBite.GetComponent<Image>(), bite, isActive);

        isActive = false;
        if(attack >= highestPriority && !isCooldown) {
            textAttack.color = Color.white;
            isActive = true;
        }
        else {
            textAttack.color = Color.gray;
        }
        behaviorBarAttack.transform.localScale = new Vector3(attack, 1f, 1f);
        UpdateBarColor(behaviorBarAttack.GetComponent<Image>(), attack, isActive);

        //isActive = false;
        if(isCooldown) {
            textOther.color = Color.yellow;            
        }
        else {
            textOther.color = Color.gray;
        }
        behaviorBarOther.transform.localScale = new Vector3(other, 1f, 1f);
        UpdateBarColor(behaviorBarOther.GetComponent<Image>(), other, isActive);

    }

    private void UpdateBarColor(Image image, float val, bool active) {

        Color col = Color.red;

        if (val < -0.25f) {
            col = Color.red;
        }
        else if(val > 0.25f) {
            col = Color.green;
        }
        else {
            col = Color.gray;
        }

        if(!active) {
            col *= 0.5f;
        }
        image.color = col;
    }
}
