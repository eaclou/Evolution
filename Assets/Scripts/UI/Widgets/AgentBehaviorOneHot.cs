using System;
using UnityEngine;
using UnityEngine.UI;

// REFACTOR: excessive conditional nesting, expose hardcoded values, 
// inconsistent formatting, shorten reference chains, remove commented-out code

/// Updates UI for selected agent stats
public class AgentBehaviorOneHot : MonoBehaviour 
{
    SelectionManager selectionManager => SelectionManager.instance;
    SelectionData currentSelection => selectionManager.currentSelection;
    Agent agent => currentSelection.agent;
    CandidateAgentData fossil => currentSelection.candidate;

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
    public GameObject contactForceGO;
    public GameObject waterVelGO;

    public GameObject food0;
    public GameObject food1;
    public GameObject food2;
    

    // WPP: Removed GetComponent calls, use nested struct pattern to store references
    public void UpdateExtras(CandidateAgentData candidate) 
    {
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
    
    // WPP: extract method
    public void UpdateBarsForFossil() 
    {
        UpdateBar(behaviorBarRest);
        UpdateBar(behaviorBarDash);
        UpdateBar(behaviorBarGuard);
        UpdateBar(behaviorBarBite);
        UpdateBar(behaviorBarAttack);
        /*behaviorBarRest.transform.localScale = Vector3.one;
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
        behaviorBarAttack.tooltip.tooltipString = "Attack";*/

        throttleGO.gameObject.SetActive(false);
    }
    
    float bite => agent.feedEffector;
    float guard => agent.defendEffector;
    float rest => agent.healEffector;
    float dash => agent.dashEffector;
    float attack => agent.attackEffector; 
    
    float highestPriority;

    // WPP: extract method
    public void UpdateBarsForLiveAgent() 
    {
        highestPriority = Mathf.Max(rest, Mathf.Max(dash, Mathf.Max(guard, Mathf.Max(bite, attack))));
        
        UpdateBar(behaviorBarRest, rest);
        UpdateBar(behaviorBarDash, dash);
        UpdateBar(behaviorBarGuard, guard);
        UpdateBar(behaviorBarBite, bite);
        UpdateBar(behaviorBarAttack, attack);
        /*bool isActive = rest >= highestPriority && !agent.isCooldown;
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
        behaviorBarAttack.tooltip.tooltipString = "Attack: " + attack.ToString("F2");*/

        if (agent.isCooldown) {
            //textOther.color = Color.yellow;
            //textOther.gameObject.SetActive(true);
            //throttleGO.gameObject.SetActive(false);
        }
        else {
            //textOther.gameObject.SetActive(false);
            throttleGO.gameObject.SetActive(true);
        }
    }
    
    //[SerializeField] Color activeColor = Color.white;
    //[SerializeField] Color inactiveColor = Color.clear;
    
    /// For live agent
    void UpdateBar(BehaviorBar bar, float effectorValue)
    {
        bool isActive = effectorValue >= highestPriority && !agent.isCooldown;
        //textRest.color = isActive ? activeColor : inactiveColor;
        bar.transform.localScale = new Vector3(1f, effectorValue * 0.1f + 0.9f,  1f);
        UpdateBarColor(behaviorBarRest.image, effectorValue, isActive);
        bar.SetTooltip(effectorValue);
    }
    
    /// For fossil
    void UpdateBar(BehaviorBar bar)
    {
        bar.transform.localScale = Vector3.one;
        UpdateBarColor(bar.image, 0f, false);
        bar.SetTooltip();
    }
    
    #region Set image bar color by float value
    
    [SerializeField] BarColor[] barColors;

    // WPP: iterate through a lookup data class with exposed threshold values bound to colors
    private void UpdateBarColor(Image image, float effectorValue, bool active) 
    {
        /*Color col = Color.red;

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
        image.color = col;*/
        
        var color = GetBarColor(effectorValue);
        if (!active) color *= 0.5f;
        image.color = color;
    }
    
    public Color GetBarColor(float effectorValue)
    {
        // Editor error handling
        if (barColors.Length <= 0)
        {
            Debug.LogError("Bar color array is empty");
            return Color.gray;
        }
    
        // If below lowest range, return the first value
        if (barColors[0].IsBelowMinimum(effectorValue))
            return barColors[0].color;
    
        // Default case: search through ranges to find matching color
        foreach (var barColor in barColors)
            if (barColor.InRange(effectorValue))
                return barColor.color;
                
        // If above highest range, return the last value
        return barColors[barColors.Length - 1].color;
    }
    
    [Serializable]
    public class BarColor
    {
        public Color color;
        public Vector2 range;
        
        public bool InRange(float value) { return value > range.x && value <= range.y; }
        public bool IsBelowMinimum(float value) { return value < range.x; }
        public bool IsAboveMaximum(float value) { return value > range.y; }
    }
    
    #endregion
    
    [Serializable]
    public class BehaviorBar
    {
        public Image image;
        public TooltipUI tooltip;
        public string tooltipLabel;
        
        public Transform transform => image.transform; 
        
        /// For live agents
        public void SetTooltip(float effectorValue) {
            tooltip.tooltipString = $"{tooltipLabel}: {effectorValue.ToString("F2")}";
        }
        
        /// For dead agents
        public void SetTooltip() {
            tooltip.tooltipString = tooltipLabel;
        }
    }
}