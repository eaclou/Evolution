using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatsHistory
{
    // 0 == decay nutrients  .x
    // 1 == plant food  .y
    // 2 == eggs food  .z
    // 3 = corpse food  .w
    // 4 = brain mutation freq
    // 5 = brain mutation amp
    // 6 = brain size bias
    // 7 = body proportion freq
    // 8 = body proportion amp
    // 9 = body sensor mutation rate
    // 10 = water current / storminess
        
    public List<Vector4> nutrientsEachGenerationList;
    public List<float> brainMutationFreqList;
    public List<float> brainMutationAmpList;
    public List<float> brainSizeBiasList;
    public List<float> bodyMutationFreqList;
    public List<float> bodyMutationAmpList;
    public List<float> bodySensorVarianceList;
    public List<float> waterCurrentsList;

    public List<float> oxygenList;
    public List<float> nutrientsList;
    public List<float> detritusList;
    public List<float> decomposersList;
    public List<float> algaeSingleList;
    public List<float> algaeParticleList;
    public List<float> zooplanktonList;
    public List<float> livingAgentsList;
    public List<float> deadAgentsList;
    public List<float> eggSacksList;
    
    SimResourceManager resources;
    SettingsManager settings;
    EnvironmentFluidManager fluid;
    
    public StatsHistory(SimResourceManager resources, SettingsManager settings, EnvironmentFluidManager fluid)
    {
        this.resources = resources;
        this.settings = settings;
        this.fluid = fluid;
    }
    
    public void Initialize()
    {
        nutrientsEachGenerationList = new List<Vector4>{Vector4.one * 0.0001f};
        brainMutationFreqList = new List<float>{0f};
        brainMutationAmpList = new List<float>{0f};
        brainSizeBiasList = new List<float>{0f};
        bodyMutationFreqList = new List<float>{0f};
        bodyMutationAmpList = new List<float>{0f};
        bodySensorVarianceList = new List<float>{0f};
        waterCurrentsList = new List<float>{0f};

        oxygenList = new List<float>{0f};
        nutrientsList = new List<float>{0f};
        detritusList = new List<float>{0f};
        decomposersList = new List<float>{0f};
        algaeSingleList = new List<float>{0f};
        algaeParticleList = new List<float>{0f};
        zooplanktonList = new List<float>{0f};
        livingAgentsList = new List<float>{0f};
        deadAgentsList = new List<float>{0f};
        eggSacksList = new List<float>{0f};
    }
    
    public void AddNewHistoricalDataEntry() 
    {
        nutrientsEachGenerationList.Add(new Vector4(resources.curGlobalAlgaeReservoir, resources.curGlobalPlantParticles, resources.curGlobalEggSackVolume, resources.curGlobalCarrionVolume));
        brainMutationFreqList.Add(settings.curTierBrainMutationFrequency);
        brainMutationAmpList.Add(settings.curTierBrainMutationAmplitude);
        brainSizeBiasList.Add(settings.curTierBrainMutationNewLink);
        bodyMutationFreqList.Add(settings.curTierBodyMutationFrequency);
        bodyMutationAmpList.Add(settings.curTierBodyMutationAmplitude);
        bodySensorVarianceList.Add(settings.curTierBodyMutationModules);
        waterCurrentsList.Add(fluid.curTierWaterCurrents);

        oxygenList.Add(resources.curGlobalOxygen);
        nutrientsList.Add(resources.curGlobalNutrients);
        detritusList.Add(resources.curGlobalDetritus);
        decomposersList.Add(resources.curGlobalDecomposers);
        algaeSingleList.Add(resources.curGlobalAlgaeReservoir);
        algaeParticleList.Add(resources.curGlobalPlantParticles);
        zooplanktonList.Add(resources.curGlobalAnimalParticles);
        livingAgentsList.Add(resources.curGlobalAgentBiomass);
        deadAgentsList.Add(resources.curGlobalCarrionVolume);
        eggSacksList.Add(resources.curGlobalEggSackVolume);
    }
    
    private void RefreshLatestHistoricalDataEntry() 
    {
        nutrientsEachGenerationList[nutrientsEachGenerationList.Count - 1] = new Vector4(resources.curGlobalAlgaeReservoir, resources.curGlobalPlantParticles, resources.curGlobalEggSackVolume, resources.curGlobalCarrionVolume);
        brainMutationFreqList[brainMutationFreqList.Count - 1] = settings.curTierBrainMutationFrequency;
        brainMutationAmpList[brainMutationAmpList.Count - 1] = settings.curTierBrainMutationAmplitude;
        brainSizeBiasList[brainSizeBiasList.Count - 1] = settings.curTierBrainMutationNewLink;
        bodyMutationFreqList[bodyMutationFreqList.Count - 1] = settings.curTierBodyMutationFrequency;
        bodyMutationAmpList[bodyMutationAmpList.Count - 1] = settings.curTierBodyMutationAmplitude;
        bodySensorVarianceList[bodySensorVarianceList.Count - 1] = settings.curTierBodyMutationModules;
        waterCurrentsList[waterCurrentsList.Count - 1] = fluid.curTierWaterCurrents;

        // [Error?] Why are all of these lists using the oxygen list count?
        oxygenList[oxygenList.Count - 1] = resources.curGlobalOxygen;
        nutrientsList[oxygenList.Count - 1] = resources.curGlobalNutrients;
        detritusList[oxygenList.Count - 1] = resources.curGlobalDetritus;
        decomposersList[oxygenList.Count - 1] = resources.curGlobalDecomposers;
        algaeSingleList[oxygenList.Count - 1] = resources.curGlobalAlgaeReservoir;
        algaeParticleList[oxygenList.Count - 1] = resources.curGlobalPlantParticles;
        zooplanktonList[oxygenList.Count - 1] = resources.curGlobalAnimalParticles;
        livingAgentsList[oxygenList.Count - 1] = resources.curGlobalAgentBiomass;
        deadAgentsList[oxygenList.Count - 1] = resources.curGlobalCarrionVolume;
        eggSacksList[oxygenList.Count - 1] = resources.curGlobalEggSackVolume;
    }
}
