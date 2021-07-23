using Playcraft;

public class SettingsManager : Singleton<SettingsManager> 
{
    //public MutationSettings mutationSettingsSupervised;
    public MutationSettings mutationSettingsVertebrates;
    public MutationSettings mutationSettingsNoneCopy;

    public SettingsEnvironment environmentSettings;
    public SettingsAlgaeParticles algaeSettings;
    public SettingsZooplankton zooplanktonSettings;
    public SettingsAgents agentSettings;
    
    public float maxGlobalNutrients = 5f;
    public float eatRateMultiplier = 1f;
    //public float energyDrainMultiplier = 1f;
    public float spawnNewFoodChance = 0.01f;
    public float nutrientDiffusionRate = 0.00125f;
    public float minSizeFeedingEfficiencyDecay = 1f;
    public float maxSizeFeedingEfficiencyDecay = 1f;
    public float minSizeFeedingEfficiencyPlant = 1f;
    public float maxSizeFeedingEfficiencyPlant = 1f;
    public float minSizeFeedingEfficiencyMeat = 1f;
    public float maxSizeFeedingEfficiencyMeat = 1f;

    public float maxFoodParticleTotalAmount = 512f;
    public float avgAlgaeParticleRadius = 0.37f;
    public float algaeParticleRadiusVariance = 4f;
    public float algaeParticleNutrientDensity = 1f;
    public float foodParticleRegrowthRate = 0.0001f;
    public float efficiencyFalloff = 0f;  // how much less food rewarded from particles that are further from optimal size for the consuming Critter

    public float maxEggFood = 1024f;
    public float eggLayingRate = 1f;

    //private float minBrainMutationRate = 0.0f;
    //private float maxBrainMutationRate = 0.05f;

    //private float minBodyMutationRate = 0.0f;
    //private float maxBodyMutationRate = 0.33f;
    //private float minBodyMutationStepSize = 0.025f;
    //private float maxBodyMutationStepSize = 0.9f;

    public int curTierBodyMutationAmplitude = 3;
    public int curTierBodyMutationFrequency = 4;
    public int curTierBodyMutationModules = 4;

    public int curTierBrainMutationAmplitude = 5;
    public int curTierBrainMutationFrequency = 5;
    public int curTierBrainMutationNewLink = 8;
    public int curTierBrainMutationNewHiddenNeuron = 4;
    public int curTierBrainMutationWeightDecay = 8;
    
    public int curTierFoodDecay = 5;  // these will need to be replacedwith new system
    public int curTierFoodPlant = 5;
    public int curTierFoodEgg = 6;
    public int curTierFoodCorpse = 6;
    

    /*public void ChangeTierBrainMutationAmplitude(int delta) {
        curTierBrainMutationAmplitude = Mathf.Clamp(curTierBrainMutationAmplitude + delta, 0, 10);
        float tierLerp = (float)curTierBrainMutationAmplitude / 10f;        
        mutationSettingsAgents.mutationStepSize = Mathf.Lerp(0.001f, 1.5f, tierLerp * tierLerp);
    }
    public void ChangeTierBrainMutationFrequency(int delta) {
        curTierBrainMutationFrequency = Mathf.Clamp(curTierBrainMutationFrequency + delta, 0, 10);
        float tierLerp = (float)curTierBrainMutationFrequency / 10f;
        mutationSettingsAgents.mutationChance = Mathf.Lerp(0.001f, 0.33f, tierLerp * tierLerp * tierLerp);
    }
    public void ChangeTierBrainMutationNewLink(int delta) {
        curTierBrainMutationNewLink = Mathf.Clamp(curTierBrainMutationNewLink + delta, 0, 10);
        float tierLerp = (float)curTierBrainMutationNewLink / 10f;
        mutationSettingsAgents.newLinkChance = Mathf.Lerp(0.001f, 1.0f, tierLerp * tierLerp);
    }
    public void ChangeTierBrainMutationNewHiddenNeuron(int delta) {
        curTierBrainMutationNewHiddenNeuron = Mathf.Clamp(curTierBrainMutationNewHiddenNeuron + delta, 0, 10);
        float tierLerp = (float)curTierBrainMutationNewHiddenNeuron / 10f;
        mutationSettingsAgents.newHiddenNodeChance = Mathf.Lerp(0f, 0.33f, tierLerp * tierLerp);
    }
    public void ChangeTierBrainMutationWeightDecay(int delta) {
        curTierBrainMutationWeightDecay = Mathf.Clamp(curTierBrainMutationWeightDecay + delta, 0, 10);
        float tierLerp = (float)curTierBrainMutationWeightDecay / 10f;
        mutationSettingsAgents.weightDecayAmount = Mathf.Lerp(0.98f, 1f, tierLerp);
    }

    public void ChangeTierBodyMutationAmplitude(int delta) {
        curTierBodyMutationAmplitude = Mathf.Clamp(curTierBodyMutationAmplitude + delta, 0, 10);
        float tierLerp = (float)curTierBodyMutationAmplitude / 10f;        
        mutationSettingsAgents.defaultBodyMutationStepSize = Mathf.Lerp(0.01f, 0.8f, tierLerp * tierLerp);
    }
    public void ChangeTierBodyMutationFrequency(int delta) {
        curTierBodyMutationFrequency = Mathf.Clamp(curTierBodyMutationFrequency + delta, 0, 10);
        float tierLerp = (float)curTierBodyMutationFrequency / 10f;
        mutationSettingsAgents.defaultBodyMutationChance = Mathf.Lerp(0.001f, 0.75f, tierLerp * tierLerp);
    }
    public void ChangeTierBodyMutationModules(int delta) {
        curTierBodyMutationModules = Mathf.Clamp(curTierBodyMutationModules + delta, 0, 10);
        float tierLerp = (float)curTierBodyMutationModules / 10f;
        mutationSettingsAgents.bodyModuleMutationChance = Mathf.Lerp(0.001f, 0.1f, tierLerp * tierLerp * tierLerp);
    }

    public void ChangeTierFoodDecay(int delta) {
        curTierFoodDecay = Mathf.Clamp(curTierFoodDecay + delta, 0, 10);
        maxGlobalNutrients = Mathf.Pow(2f, (float)(curTierFoodDecay + 2)); // 1 - 1024 // Mathf.Lerp(8f, 512f, lerp);
        float lerp = (float)curTierFoodDecay / 10f;
        spawnNewFoodChance = Mathf.Lerp(0.005f, 0.05f, lerp);
    }
    public void ChangeTierFoodPlant(int delta) {
        curTierFoodPlant = Mathf.Clamp(curTierFoodPlant + delta, 0, 10);
        float lerp = (float)curTierFoodPlant / 10f;
        foodParticleRegrowthRate = Mathf.Lerp(0.001f, 0.05f, lerp * lerp);
        avgAlgaeParticleRadius = Mathf.Lerp(1f, 1.75f, lerp);
        algaeParticleNutrientDensity = Mathf.Lerp(.02f, 0.06f, lerp);
        maxFoodParticleTotalAmount = Mathf.Pow(2f, (float)(curTierFoodPlant + 2)); // 1 - 1024 
    }
    public void ChangeTierFoodEgg(int delta) {
        curTierFoodEgg = Mathf.Clamp(curTierFoodEgg + delta, 0, 10);
        float lerp = (float)curTierFoodEgg / 10f;
    }
    public void ChangeTierFoodCorpse(int delta) {
        curTierFoodCorpse = Mathf.Clamp(curTierFoodCorpse + delta, 0, 10);
        float lerp = (float)curTierFoodCorpse / 10f;
    }
    */
    public void UpdateValuesFromCurTiers() {
        
    }

    public void Initialize() {
        //mutationSettingsSupervised = new MutationSettings(0.5f, 0.015f, 1f, 0.005f, 1f, 0.1f, 0.001f);
        mutationSettingsVertebrates = new MutationSettings(0.5f, 0.5f, 0.05f, 0.025f, 0.995f, 0.95f, 0.1f, 1f);
        mutationSettingsNoneCopy = new MutationSettings();
/*
        ChangeTierFoodDecay(0);
        ChangeTierFoodPlant(0);
        ChangeTierFoodEgg(0);
        ChangeTierFoodCorpse(0);

        ChangeTierBrainMutationAmplitude(0);
        ChangeTierBrainMutationFrequency(0);
        ChangeTierBrainMutationNewLink(0);
        ChangeTierBrainMutationNewHiddenNeuron(0);
        ChangeTierBrainMutationWeightDecay(0);

        ChangeTierBodyMutationAmplitude(0);
        ChangeTierBodyMutationFrequency(0);
        ChangeTierBodyMutationModules(0);
        */
        //mutationSettingsRandomBody = new MutationSettings(0.25f, 0.02f, 1f, 0.0f, 0.999f, 0.0f, 0.0f);
        //mutationSettingsRandomBody.defaultBodyMutationChance = 0.05f;
        //mutationSettingsRandomBody.defaultBodyMutationStepSize = 0.535f;
    }
}
