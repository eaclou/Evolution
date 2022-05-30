using System;
using UnityEngine;
using UnityEngine.UI;

/// Updates UI for selected agent stats
public class AgentBehaviorOneHot : MonoBehaviour 
{
    SelectionManager selectionManager => SelectionManager.instance;
    SelectionData currentSelection => selectionManager.currentSelection;
    Agent agent => currentSelection.agent;
    CandidateAgentData fossil => currentSelection.candidate;
    
    public BehaviorBar[] behaviorBars;
    public BehaviorBar[] communicationBars;

    public GameObject throttleGO;
    public GameObject contactForceGO;
    public GameObject waterVelGO;

    public GameObject plantFood;
    public GameObject animalFood;
    public GameObject eggFood;
    
    [Range(0, 1)] [SerializeField] float outCommLightnessOnDeath = 0f;
    
    public void UpdateExtrasOnDeath() 
    {
        foreach (var bar in communicationBars)
        {
            bar.SetImageGrayscale(outCommLightnessOnDeath);
            bar.SetTooltip();
        }
        
        contactForceGO.SetActive(false);
        
        throttleGO.transform.rotation = Quaternion.identity;;
        throttleGO.transform.localScale = Vector3.zero;
        
        waterVelGO.transform.rotation = Quaternion.identity;
        waterVelGO.transform.localScale = Vector3.zero;
        
        plantFood.transform.rotation = Quaternion.identity;
        animalFood.transform.rotation = Quaternion.identity;
        eggFood.transform.rotation = Quaternion.identity;
    }
    
    public void UpdateExtras(Agent agentRef) 
    {
        for (int i = 0; i < communicationBars.Length; i++)
        {
            var commValue = agentRef.communicationModule.GetOutChannelValue(i);
            communicationBars[i].SetImageGrayscale(commValue);
            communicationBars[i].SetTooltip(commValue);
        }

        if (!agentRef.candidateRef.candidateGenome.bodyGenome.data.hasComms)
            foreach (var bar in communicationBars)
                bar.SetTooltip("OutComms (disable)");

        //float sigma = Mathf.Atan2(agentRef.movementModule.throttleY[0], agentRef.movementModule.throttleX[0]) * Mathf.Rad2Deg;// Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(agentRef.movementModule.throttleX[0], agentRef.movementModule.throttleY[0], 0.0f)); // agentRef.movementModule.throttleX[0];
        //sigma -= 90f;
        //throttleGO.transform.rotation = Quaternion.Euler(0f, 0f, sigma);
        ApplySigmaRotation(throttleGO, agentRef.movementModule.throttleX[0], agentRef.movementModule.throttleY[0]);
        throttleGO.transform.localScale = new Vector3(1f, agentRef.movementModule.throttle.magnitude, 1f);

        //float sigmaWater = Mathf.Atan2(agentRef.environmentModule.waterVelY[0], agentRef.environmentModule.waterVelX[0]) * Mathf.Rad2Deg;// Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(agentRef.environmentModule.waterVelX[0], agentRef.environmentModule.waterVelY[0], 0.0f)); // agentRef.movementModule.throttleX[0];
        //sigmaWater -= 90f;
        //waterVelGO.transform.rotation = Quaternion.Euler(0f, 0f, sigmaWater);
        ApplySigmaRotation(waterVelGO, agentRef.environmentModule.waterVelX[0], agentRef.environmentModule.waterVelY[0]);
        waterVelGO.transform.localScale = new Vector3(1f, Mathf.Clamp01(new Vector2(agentRef.environmentModule.waterVelX[0], agentRef.environmentModule.waterVelY[0]).magnitude * 50f), 1f);

        var isContact = agentRef.coreModule.isContact[0] > 0.5f;
        if (isContact) {
            //float sigmaContact = Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(agentRef.coreModule.contactForceX[0], agentRef.coreModule.contactForceY[0], 0.0f)); // agentRef.movementModule.throttleX[0];
            //float sigmaContact = Mathf.Atan2(agentRef.coreModule.contactForceY[0], agentRef.coreModule.contactForceX[0]) * Mathf.Rad2Deg;
            //sigmaContact -= 90f;
            //contactForceGO.transform.rotation = Quaternion.Euler(0f, 0f, sigmaContact);
            ApplySigmaRotation(contactForceGO, agentRef.coreModule.contactForceX[0], agentRef.coreModule.contactForceY[0]);
        }
        contactForceGO.SetActive(isContact);
        
        ApplySigmaRotation(plantFood, agentRef.foodModule.foodPlantDirX[0], agentRef.foodModule.foodPlantDirY[0]);
        ApplySigmaRotation(animalFood, agentRef.foodModule.foodAnimalDirX[0], agentRef.foodModule.foodAnimalDirY[0]);
        ApplySigmaRotation(eggFood, agentRef.foodModule.foodEggDirX[0], agentRef.foodModule.foodEggDirY[0]);
    }
    
    void ApplySigmaRotation(GameObject obj, float xDirection, float yDirection)
    {
        float sigma = Mathf.Atan2(yDirection, xDirection) * Mathf.Rad2Deg - 90f;
        obj.transform.rotation = Quaternion.Euler(0f, 0f, sigma);
    }

    public void UpdateBarsForLiveAgent() 
    {
        float highestPriority = agent.GetMostActiveEffectorValue();

        foreach (var bar in behaviorBars)
            UpdateBar(bar, GetEffectorValueForBehavior(bar.type), highestPriority);

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
    
    // * Consider moving to Agent if this functionality is useful elsewhere
    float GetEffectorValueForBehavior(BehaviorType behavior)
    {
        switch (behavior)
        {
            case BehaviorType.Attack: return agent.attackEffector;
            case BehaviorType.Bite: return agent.feedEffector;
            case BehaviorType.Dash: return agent.dashEffector;
            case BehaviorType.Guard: return agent.defendCooldown;
            case BehaviorType.Rest: return agent.healEffector;
            default: return 0f;
        }
    }

    /// For live agent
    void UpdateBar(BehaviorBar bar, float effectorValue, float highestPriority)
    {
        bool isActive = effectorValue >= highestPriority && !agent.isCooldown;
        //textRest.color = isActive ? activeColor : inactiveColor;
        bar.transform.localScale = new Vector3(1f, effectorValue * 0.1f + 0.9f,  1f);
        UpdateBarColor(bar.image, effectorValue, isActive);
        bar.SetTooltip(effectorValue);
    }
    
    public void UpdateBarsForFossil() 
    {
        foreach (var bar in behaviorBars)
            UpdateBar(bar);

        throttleGO.gameObject.SetActive(false);
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

    private void UpdateBarColor(Image image, float effectorValue, bool active) 
    {
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
        public BehaviorType type;
        public Image image;
        public TooltipUI tooltip;
        public string tooltipLabel;
        
        public Transform transform => image.transform;

        /// For live agents
        public void SetTooltip(float effectorValue) {
            SetTooltip($"{tooltipLabel}: {effectorValue.ToString("F2")}");
        }
        
        /// For dead agents
        public void SetTooltip() {
            SetTooltip(tooltipLabel);
        }
        
        public void SetTooltip(string value) {
            tooltip.tooltipString = value;
        }
        
        public void SetImageGrayscale(float value) {
            image.color = Color.Lerp(Color.black, Color.white, value);
        }
    }
    
    public enum BehaviorType 
    {
        Rest,
        Dash,
        Guard,
        Bite,
        Attack,
        Other,
    }
}