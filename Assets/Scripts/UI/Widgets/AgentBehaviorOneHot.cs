using System;
using UnityEngine;
using UnityEngine.UI;

// REFACTOR: excessive conditional nesting, expose hardcoded values, 
// inconsistent formatting, shorten reference chains, remove commented-out code

/// Updates UI for selected agent stats
public class AgentBehaviorOneHot : MonoBehaviour 
{
    public BehaviorBar behaviorBarRest;
    public BehaviorBar behaviorBarDash;
    public BehaviorBar behaviorBarGuard;
    public BehaviorBar behaviorBarBite;
    public BehaviorBar behaviorBarAttack;
    public BehaviorBar behaviorBarOther;

    public BehaviorBar outComm0;
    public BehaviorBar outComm1;
    public BehaviorBar outComm2;
    public BehaviorBar outComm3;

    public GameObject throttleGO;
    //public GameObject mouthTriggerGO;
    //public GameObject isContactGO;
    public GameObject contactForceGO;

    //public GameObject waterDepthGO;
    public GameObject waterVelGO;

    public GameObject food0;
    public GameObject food1;
    public GameObject food2;
    

    // WPP: Removed GetComponent calls, use nested struct pattern to store references
    public void UpdateExtras(CandidateAgentData candidate) {
        //textRest.gameObject.SetActive(false);
        //textDash.gameObject.SetActive(false);
        //textGuard.gameObject.SetActive(false);
        //textBite.gameObject.SetActive(false);
        //textAttack.gameObject.SetActive(false);
        //textOther.gameObject.SetActive(false);
        outComm0.image.color = Color.Lerp(Color.black, Color.white, 0f);
        outComm0.tooltip.tooltipString = "OutComm0";
        outComm1.image.color = Color.Lerp(Color.black, Color.white, 0f);
        outComm1.tooltip.tooltipString = "OutComm1";
        outComm2.image.color = Color.Lerp(Color.black, Color.white, 0f);
        outComm2.tooltip.tooltipString = "OutComm2";
        outComm3.image.color = Color.Lerp(Color.black, Color.white, 0f);
        outComm3.tooltip.tooltipString = "OutComm3";
        
        float sigma = 0f;
        throttleGO.transform.rotation = Quaternion.Euler(0f, 0f, sigma);
        throttleGO.transform.localScale = Vector3.zero;

        float sigmaWater = 0f;
        waterVelGO.transform.rotation = Quaternion.Euler(0f, 0f, sigmaWater);
        waterVelGO.transform.localScale = Vector3.zero;

        contactForceGO.SetActive(false);
        
        float sigmaFood0 = 0f;
        food0.transform.rotation = Quaternion.Euler(0f, 0f, sigmaFood0);

        float sigmaFood1 = 0f;
        food1.transform.rotation = Quaternion.Euler(0f, 0f, sigmaFood1);

        float sigmaFood2 = 0f;
        food2.transform.rotation = Quaternion.Euler(0f, 0f, sigmaFood2);
    }
    
    public void UpdateExtras(Agent agentRef) 
    {
        /*
        waterDepthGO.GetComponent<Text>().text = "Water Depth: " + agentRef.waterDepth.ToString() + 
                                                 "\ncontact:(" + agentRef.coreModule.contactForceX[0].ToString() + ", " + agentRef.coreModule.contactForceY[0].ToString() + ")" +
                                                 "\nmeat eaten: " + (agentRef.candidateRef.performanceData.totalFoodEatenZoop * 1000f).ToString("F0") + 
                                                 "\nplants eaten: " + (agentRef.candidateRef.performanceData.totalFoodEatenPlant * 1000f).ToString("F0");
       */
        
        outComm0.image.color = Color.Lerp(Color.black, Color.white, agentRef.communicationModule.outComm0[0]);
        outComm0.tooltip.tooltipString = "OutComm0: " + agentRef.communicationModule.outComm0[0].ToString("F2");
        outComm1.image.color = Color.Lerp(Color.black, Color.white, agentRef.communicationModule.outComm1[0]);
        outComm1.tooltip.tooltipString = "OutComm1: " + agentRef.communicationModule.outComm1[0].ToString("F2");
        outComm2.image.color = Color.Lerp(Color.black, Color.white, agentRef.communicationModule.outComm2[0]);
        outComm2.tooltip.tooltipString = "OutComm2: " + agentRef.communicationModule.outComm2[0].ToString("F2");
        outComm3.image.color = Color.Lerp(Color.black, Color.white, agentRef.communicationModule.outComm3[0]);
        outComm3.tooltip.tooltipString = "OutComm3: " + agentRef.communicationModule.outComm3[0].ToString("F2");

        if (!agentRef.candidateRef.candidateGenome.bodyGenome.data.hasComms) {
            outComm0.tooltip.tooltipString = "OutComms (disabled)";
            outComm1.tooltip.tooltipString = "OutComms (disabled)";
            outComm2.tooltip.tooltipString = "OutComms (disabled)";
            outComm3.tooltip.tooltipString = "OutComms (disabled)";
        }
        
        float sigma = Mathf.Atan2(agentRef.movementModule.throttleY[0], agentRef.movementModule.throttleX[0]) * Mathf.Rad2Deg;// Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(agentRef.movementModule.throttleX[0], agentRef.movementModule.throttleY[0], 0.0f)); // agentRef.movementModule.throttleX[0];
        sigma -= 90f;
        throttleGO.transform.rotation = Quaternion.Euler(0f, 0f, sigma);
        throttleGO.transform.localScale = new Vector3(1f, agentRef.movementModule.throttle.magnitude, 1f);

        float sigmaWater = Mathf.Atan2(agentRef.environmentModule.waterVelY[0], agentRef.environmentModule.waterVelX[0]) * Mathf.Rad2Deg;// Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(agentRef.environmentModule.waterVelX[0], agentRef.environmentModule.waterVelY[0], 0.0f)); // agentRef.movementModule.throttleX[0];
        sigmaWater -= 90f;
        waterVelGO.transform.rotation = Quaternion.Euler(0f, 0f, sigmaWater);
        waterVelGO.transform.localScale = new Vector3(1f, Mathf.Clamp01(new Vector2(agentRef.environmentModule.waterVelX[0], agentRef.environmentModule.waterVelY[0]).magnitude * 50f), 1f);

        if (agentRef.coreModule.isContact[0] > 0.5f) {
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
    
    public void UpdateBars(CandidateAgentData candidate) 
    {
        Color activeColor = Color.white;
        Color inactiveColor = Color.clear;

        behaviorBarRest.transform.localScale = Vector3.one;
        UpdateBarColor(behaviorBarRest.image, 0f, false);
        behaviorBarRest.tooltip.tooltipString = "Rest";

        behaviorBarDash.transform.localScale = Vector3.one;
        UpdateBarColor(behaviorBarDash.image, 0f, false);
        behaviorBarDash.tooltip.tooltipString = "Dash";

        behaviorBarGuard.transform.localScale = Vector3.one; 
        UpdateBarColor(behaviorBarGuard.image, 0f, false);
        behaviorBarGuard.tooltip.tooltipString = "Guard";

        behaviorBarBite.transform.localScale = Vector3.one; 
        UpdateBarColor(behaviorBarBite.image, 0f, false);
        behaviorBarBite.tooltip.tooltipString = "Bite";

        behaviorBarAttack.transform.localScale = Vector3.one;
        UpdateBarColor(behaviorBarAttack.image, 0f, false);
        behaviorBarAttack.tooltip.tooltipString = "Attack";

        //UpdateBarColor(behaviorBarOther.GetComponent<Image>(), 0f, false);

        //textOther.gameObject.SetActive(false);
        throttleGO.gameObject.SetActive(false);
    }
    
    public void UpdateBars(Agent agent) 
    {
        float bite = agent.feedEffector;
        float guard = agent.defendEffector;
        float rest = agent.healEffector;
        float dash = agent.dashEffector;
        float attack = agent.attackEffector;
        float highestPriority = Mathf.Max(rest, Mathf.Max(dash, Mathf.Max(guard, Mathf.Max(bite, attack))));

        Color activeColor = Color.white;
        Color inactiveColor = Color.clear;

        bool isActive = rest >= highestPriority && !agent.isCooldown;
        //textRest.color = isActive ? activeColor : inactiveColor;

        behaviorBarRest.transform.localScale = new Vector3(rest * 0.1f + 0.9f, rest * 0.1f + 0.9f,  1f);
        UpdateBarColor(behaviorBarRest.image, rest, isActive);
        behaviorBarRest.tooltip.tooltipString = "Rest: " + rest.ToString("F2");
        
        isActive = dash >= highestPriority && !agent.isCooldown;
        //textDash.color = isActive ? activeColor : inactiveColor;

        behaviorBarDash.transform.localScale = Vector3.one * (dash * 0.1f + 0.9f); // new Vector3(dash * 0.5f + 0.5f, dash * 0.5f + 0.5f, 1f);
        UpdateBarColor(behaviorBarDash.image, dash, isActive);
        behaviorBarDash.tooltip.tooltipString = "Dash: " + dash.ToString("F2");

        isActive = guard >= highestPriority && !agent.isCooldown;
        //textGuard.color = isActive ? activeColor : inactiveColor;

        behaviorBarGuard.transform.localScale = Vector3.one * (guard * 0.1f + 0.9f); // new Vector3(guard * 0.5f + 0.5f, guard * 0.5f + 0.5f,  1f);
        UpdateBarColor(behaviorBarGuard.image, guard, isActive);
        behaviorBarGuard.tooltip.tooltipString = "Guard: " + guard.ToString("F2");

        isActive = bite >= highestPriority && !agent.isCooldown;
        //textBite.color = isActive ? activeColor : inactiveColor;

        behaviorBarBite.transform.localScale = Vector3.one * (bite * 0.1f + 0.9f); // new Vector3(bite * 0.5f + 0.5f, bite * 0.5f + 0.5f, 1f);
        UpdateBarColor(behaviorBarBite.image, bite, isActive);
        behaviorBarBite.tooltip.tooltipString = "Bite: " + bite.ToString("F2");

        isActive = attack >= highestPriority && !agent.isCooldown;
        //textAttack.color = isActive ? activeColor : inactiveColor;

        behaviorBarAttack.transform.localScale = Vector3.one * (attack * 0.1f + 0.9f); // new Vector3(attack * 0.5f + 0.5f, attack * 0.5f + 0.5f, 1f);
        UpdateBarColor(behaviorBarAttack.image, attack, isActive);
        behaviorBarAttack.tooltip.tooltipString = "Attack: " + attack.ToString("F2");

        //isActive = false;
        if (agent.isCooldown) {
            //textOther.color = Color.yellow;
            //textOther.gameObject.SetActive(true);
            //throttleGO.gameObject.SetActive(false);
        }
        else {
            //textOther.gameObject.SetActive(false);
            throttleGO.gameObject.SetActive(true);
        }
        //behaviorBarOther.transform.localScale = Vector3.one * (other * 0.5f + 0.5f);
        //UpdateBarColor(behaviorBarOther.GetComponent<Image>(), agent.coreModule.healEffector[0], isActive);
    }

    // * WPP: iterate through a lookup data class with exposed threshold values bound to colors
    private void UpdateBarColor(Image image, float val, bool active) 
    {
        Color col = Color.red;

        if (val < -0.25f) {
            col = Color.red;
        }
        else if (val > 0.25f) {
            col = Color.green;
        }
        else {
            col = Color.gray;
        }

        if (!active) {
            col *= 0.5f;
        }
        col.a = 1f;
        image.color = col;
    }
}

[Serializable]
public class BehaviorBar
{
    public Image image;
    public TooltipUI tooltip;
    
    public Transform transform => image.transform;
}