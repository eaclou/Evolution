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

    public GameObject outComm0;
    public GameObject outComm1;
    public GameObject outComm2;
    public GameObject outComm3;

    public GameObject throttleGO;
    public GameObject mouthTriggerGO;
    public GameObject isContactGO;
    public GameObject contactForceGO;

    public GameObject waterDepthGO;
    public GameObject waterVelGO;

    public GameObject food0;
    public GameObject food1;
    public GameObject food2;

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

    public void UpdateExtras(Agent agentRef) {

        waterDepthGO.GetComponent<Text>().text = "Water Depth: " + agentRef.waterDepth.ToString() + 
                                                 "\ncontact:(" + agentRef.coreModule.contactForceX[0].ToString() + ", " + agentRef.coreModule.contactForceY[0].ToString() + ")" +
                                                 "\nmeat eaten: " + (agentRef.totalFoodEatenZoop * 1000f).ToString("F0") + 
                                                 "\nplants eaten: " + (agentRef.totalFoodEatenPlant * 1000f).ToString("F0");




        outComm0.GetComponent<Image>().color = Color.Lerp(Color.black, Color.white, agentRef.communicationModule.outComm0[0]);
        outComm1.GetComponent<Image>().color = Color.Lerp(Color.black, Color.white, agentRef.communicationModule.outComm1[0]);
        outComm2.GetComponent<Image>().color = Color.Lerp(Color.black, Color.white, agentRef.communicationModule.outComm2[0]);
        outComm3.GetComponent<Image>().color = Color.Lerp(Color.black, Color.white, agentRef.communicationModule.outComm3[0]);

        float sigma = Mathf.Atan2(agentRef.movementModule.throttleY[0], agentRef.movementModule.throttleX[0]) * Mathf.Rad2Deg;// Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(agentRef.movementModule.throttleX[0], agentRef.movementModule.throttleY[0], 0.0f)); // agentRef.movementModule.throttleX[0];
        sigma -= 90f;
        throttleGO.transform.rotation = Quaternion.Euler(0f, 0f, sigma);
        throttleGO.transform.localScale = new Vector3(1f, new Vector2(agentRef.movementModule.throttleX[0], agentRef.movementModule.throttleY[0]).magnitude, 1f);

        float sigmaWater = Mathf.Atan2(agentRef.environmentModule.waterVelY[0], agentRef.environmentModule.waterVelX[0]) * Mathf.Rad2Deg;// Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(agentRef.environmentModule.waterVelX[0], agentRef.environmentModule.waterVelY[0], 0.0f)); // agentRef.movementModule.throttleX[0];
        sigmaWater -= 90f;
        waterVelGO.transform.rotation = Quaternion.Euler(0f, 0f, sigmaWater);

        if(agentRef.coreModule.isContact[0] > 0.5f) {
            //float sigmaContact = Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(agentRef.coreModule.contactForceX[0], agentRef.coreModule.contactForceY[0], 0.0f)); // agentRef.movementModule.throttleX[0];
            float sigmaContact = Mathf.Atan2(agentRef.coreModule.contactForceY[0], agentRef.coreModule.contactForceX[0]) * Mathf.Rad2Deg;
            sigmaContact -= 90f;
            contactForceGO.transform.rotation = Quaternion.Euler(0f, 0f, sigmaContact);
            contactForceGO.SetActive(true);
        }
        else {
            contactForceGO.SetActive(false);
        }

        float sigmaFood0 = Mathf.Atan2(agentRef.foodModule.foodPlantDirY[0], agentRef.foodModule.foodPlantDirX[0]) * Mathf.Rad2Deg; // agentRef.movementModule.throttleX[0];
        sigmaFood0 -= 90f;
        food0.transform.rotation = Quaternion.Euler(0f, 0f, sigmaFood0);

        float sigmaFood1 = Mathf.Atan2(agentRef.foodModule.foodAnimalDirY[0], agentRef.foodModule.foodAnimalDirX[0]) * Mathf.Rad2Deg; // agentRef.movementModule.throttleX[0];
        sigmaFood1 -= 90f;
        food1.transform.rotation = Quaternion.Euler(0f, 0f, sigmaFood1);

        float sigmaFood2 = Mathf.Atan2(agentRef.foodModule.foodEggDirY[0], agentRef.foodModule.foodEggDirX[0]) * Mathf.Rad2Deg; // agentRef.movementModule.throttleX[0];
        sigmaFood2 -= 90f;
        food2.transform.rotation = Quaternion.Euler(0f, 0f, sigmaFood2);
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
        behaviorBarRest.transform.localScale = new Vector3(rest * 0.5f + 0.5f, rest * 0.5f + 0.5f,  1f);
        UpdateBarColor(behaviorBarRest.GetComponent<Image>(), rest, isActive);
        
        isActive = false;
        if(dash >= highestPriority && !isCooldown) {
            textDash.color = Color.white;
            isActive = true;
        }
        else {
            textDash.color = Color.gray;
        }
        behaviorBarDash.transform.localScale = Vector3.one * (dash * 0.5f + 0.5f); // new Vector3(dash * 0.5f + 0.5f, dash * 0.5f + 0.5f, 1f);
        UpdateBarColor(behaviorBarDash.GetComponent<Image>(), dash, isActive);

        isActive = false;
        if(guard >= highestPriority && !isCooldown) {
            textGuard.color = Color.white;
            isActive = true;
        }
        else {
            textGuard.color = Color.gray;
        }
        behaviorBarGuard.transform.localScale = Vector3.one * (guard * 0.5f + 0.5f); // new Vector3(guard * 0.5f + 0.5f, guard * 0.5f + 0.5f,  1f);
        UpdateBarColor(behaviorBarGuard.GetComponent<Image>(), guard, isActive);

        isActive = false;
        if(bite >= highestPriority && !isCooldown) {
            textBite.color = Color.white;
            isActive = true;
        }
        else {
            textBite.color = Color.gray;
        }
        behaviorBarBite.transform.localScale = Vector3.one * (bite * 0.5f + 0.5f); // new Vector3(bite * 0.5f + 0.5f, bite * 0.5f + 0.5f, 1f);
        UpdateBarColor(behaviorBarBite.GetComponent<Image>(), bite, isActive);

        isActive = false;
        if(attack >= highestPriority && !isCooldown) {
            textAttack.color = Color.white;
            isActive = true;
        }
        else {
            textAttack.color = Color.gray;
        }
        behaviorBarAttack.transform.localScale = Vector3.one * (attack * 0.5f + 0.5f); // new Vector3(attack * 0.5f + 0.5f, attack * 0.5f + 0.5f, 1f);
        UpdateBarColor(behaviorBarAttack.GetComponent<Image>(), attack, isActive);

        //isActive = false;
        if(isCooldown) {
            textOther.color = Color.yellow;
            textOther.gameObject.SetActive(true);
            throttleGO.gameObject.SetActive(false);
        }
        else {
            textOther.gameObject.SetActive(false);
            throttleGO.gameObject.SetActive(true);
        }
        behaviorBarOther.transform.localScale = Vector3.one * (other * 0.5f + 0.5f); // new Vector3(other, other, 1f);
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
