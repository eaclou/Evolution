using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour {

    public MutationSettings mutationSettingsSupervised;
    public MutationSettings mutationSettingsPersistent;

    public MutationSettings mutationSettingsRandomBody;

    public float maxGlobalNutrients = 5f;
    public float eatRateMultiplier = 1f;
    public float energyDrainMultiplier = 1f;
    public float spawnNewFoodChance = 0.01f;
    public float foodDiffusionRate = 0.0125f;
    public float minSizeFeedingEfficiency = 1f;
    public float maxSizeFeedingEfficiency = 1f;

    public float maxFoodParticleTotalAmount = 512f;
    public float avgFoodParticleRadius = 0.25f;
    public float foodParticleRadiusVariance = 4f;
    public float foodParticleNutrientDensity = 1f;
    public float efficiencyFalloff = 0f;  // how much less food rewarded from particles that are further from optimal size for the consuming Critter

    public float minSizeCritterSpeed = 150f;
    public float maxSizeCritterSpeed = 150f;
    

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Initialize() {
        mutationSettingsSupervised = new MutationSettings(0.5f, 0.015f, 1f, 0.005f, 1f, 0.1f, 0.001f);
        mutationSettingsPersistent = new MutationSettings(0.25f, 0.012f, 1f, 0.0f, 0.9975f, 0.0f, 0.0f);

        mutationSettingsRandomBody = new MutationSettings(0.25f, 0.02f, 1f, 0.0f, 0.999f, 0.0f, 0.0f);
        mutationSettingsRandomBody.defaultBodyMutationChance = 1f;
        mutationSettingsRandomBody.defaultBodyMutationStepSize = 0.4f;
    }
}
