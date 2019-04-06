using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimResourceManager {

    //public float baseSolarEnergy = 1f;
    public float curGlobalOxygen = 1f;
    public float curGlobalNutrients = 1f;
    public float curGlobalDetritus = 1f;
    public float curGlobalDecomposers = 1f;
    public float curGlobalAlgaeReservoir = 0f;  // separate from algaeParticles -- takes place of algaeGrid
    public float curGlobalAlgaeParticles = 0f;
    public float curGlobalAnimalParticles = 0f;
    public float curGlobalEggSackVolume = 0f;    
    public float curGlobalAgentBiomass = 0f;
    public float curGlobalCarrionVolume = 0f;

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
    
    public float oxygenProducedByAlgaeParticlesLastFrame = 0f;
    public float wasteProducedByAlgaeParticlesLastFrame = 0f;
    public float nutrientsUsedByAlgaeParticlesLastFrame = 0f;

    public float oxygenUsedByDecomposersLastFrame = 0f;
    public float nutrientsProducedByDecomposersLastFrame = 0f;
    public float detritusRemovedByDecomposersLastFrame = 0f;
    
    
	public SimResourceManager() {
        // constructor
        // TEMP:::::
        curGlobalOxygen = 100f;
        curGlobalNutrients = 200f;
        curGlobalDecomposers = 0f;
        curGlobalDetritus = 50f;
    }

    public void Tick(SettingsManager settings, TrophicLayersManager trophicLayersManager) {
        
        // Algae Reservoir Growth:
        //float algaeGrowthEfficiency = 0.1f;
        float algaeGrowthNutrientMask = Mathf.Clamp01(curGlobalNutrients * settings.algaeSettings._AlgaeGrowthNutrientsMask);
        float algaeReservoirGrowth = 0f;
        float wasteProducedByAlgaeReservoirLastFrame = 0f;
        if(trophicLayersManager.GetAlgaeOnOff()) {
            // NEW::  Mirrors AlgaeParticles:
            //curGlobalAlgaeReservoir = curGlobalAlgaeParticles;

            /*
            algaeReservoirGrowth = settings.environmentSettings._BaseSolarEnergy * algaeGrowthNutrientMask * settings.algaeSettings._AlgaeGrowthEfficiency;
            curGlobalAlgaeReservoir += algaeReservoirGrowth;  // Do this Properly!!!
            //wasteProducedByAlgaeReservoirLastFrame = curGlobalAlgaeReservoir * 0.0005f;
            //curGlobalAlgaeReservoir -= wasteProducedByAlgaeReservoirLastFrame;
            // Growth depends on: Light + Nutrients + Current Biomass
            // Animal Particles Consume Algae Reservoir (single global value):
            // Depends on: GlobalAlgaeConcentration + Current Biomass
            curGlobalAlgaeReservoir -= algaeConsumedByAnimalParticlesLastFrame;        
            // Cap:
            curGlobalAlgaeReservoir = Mathf.Max(0f, curGlobalAlgaeReservoir);
            curGlobalAlgaeReservoir = Mathf.Min(curGlobalAlgaeReservoir, 1000f); 
            */
        }
        

        // OXYGEN:
        curGlobalOxygen -= oxygenUsedByAnimalParticlesLastFrame;
        curGlobalOxygen -= oxygenUsedByAgentsLastFrame;
        //float algaeReservoirOxygenProductionEfficiency = 0.001f;
        
        if(trophicLayersManager.GetAlgaeOnOff()) {
            // Revisit how this works? producing oxygen solely proportionally to current mass == leaky  -- tied to growth instead???
            //oxygenProducedByAlgaeReservoirLastFrame = curGlobalAlgaeReservoir * settings.algaeSettings._AlgaeReservoirOxygenProductionEfficiency;
            //curGlobalOxygen += oxygenProducedByAlgaeReservoirLastFrame; // *** Combined Addition of Oxygen produced by plants ***  REFACTOR!!!
            // also add algaeParticle Oxygen production:        
            curGlobalOxygen += oxygenProducedByAlgaeParticlesLastFrame;
        }

        float nutrientsProduced = 0f;
        float decomposersTotalProductivity = 0f;
        if(trophicLayersManager.GetDecomposersOnOff()) {
            float decomposersOxygenMask = Mathf.Clamp01(curGlobalOxygen * settings.environmentSettings._DecomposersOxygenMask);
            float decomposersDetritusMask = Mathf.Clamp01(curGlobalDetritus * settings.environmentSettings._DecomposersDetritusMask);
            decomposersTotalProductivity = curGlobalDecomposers * settings.environmentSettings._BaseDecompositionRate * decomposersOxygenMask * decomposersDetritusMask;
                
            curGlobalDecomposers = Mathf.Lerp(curGlobalDecomposers, curGlobalDetritus, 0.0005f); // *** TEMP:: Scale decomposers with detritus amount
            if(curGlobalDecomposers > 150f) {
                curGlobalDecomposers = 150f;  // try capping?
            }

            detritusRemovedByDecomposersLastFrame = decomposersTotalProductivity;
            curGlobalDetritus -= decomposersTotalProductivity;

            nutrientsProduced = decomposersTotalProductivity * settings.environmentSettings._DetritusToNutrientsEfficiency;
            nutrientsProducedByDecomposersLastFrame = nutrientsProduced;
        }

        // waste from algaeReservoir???
        //curGlobalDetritus += wasteProducedByAlgaeReservoirLastFrame;
        curGlobalDetritus += wasteProducedByAnimalParticlesLastFrame; 
        curGlobalDetritus += wasteProducedByAlgaeParticlesLastFrame;
        curGlobalDetritus += wasteProducedByAgentsLastFrame;
        curGlobalDetritus = Mathf.Max(0f, curGlobalDetritus); // cap at 0f
        curGlobalDetritus = Mathf.Min(curGlobalDetritus, 1000f);
                   
        
        //nutrientsUsedByAlgaeReservoirLastFrame = algaeReservoirGrowth;

        curGlobalNutrients += nutrientsProduced;
        //curGlobalNutrients -= algaeReservoirGrowth;
        curGlobalNutrients -= nutrientsUsedByAlgaeParticlesLastFrame;
        curGlobalNutrients = Mathf.Max(0f, curGlobalNutrients); // cap at 0f
        curGlobalNutrients = Mathf.Min(curGlobalNutrients, 1000f);
        
        if(trophicLayersManager.GetDecomposersOnOff()) {
            oxygenUsedByDecomposersLastFrame = decomposersTotalProductivity * settings.environmentSettings._DecompositionOxygenUsage;
            curGlobalOxygen -= oxygenUsedByDecomposersLastFrame;
            curGlobalOxygen = Mathf.Max(0f, curGlobalOxygen);
            curGlobalOxygen = Mathf.Min(curGlobalOxygen, 1000f);
        }
    }
}
