using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimResourceManager {
    SettingsManager settings => SettingsManager.instance;

    //public float baseSolarEnergy = 1f;
    public float curGlobalOxygen = 1f;
    public float curGlobalNutrients = 1f;
    public float curGlobalDetritus = 1f;
    public float curGlobalDecomposers = 1f;
    public float curGlobalAlgaeReservoir = 0f;  // separate from algaeParticles -- takes place of algaeGrid
    public float curGlobalPlantParticles = 0f;
    public float curGlobalAnimalParticles = 0f;
    public float curGlobalEggSackVolume = 0f;    
    public float curGlobalAgentBiomass = 0f;
    public float curGlobalCarrionVolume = 0f;

    public float curTotalMass = 0f;

    public float oxygenUsedByAnimalParticlesLastFrame = 0f;
    public float wasteProducedByAnimalParticlesLastFrame = 0f;
    public float algaeConsumedByAnimalParticlesLastFrame = 0f;

    public float oxygenUsedByAgentsLastFrame = 0f;
    public float wasteProducedByAgentsLastFrame = 0f;
    public float algaeConsumedByAgentsLastFrame = 0f;
    public float zooplanktonConsumedByAgentsLastFrame = 0f;
    public float eggsConsumedByAgentsLastFrame = 0f;
    public float carrionConsumedByAgentsLastFrame = 0f;
    public float agentsConsumedByAgentsLastFrame = 0f;

    public float oxygenProducedByAlgaeReservoirLastFrame = 0f;
    public float wasteProducedByAlgaeReservoirLastFrame = 0f;
    public float nutrientsUsedByAlgaeReservoirLastFrame = 0f;
    
    public float oxygenProducedByPlantParticlesLastFrame = 0f;
    public float wasteProducedByPlantParticlesLastFrame = 0f;
    public float nutrientsUsedByPlantParticlesLastFrame = 0f;

    public float oxygenUsedByDecomposersLastFrame = 0f;
    public float nutrientsProducedByDecomposersLastFrame = 0f;
    public float detritusRemovedByDecomposersLastFrame = 0f;

    public SimResource[] simResourcesArray;
    //public List<SpeciesDataPoint> resourceDataListAlgae;
    //public List<SpeciesDataPoint> resourceDataListPlants;
    //public List<SpeciesDataPoint> resourceDataListMicrobes;
    //public List<SpeciesDataPoint> resourceDataListNutrients;
    //public List<SpeciesDataPoint> resourceDataListWaste;
    //public List<SpeciesDataPoint> resourceDataListDecomposers;
    //public List<SpeciesDataPoint> resourceDataListAnimals;
    int maxNumDataPointEntries = 128;
    
	public SimResourceManager() {
        // constructor
        simResourcesArray = new SimResource[7];
        simResourcesArray[0] = new SimResource("NutrientGrid", maxNumDataPointEntries, Color.yellow);
        simResourcesArray[1] = new SimResource("AlgaeGrid", maxNumDataPointEntries, Color.cyan);
        simResourcesArray[2] = new SimResource("DecomposerGrid", maxNumDataPointEntries, new Color(1f, 0.7f, 0.3f));
        simResourcesArray[3] = new SimResource("WasteGrid", maxNumDataPointEntries, new Color(0.5f, 0.5f, 0.5f));
        simResourcesArray[4] = new SimResource("Plants", maxNumDataPointEntries, new Color(0.2f, 1f, 0.3f));
        simResourcesArray[5] = new SimResource("Microbes", maxNumDataPointEntries, new Color(0.931f, 0.1f, 1f));
        simResourcesArray[6] = new SimResource("Animals", maxNumDataPointEntries, new Color(1f, 0.6f, 0.87f));
        // TEMP:::::
        curGlobalOxygen = 100f;
        curGlobalNutrients = 0f;
        curGlobalDecomposers = 0f;
        curGlobalDetritus = 0f;
        
    }

    public ResourceDataPoint GetResourceDataAtTime(SimResource resource, float t) {
        //SimResource[] resourceValuesArray = new SimResource[simResourcesArray.Length];
        ResourceDataPoint data = resource.GetDataPointAtTime(t); // ANIMALS=6

        return data;
    }

    
    public void AddNewResourcesAll(int timestep) {
        simResourcesArray[0].AddNewResourceDataEntry(timestep, curGlobalNutrients);
        simResourcesArray[1].AddNewResourceDataEntry(timestep, curGlobalAlgaeReservoir);
        simResourcesArray[2].AddNewResourceDataEntry(timestep, curGlobalDetritus);
        simResourcesArray[3].AddNewResourceDataEntry(timestep, curGlobalDecomposers);
        simResourcesArray[4].AddNewResourceDataEntry(timestep, curGlobalPlantParticles);
        simResourcesArray[5].AddNewResourceDataEntry(timestep, curGlobalAnimalParticles);
        simResourcesArray[6].AddNewResourceDataEntry(timestep, curGlobalAgentBiomass);

    }
    

    public void Tick(TrophicLayersManager trophicLayersManager, VegetationManager veggieManager) 
    {
        float nutrientsProduced = 0f;
        float decomposersTotalProductivity = 0f;
        if(trophicLayersManager.IsLayerOn(KnowledgeMapId.Decomposers)) {
            float decomposersOxygenMask = Mathf.Clamp01(curGlobalOxygen * settings.environmentSettings._DecomposersOxygenMask);
            float decomposersDetritusMask = Mathf.Clamp01(curGlobalDetritus * settings.environmentSettings._DecomposersDetritusMask);
            decomposersTotalProductivity = curGlobalDecomposers * settings.environmentSettings._BaseDecompositionRate * decomposersOxygenMask * decomposersDetritusMask;
            
            detritusRemovedByDecomposersLastFrame = decomposersTotalProductivity;
            curGlobalDetritus -= decomposersTotalProductivity;

            nutrientsProduced = decomposersTotalProductivity * settings.environmentSettings._DetritusToNutrientsEfficiency;
            nutrientsProducedByDecomposersLastFrame = nutrientsProduced;
        }

        // waste from algaeReservoir???
        //curGlobalDetritus += wasteProducedByAlgaeReservoirLastFrame;
        curGlobalDetritus += wasteProducedByAnimalParticlesLastFrame;   // will have to add these inputs into the resourceGrid sim texture
        curGlobalDetritus += wasteProducedByPlantParticlesLastFrame;
        curGlobalDetritus += wasteProducedByAgentsLastFrame;
        curGlobalDetritus = Mathf.Max(0f, curGlobalDetritus); // cap at 0f
        curGlobalDetritus = Mathf.Min(curGlobalDetritus, 1000f);
        
        curGlobalNutrients += nutrientsProduced;
        //curGlobalNutrients -= algaeReservoirGrowth;
        curGlobalNutrients -= nutrientsUsedByPlantParticlesLastFrame;
        curGlobalNutrients = Mathf.Max(0f, curGlobalNutrients); // cap at 0f
        curGlobalNutrients = Mathf.Min(curGlobalNutrients, 1000f);
        
        // ***** TEMP!!!!!
        curGlobalNutrients = veggieManager.curGlobalNutrientGridValues.x;
        curGlobalDetritus = veggieManager.curGlobalNutrientGridValues.y + wasteProducedByAgentsLastFrame + wasteProducedByAnimalParticlesLastFrame;
        curGlobalDecomposers = veggieManager.curGlobalNutrientGridValues.z;
        curGlobalAlgaeReservoir = veggieManager.curGlobalNutrientGridValues.w;
        //oxygenUsedByAgentsLastFrame
        curTotalMass = curGlobalNutrients + curGlobalDetritus + curGlobalDecomposers + curGlobalAlgaeReservoir;
    }
    
    public void SetGlobalBiomassVolumes(float totalEggSackVolume, float totalCarrionVolume, float totalAgentBiomass, float weightedAvgLerpVal)
    {
        //Debug.Log("ProcessAgentScores eggVol: " + foodManager.curGlobalEggSackVolume.ToString() + ", carrion: " + foodManager.curGlobalCarrionVolume.ToString());
        curGlobalEggSackVolume = Mathf.Lerp(curGlobalEggSackVolume, totalEggSackVolume, weightedAvgLerpVal);
        curGlobalCarrionVolume = Mathf.Lerp(curGlobalCarrionVolume, totalCarrionVolume, weightedAvgLerpVal);
        curGlobalAgentBiomass = Mathf.Lerp(curGlobalAgentBiomass, totalAgentBiomass, weightedAvgLerpVal);
    }
}
