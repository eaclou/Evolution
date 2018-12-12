using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour {

    //public MutationSettings mutationSettingsSupervised;
    public MutationSettings mutationSettingsPersistent;

    //public MutationSettings mutationSettingsRandomBody;

    public float maxGlobalNutrients = 5f;
    public float eatRateMultiplier = 1f;
    public float energyDrainMultiplier = 0.2f;
    public float spawnNewFoodChance = 0.01f;
    public float foodDiffusionRate = 0.0125f;
    public float minSizeFeedingEfficiencyDecay = 1f;
    public float maxSizeFeedingEfficiencyDecay = 1f;
    public float minSizeFeedingEfficiencyPlant = 1f;
    public float maxSizeFeedingEfficiencyPlant = 1f;
    public float minSizeFeedingEfficiencyMeat = 1f;
    public float maxSizeFeedingEfficiencyMeat = 1f;

    public float maxFoodParticleTotalAmount = 512f;
    public float avgFoodParticleRadius = 0.25f;
    public float foodParticleRadiusVariance = 4f;
    public float foodParticleNutrientDensity = 1f;
    public float foodParticleRegrowthRate = 0.0001f;
    public float efficiencyFalloff = 0f;  // how much less food rewarded from particles that are further from optimal size for the consuming Critter

    public float maxEggFood = 1024f;
    public float eggLayingRate = 1f;

    private float minBrainMutationRate = 0.0f;
    private float maxBrainMutationRate = 0.05f;

    private float minBodyMutationRate = 0.0f;
    private float maxBodyMutationRate = 0.33f;
    private float minBodyMutationStepSize = 0.025f;
    private float maxBodyMutationStepSize = 0.9f;

    public int curTierBodyMutation = 4;
    public int curTierBrainMutation = 4;
    public int curTierFoodDecay = 6;
    public int curTierFoodPlant = 4;
    public int curTierFoodEgg = 4;
    public int curTierFoodCorpse = 4;

    
    
    //public float minSizeCritterSpeed = 150f;
    //public float maxSizeCritterSpeed = 150f;
    public void SetGlobalMutationRate(float normalizedVal) {
        mutationSettingsPersistent.defaultBodyMutationChance = Mathf.Lerp(minBodyMutationRate, maxBodyMutationRate, normalizedVal * normalizedVal);
        mutationSettingsPersistent.defaultBodyMutationStepSize = Mathf.Lerp(minBodyMutationStepSize, maxBodyMutationStepSize, normalizedVal);
        Debug.Log("mutateRate: " + mutationSettingsPersistent.defaultBodyMutationChance.ToString());
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ChangeTierBrainMutation(int delta) {
        curTierBrainMutation = Mathf.Clamp(curTierBrainMutation + delta, 0, 10);
        float brainMutationLerp = (float)curTierBrainMutation / 10f;
        mutationSettingsPersistent.mutationChance = Mathf.Lerp(0.001f, 0.06f, brainMutationLerp);
    }
    public void ChangeTierBodyMutation(int delta) {
        curTierBodyMutation = Mathf.Clamp(curTierBodyMutation + delta, 0, 10);
        float bodyMutationLerp = (float)curTierBodyMutation / 10f;
        mutationSettingsPersistent.defaultBodyMutationChance = Mathf.Lerp(0.01f, 0.5f, bodyMutationLerp);
    }
    public void ChangeTierFoodDecay(int delta) {
        curTierFoodDecay = Mathf.Clamp(curTierFoodDecay + delta, 0, 10);
        maxGlobalNutrients = Mathf.Pow(2f, curTierFoodDecay); // 1 - 1024 // Mathf.Lerp(8f, 512f, lerp);
        float lerp = (float)curTierFoodDecay / 10f;
        spawnNewFoodChance = Mathf.Lerp(0.005f, 0.04f, lerp);
    }
    public void ChangeTierFoodPlant(int delta) {
        curTierFoodPlant = Mathf.Clamp(curTierFoodPlant + delta, 0, 10);
        float lerp = (float)curTierFoodPlant / 10f;
        foodParticleRegrowthRate = Mathf.Lerp(0.001f, 0.01f, lerp);
        maxFoodParticleTotalAmount = Mathf.Pow(2f, curTierFoodPlant); // 1 - 1024 
    }
    public void UpdateValuesFromCurTiers() {
        

        
    }

    public void Initialize() {
        //mutationSettingsSupervised = new MutationSettings(0.5f, 0.015f, 1f, 0.005f, 1f, 0.1f, 0.001f);
        mutationSettingsPersistent = new MutationSettings(0.25f, 0.025f, 1f, 0.01f, 0.997f, 0.6f, 0.01f);

        //mutationSettingsRandomBody = new MutationSettings(0.25f, 0.02f, 1f, 0.0f, 0.999f, 0.0f, 0.0f);
        //mutationSettingsRandomBody.defaultBodyMutationChance = 0.05f;
        //mutationSettingsRandomBody.defaultBodyMutationStepSize = 0.535f;
    }
}
